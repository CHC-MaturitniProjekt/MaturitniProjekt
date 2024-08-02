using System;
using System.Collections;
using System.Collections.Generic;
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
        
    }

    private void Input_MoveEvent(Vector2 obj)
    {
        movementInput = obj;
    }

    void Update()
    {
        Vector3 movementOutput = new Vector3(movementInput.x, 0, movementInput.y);
        rb.AddForce(movementOutput * Time.deltaTime * speed);
    }
}
