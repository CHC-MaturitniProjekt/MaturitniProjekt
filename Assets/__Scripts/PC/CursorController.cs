using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    
    [SerializeField] private InputReader input;
    private Vector2 mouseMove = Vector2.zero;
    [SerializeField] private RectTransform cursorTransform;
    CameraController camController;
    void Start()
    {
        camController = FindObjectOfType<CameraController>();
        input.LookEvent += Input_LookEvent;
    }
    
    private void Input_LookEvent(Vector2 obj)
    {
        mouseMove = obj;
    }
    
    
    void Update()
    {
        if (!camController.isUsingPC) return;
        Vector3 mousePosition = Input.mousePosition;
        cursorTransform.position = mousePosition;
    }
}
