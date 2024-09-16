using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private InputReader input;

    [SerializeField] private CinemachineVirtualCamera playerCam;
    [SerializeField] private CinemachineVirtualCamera[] cameras;

    private bool isCameraModeActive = false;
    private int activeCameraIndex = 0;

    void Start()
    {
        input.CamModeEvent += Input_CamModeEvent;
        input.CamIndexIncrement += () => SwitchCamera(1);
        input.CamIndexDecrement += () => SwitchCamera(-1);

        playerCam.Priority = 10;
        foreach (var cam in cameras)
        {
            cam.Priority = 0;
        }
    }

    private void Input_CamModeEvent()
    {
        if (isCameraModeActive)
        {
            ExitCameraMode();
        }
        else
        {
            EnterCameraMode();
        }
    }

    private void EnterCameraMode()
    {
        isCameraModeActive = true;

        playerCam.Priority = 0;
        activeCameraIndex = 0;
        cameras[activeCameraIndex].Priority = 10;
    }

    private void ExitCameraMode()
    {
        isCameraModeActive = false;

        foreach (var cam in cameras)
        {
            cam.Priority = 0;
        }
        playerCam.Priority = 10;
    }

    private void SwitchCamera(int direction)
    {
        if (!isCameraModeActive) return;

        cameras[activeCameraIndex].Priority = 0;

        activeCameraIndex += direction;
        if (activeCameraIndex < 0) activeCameraIndex = cameras.Length - 1;
        if (activeCameraIndex >= cameras.Length) activeCameraIndex = 0;

        cameras[activeCameraIndex].Priority = 10;
    }
}
