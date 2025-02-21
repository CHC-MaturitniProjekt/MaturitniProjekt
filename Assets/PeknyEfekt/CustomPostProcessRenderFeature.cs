using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessRenderFeature : ScriptableRendererFeature
{
    [SerializeField]
    private Shader bloomShader;
    [SerializeField]
    private Shader compositShader;

    private Material bloomMaterial;
    private Material compositMaterial;

    private CustomPostProcessPass customPass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(customPass != null)
            renderer.EnqueuePass(customPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (customPass != null)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                customPass.ConfigureInput(ScriptableRenderPassInput.Depth);
                customPass.ConfigureInput(ScriptableRenderPassInput.Color);
                customPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
            }
        }
    }

    public override void Create()
    {
        if (bloomShader != null && compositShader != null)
        {
            bloomMaterial = CoreUtils.CreateEngineMaterial(bloomShader);
            compositMaterial = CoreUtils.CreateEngineMaterial(compositShader);

            customPass = new CustomPostProcessPass(bloomMaterial, compositMaterial);
        }
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(bloomMaterial);
        CoreUtils.Destroy(compositMaterial);
    }
}
