using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float detectionAngle = 80f;
    [SerializeField] private float stopFollowingRadius = 12f;
    [SerializeField] private float stopDistance = 2f;

    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private bool isWaiting;

    private NPCBrain npcBrain;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        npcBrain = GetComponent<NPCBrain>();

        if (waypoints.Length > 0)
        {
            npcBrain.currentBehaviour = NPCBrain.NPCBehaviour.Wander;
        }
    }

    private void Update()
    {
        if (isWaiting || !agent.isOnNavMesh)
            return;

        switch (npcBrain.currentBehaviour)
        {
            case NPCBrain.NPCBehaviour.Wander:
                HandlePatrol();
                break;

            case NPCBrain.NPCBehaviour.FollowPlayer:
                HandleFollowPlayer();
                break;
            case NPCBrain.NPCBehaviour.RunAway:
                HandleRunAway();
                break;
            case NPCBrain.NPCBehaviour.LookAtPlayer:
                LookAtPlayer();
                break;
        }

        DetectPlayer();
    }

    private void DetectPlayer()
    {
        Vector3 directionToPlayer = playerPosition.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (distanceToPlayer < detectionRadius && angleToPlayer < detectionAngle)
        {
            npcBrain.currentBehaviour = NPCBrain.NPCBehaviour.LookAtPlayer;
        }
        else if (distanceToPlayer > stopFollowingRadius)
        {
            agent.isStopped = false;
            npcBrain.currentBehaviour = NPCBrain.NPCBehaviour.Wander;
        }

        Debug.DrawRay(transform.position, transform.forward * detectionRadius, Color.green);
        Debug.DrawRay(transform.position, directionToPlayer, Color.red);
    }

    private void HandlePatrol()
    {
        if (waypoints.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private void HandleFollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition.position);

        if (distanceToPlayer > stopDistance)
        {
            agent.SetDestination(playerPosition.position);
        }
        else
        {
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
    }

    private void HandleRunAway()
    {
        Vector3 directionToPlayer = playerPosition.position - transform.position;
        Vector3 runAwayDirection = -directionToPlayer;
        agent.SetDestination(transform.position + runAwayDirection);
    }
    
    private void LookAtPlayer()
    {
        Vector3 directionToPlayer = playerPosition.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        agent.isStopped = true;
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        isWaiting = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!agent.isOnNavMesh) return;

        agent.velocity = Vector3.zero;
        agent.ResetPath();
        if (npcBrain.currentBehaviour == NPCBrain.NPCBehaviour.Wander && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
        else if (npcBrain.currentBehaviour == NPCBrain.NPCBehaviour.FollowPlayer)
        {
            agent.SetDestination(playerPosition.position);
        }
    }
}