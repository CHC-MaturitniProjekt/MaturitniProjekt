using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera playerCam;  // Player camera
    [SerializeField] private CinemachineVirtualCamera[] cameras;  // Array of 4 additional cameras

    private bool isCameraModeActive = false;   // Tracks if we are in "camera mode"
    private int activeCameraIndex = 0;         // Tracks the currently active camera in "camera mode"

    void Start()
    {
        // Initialize priorities: Player camera active, other cameras inactive
        playerCam.Priority = 10;
        foreach (var cam in cameras)
        {
            cam.Priority = 0;
        }
    }

    void Update()
    {
        // Press F to toggle between "player camera" and "camera mode"
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isCameraModeActive)
            {
                ExitCameraMode();  // Return to player camera
            }
            else
            {
                EnterCameraMode(); // Enter camera mode and switch to first camera
            }
        }

        // If we're in camera mode, handle Q/E input to toggle between cameras
        if (isCameraModeActive)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchCamera(-1);  // Cycle to previous camera
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SwitchCamera(1);   // Cycle to next camera
            }
        }
    }

    // Method to enter camera mode
    private void EnterCameraMode()
    {
        isCameraModeActive = true;

        // Lower the priority of the player camera
        playerCam.Priority = 0;

        // Switch to the first camera in the array
        activeCameraIndex = 0;
        cameras[activeCameraIndex].Priority = 10;
    }

    // Method to exit camera mode and return to player camera
    private void ExitCameraMode()
    {
        isCameraModeActive = false;

        // Lower the priority of all the other cameras
        foreach (var cam in cameras)
        {
            cam.Priority = 0;
        }

        // Return to the player camera
        playerCam.Priority = 10;
    }

    // Method to switch between cameras in the array
    private void SwitchCamera(int direction)
    {
        // Deactivate current camera
        cameras[activeCameraIndex].Priority = 0;

        // Update the activeCameraIndex, looping within the bounds of the array
        activeCameraIndex += direction;
        if (activeCameraIndex < 0) activeCameraIndex = cameras.Length - 1;
        if (activeCameraIndex >= cameras.Length) activeCameraIndex = 0;

        // Activate the new camera
        cameras[activeCameraIndex].Priority = 10;
    }
}
