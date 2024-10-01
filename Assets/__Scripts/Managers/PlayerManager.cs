using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private float playerSprintTime;
    private PickUp pickUpScript;

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

    public void PickUpItem(GameObject item)
    {
        
        pickUpScript.CarryItem(item);
    }

    public void DropItem(GameObject item)
    {
        pickUpScript.DropItem(item);
    }
}


//ziskat item z raycastu
//poslat do PickUp prefab
//smazat objekt ze sceny
//spawnout objekt na ItemPosition zmenseny
//pridat input na dropnuti
//pri dropnuti se dropne item z pozice ruky s nejakym forcem dopredu

//vymyselt system na rozdeleni objektu na pickable
//dost mozna bude vic itemu ktere muze mit
//ui pro itemy?