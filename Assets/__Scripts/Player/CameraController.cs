using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform head;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private InputReader input;
    [SerializeField] private float fov;
    [SerializeField] private float runFovIncrease;
    [SerializeField] private float jumpFovIncrease;
    public float mouseSensitivity = 100f;

    [Header("Head Bob Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkAmount;
    [SerializeField] private float runSpeed;
    [SerializeField] private float runAmount;
    [SerializeField] private float idleSpeed;
    [SerializeField] private float idleAmount;

    private float xRotation = 0f;
    private Vector2 mouseMove = Vector2.zero;
    private float timer = 0.0f;
    private Vector3 initialCameraPosition;
    private Vector3 originalHeadPosition;
    private bool isHeadOffsetApplied = false;
    private bool shouldAlignForward = false;
    private float alignSpeed = 5f;
    private Quaternion targetCamRotation;
    private float targetXRotation;

    public bool isInConvo = false;

    private void Start()
    {   
        Cursor.lockState = CursorLockMode.Locked;
        input.LookEvent += Input_LookEvent;
        cam.Lens.FieldOfView = fov;

        if (head != null)
        {
            originalHeadPosition = head.localPosition;
        }
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
        }

        if (shouldAlignForward)
        {
            xRotation = Mathf.Lerp(xRotation, targetXRotation, Time.deltaTime * alignSpeed);
            cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, targetCamRotation, Time.deltaTime * alignSpeed);

            // Když jsme blízko cíle, ukonči zarovnání
            if (Quaternion.Angle(cam.transform.localRotation, targetCamRotation) < 0.1f)
            {
                shouldAlignForward = false;
                cam.transform.localRotation = targetCamRotation;
                xRotation = targetXRotation;
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (!PlayerManager.Instance.isDisabled)
        {
            Look();
        }
    }
    
    public void ResetCameraPosition()
    {
        if (head != null)
        {
            head.localPosition = originalHeadPosition;
            cam.transform.localPosition = initialCameraPosition;
        }
    }

    public void AdjustCameraHeight(SitInteract.SitType sitType)
    {
        float heightOffset = 0f;

        switch (sitType)
        {
            case SitInteract.SitType.Bench:
                heightOffset = -0.834f;
                break;
            case SitInteract.SitType.Sofa:
                heightOffset = -1f;
                break;
            case SitInteract.SitType.Bed:
                heightOffset = -0.4f;
                break;
            case SitInteract.SitType.Ground:
                heightOffset = -0.86f;
                break;
        }

        if (head != null)
        {
            head.localPosition = originalHeadPosition + new Vector3(0, heightOffset, 0);
        }
    }

    public void AlignCameraForward()
    {
        shouldAlignForward = true;

        // Cíl = rovný pohled dopředu podle hráče
        targetXRotation = 0f;
        targetCamRotation = Quaternion.Euler(targetXRotation, 90f, 0f); // 90 je tvoje základní otočení v Look()
    }

    public void disableAligForward()
    {
        shouldAlignForward = false;
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
}
