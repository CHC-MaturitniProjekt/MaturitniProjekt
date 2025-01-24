using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance { get; private set; }

    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private float maxSprintTime = 5f;

    [SerializeField] private Volume volume;
    private Vignette vignette;

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
            KeepMaxVignetteEffect();
        }
        else
        {
            SmoothResetVignetteEffect();
        }
    }

    private void UpdateVignetteEffect(float sprintTime)
    {
        float sprintRatio = Mathf.Clamp01(sprintTime / maxSprintTime);
        float targetIntensity = Mathf.Lerp(0.35f, 0.05f, sprintRatio); 
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * 5f);
    }

    private void KeepMaxVignetteEffect()
    {
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.35f, Time.deltaTime * 5f);
    }

    private void SmoothResetVignetteEffect()
    {
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime * 2f);
    }
}
