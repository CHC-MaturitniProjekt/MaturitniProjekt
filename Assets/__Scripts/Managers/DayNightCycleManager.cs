using System;
using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    private TimeManager timeManager;
    [Header("Light Objects")]
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private float maxSunIntensity;
    [SerializeField] private float maxMoonIntensity;
    
    [Header("Sunrise settings")]
    [SerializeField] private Material sunrise;
    [SerializeField] private Color sunriseLightColor;
    [SerializeField] private Color sunriseFogColor;
    [SerializeField] private float sunriseFogIntensity;
    
    [Header("Morning settings")]
    [SerializeField] private Material morning;
    [SerializeField] private Color morningLightColor;
    [SerializeField] private Color morningFogColor;
    [SerializeField] private float morningFogIntensity;
    
    [Header("Day settings")]
    [SerializeField] private Material day;
    [SerializeField] private Color dayLightColor;
    [SerializeField] private Color dayFogColor;
    [SerializeField] private float dayFogIntensity;
    
    [Header("Sunset settings")]
    [SerializeField] private Material sunset;
    [SerializeField] private Color sunsetLightColor;
    [SerializeField] private Color sunsetFogColor;
    [SerializeField] private float sunsetFogIntensity;
    
    [Header("Night settings")]
    [SerializeField] private Material night;
    [SerializeField] private Color nightLightColor;
    [SerializeField] private Color nightFogColor;
    [SerializeField] private float nightFogIntensity;
    
    [SerializeField] private float skyboxTransitionTime = 2f;

    private Material currentSkybox;
    private Material targetSkybox;
    private float skyboxTransitionTimer = 0f;

    private void Awake()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void Start()
    {
        if (sun == null || moon == null)
        {
            Debug.LogError("Sun or Moon is not assigned!");
            return;
        }
        
        currentSkybox = RenderSettings.skybox;

        UpdateSkyboxAndLights(timeManager.GetWorldTime(), true);
    }

    private void Update()
    {
        if (timeManager == null || sun == null || moon == null) return;
        UpdateSkyboxAndLights(timeManager.GetWorldTime(), false);
        UpdateShaderLightDirections();
    }

    private void UpdateSkyboxAndLights(float timeOfDay, bool instantSet)
    {
        UpdateSunAndMoonRotation(timeOfDay, instantSet);
        UpdateSkybox(timeOfDay);
        UpdateLightIntensity(timeOfDay);
    }

    private void UpdateSunAndMoonRotation(float timeOfDay, bool instantSet)
    {
        float normalizedTime = timeOfDay / 1440f;
        float sunRotation = normalizedTime * 360f - 90f;
        float moonRotation = sunRotation + 180f;

        Quaternion targetSunRotation = Quaternion.Euler(sunRotation, 6, 0);
        Quaternion targetMoonRotation = Quaternion.Euler(moonRotation, 6, 0);

        if (instantSet)
        {
            sun.transform.rotation = targetSunRotation;
            moon.transform.rotation = targetMoonRotation;
        }
        else
        {
            sun.transform.rotation = Quaternion.Slerp(sun.transform.rotation, targetSunRotation, Time.deltaTime * 0.5f);
            moon.transform.rotation = Quaternion.Slerp(moon.transform.rotation, targetMoonRotation, Time.deltaTime * 0.5f);
        }
    }

    private void UpdateLightIntensity(float timeOfDay)
    {
        if (timeOfDay >= 300 && timeOfDay < 1080)
        {
            float normalizedTime = (timeOfDay - 300) / 780f;
            sun.intensity = Mathf.Sin(normalizedTime * Mathf.PI) * maxSunIntensity;
            moon.intensity = 0f;
        }
        else
        {
            float normalizedTime = (timeOfDay >= 1080) ? (timeOfDay - 1080) / 360f : (1440 - timeOfDay + 360) / 360f;
            sun.intensity = 0f;
            moon.intensity = Mathf.Sin(normalizedTime * Mathf.PI) * maxMoonIntensity;
        }
    }
    
    private void UpdateShaderLightDirections()
    {
        Vector3 sunDirection = -sun.transform.forward;
        Vector3 moonDirection = -moon.transform.forward; 
        
        Shader.SetGlobalVector("_SunDirection", sunDirection);
        Shader.SetGlobalVector("_MoonDirection", moonDirection);
    }
    
    private void UpdateSkybox(float timeOfDay)
    {
        Material newSkybox = GetSkyboxForTime(timeOfDay);
        if (newSkybox != targetSkybox)
        {
            targetSkybox = newSkybox;
            skyboxTransitionTimer = 0f;
        }

        if (currentSkybox != targetSkybox)
        {
            skyboxTransitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(skyboxTransitionTimer / skyboxTransitionTime);

            LerpSkybox(currentSkybox, targetSkybox, t);

            if (t >= 1f)
            {
                currentSkybox = targetSkybox;
                RenderSettings.skybox = currentSkybox;
            }
        }
        
        UpdateLightAndFogColors(timeOfDay);

    }

    private void LerpSkybox(Material from, Material to, float t)
    {
        Material lerpedMaterial = new Material(from);
        lerpedMaterial.Lerp(from, to, t);
        RenderSettings.skybox = lerpedMaterial;
    }

   private void UpdateLightAndFogColors(float timeOfDay)
    {
        float normalizedTime;
        float targetFogDensity = 0f;
        Color targetAmbientLight = RenderSettings.ambientLight;
        Color targetFogColor = RenderSettings.fogColor;
        
        if (timeOfDay >= 300 && timeOfDay < 420)
        {
            normalizedTime = (timeOfDay - 300) / 120f;
            targetAmbientLight = sunriseLightColor;
            targetFogColor = sunriseFogColor;
            targetFogDensity = Mathf.Sin(normalizedTime * Mathf.PI) * -sunriseFogIntensity;
        }
        else if (timeOfDay >= 420 && timeOfDay < 600)
        {
            normalizedTime = (timeOfDay - 420) / 180f;
            targetAmbientLight = morningLightColor;
            targetFogColor = morningFogColor;
            targetFogDensity = Mathf.Sin(normalizedTime * Mathf.PI) * -morningFogIntensity;
        }
        else if (timeOfDay >= 600 && timeOfDay < 1020)
        {
            normalizedTime = (timeOfDay - 600) / 420f;
            targetAmbientLight = dayLightColor;
            targetFogColor = dayFogColor;
            targetFogDensity = Mathf.Sin(normalizedTime * Mathf.PI) * -dayFogIntensity;
        }
        else if (timeOfDay >= 1020 && timeOfDay < 1140)
        {
            normalizedTime = (timeOfDay - 1020) / 120f;
            targetAmbientLight = sunsetLightColor;
            targetFogColor = sunsetFogColor;
            targetFogDensity = Mathf.Sin(normalizedTime * Mathf.PI) * -sunsetFogIntensity;
        }
        else
        {
            normalizedTime = (timeOfDay >= 1140) ? (timeOfDay - 1140) / 300f : (timeOfDay + 300) / 300f;
            targetAmbientLight = nightLightColor;
            targetFogColor = nightFogColor;
            targetFogDensity = Mathf.Sin(normalizedTime * Mathf.PI) * -nightFogIntensity;
        }
        
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbientLight, Time.deltaTime * 0.5f);
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * 0.5f);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * 0.5f);
    }
    
    
    private Material GetSkyboxForTime(float timeOfDay)
    {
        switch (timeOfDay)
        {
            case >= 300 and < 420:
                return sunrise;
            case >= 420 and < 600:
                return morning;
            case >= 600 and < 1020:
                return day;
            case >= 1020 and < 1140:
                return sunset;
            case >= 1140 or < 300:
                return night;
            default:
                Debug.LogError("timeOfDay error");
                return night;
        }
    }
}