using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessPass : ScriptableRenderPass
{
    private Material bloomMaterial;
    private Material compositeMaterial;
    
    const int maxPyramidSize = 16;
    private int[] bloomMipUp;
    private int[] bloomMipDown;
    private RTHandle[] rtBloomMipUp;
    private RTHandle[] rtBloomMipDown;
    private GraphicsFormat hdrFormat;
    
    public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial)
    {
        this.bloomMaterial = bloomMaterial;
        this.compositeMaterial = compositeMaterial;

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        
        bloomMipUp = new int[maxPyramidSize];
        bloomMipDown = new int[maxPyramidSize];
        rtBloomMipUp = new RTHandle[maxPyramidSize];
        rtBloomMipDown = new RTHandle[maxPyramidSize];
        
        for (int i = 0; i < maxPyramidSize; i++)
        {
            bloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            bloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            rtBloomMipUp[i] = RTHandles.Alloc(bloomMipUp[i], name: "_BloomMipUp" + i);
            rtBloomMipDown[i] = RTHandles.Alloc(bloomMipDown[i], name: "_BloomMipDown" + i);
        }
        
        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
        if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage))
        {
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else
        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear ? 
                GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm;
        }
    } 
    
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        descriptor = renderingData.cameraData.cameraTargetDescriptor;
    }
    
    public void SetTarget(RTHandle cameraColorTargetHandle, RTHandle cameraDepthTargetHandle)
    {
        this.cameraColorTargetHandle = cameraColorTargetHandle;
        this.cameraDepthTargetHandle = cameraDepthTargetHandle;
    }
    
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        VolumeStack stack = VolumeManager.instance.stack;
        bloomEffect = stack.GetComponent<DottedBloom>();
        
        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilerSampler("Custom post process effect")))
        {
            SetupBloom(cmd, cameraColorTargetHandle);
        }
       
    }
}
