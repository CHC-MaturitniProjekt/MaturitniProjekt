using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private InputReader input;
    [SerializeField] private float fov;
    [SerializeField] private float runFovIncrease;
    [SerializeField] private float jumpFovIncrease;
    //[SerializeField] private float crouchFovIncrease;
    public float mouseSensitivity = 100f;
    //[SerializeField] private float crouchHeight;

    [Header("Head Bob Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkAmount;
    [SerializeField] private float runSpeed;
    [SerializeField] private float runAmount;
    [SerializeField] private float idleSpeed;
    [SerializeField] private float idleAmount;
    /*[SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchAmount;*/

    private float xRotation = 0f;
    private Vector2 mouseMove = Vector2.zero;
    private float timer = 0.0f;
    private Vector3 initialCameraPosition;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    public bool isInConvo = false;
    
    private void Start()
    {   
        Cursor.lockState = CursorLockMode.Locked;
        input.LookEvent += Input_LookEvent;
        cam.Lens.FieldOfView = fov;
    }

    private void Input_LookEvent(Vector2 obj)
    {
        mouseMove = obj;
    }

    private void LateUpdate()
    {
        if (!PlayerManager.Instance.isDisabled)
        {
            FovChange();
            HeadBob();

            //Crouch();
        }
    }
    
    private void FixedUpdate()
    {
        if (!PlayerManager.Instance.isDisabled)
        {
            Look();
        }
    }

    private void Look()
    { 
        float mouseX = mouseMove.x * (!isInConvo ? mouseSensitivity : 1f) * Time.deltaTime;
        float mouseY = mouseMove.y * (!isInConvo ? mouseSensitivity : 1f) * Time.deltaTime;
    
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);                             
        
        cam.transform.localRotation = Quaternion.Euler(xRotation, 90f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void FovChange()
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
            /*case PlayerManager.MovementState.Crouching:           NO CROUCH IN EARLY ACCESS
                targetFov = fov + crouchFovIncrease;
                break;
            case PlayerManager.MovementState.CrouchRun:
                targetFov = fov + (crouchFovIncrease + runFovIncrease) / 2;
                break;*/
            default:
                targetFov = fov;
                break;
        }
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFov, Time.deltaTime * 3);
    }

    private float currentBobbingAmount = 0f;
    private float currentBobbingSpeed = 0f; 

    private void HeadBob()
    {
        float targetBobbingSpeed;
        float targetBobbingAmount;

        switch (PlayerManager.Instance.CurrentState)
        {
            case PlayerManager.MovementState.Running:
                targetBobbingSpeed = runSpeed;
                targetBobbingAmount = runAmount;
                break;
            case PlayerManager.MovementState.Walking:
                targetBobbingSpeed = walkSpeed;
                targetBobbingAmount = walkAmount;
                break;
            default:
                targetBobbingSpeed = idleSpeed;
                targetBobbingAmount = idleAmount;
                break;
        }

        currentBobbingSpeed = Mathf.Lerp(currentBobbingSpeed, targetBobbingSpeed, Time.deltaTime * 5f);
    
        if (PlayerManager.Instance.isRecovering)
        {
            targetBobbingAmount += 0.1f;
        }
        currentBobbingAmount = Mathf.Lerp(currentBobbingAmount, targetBobbingAmount, Time.deltaTime * 3f);

        timer += Time.deltaTime * currentBobbingSpeed * 10;
        float waveslice = Mathf.Sin(timer);

        cam.transform.localPosition = initialCameraPosition + new Vector3(0, waveslice * currentBobbingAmount, 0);
    }

    /*private void Crouch()
    {
        if (PlayerManager.Instance.CurrentState == PlayerManager.MovementState.Crouching || PlayerManager.Instance.CurrentState == PlayerManager.MovementState.CrouchRun)
        {
            cam.transform.localPosition += new Vector3(0, crouchHeight, 0);
        } 
        else
        {
            cam.transform.localPosition += initialCameraPosition;
        }
        
    }*/
    
}
