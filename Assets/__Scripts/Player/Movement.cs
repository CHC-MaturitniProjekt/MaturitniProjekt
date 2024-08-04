using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("InputReader")]
    [SerializeField] private InputReader input;

    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    private bool isGrounded;
    public LayerMask groundLayer;
    private float rayLength = 0.1f;

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

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 forwardMovement = transform.forward * -movementInput.x;
        Vector3 rightMovement = transform.right * movementInput.y;
        Vector3 movement = (forwardMovement + rightMovement).normalized * speed * Time.fixedDeltaTime;

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
