using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private float playerSprintTime;
    private float playerSprintRecoveryT;
    private PickUp pickUpScript;
    public bool isDisabled = false;
    public bool isRecovering;
    
    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        //Crouching,
        Jumping,
        //CrouchRun
    }

    public MovementState CurrentState { get; private set; }

    private void Awake()
    {
        pickUpScript = FindObjectOfType<PickUp>();

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

    public void EnablePlayerWithDelay()
    {
        StartCoroutine(EnablingPlayerAfterDelay());
    }

    private IEnumerator EnablingPlayerAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        isDisabled = false;
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
    
    public float GetPlayerSprintRecoveryTime()
    {
        return playerSprintRecoveryT;
    }

    public void PickUpItem(GameObject item)
    {
        
        pickUpScript.CarryItem(item);
    }
    
}