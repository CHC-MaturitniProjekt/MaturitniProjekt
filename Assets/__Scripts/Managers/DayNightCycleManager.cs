using System;
using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    private TimeManager timeManager;
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private float maxSunIntensity;
    [SerializeField] private float maxMoonIntensity;
    [SerializeField] private Material[] skyboxes; // 0: Sunrise, 1: Morning, 2: Day, 3: Sunset, 4: Night
    [SerializeField] private float skyboxTransitionTime = 2f;

    private int currentSkyboxIndex = 0;
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
        if (skyboxes.Length != 5)
        {
            Debug.LogError("Five skyboxes required: Sunrise, Morning, Day, Sunset, Night!");
            return;
        }
        
        UpdateSkyboxAndLights(timeManager.GetWorldTime(), true);
    }

    private void Update()
    {
        if (timeManager == null || sun == null || moon == null || skyboxes.Length != 5) return;
        UpdateSkyboxAndLights(timeManager.GetWorldTime(), false);
    }

    private void UpdateSkyboxAndLights(float timeOfDay, bool instantSet)
    {
        UpdateSunAndMoonRotation(timeOfDay, instantSet);
        UpdateSkybox(timeOfDay);
        UpdateLightIntensity(timeOfDay);
    }

    private void UpdateSunAndMoonRotation(float timeOfDay, bool instantSet)
    {
        float normalizedTime = timeOfDay / 1440f; // Normalize time to 0-1 range
        float sunRotation = normalizedTime * 360f - 90f; // -90 to 270 degrees
        float moonRotation = sunRotation + 180f; // Moon opposite to sun

        Quaternion targetSunRotation = Quaternion.Euler(sunRotation, 0, 0);
        Quaternion targetMoonRotation = Quaternion.Euler(moonRotation, 0, 0);

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
        if (timeOfDay >= 300 && timeOfDay < 1080) // Daytime (5 AM - 6 PM)
        {
            sun.intensity = Mathf.Lerp(0f, maxSunIntensity, (timeOfDay - 300) / 780f);
            moon.intensity = 0f;
        }
        else // Nighttime (6 PM - 5 AM)
        {
            sun.intensity = 0f;
            moon.intensity = Mathf.Lerp(0f, maxMoonIntensity, (timeOfDay >= 1080) ? (timeOfDay - 1080) / 360f : (1440 - timeOfDay) / 300f);
        }
    }

    private void UpdateSkybox(float timeOfDay)
    {
        int newSkyboxIndex = GetSkyboxIndexForTime(timeOfDay);
        if (newSkyboxIndex != currentSkyboxIndex)
        {
            skyboxTransitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(skyboxTransitionTimer / skyboxTransitionTime);

            RenderSettings.skybox.Lerp(skyboxes[currentSkyboxIndex], skyboxes[newSkyboxIndex], t);

            if (t >= 1f)
            {
                currentSkyboxIndex = newSkyboxIndex;
                skyboxTransitionTimer = 0f;
            }
        }
        else
        {
            skyboxTransitionTimer = 0f;
        }
    }

    private int GetSkyboxIndexForTime(float timeOfDay)
    {
        if (timeOfDay >= 240 && timeOfDay < 300) return 0; // Sunrise (4:00 AM - 5:00 AM)
        if (timeOfDay >= 300 && timeOfDay < 480) return 1; // Morning (5:00 AM - 8:00 AM)
        if (timeOfDay >= 480 && timeOfDay < 1080) return 2; // Day (8:00 AM - 6:00 PM)
        if (timeOfDay >= 1080 && timeOfDay < 1260) return 3; // Sunset (6:00 PM - 9:00 PM)
        return 4; // Night (9:00 PM - 4:00 AM)
    }
}