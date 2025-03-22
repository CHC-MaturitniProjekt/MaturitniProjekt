using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    private TimeManager timeManager;
    [SerializeField] private Light sun; // Main directional light (sun)
    [SerializeField] private Material[] skyboxes; // Array of skybox materials
    [SerializeField] private float skyboxTransitionTime = 2f; // Time to transition between skyboxes

    private int currentSkyboxIndex = 0;
    private float skyboxTransitionTimer = 0f;

    private void Awake()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void Start()
    {
        if (sun == null)
        {
            Debug.LogError("Sun (Directional Light) is not assigned!");
            return;
        }

        if (skyboxes.Length == 0)
        {
            Debug.LogError("No skyboxes assigned!");
            return;
        }

        // Set the initial skybox based on the current time
        float timeOfDay = timeManager.GetWorldTime();
        currentSkyboxIndex = GetSkyboxIndexForTime(timeOfDay);
        RenderSettings.skybox = skyboxes[currentSkyboxIndex];

        // Initialize sun rotation based on the current time
        UpdateSunRotation(timeOfDay);
    }

    private void Update()
    {
        if (timeManager == null || sun == null || skyboxes.Length == 0) return;

        // Update sun rotation based on the time of day
        UpdateSunRotation(timeManager.GetWorldTime());

        // Update skybox based on the time of day
        UpdateSkybox();
    }

    private void UpdateSunRotation(float timeOfDay)
    {
        float sunRotation;

        if (timeOfDay >= 300 && timeOfDay < 480) // Morning (5:00 AM - 8:00 AM)
        {
            // Rotate from 0 to 50 degrees
            sunRotation = Mathf.Lerp(0, 50, (timeOfDay - 300) / 180f);
        }
        else if (timeOfDay >= 480 && timeOfDay < 1080) // Day (8:00 AM - 6:00 PM)
        {
            // Rotate from 50 to 160 degrees
            sunRotation = Mathf.Lerp(50, 160, (timeOfDay - 480) / 600f);
        }
        else if (timeOfDay >= 1080 && timeOfDay < 1260) // Evening (6:00 PM - 9:00 PM)
        {
            // Rotate from 160 to 180 degrees
            sunRotation = Mathf.Lerp(160, 180, (timeOfDay - 1080) / 180f);
        }
        else // Night (9:00 PM - 5:00 AM)
        {
            // Instantly reset the sun's rotation to 0 degrees at the start of the night
            sunRotation = 0;
        }

        // Smoothly rotate the sun (except during the reset at night)
        if (timeOfDay < 1260 || timeOfDay >= 300) // Avoid smoothing during the reset
        {
            Quaternion targetRotation = Quaternion.Euler(sunRotation, 0, 0);
            sun.transform.rotation = Quaternion.Slerp(sun.transform.rotation, targetRotation, Time.deltaTime * 0.5f); // Adjust the multiplier for smoother rotation
        }
        else
        {
            // Instantly set the sun's rotation at the start of the night
            sun.transform.rotation = Quaternion.Euler(sunRotation, 0, 0);
        }
    }

    private void UpdateSkybox()
    {
        float timeOfDay = timeManager.GetWorldTime();
        int newSkyboxIndex = GetSkyboxIndexForTime(timeOfDay);

        if (newSkyboxIndex != currentSkyboxIndex)
        {
            skyboxTransitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(skyboxTransitionTimer / skyboxTransitionTime);

            // Lerp between the current skybox and the new skybox
            RenderSettings.skybox.Lerp(skyboxes[currentSkyboxIndex], skyboxes[newSkyboxIndex], t);

            if (t >= 1f)
            {
                currentSkyboxIndex = newSkyboxIndex;
                skyboxTransitionTimer = 0f;
            }
        }
    }

    private int GetSkyboxIndexForTime(float timeOfDay)
    {
        // Example: Divide the day into 4 parts for 4 different skyboxes
        if (timeOfDay >= 300 && timeOfDay < 480) return 0; // Morning (5:00 AM - 8:00 AM)
        if (timeOfDay >= 480 && timeOfDay < 1080) return 1; // Day (8:00 AM - 6:00 PM)
        if (timeOfDay >= 1080 && timeOfDay < 1260) return 2; // Evening (6:00 PM - 9:00 PM)
        return 3; // Night (9:00 PM - 5:00 AM)
    }
}