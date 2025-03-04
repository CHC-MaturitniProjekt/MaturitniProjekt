using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private InputReader input;

    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private CinemachineCamera pcCam;
    [SerializeField] private CinemachineCamera[] cameras;

    private bool isCameraModeActive = false;
    private int activeCameraIndex = 0;
    
    void Start()
    {
        input.CamModeEvent += Input_SecurityCamModeEvent;
        input.CamIndexIncrement += () => SwitchSecurityCamera(1);
        input.CamIndexDecrement += () => SwitchSecurityCamera(-1);

        playerCam.Priority = new PrioritySettings { Value = 10 };
        foreach (var cam in cameras)
        {
            cam.Priority = new PrioritySettings { Value = 0 };
        }
    }

    private void switchCamera(CinemachineCamera from, CinemachineCamera to)
    {
        from.Priority = new PrioritySettings { Value = 0 };
        to.Priority = new PrioritySettings { Value = 10 };
    }

    private void Input_SecurityCamModeEvent()
    {
        if (isCameraModeActive)
        {
            ExitSecurityCameraMode();
        }
        else
        {
            EnterSecurityCameraMode();
        }
    }

    private void EnterSecurityCameraMode()
    {
        isCameraModeActive = true;
        activeCameraIndex = 0;
        switchCamera(playerCam, cameras[activeCameraIndex]);
    }

    private void ExitSecurityCameraMode()
    {
        isCameraModeActive = false;
        switchCamera(cameras[activeCameraIndex], playerCam);
    }

    private void SwitchSecurityCamera(int direction)
    {
        if (!isCameraModeActive) return;

        var old = activeCameraIndex;
        activeCameraIndex += direction;
        if (activeCameraIndex < 0) activeCameraIndex = cameras.Length - 1;
        if (activeCameraIndex >= cameras.Length) activeCameraIndex = 0;

        switchCamera(cameras[old], cameras[activeCameraIndex]);
    }
}
