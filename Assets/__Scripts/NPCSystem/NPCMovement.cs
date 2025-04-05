using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class NPCMovement : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointWaitTime = 2f;
    private int lastWaypointIndex = -1;

    
    [Header("Player Interaction")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float detectionAngle = 80f;
    [SerializeField] private float stopDistance = 2f;

    [Header("References")]
    [SerializeField] private Transform headBone;
    
    private NavMeshAgent agent;
    private NPCState state;
    private NPCAnimation animation;
    private NPCBrain npcBrain;
    private int currentWaypointIndex;
    private bool isWaiting;

    private List<string> canSitOn = new List<string>();
    private GameObject currentSittableObject;

    private GameObject currentSeat;
    private Quaternion originalRotation;
    private Coroutine sitRoutine;
    
    private Vector3 lastPosition;
    private float stuckCheckInterval = 1f;
    private float stuckThreshold = 0.1f;
    private float pathTimeout = 10f;
    private float lastPathCalculationTime;

    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        state = GetComponent<NPCState>();
        animation = GetComponent<NPCAnimation>();
        npcBrain = GetComponent<NPCBrain>();
    }

    private void Start()
    {
        if (waypoints.Length > 0)
        {
            SetNextWaypointDestination();
        }
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        AssertParams();
        lastPosition = transform.position;
        lastPathCalculationTime = Time.time;
        StartCoroutine(StuckCheckRoutine());
    }

    private void AssertParams()
    {
        var npcSO = npcBrain.GetNPCSO();
        switch (npcSO.NPCBehaviourType)
        {
            case NPCScriptableObject.NPCBehaviourTypes.Homeless:
                canSitOn.Add("GroundSit");
                break;
            case NPCScriptableObject.NPCBehaviourTypes.Basic:
            case NPCScriptableObject.NPCBehaviourTypes.Fancy:
                canSitOn.Add("Bench");
                canSitOn.Add("Sofa");
                break;
            case NPCScriptableObject.NPCBehaviourTypes.Stationary:
                break;
        }
    }
    
    private IEnumerator StuckCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(stuckCheckInterval);
        
            if (state.IsOverriden || isWaiting || state.IsRunningAway) 
                continue;
            
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
        
            if (distanceMoved < stuckThreshold && agent.hasPath && !agent.isStopped)
            {
                if (Time.time - lastPathCalculationTime > pathTimeout)
                {
                    Debug.Log($"{name} is stuck, finding new destination");
                    HandleWander();
                    lastPathCalculationTime = Time.time;
                }
            }
        }
    }


    private void FixedUpdate()
    {
        if (!agent.isOnNavMesh) return;
        
        UpdateMovementState();
    }

    private void LateUpdate()
    {
        if (!playerTransform || state.IsRunningAway) return;
        
        DetectPlayer();
    }

    private void UpdateMovementState()
    {
        if (isWaiting || state.IsOverriden)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Idle);
            animation.SetMovementSpeed(0);
            return;
        }

        animation.SetMovementSpeed(agent.velocity.magnitude > 0.1f ? 1 : 0);
    }

    public void HandleWander()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints set for wandering!");
            return;
        }
    
        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            return;
        }
    
        if (Random.value < 0.2f)
        {
            HandleSit();
            StartCoroutine(SitForRandomTime());
            return;
        }
    
        Vector3 newDestination;
        int attempts = 0;
        const int maxAttempts = 10;
        const float minWanderDistance = 5f;
    
        do
        {
            Transform randomWaypoint = waypoints[Random.Range(0, waypoints.Length)];
            if (NavMesh.SamplePosition(randomWaypoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                newDestination = hit.position;
            }
            else
            {
                newDestination = randomWaypoint.position;
            }
        
            attempts++;
        } 
        while (Vector3.Distance(transform.position, newDestination) < minWanderDistance && 
               attempts < maxAttempts);

        lastPathCalculationTime = Time.time;
    
        HandleGoTo(newDestination);
    
        if (ShouldWaitAtWaypoint())
        {
            StartCoroutine(WaitAtWaypointRoutine());
        }
    }
    
    private IEnumerator SitForRandomTime()
    {
        yield return new WaitForSeconds(Random.Range(3f, 7f));
        InterruptSit();
    }
    
    public void HandleGoTo(Vector3 pos, bool behaviourGoTo = false)
    {
        lastPathCalculationTime = Time.time;
    
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(pos, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(pos);
        }
        else
        {
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning($"Failed to find valid path to {pos}");
                npcBrain.SetBehavior(NPCBrain.NPCBehavior.Idle);
                return;
            }
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && behaviourGoTo)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Idle);
        }
    }

    public void HandleIdle()
    {
        agent.isStopped = true;
        StartCoroutine(IdleTimeoutRoutine());
    }
    
    private IEnumerator IdleTimeoutRoutine()
    {
        yield return new WaitForSeconds(7f);
        if (npcBrain.GetCurrentBehavior() == NPCBrain.NPCBehavior.Idle)
        {
            Debug.Log($"{name} has been idle for too long, switching to wander.");
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
        }
    }

    public void HandleSit()
    {
        if (sitRoutine != null) return;

        GameObject closestObject = null;
        float closestDistance = Mathf.Infinity;

        foreach (string tag in canSitOn)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects)
            {
                if (NPCManager.Instance.IsSeatReserved(obj.transform)) continue;
                
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }
        }

        if (closestObject == null)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
            return;
        }


        bool reserved = false;
        if (!NPCManager.Instance.IsSeatReserved(closestObject.transform))
        {
            reserved = NPCManager.Instance.ReserveSeat(closestObject.transform, gameObject);
        }
        if (!reserved)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
            return;
        }

        currentSeat = closestObject;
        HandleGoTo(closestObject.transform.position);
        sitRoutine = StartCoroutine(ArrivedAtSpot(closestObject.transform));
    }

    private IEnumerator ArrivedAtSpot(Transform seatTransform)
    {
        while (Vector3.Distance(transform.position, seatTransform.position) > agent.stoppingDistance)
        {
            yield return null;
        }

        agent.isStopped = true;
        animation.SetMovementSpeed(0);

        originalRotation = transform.rotation;
        transform.position = seatTransform.position;

        animation.Sit("isGroundSitting");

        while (npcBrain.GetCurrentBehavior() == NPCBrain.NPCBehavior.Sit && !state.IsOverriden)
        {
            float distanceFromSeat = Vector3.Distance(transform.position, seatTransform.position);
            if (distanceFromSeat > 0.5f) break;

            if (playerTransform && Vector3.Distance(transform.position, playerTransform.position) < detectionRadius)
            {
                Vector3 dir = playerTransform.position - headBone.position;
                headBone.LookAt(playerTransform.position);
                headBone.Rotate(30, 0, 0);
            }

            yield return null;
        }

        animation.ResetSit("isGroundSitting");
        transform.rotation = originalRotation;
        agent.isStopped = false;

        if (currentSeat != null)
        {
            NPCManager.Instance.ReleaseSeat(currentSeat.transform);
            currentSeat = null;
        }

        sitRoutine = null;

        npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
    }

    public void InterruptSit()
    {
        if (sitRoutine != null)
        {
            StopCoroutine(sitRoutine);
            sitRoutine = null;

            animation.ResetSit("isGroundSitting");
            transform.rotation = originalRotation;
            agent.isStopped = false;
            
            if (currentSeat != null)
            {
                NPCManager.Instance.ReleaseSeat(currentSeat.transform);
                currentSeat = null;
            }
        }
    }
    
    public void HandleGoHome()
    {
        if (npcBrain.npcHouse == null) return;

        HandleGoTo(npcBrain.npcHouse.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            gameObject.SetActive(false);
        } 
    }

    public void HandleFollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > stopDistance)
        {
            agent.isStopped = false;
            
            HandleGoTo(playerTransform.position);
        }
        else
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Idle);
            agent.ResetPath();
        }
    }

    public void HandleRunAway()
    {
        if (!playerTransform || !agent) return;
        
        agent.isStopped = false;
        
        Vector3 runDirection = (transform.position - playerTransform.position).normalized;
        Vector3 targetPosition = transform.position + runDirection * 20f;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 20f, NavMesh.AllAreas))
        {
            HandleGoTo(hit.position);
            StartCoroutine(RunAwayTimerRoutine());
        }
    }

    public void HandleLookAt()
    {
        if (!playerTransform) return;

        Vector3 directionToPlayer = playerTransform.position - headBone.position;
        float angleToPlayer = Vector3.SignedAngle(headBone.forward, directionToPlayer, Vector3.up);

        npcBrain.SetBehavior(NPCBrain.NPCBehavior.Idle);

        if (!state.IsSitting && Mathf.Abs(angleToPlayer) > 80)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else
        {
            headBone.LookAt(playerTransform.position); 
            headBone.Rotate(30, 0, 0);
        }
    }

    private void DetectPlayer()
    {
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        float distance = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        
        state.IsLookingAtPlayer = distance < detectionRadius && angle < detectionAngle;
        
        if (distance > detectionRadius)
        {
            agent.isStopped = false;
            return;
        } 
        
        if (distance < detectionRadius && angle < detectionAngle && !state.IsRunningAway)
        {
            HandleLookAt();
        }
        
        state.IsLookingAtPlayer = false;

    }

    private bool ShouldWaitAtWaypoint()
    {
        return !agent.pathPending && 
               agent.remainingDistance <= agent.stoppingDistance &&
               !isWaiting;
    }

    private IEnumerator WaitAtWaypointRoutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waypointWaitTime);
        
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        SetNextWaypointDestination();
        isWaiting = false;
    }

    private IEnumerator RunAwayTimerRoutine()
    {
        yield return new WaitForSeconds(2f);
        state.IsRunningAway = false;
        npcBrain.SetBehavior(NPCBrain.NPCBehavior.Idle);
        
    }

    private void SetNextWaypointDestination() 
    {
        HandleGoTo(waypoints[currentWaypointIndex].position);
    }
}