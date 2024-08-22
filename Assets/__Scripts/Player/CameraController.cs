using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera cam;
    [SerializeField] private InputReader input;
    [SerializeField] private float fov;
    [SerializeField] private float runFovIncrease;
    [SerializeField] private float jumpFovIncrease;
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Head Bob Settings")]
    [SerializeField] private float walkSpeed = 0.18f;
    [SerializeField] private float walkAmount = 0.2f;
    [SerializeField] private float runSpeed = 0.25f;
    [SerializeField] private float runAmount = 0.3f;
    [SerializeField] private float idleSpeed = 0.05f;
    [SerializeField] private float idleAmount = 0.05f;

    private float xRotation = 0f;
    private Vector2 mouseMove = Vector2.zero;
    private float timer = 0.0f;
    private Vector3 initialCameraPosition;

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
        Debug.Log(PlayerManager.Instance.CurrentState);
        FovChange();

        Look();
        HeadBob();
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

    public void FovChange()
    {
        float targetFov;

        switch (PlayerManager.Instance.CurrentState)
        {
            case PlayerManager.MovementState.Running:
                targetFov = fov + runFovIncrease;
                break;
            case PlayerManager.MovementState.Jumping:
                targetFov = fov + jumpFovIncrease;
                break;
            default:
                targetFov = fov;
                break;
        }
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 3);
    }

    public void HeadBob()
    {
        float bobbingSpeed;
        float bobbingAmount;

        switch (PlayerManager.Instance.CurrentState)
        {
            case PlayerManager.MovementState.Running:
                bobbingSpeed = runSpeed;
                bobbingAmount = runAmount;
                break;
            case PlayerManager.MovementState.Walking:
                bobbingSpeed = walkSpeed;
                bobbingAmount = walkAmount;
                break;
            default:
                bobbingSpeed = idleSpeed;
                bobbingAmount = idleAmount;
                break;
        }

        timer += Time.deltaTime * bobbingSpeed * 10;
        float waveslice = Mathf.Sin(timer);
        cam.transform.localPosition = initialCameraPosition + new Vector3(0, waveslice * bobbingAmount, 0);
        
    }

    public void Exhaust()
    {
        if(PlayerManager.Instance.CurrentState == PlayerManager.MovementState.Running)
        {
            //
        }
    }
}
