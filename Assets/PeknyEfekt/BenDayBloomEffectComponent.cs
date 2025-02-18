using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable,VolumeComponentMenu("Custom/Ben Day Bloom")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public class BenDayBloomEffectComponent : VolumeComponent, IPostProcessComponent
{
    [Header("Bloom Settings")]

    public MinFloatParameter threshold = new MinFloatParameter(0.9f, 0f);
    public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);
    public ClampedFloatParameter scatter = new ClampedFloatParameter(0.7f, 0f, 1f);
    public MinFloatParameter clamp = new MinFloatParameter(65472f, 0f);
    public ColorParameter tint = new ColorParameter(Color.white, false, false, true);
    public ClampedIntParameter maxIterations = new ClampedIntParameter(6, 2, 8);
    public BoolParameter highQualityFiltering = new BoolParameter(false);

    public bool IsActive() => intensity.value > 0f;

    [Header("Benday")]
    public IntParameter dotsDensity = new IntParameter(10, true);
    public ClampedFloatParameter dotsCutoff = new ClampedFloatParameter(0.4f, 0, 1, true);
    public Vector2Parameter scrollDirection = new Vector2Parameter(new Vector2());


    public bool IsTileCompatible()
    {
        return false;
    }
}
