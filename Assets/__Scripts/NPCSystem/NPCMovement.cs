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
    private float waypointArrivalTime = -1f;
    
    [Header("Player Interaction")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float detectionAngle = 80f;
    [SerializeField] private float stopDistance = 2f;
    private Quaternion defaultHeadLocalRotation;

    [Header("References")]
    [SerializeField] private Transform headBone;

    [Header("Store interactions")] 
    [Header("Market")]
    [SerializeField] private Transform marketEntrance;
    [SerializeField] private Transform marketExit;
    [SerializeField] private Transform markerCounter;
    public bool isAtMarket = false;
    [Header("Medical")]
    [SerializeField] private Transform medicalEntrance;
    [SerializeField] private Transform medicalExit;
    [SerializeField] private Transform medicalCounter;
    public bool isAtMedical = false;
    [Header("BodyMods")]
    [SerializeField] private Transform bodymodEntrance;
    [SerializeField] private Transform bodymodExit;
    [SerializeField] private Transform bodymodCounter;
    public bool isAtBodymod = false;
    private StoreType selectedStore;

    [Header("Story npcs")] [SerializeField]
    private List<Transform> assignedSeats;
    
    public enum StoreType { None, Market, Medical, BodyMod }
    public StoreType currentStore = StoreType.None;
    
    private NavMeshAgent agent;
    private NPCState state;
    private NPCAnimation animation;
    private NPCBrain npcBrain;
    private Rigidbody npcRb;
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
        npcRb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (waypoints.Length > 0)
        {
            SetNextWaypointDestination();
        }

        AssertParams();
        lastPosition = transform.position;
        lastPathCalculationTime = Time.time;
        defaultHeadLocalRotation = headBone.localRotation;
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
            waypointArrivalTime = -1f;
            return;
        }
        
        if (waypointArrivalTime < 0f)
        {
            waypointArrivalTime = Time.time;
            return;
        }
        
        if (Time.time - waypointArrivalTime < waypointWaitTime)
        {
            return;
        }
    
        waypointArrivalTime = -1f;

        if (agent.isStopped) agent.isStopped = false;
    
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
        const float randomRadius = 10f;

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

        if (attempts >= maxAttempts)
        {
            do
            {
                Vector3 randomDirection = Random.insideUnitSphere * randomRadius;
                randomDirection += transform.position;
                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    newDestination = hit.position;
                }
                else
                {
                    newDestination = randomDirection;
                }

                attempts++;
            }
            while (Vector3.Distance(transform.position, newDestination) < minWanderDistance &&
                   attempts < maxAttempts);
        }

        lastPathCalculationTime = Time.time;

        HandleGoTo(newDestination);
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
        NPCBrain.NPCBehavior tempBehavior = npcBrain.GetCurrentBehavior();
        yield return new WaitForSeconds(7f);
        agent.isStopped = false;
        if (npcBrain.GetCurrentBehavior() == NPCBrain.NPCBehavior.Idle)
        {
            npcBrain.SetBehavior(tempBehavior);
        }
    }

    public void HandleSit()
    {
        if (sitRoutine != null) return;

        GameObject targetSeat = null;
        if (assignedSeats != null && assignedSeats.Count > 0)
        {
            float closestDistance = Mathf.Infinity;
            foreach (var seat in assignedSeats)
            {
                if (seat == null || NPCManager.Instance.IsSeatReserved(seat)) continue;

                float distance = Vector3.Distance(transform.position, seat.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetSeat = seat.gameObject;
                }
            }

            if (targetSeat == null)
            {
                npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
                return;
            }

            NPCManager.Instance.ReserveSeat(targetSeat.transform, gameObject);
        }
        else
        {
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
                        targetSeat = obj;
                    }
                }
            }

            if (targetSeat == null)
            {
                npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
                return;
            }

            bool reserved = NPCManager.Instance.ReserveSeat(targetSeat.transform, gameObject);
            if (!reserved && !npcBrain.GetNPCSO().storyImportant)
            {
                npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
                return;
            }
        }

        currentSeat = targetSeat;
        HandleGoTo(targetSeat.transform.position);
        sitRoutine = StartCoroutine(ArrivedAtSpot(targetSeat));
    }
    private IEnumerator ArrivedAtSpot(GameObject seatObject)
    {
        while (Vector3.Distance(transform.position, seatObject.transform.position) > agent.stoppingDistance)
        {
            yield return null;
        }

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        state.IsSitting = true;
        animation.SetMovementSpeed(0);
        originalRotation = transform.rotation;

        Transform sitPosition = seatObject.transform.childCount > 0 ? seatObject.transform.GetChild(0) : seatObject.transform;
        transform.position = sitPosition.position;
        transform.rotation = sitPosition.rotation;

        string sitAnim = "";
        switch (seatObject.tag)
        {
            case "Sofa":
                sitAnim = "isSofaSitting";
                break;
            case "Bench":
                sitAnim = "isBenchSitting";
                break;
            case "GroundSit":
                sitAnim = "isGroundSitting";
                break;
            case "Layable":
                sitAnim = "isBedSitting";
                break;
        }

        animation.Sit(sitAnim);

        Vector3 lastSitPos = transform.position;
        float pushThreshold = 0.2f;
        
        while (npcBrain.GetCurrentBehavior() == NPCBrain.NPCBehavior.Sit && !state.IsOverriden)
        {
            float movedDist = Vector3.Distance(transform.position, lastSitPos);
            if (movedDist > pushThreshold)
            {
                break;
            }
            yield return null;
        }

        animation.ResetSit(sitAnim);
        transform.rotation = originalRotation;

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;
        state.IsSitting = false;

        if (npcBrain.GetNPCSO().NPCBehaviourType == NPCScriptableObject.NPCBehaviourTypes.Quan)
        {
            npcRb.isKinematic = true;
        }

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

            animation.ResetSit("isSofaSitting");
            animation.ResetSit("isBenchSitting");
            animation.ResetSit("isBedSitting");
            animation.ResetSit("isGroundSitting");

            transform.rotation = originalRotation;
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
            state.IsSitting = false;

            if (currentSeat != null)
            {
                NPCManager.Instance.ReleaseSeat(currentSeat.transform);
                currentSeat = null;
            }

            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
        }
    }
    
    public void HandleGoHome()
    {
        if (npcBrain.npcHouse == null) return;

        if (isAtBodymod || isAtMarket || isAtMedical)
        {
            HandleExitStore();
        }
        else
        {
            HandleGoTo(npcBrain.npcHouse.position);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                NPCManager.Instance.DisableNPC(gameObject);
            }
        }
    }

    private StoreType GetRandomStore()
    {
        return new List<StoreType>
        {
            StoreType.Market,
            StoreType.Medical,
            StoreType.BodyMod
        }[Random.Range(0, 3)];
    }
    
    public void HandleGoToStore(StoreType storeType = StoreType.None)
    {
        Transform exit = null;
        Transform entrance = null;
        
        selectedStore = storeType != StoreType.None ? storeType : GetRandomStore();
        
        switch (selectedStore)
        {
            case StoreType.Market:
                exit = marketExit;
                entrance = marketEntrance;
                break;
            case StoreType.Medical:
                exit = medicalExit;
                entrance = medicalEntrance;
                break;
            case StoreType.BodyMod:
                exit = bodymodExit;
                entrance = bodymodEntrance;
                break;
        }
    
        if (entrance == null) return;
        
        HandleGoTo(entrance.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.Warp(exit.position);
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);

            isAtMarket = selectedStore == StoreType.Market;
            isAtMedical = selectedStore == StoreType.Medical;
            isAtBodymod = selectedStore == StoreType.BodyMod;
            currentStore = selectedStore;
        }

        if (npcBrain.isShopkeeper)
        {
            GetBehindCounter();
        }
    }

    private void GetBehindCounter()
    {
        Transform counterWaypoint = null;

        switch (currentStore)
        {
            case StoreType.Market:
                counterWaypoint = markerCounter;
                break;
            case StoreType.Medical:
                counterWaypoint = medicalCounter;
                break;
            case StoreType.BodyMod:
                counterWaypoint = bodymodCounter;
                break;
        }

        if (counterWaypoint != null)
        {
            agent.stoppingDistance = 0.3f;
            HandleGoTo(counterWaypoint.position);
        }
    }
    
    public void HandleExitStore()
    {
        Transform exit = null;
        Transform entrance = null;

        switch (currentStore)
        {
            case StoreType.Market:
                exit = marketExit;
                entrance = marketEntrance;
                break;
            case StoreType.Medical:
                exit = medicalExit;
                entrance = medicalEntrance;
                break;
            case StoreType.BodyMod:
                exit = bodymodExit;
                entrance = bodymodEntrance;
                break;
        }

        if (exit != null && entrance != null)
        {
            HandleGoTo(exit.position);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.Warp(entrance.position);
                isAtMarket = false;
                isAtBodymod = false;
                isAtMedical = false;
                currentStore = StoreType.None;
                selectedStore = StoreType.None;

                npcBrain.SetBehavior(NPCBrain.NPCBehavior.Wander);
            }
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

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        headBone.LookAt(playerTransform.position, Vector3.up);
        headBone.Rotate(30, 0, 0);

        if (distance >= detectionRadius) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0f;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float velocityMagnitude = agent.velocity.magnitude;

        if (!state.IsSitting && velocityMagnitude < 0.1f && angleToPlayer > 20f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void DetectPlayer()
    {
        if (!playerTransform) return;

        Vector3 dirToPlayer = playerTransform.position - transform.position;
        float distance = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (distance < detectionRadius && angle < detectionAngle)
        {
            HandleLookAt();
        }
        else
        {
            headBone.localRotation = defaultHeadLocalRotation;
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
        }
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