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
    [SerializeField] private float jumpForce;
    private bool isGrounded;
    public LayerMask groundLayer;
    [SerializeField] private float rayLength = 4f;

    private Vector2 movementInput;
    private Rigidbody rb;

    //-------------------
    //       TODO
    // -sprint recovery
    //
    //-------------------
    public void Dl<T>(T var)
    {
        Debug.Log(var);
    }

    void Start()
    {
        input.MoveEvent += OnMoveInput;
        input.JumpEvent += OnJumpInput;
        input.SprintEvent += OnSprintInput;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        currentSprintTime = sprintTime;
    }

    void FixedUpdate()
    {
        Move();
        GroundCheck();
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
        isSprinting = !isSprinting;
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
            Dl("Sprint time: " + currentSprintTime);
        }
        else
        {
            isSprinting = false;
            currentSprintTime = sprintTime;
        }
    }

    private void SprintRecovery()
    {
        currentSprintTime += Time.deltaTime;
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(rb.position, Vector3.down, rayLength, groundLayer);
        Debug.DrawRay(rb.position, Vector3.down * rayLength, Color.yellow);
    }
}
