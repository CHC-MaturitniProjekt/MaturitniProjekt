using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class NPCBrain : MonoBehaviour
{
    private NPCState state;
    private NPCMovement movement;
    [SerializeField] private NPCBehavior currentBehavior;
    [SerializeField] private NPCScriptableObject npcInfo;
    public Transform npcHouse;

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
            case NPCBehavior.Sit:
                movement.HandleSit();
                break;
            case NPCBehavior.GoHome:
                movement.HandleGoHome();
                break;
        }
    }
    
    private void NPCCycles()
    {
        float currentHour = timeManager.GetWorldTime()/60f;
        float activeValue = npcInfo.NPCActiveTimeCurve.Evaluate(currentHour);
        
        if (currentHour >= 22f || currentHour < 6f)
        {
            if (currentBehavior != NPCBehavior.GoHome)
            {
                SetBehavior(NPCBehavior.GoHome);
            }
            return;
        }
        
        if (currentHour >= 6f && currentHour < 22f && currentBehavior == NPCBehavior.GoHome)
        {
            Vector3 safeSpawn = npcHouse.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(safeSpawn, out hit, 2f, NavMesh.AllAreas))
            {
                gameObject.SetActive(true);
                GetComponent<NavMeshAgent>().Warp(hit.position);
                SetBehavior(NPCBehavior.Idle);
                return;
            } 
        }
        
        if (Random.value < npcInfo.NPCRandomness * activeValue * Time.deltaTime)
        {
            NPCBehavior randomChoice = GetRandomBehavior();
            SetBehavior(randomChoice);
        }
    }
    
    private NPCBehavior GetRandomBehavior()
    {
        var possible = new List<NPCBehavior> { NPCBehavior.Wander, NPCBehavior.Sit, NPCBehavior.Idle };
        return possible[Random.Range(0, possible.Count)];
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
         playerCam.isInConvo = true;
    }
    
    public void EndConversation()
    {
        currentBehavior = AfterDialogueBehavior;
        
        playerCam.isInConvo = false;

    }

    public void SetBehavior(NPCBehavior newBehavior, float duration = 0)
    {
        if (currentBehavior == NPCBehavior.Sit && newBehavior != NPCBehavior.Sit)
        {
            movement.InterruptSit();
        }

        if (duration > 0 && currentBehavior != NPCBehavior.GoTo)
        {
            StartCoroutine(OverrideBehaviorRoutine(newBehavior, duration));
            return;
        }

        currentBehavior = newBehavior;
        state.IsRunningAway = newBehavior == NPCBehavior.RunAway;
    }
    
    public NPCBehavior GetCurrentBehavior()
    {
        return currentBehavior;
    }
    
    private IEnumerator OverrideBehaviorRoutine(NPCBehavior tempBehavior, float duration)
    {
        if (currentBehavior == NPCBehavior.Sit && tempBehavior != NPCBehavior.Sit)
        {
            movement.InterruptSit();
        }

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
        GoTo,
        Sit,
        GoHome
    }
}