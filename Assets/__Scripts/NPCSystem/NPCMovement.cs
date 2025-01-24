using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float stopFollowingRadius = 12f;
    [SerializeField] private float stopDistance = 2f;

    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private bool isWaiting;
    private NPCBehaviour currentBehaviour;

    public enum NPCBehaviour
    {
        Patrol,
        FollowPlayer
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (waypoints.Length > 0)
        {
            SetNPCBehaviour(NPCBehaviour.Patrol);
        }
        else
        {
            Debug.LogWarning("No waypoints assigned to NPCMovement.");
        }
    }

    private void Update()
    {
        if (isWaiting || !agent.isOnNavMesh)
            return;

        switch (currentBehaviour)
        {
            case NPCBehaviour.Patrol:
                HandlePatrol();
                break;

            case NPCBehaviour.FollowPlayer:
                HandleFollowPlayer();
                break;
        }

        DetectPlayer();
    }

    private void SetNPCBehaviour(NPCBehaviour behaviour)
    {
        currentBehaviour = behaviour;

        if (behaviour == NPCBehaviour.Patrol && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
            Debug.Log("Patrolling" + waypoints[currentWaypointIndex].position);
        }
    }

    private void DetectPlayer()
    {
        Vector3 directionToPlayer = playerPosition.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        Debug.DrawRay(transform.position, transform.forward * detectionRadius, Color.green);
        Debug.DrawRay(transform.position, directionToPlayer, Color.red);

        if (distanceToPlayer < detectionRadius && angleToPlayer < 80f && currentBehaviour != NPCBehaviour.FollowPlayer)
        {
            SetNPCBehaviour(NPCBehaviour.FollowPlayer);
            Debug.Log("Following player");
        }
        else if (distanceToPlayer > stopFollowingRadius && currentBehaviour == NPCBehaviour.FollowPlayer)
        {
            SetNPCBehaviour(NPCBehaviour.Patrol);
            Debug.Log("Patrolling");
        }
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
        Debug.Log("Distance to player: " + distanceToPlayer);
        
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
        if (currentBehaviour == NPCBehaviour.Patrol && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
        else if (currentBehaviour == NPCBehaviour.FollowPlayer)
        {
            agent.SetDestination(playerPosition.position);
        }
    }
}