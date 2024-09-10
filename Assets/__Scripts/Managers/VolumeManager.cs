using System.Collections;
using System.Runtime.CompilerServices;
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
        Debug.Log(volume.profile.TryGet(out vignette));

        vignette.intensity.value = 0f;
    }

    private void Update()
    {
        if (PlayerManager.Instance.CurrentState == PlayerManager.MovementState.Running)
        {
            UpdateVignetteEffect();
        }
    }

    private void UpdateVignetteEffect()
    {
        float sprintTime = PlayerManager.Instance.GetPlayerSprintTime();
        float sprintRatio = Mathf.Clamp01(sprintTime / maxSprintTime);
        vignette.intensity.value = Mathf.Lerp(0.35f, 0.01f, sprintRatio);

    }
}