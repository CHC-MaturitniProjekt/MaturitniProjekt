using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private InputReader input;

    [SerializeField] private float speed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float sprintTime;
    private float currentSprintTime;

    public bool isSprinting = false;
    [SerializeField] private float sprintRecoveryTime;
    [SerializeField] private float jumpForce;
    private bool isJumping = false;
    private bool isGrounded;
    public LayerMask groundLayer;

    private Vector2 movementInput;
    private Rigidbody rb;

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

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        currentSprintTime = sprintTime;
    }

    void FixedUpdate()
    {
        Move();
        GroundCheck();
        //Dl("Sprint time: " + currentSprintTime);

        SetMovementStates();
    }

    private void OnMoveInput(Vector2 input)
    {
        movementInput = input;
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
        if (movementInput != Vector2.zero && currentSprintTime > 0)
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
        float moveSpeed = isSprinting ? sprintSpeed : speed;
        Vector3 movement = GetMovementInfo(moveSpeed);

        rb.MovePosition(rb.position + movement);

        if (isSprinting)
        {
            SprintTimer();
        }
        else if (currentSprintTime < sprintTime && !isSprinting)
        {
            SprintRecovery();
        }
    }

    private Vector3 GetMovementInfo(float moveSpeed)
    {
        Vector3 forwardMovement = transform.forward * -movementInput.x;
        Vector3 rightMovement = transform.right * movementInput.y;
        return (forwardMovement + rightMovement).normalized * moveSpeed * Time.fixedDeltaTime;
    }

    private void SprintTimer()
    {
        if (currentSprintTime > 0)
        {
            currentSprintTime -= Time.deltaTime;
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
            currentSprintTime += sprintRecoveryTime * Time.deltaTime;
        }

    }

    private void Jump()
    {
        isJumping = true;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        }
        else if (isGrounded)
        {
            if (movementInput == Vector2.zero)
            {
                PlayerManager.Instance.SetMovementState(PlayerManager.MovementState.Idle);
            }
            else if (isSprinting)
            {
                PlayerManager.Instance.SetMovementState(PlayerManager.MovementState.Running);
            }
            else
            {
                PlayerManager.Instance.SetMovementState(PlayerManager.MovementState.Walking);
            }
        }
    }
}
