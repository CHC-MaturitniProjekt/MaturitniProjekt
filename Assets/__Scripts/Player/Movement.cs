using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("InputReader")]
    [SerializeField] private InputReader input;

    [SerializeField] private float speed;

    private Vector2 movementInput;
    private Rigidbody rb;

    void Start()
    {
        input.MoveEvent += Input_MoveEvent;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Input_MoveEvent(Vector2 obj)
    {
        movementInput = obj;
    }


    void FixedUpdate()
    {
        Vector3 movementOutput = new Vector3(movementInput.y, 0, movementInput.x);

        rb.AddForce(movementOutput * speed, ForceMode.Acceleration);
    }
}