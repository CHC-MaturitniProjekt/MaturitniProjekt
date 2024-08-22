using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera cam;
    [SerializeField] private InputReader input;
    [SerializeField] private float fov;
    [SerializeField] private float fovIncrease;
    [SerializeField] private float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private Vector2 mouseMove = Vector2.zero;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        input.LookEvent += Input_LookEvent;
        cam.fieldOfView = fov;
    }

    private void Input_LookEvent(Vector2 obj)
    {
        mouseMove = obj;
    }

    private void FixedUpdate()
    {
        //Debug.Log(PlayerManager.Instance.CurrentState);
        Look();
    }

    private void Look()
    {
        float mouseX = mouseMove.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseMove.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 90f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public void FovChange(bool isSprinting)
    {
        switch (isSprinting)
        {
            case true:
                cam.fieldOfView = fov + fovIncrease;
                break;
            case false:
                cam.fieldOfView = fov;
                break;
            default:
                break;
        }
            
    }
}
