using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private InputReader input;
    [SerializeField] private float fov;
    [SerializeField] private float runFovIncrease;
    [SerializeField] private float jumpFovIncrease;
    [SerializeField] private float crouchFovIncrease;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float crouchHeight;

    [Header("Head Bob Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkAmount;
    [SerializeField] private float runSpeed;
    [SerializeField] private float runAmount;
    [SerializeField] private float idleSpeed;
    [SerializeField] private float idleAmount;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchAmount;

    private float xRotation = 0f;
    private Vector2 mouseMove = Vector2.zero;
    private float timer = 0.0f;
    private Vector3 initialCameraPosition;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    
    [HideInInspector] public bool isUsingPC;
    CursorController cursorController;
    
    private void Start()
    {
        cursorController = FindObjectOfType<CursorController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        input.LookEvent += Input_LookEvent;
        cam.m_Lens.FieldOfView = fov;
        
        isUsingPC = false;
    }

    private void Input_LookEvent(Vector2 obj)
    {
        mouseMove = obj;
    }

    private void LateUpdate()
    {
        FovChange();
        Look();
        HeadBob();

        Crouch();
    }

    private void Look()
    { 
        if (!isUsingPC) 
        { 
            /*
            currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, mouseMove, ref currentMouseDeltaVelocity, 0.05f);
            */

            float mouseX = mouseMove.x * mouseSensitivity * Time.deltaTime;
            float mouseY = mouseMove.y * mouseSensitivity * Time.deltaTime;

        
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);                             
            
            cam.transform.localRotation = Quaternion.Euler(xRotation, 90f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {   
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
        
       
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
            case PlayerManager.MovementState.Crouching:
                targetFov = fov + crouchFovIncrease;
                break;
            case PlayerManager.MovementState.CrouchRun:
                targetFov = fov + (crouchFovIncrease + runFovIncrease) / 2;
                break;
            default:
                targetFov = fov;
                break;
        }
        cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, targetFov, Time.deltaTime * 3);
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
            case PlayerManager.MovementState.Crouching:
                bobbingSpeed = crouchSpeed;
                bobbingAmount = crouchAmount;
                break;
            case PlayerManager.MovementState.CrouchRun:
                bobbingSpeed = (crouchSpeed + runSpeed) / 2;
                bobbingAmount = (crouchAmount + runAmount) / 2;
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

    private void Crouch()
    {
        if (PlayerManager.Instance.CurrentState == PlayerManager.MovementState.Crouching || PlayerManager.Instance.CurrentState == PlayerManager.MovementState.CrouchRun)
        {
            cam.transform.localPosition += new Vector3(0, crouchHeight, 0);
        } 
        else
        {
            cam.transform.localPosition += initialCameraPosition;
        }
        
    }

    public void Exhaust()
    {
        if(PlayerManager.Instance.CurrentState == PlayerManager.MovementState.Running)
        {
            //
        }
    }
}
