using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [SerializeField] private NPCState state;
    [SerializeField] private NPCMovement movement;
    [SerializeField] private NPCBehavior currentBehavior;
    [SerializeField] private NPCScriptableObject npcInfo;

    private List<Transform> waypoints;

    private CameraController playerCam;
    private TimeManager timeManager;
    private WaypointManager waypointManager;
    private Transform currentWaypoint;
    public NPCBehavior AfterDialogueBehavior {get; set;}

    private NPCBehavior tempBehaviour;
    private void Awake()
    {
        state = GetComponent<NPCState>();
        movement = GetComponent<NPCMovement>();
        playerCam = FindFirstObjectByType<CameraController>();
        timeManager = FindAnyObjectByType<TimeManager>();
        waypointManager = FindAnyObjectByType<WaypointManager>();
    }

    private void Start()
    {
        waypoints = npcInfo.NPCWayPointNames
            .Select(name => WaypointManager.Instance.GetWaypoint(name))
            .Where(transform => transform != null)
            .ToList();
    }

    private void Update()
    {
        if (state.IsOverriden) return;
        UpdateBehavior();
        NPCCycles();
    }

    public NPCScriptableObject GetNPCSO()
    {
        return npcInfo;
    }

    private void UpdateBehavior()
    {
        switch (currentBehavior)
        {
            case NPCBehavior.Wander:
                HandleWanderBehavior();
                break;
            case NPCBehavior.FollowPlayer:
                movement.HandleFollowPlayer();
                break;
            case NPCBehavior.RunAway:
                HandleRunAwayBehavior();
                break;
            case NPCBehavior.LookAtPlayer:
                movement.HandleLookAt();
                break;
            case NPCBehavior.GoTo:
                movement.HandleGoTo(currentWaypoint.position);
                break;
            case NPCBehavior.Idle:
                movement.HandleIdle();
                break;
        }
    }
    
    private void NPCCycles() 
    {
        switch (timeManager.GetWorldTime())
        {
            case 480:
                break;
            case 840: 
                SetBehavior(NPCBehavior.GoTo);
                currentWaypoint = waypointManager.GetWaypoint("DumWaypoint");
                break;
            case 1140:
                SetBehavior(NPCBehavior.Wander);
                break;
            default:
                break;
        }
    }
    

    private void HandleWanderBehavior()
    {
        if (!state.IsLookingAtPlayer && !state.IsRunningAway) 
        {
            movement.HandleWander();
        }
    }

    private void HandleRunAwayBehavior()
    {
        if (!state.IsRunningAway) state.IsRunningAway = true;
        
        movement.HandleRunAway();
    }
    
    public void StartConversation()
    {
         tempBehaviour = currentBehavior;
         Debug.Log("Start Conversation: " + tempBehaviour);
         
         playerCam.isInConvo = true;
    }
    
    public void EndConversation()
    {
        currentBehavior = AfterDialogueBehavior;
        
        playerCam.isInConvo = false;

    }

    public void SetBehavior(NPCBehavior newBehavior, float duration = 0)
    {
        if (duration > 0 && currentBehavior != NPCBehavior.GoTo)
        {
            StartCoroutine(OverrideBehaviorRoutine(newBehavior, duration));
            return;
        }
        
        currentBehavior = newBehavior;
        if (newBehavior == NPCBehavior.RunAway) 
        {
            state.IsRunningAway = true;
        }
        else 
        {
            state.IsRunningAway = false;
        }
    }

    private IEnumerator OverrideBehaviorRoutine(NPCBehavior tempBehavior, float duration)
    {
        var originalBehavior = currentBehavior;
        state.IsOverriden = true;
        currentBehavior = tempBehavior;
        
        yield return new WaitForSeconds(duration);
        
        currentBehavior = originalBehavior;
        state.IsOverriden = false;
    }

    public enum NPCBehavior
    {
        Wander,
        FollowPlayer,
        RunAway,
        Idle,
        LookAtPlayer,
        GoTo
    }
}