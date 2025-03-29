using System;
using System.Collections;
using System.Collections.Generic;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class BuildingGradient
{
    public string buildingName;
    public Gradient gradient;
}


public class VolumeManager : MonoBehaviour
{
    private static VolumeManager Instance { get; set; }

    [SerializeField] private float maxSprintTime = 5f;
    [SerializeField] private float maxRecoveryTime = 3f;
    [SerializeField] private TransitionAnimator transitionAnimator;
    [SerializeField] private TransitionProfile transitionIn;
    [SerializeField] private TransitionProfile transitionOut;

    [SerializeField] private Volume volume;
    private Vignette vignette;

    private float targetVignetteIntensity = 0f;

    [SerializeField] private List<BuildingGradient> gradientList = new List<BuildingGradient>();
    private Dictionary<string, Gradient> gradientDictionary = new Dictionary<string, Gradient>();

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (volume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f;
        }

        transitionAnimator = FindFirstObjectByType<TransitionAnimator>();
    }

    private void Update()
    {
        float sprintTime = PlayerManager.Instance.GetPlayerSprintTime();
        float sprintRecoveryTime = PlayerManager.Instance.GetPlayerSprintRecoveryTime();

        if (sprintTime > 0)
        {
            UpdateVignetteEffect(sprintTime);
        }
        else if (sprintRecoveryTime > 0)
        {
            RecoverVignetteEffect(sprintRecoveryTime);
        }
        
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * 5f);
    }

    private void UpdateVignetteEffect(float sprintTime)
    {
        float sprintRatio = Mathf.Clamp01(sprintTime / maxSprintTime);
        targetVignetteIntensity = Mathf.Lerp(0.4f, 0.05f, sprintRatio);
    }

    private void RecoverVignetteEffect(float sprintRecoveryTime)
    {
        float recoveryRatio = 1f - Mathf.Clamp01(sprintRecoveryTime / maxRecoveryTime);
        targetVignetteIntensity = Mathf.Lerp(0.4f, 0f, recoveryRatio); 
    }

    private void Start()
    {
        foreach (var item in gradientList)
        {
            if (!gradientDictionary.ContainsKey(item.buildingName))
            {
                gradientDictionary[item.buildingName] = item.gradient;
            }
        }
    }
   
    public void PlayTeleportTransition(string buildingName)
    {
        if (gradientDictionary.TryGetValue(buildingName, out Gradient gradient))
        {
            ApplyGradientToTransition(gradient);
        }
        else
        {
            Debug.LogWarning($"No gradient found for {buildingName}, using default.");
        }

        StartCoroutine(PlayTransitionSequence());
    }

    private void ApplyGradientToTransition(Gradient gradient)
    {
        transitionIn.gradient = gradient;
        transitionOut.gradient = gradient;
    }

    private IEnumerator PlayTransitionSequence()
    {
        transitionAnimator.SetProgress(0);
        transitionAnimator.SetProfile(transitionIn);
        transitionAnimator.Play();

        yield return new WaitForSeconds(transitionAnimator.profile.duration - 0.9f);

        transitionAnimator.SetProgress(0.15f);
        transitionAnimator.SetProfile(transitionOut);
        transitionAnimator.Play();
    }
}