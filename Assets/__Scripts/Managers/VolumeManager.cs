using System;
using System.Collections;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance { get; private set; }

    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private float maxSprintTime = 5f;
    [SerializeField] private float maxRecoveryTime = 3f;
    [SerializeField] private TransitionAnimator transitionAnimator;
    [SerializeField] private TransitionProfile transitionIn;
    [SerializeField] private TransitionProfile transitionOut;

    [SerializeField] private Volume volume;
    private Vignette vignette;

    private float targetVignetteIntensity = 0f; 

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
    
    public void PlayTeleportTransition()
    {
        StartCoroutine(PlayTransitionSequence());
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
