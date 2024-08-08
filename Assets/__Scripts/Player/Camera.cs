using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform playerBody;
    [SerializeField] private InputReader input;
    [SerializeField] private float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private Vector2 mouseMove = Vector2.zero;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        input.LookEvent += Input_LookEvent;
    }

    private void Input_LookEvent(Vector2 obj)
    {
        mouseMove = obj;
    }

    private void FixedUpdate()
    {
        Look();
    }

    private void Look()
    {
        float mouseX = mouseMove.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseMove.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCam.localRotation = Quaternion.Euler(xRotation, 90f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
