using System;
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
    
    private bool isLookingAtPlayer;
    
    [SerializeField] private Transform headBone;
    [SerializeField] private Animator animator;
    
    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private bool isWaiting;
    public bool isRunningAway;

    private NPCBrain npcBrain;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        npcBrain = GetComponent<NPCBrain>();

        if (waypoints.Length > 0)
        {
            npcBrain.SetBehaviour(NPCBrain.NPCBehaviour.Wander);
        }
    }

    private void Update()
    {
        if (isWaiting || !agent.isOnNavMesh)
        {
            agent.isStopped = true;
            return;
        }
            

        DetectPlayer();
        //ScanSurroundings();
    }
    
    private void LateUpdate()
    {
        if (isLookingAtPlayer)
        {
            HandleLookAt();
        }
    }

    public void HandleWander()
    {
        if (agent.isStopped) agent.isStopped = false;
        
        if (waypoints.Length == 0)
        {
            npcBrain.SetBehaviour(NPCBrain.NPCBehaviour.Idle);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }
        else if (!isWaiting)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    public void HandleFollowPlayer()
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

    public void HandleRunAway()                             //nefaka
    {
        if (isRunningAway) return;
        
        isRunningAway = true;
        
        agent.isStopped = false;
        
        Vector3 directionToPlayer = playerPosition.position - transform.position;
        Vector3 runAwayDirection = -directionToPlayer.normalized;
        agent.SetDestination(transform.position + runAwayDirection * 20f);
        Debug.Log("Running away from player");
    }
    
    public void HandleLookAt()
    {
        Vector3 directionToPlayer = playerPosition.position - headBone.position;
        float angleToPlayer = Vector3.SignedAngle(-headBone.forward, directionToPlayer, Vector3.up);
        
        if (npcBrain.currentBehaviour == NPCBrain.NPCBehaviour.LookAtPlayer && angleToPlayer < -80 || angleToPlayer > 80)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        else if (angleToPlayer > -80 && angleToPlayer < 80)
        {
            headBone.LookAt(playerPosition);
            headBone.Rotate(-20, 180, 0);
        }
        else
        {
            headBone.rotation = Quaternion.Slerp(headBone.rotation, Quaternion.LookRotation(-transform.forward), Time.deltaTime * 5f);
        }

        agent.isStopped = true;

    }

    private void DetectPlayer()
    {
        Vector3 directionToPlayer = playerPosition.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (distanceToPlayer < detectionRadius && angleToPlayer < detectionAngle)
        {
            isLookingAtPlayer = true;
        }
        else if (distanceToPlayer > stopFollowingRadius && npcBrain.currentBehaviour != NPCBrain.NPCBehaviour.RunAway)
        {
            agent.isStopped = false;
            isLookingAtPlayer = false;
        }

        Debug.DrawRay(transform.position, transform.forward * detectionRadius, Color.green);
        Debug.DrawRay(transform.position, directionToPlayer, Color.red);
    }

    /*private void ScanSurroundings()
    {
        bool originalAvoidance = agent.obstacleAvoidanceType != ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Item"))
            {
                npcBrain.SetBehaviour(NPCBrain.NPCBehaviour.RunAway);
            }
        }

        agent.obstacleAvoidanceType = originalAvoidance ? ObstacleAvoidanceType.HighQualityObstacleAvoidance : ObstacleAvoidanceType.NoObstacleAvoidance;
    }*/

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

        if (agent.velocity.magnitude < 0.1f)
        {
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
}