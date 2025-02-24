using System;
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

    private bool isSprinting = false;
    [SerializeField] private float sprintRecoveryTime;
    [SerializeField] private float jumpForce;
    private bool isJumping = false;
    private bool isGrounded;
    public LayerMask groundLayer;
    private bool isCrouched;

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
    }

    private void OnCrouchInput()
    {
        Crouch();
    }

    void FixedUpdate()
    {
        Move();
        GroundCheck();
        SetMovementStates();
    }

    private void OnMoveInput(Vector2 input)
    {
        if (!camController.isInConvo)
        {
            movementInput = input;
        }
    }

    private void OnJumpInput()
    {
        if (isGrounded)
        {
            Jump();
        }
    }

    private void OnSprintInput()
    {
        if (movementInput != Vector2.zero && currentSprintTime > 0 && !camController.isInConvo)
        {
            isSprinting = true;
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

        if (isSprinting)
        {
            SprintTimer();
        }
        else if (currentSprintTime < sprintTime && !isSprinting)
        {
            SprintRecovery();
        }
        
        animator.SetFloat("X", rb.velocity.magnitude);
    }

    private Vector3 GetMovementInfo(float moveSpeed)
    {
        Vector3 forwardMovement = transform.forward * movementInput.y;
        Vector3 rightMovement = transform.right * movementInput.x;
        return (forwardMovement + rightMovement).normalized * moveSpeed;
    }

    private void SprintTimer()
    {
        if (currentSprintTime > 0)
        {
            currentSprintTime -= Time.deltaTime;
            PlayerManager.Instance.SetPlayerSprintTime(currentSprintTime);
        }
        else
        {
            isSprinting = false;
            currentSprintTime = 0;
        }
    }

    private void SprintRecovery()
    {
        if (currentSprintTime < sprintTime)
        {
            PlayerManager.Instance.SetPlayerRecoveryTime(sprintRecoveryTime);
            currentSprintTime += sprintRecoveryTime * Time.deltaTime;
        }

    }

    private void Jump()
    {
        isJumping = true;
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z); 
    }

    private void Crouch()
    {
        isCrouched = !isCrouched;
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(rb.position, Vector3.down, 1.05f, groundLayer);

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

        if (movementInput == Vector2.zero)
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
        }
    }
}
