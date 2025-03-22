using System;
using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    private TimeManager timeManager;
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private float maxSunIntensity;
    [SerializeField] private float maxMoonIntensity;
    [SerializeField] private Material sunrise;
    [SerializeField] private Material morning;
    [SerializeField] private Material day;
    [SerializeField] private Material sunset;
    [SerializeField] private Material night;
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
            Debug.LogError("Sun or Moon (Directional Light) is not assigned!");
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
    }

    private void LerpSkybox(Material from, Material to, float t)
    {
        Material lerpedMaterial = new Material(from);
        lerpedMaterial.Lerp(from, to, t);
        RenderSettings.skybox = lerpedMaterial;
    }


    private Material GetSkyboxForTime(float timeOfDay)
    {
        Debug.Log("Current timeOfDay: " + timeOfDay);

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
                Debug.LogWarning("timeOfDay does not fall within any expected range.");
                return night;
        }
    }
}