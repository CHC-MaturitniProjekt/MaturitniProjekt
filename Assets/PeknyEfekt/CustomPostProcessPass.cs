using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class CustomPostProcessPass : ScriptableRenderPass
{
    private Material bloomMaterial;
    private Material compositeMaterial;

    private RenderTextureDescriptor descriptor;

    private RTHandle cameraColorTargetHandle;
    private RTHandle cameraDepthTargetHandle;

    const int MaxPyramidSize = 16;

    private int[] m_bloomMipUp;
    private int[] m_bloomMipDown;
    
    private RTHandle[] bloomMipUp;
    private RTHandle[] bloomMipDown;
    private GraphicsFormat hdrFormat;

    private BenDayBloomEffectComponent bloomEffect;

    public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial)
    {
        this.bloomMaterial = bloomMaterial;
        this.compositeMaterial = compositeMaterial;

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing; // Or AfterRenderingOpaques

        m_bloomMipUp = new int[MaxPyramidSize];
        m_bloomMipDown = new int[MaxPyramidSize];
        bloomMipUp = new RTHandle[MaxPyramidSize];
        bloomMipDown = new RTHandle[MaxPyramidSize];
        for (int i = 0; i < MaxPyramidSize; i++)
        {
            m_bloomMipUp[i] = Shader.PropertyToID($"_BloomMipUp" + i);
            m_bloomMipDown[i] = Shader.PropertyToID($"_BloomMipDown" + i);
            bloomMipUp[i] = RTHandles.Alloc($"_BloomMipUp{i}");
            bloomMipDown[i] = RTHandles.Alloc($"_BloomMipDown{i}");
        }

        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
        hdrFormat = SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage)
            ? GraphicsFormat.B10G11R11_UFloatPack32
            : QualitySettings.activeColorSpace == ColorSpace.Linear
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        descriptor = renderingData.cameraData.cameraTargetDescriptor;
    }

    public void SetTarget(RTHandle colorHandle, RTHandle depthHandle)
    {
        cameraColorTargetHandle = colorHandle;
        cameraDepthTargetHandle = depthHandle;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (bloomMaterial == null || compositeMaterial == null) return;

        VolumeStack stack = UnityEngine.Rendering.VolumeManager.instance.stack; // Corrected: VolumeManager.Instance.stack
        bloomEffect = stack.GetComponent<BenDayBloomEffectComponent>();

        if (bloomEffect == null) return;

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Effects")))
        {
            SetupBloom(cmd, cameraColorTargetHandle);

            compositeMaterial.SetFloat("_Cutoff", bloomEffect.dotsCutoff.value);
            compositeMaterial.SetFloat("_Density", bloomEffect.dotsDensity.value);
            compositeMaterial.SetVector("_Direction", bloomEffect.scrollDirection.value);

            Blitter.BlitCameraTexture(cmd, cameraColorTargetHandle, cameraColorTargetHandle, compositeMaterial, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    private void SetupBloom(CommandBuffer cmd, RTHandle source)
    {
        int downres = 1;
        int tw = descriptor.width >> downres;
        int th = descriptor.height >> downres;

        int maxSize = Mathf.Max(tw, th);
        int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
        int mipCount = Mathf.Clamp(iterations, 1, bloomEffect.maxIterations.value);

        float clamp = bloomEffect.clamp.value;
        float threshold = Mathf.GammaToLinearSpace(bloomEffect.threshold.value);
        float thresholdKnee = threshold * 0.5f;

        float scatter = Mathf.Lerp(0.05f, 0.95f, bloomEffect.scatter.value);
        var m_bloomMaterial = bloomMaterial;
        
        m_bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));

        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);
        for (int i = 0; i < mipCount; i++)
        {
            RenderingUtils.ReAllocateIfNeeded(ref bloomMipUp[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: bloomMipUp[i].name);
            RenderingUtils.ReAllocateIfNeeded(ref bloomMipDown[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: bloomMipDown[i].name);
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
        }

        if (bloomEffect.maxIterations.value > 6)
        {
            bloomMaterial.EnableKeyword("_BLOOM_HQ");
            Debug.Log("Enabled _BLOOM_HQ keyword");
        }
        else
        {
            bloomMaterial.DisableKeyword("_BLOOM_HQ");
            Debug.Log("Disabled _BLOOM_HQ keyword");
        }

        bloomMaterial.mainTexture = source; // Assign the source texture!

        Blitter.BlitCameraTexture(cmd, source, bloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 0);

        var lastDown = bloomMipDown[0];
        for (int i = 1; i < mipCount; i++)
        {
            Blitter.BlitCameraTexture(cmd, lastDown, bloomMipUp[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 1);
            Blitter.BlitCameraTexture(cmd, bloomMipUp[i], bloomMipDown[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 2);

            lastDown = bloomMipDown[i];
        }

        for (int i = mipCount - 2; i >= 0; i--)
        {
            var lowMip = (i == mipCount - 2) ? bloomMipDown[i + 1] : bloomMipUp[i + 1];
            var highMip = bloomMipDown[i];
            var dst = bloomMipUp[i];

            cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
            Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);
        }

        cmd.SetGlobalTexture("_Bloom_Texture", bloomMipUp[0]);
        cmd.SetGlobalFloat("_BloomIntensity", bloomEffect.intensity.value);
    }
    
    RenderTextureDescriptor GetCompatibleDescripator() => GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, hdrFormat);
    
    RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
        => GetCompatibleDescriptor(descriptor, width, height, format, depthBufferBits);

    internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
    {
        desc.depthBufferBits = (int)depthBufferBits;
        desc.msaaSamples = 1;
        desc.width = width;
        desc.height = height;
        desc.graphicsFormat = format;
        return desc;
    }
}