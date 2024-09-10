using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private float playerSprintTime;

    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Crouching,
        Jumping,
        CrouchRun
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

    public void SetMovementState(MovementState newState)
    {
        CurrentState = newState;
    }

    public void SetPlayerSprintTime(float sprintTime)
    {
        playerSprintTime = sprintTime;
    }

    public float GetPlayerSprintTime()
    {
        return playerSprintTime;
    }
}
