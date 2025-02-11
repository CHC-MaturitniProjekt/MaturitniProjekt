using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointWaitTime = 2f;
    
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
    private int currentWaypointIndex;
    private bool isWaiting;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        state = GetComponent<NPCState>();
        animation = GetComponent<NPCAnimation>();
    }

    private void Start()
    {
        if (waypoints.Length > 0)
        {
            SetNextWaypointDestination();
        }
    }

    private void Update()
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
            agent.isStopped = true;
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
        else
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        if (ShouldWaitAtWaypoint())
        {
            StartCoroutine(WaitAtWaypointRoutine());
        }
    }

    public void HandleFollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > stopDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            agent.isStopped = true;
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
            agent.SetDestination(hit.position);
            StartCoroutine(RunAwayTimerRoutine());
        }
    }

    public void HandleLookAt()
    {
        Vector3 directionToPlayer = playerTransform.position - headBone.position;
        float angleToPlayer = Vector3.SignedAngle(headBone.forward, directionToPlayer, Vector3.up);
        
        agent.isStopped = true;

        if (Mathf.Abs(angleToPlayer) > 80)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else
        {
            headBone.LookAt(playerTransform);
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
        agent.isStopped = true;
    }

    private void SetNextWaypointDestination()
    {
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}