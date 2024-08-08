using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private InputReader input;

    [SerializeField] private float speed;
    [SerializeField] private float sprintSpeed;
    private bool isSprinting = false;
    [SerializeField] private float jumpForce;
    private bool isGrounded;
    public LayerMask groundLayer;
    [SerializeField] private float rayLength = 4f;

    private Vector2 movementInput;
    private Rigidbody rb;

    public void Dl<T>(T var)
    {
        Debug.Log(var);
    }

    void Start()
    {
        input.MoveEvent += Input_MoveEvent;
        input.JumpEvent += Input_JumpEvent;
        input.SprintEvent += Input_SprintEvent;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Input_JumpEvent()
    {
        Jump();
    }

    private void Input_MoveEvent(Vector2 obj)
    {
        movementInput = obj;
    }
    private void Input_SprintEvent()
    {
        isSprinting = !isSprinting;
    }

    void FixedUpdate()
    {
        Move();

        Debug.DrawRay(rb.position, Vector3.down, Color.yellow, rayLength);
        Dl("Sprint: " + isSprinting);
    }

    private void Move()
    {
        Vector3 forwardMovement = transform.forward * -movementInput.x;
        Vector3 rightMovement = transform.right * movementInput.y;
        Vector3 movement;

        if(!isSprinting)
        {
            movement = (forwardMovement + rightMovement).normalized * speed * Time.fixedDeltaTime;
        }
        else
        {
            movement = (forwardMovement + rightMovement).normalized * sprintSpeed * Time.fixedDeltaTime;
        }

        rb.MovePosition(rb.position + movement);
    }

    private void Jump()
    {
        isGrounded = Physics.Raycast(rb.position, Vector3.down, rayLength, groundLayer);
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
