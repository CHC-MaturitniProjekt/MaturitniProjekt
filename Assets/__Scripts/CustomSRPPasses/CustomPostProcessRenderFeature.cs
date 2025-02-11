using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader bloomShader;
    [SerializeField] private Shader compositeShader;
    
    private Material bloomMaterial;
    private Material compositeMaterial;
    
    private CustomPostProcessPass customPass;
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customPass);
    }

    public override void Create()
    {
        bloomMaterial = CoreUtils.CreateEngineMaterial(bloomShader);
        compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);
        
        customPass = new CustomPostProcessPass(bloomMaterial, compositeMaterial);
    }
    
    protected override void Dispose(bool disposing) 
    {
        CoreUtils.Destroy(bloomMaterial);
        CoreUtils.Destroy(compositeMaterial);
    }
    
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if(renderingData.cameraData.camera.cameraType == CameraType.Game)
        {
            pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            pass.ConfigureInput(ScriptableRenderPassInput.Color);
            pass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
        }
    }
}
