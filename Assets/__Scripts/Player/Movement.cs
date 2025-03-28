using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private InputReader input;

    [SerializeField] private float speed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float sprintTime;
    private float currentSprintTime;
    
    public float sprintDrainRate = 1f;
    public float sprintRecoveryRate = 1f;
    public float minRecoveryDelay = 0.5f;
    public float maxRecoveryDelay = 2f;

    private bool isSprinting = false;
    [SerializeField] private float sprintRecoveryTime;
    [SerializeField] private float jumpForce;
    private bool isJumping = false;
    private bool isGrounded;
    private bool isCrouched;

    private PlayerManager playerManager;
    
    private Animator animator;
    
    private Vector2 movementInput;
    private Rigidbody rb;
    private CameraController camController;

    public void Dl<T>(T var)
    {
        Debug.Log(var);
    }

    void Start()
    {
        input.MoveEvent += OnMoveInput;
        input.JumpEvent += OnJumpInput;
        input.SprintStart += OnSprintInput;
        input.SprintEnd += OnSprintEnd;
        input.CrouchEvent += OnCrouchInput;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        animator = GetComponent<Animator>();

        currentSprintTime = sprintTime;
        camController = GetComponent<CameraController>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        
        currentSprintTime = sprintTime; 
    }

    private void OnCrouchInput()
    {
        if (!PlayerManager.Instance.isDisabled)
        {
            Crouch();
        }
    }

    void FixedUpdate()
    {
        if (!PlayerManager.Instance.isDisabled)
        {
            Move();
            HandleSprint();
            GroundCheck();
            SetMovementStates();
        }
    }

    private void OnMoveInput(Vector2 input)
    {
        if (!camController.isInConvo && !PlayerManager.Instance.isDisabled)
        {
            movementInput = input;
        }
    }

    private void OnJumpInput()
    {
        if (!PlayerManager.Instance.isDisabled)
        {
            if (isGrounded)
            {
                Jump();
            }
        }
    }

    private void OnSprintInput()
    {
        if (!PlayerManager.Instance.isDisabled)
        {
            if (movementInput != Vector2.zero && currentSprintTime > 0 && !camController.isInConvo)
            {
                isSprinting = true;
            }
        }
    }

    private void OnSprintEnd()
    {
        isSprinting = false;
    }

    private void Move()
    {
        float moveSpeed;
        if (isSprinting && !isCrouched)
        {
            moveSpeed = Mathf.Lerp(rb.velocity.magnitude, sprintSpeed, Time.fixedDeltaTime * 5f);
        }
        else if (isCrouched && !isSprinting)
        {
            moveSpeed = Mathf.Lerp(rb.velocity.magnitude, crouchSpeed, Time.fixedDeltaTime * 5f);
        }
        else if (isCrouched && isSprinting)
        {
            moveSpeed = Mathf.Lerp(rb.velocity.magnitude, (crouchSpeed + sprintSpeed) / 2, Time.fixedDeltaTime * 5f);
        }
        else
        {
            moveSpeed = Mathf.Lerp(rb.velocity.magnitude, speed, Time.fixedDeltaTime * 5f);
        }

        Vector3 movement = GetMovementInfo(moveSpeed);
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        
        animator.SetFloat("X", rb.velocity.magnitude);
    }

    private Vector3 GetMovementInfo(float moveSpeed)
    {
        Vector3 forwardMovement = transform.forward * movementInput.y;
        Vector3 rightMovement = transform.right * movementInput.x;
        return (forwardMovement + rightMovement).normalized * moveSpeed;
    }
    
    private Coroutine recoveryCoroutine;

    private void HandleSprint()
    {
        if (isSprinting && currentSprintTime > 0)
        {
            currentSprintTime -= sprintDrainRate * Time.deltaTime;
            PlayerManager.Instance.SetPlayerSprintTime(currentSprintTime);

            if (currentSprintTime <= 0)
            {
                isSprinting = false;
                StartCoroutine(SprintRecovery(maxRecoveryDelay));
            }

            if (recoveryCoroutine != null)
            {
                StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = null;
                playerManager.isRecovering = false;
            }
        }
        else if (!isSprinting && currentSprintTime < sprintTime)
        {
            if (!playerManager.isRecovering)
            {
                float usedSprint = sprintTime - currentSprintTime;
                float recoveryDelay = Mathf.Lerp(minRecoveryDelay, maxRecoveryDelay, usedSprint / sprintTime);

                recoveryCoroutine = StartCoroutine(SprintRecovery(recoveryDelay));
            }
        }
        else if (currentSprintTime >= sprintTime) 
        {
            playerManager.isRecovering = false;
        }

        if (playerManager.isRecovering)
        {
            isSprinting = false;
        }
    }


    private IEnumerator SprintRecovery(float delay = default)
    {
        if (playerManager.isRecovering) yield break;
        
        playerManager.isRecovering = true;

        yield return new WaitForSeconds(delay);

        while (currentSprintTime < sprintTime)
        {
            currentSprintTime += sprintRecoveryRate * Time.deltaTime;
            PlayerManager.Instance.SetPlayerSprintTime(currentSprintTime);
            yield return null;
        }

        currentSprintTime = sprintTime;
        playerManager.isRecovering = false;
    }

    private void Jump()
    {
        isJumping = true;

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 preservedSpeed = horizontalVelocity.normalized * Mathf.Min(horizontalVelocity.magnitude, speed);

        rb.velocity = new Vector3(preservedSpeed.x, jumpForce, preservedSpeed.z);
    }

    private void Crouch()
    {
        isCrouched = !isCrouched;
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(rb.position, Vector3.down, 0.3f);

        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
        }
    }

    private void SetMovementStates()
    {
        if (isJumping)
        {
            PlayerManager.Instance.SetMovementState(PlayerManager.MovementState.Jumping);
            return;
        }

        if (!isGrounded) return;

        if (movementInput != Vector2.zero)
        {
            PlayerManager.Instance.SetMovementState((isSprinting || sprintTime <= 0) ? PlayerManager.MovementState.Running : PlayerManager.MovementState.Walking);
        }
        else
        {
            PlayerManager.Instance.SetMovementState(PlayerManager.MovementState.Idle);
        }
        
        /*if (movementInput == Vector2.zero)    CROUCH LOGIC REMOVED IN EARLY ACCESS CUZ OF ANIMATIONS
        {
            PlayerManager.Instance.SetMovementState(!isCrouched ? PlayerManager.MovementState.Idle : PlayerManager.MovementState.Crouching);
        }
        else if (isCrouched)
        {
            PlayerManager.Instance.SetMovementState(isSprinting ? PlayerManager.MovementState.CrouchRun : PlayerManager.MovementState.Crouching);
        }
        else
        {
            PlayerManager.Instance.SetMovementState(isSprinting ? PlayerManager.MovementState.Running : PlayerManager.MovementState.Walking);
        }*/
    }
}
