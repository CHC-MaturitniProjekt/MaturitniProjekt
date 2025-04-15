using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private InputReader input;
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private CinemachineCamera pcCam;
    [SerializeField] private CinemachineCamera[] cameras;
    private CinemachineCamera currentCamera;

    private bool isCameraModeActive = false;
    private int activeCameraIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //input.CamModeEvent += Input_SecurityCamModeEvent;
        //input.CamIndexIncrement += () => SwitchSecurityCamera(1);
        //input.CamIndexDecrement += () => SwitchSecurityCamera(-1);

        playerCam.Priority = new PrioritySettings { Value = 10 };
        currentCamera = playerCam;

        foreach (var cam in cameras)
        {
            cam.Priority = new PrioritySettings { Value = 0 };
        }
    }

    private void switchCamera(CinemachineCamera to)
    {
        currentCamera.Priority = new PrioritySettings { Value = 0 };
        to.Priority = new PrioritySettings { Value = 10 };
        currentCamera = to;
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
        switchCamera(cameras[activeCameraIndex]);
    }

    private void ExitSecurityCameraMode()
    {
        isCameraModeActive = false;
        switchCamera(playerCam);
    }

    private void SwitchSecurityCamera(int direction)
    {
        if (!isCameraModeActive) return;

        activeCameraIndex += direction;
        if (activeCameraIndex < 0) activeCameraIndex = cameras.Length - 1;
        if (activeCameraIndex >= cameras.Length) activeCameraIndex = 0;

        switchCamera(cameras[activeCameraIndex]);
    }

    public void EnterPcCamera()
    {
        switchCamera(pcCam);
    }

    public void EnterPlayerCamera()
    {
        switchCamera(playerCam);
    }
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

}
