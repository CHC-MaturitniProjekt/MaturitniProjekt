using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private Movement _movement;
    private CameraController _camera;

    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Crouching,
        Jumping
    }

    public MovementState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _movement = FindObjectOfType<Movement>();
        _camera = FindObjectOfType<CameraController>();
    }

    void Update()
    {
        FovChange();
    }

    private void FovChange()
    {
        _camera.FovChange(_movement.isSprinting);
    }

    public void SetMovementState(MovementState newState)
    {
        CurrentState = newState;
    }
}
