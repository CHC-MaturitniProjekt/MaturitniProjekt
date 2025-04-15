using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPCBrain : MonoBehaviour
{
    private NPCState state;
    private NPCMovement movement;
    [SerializeField] private NPCBehavior currentBehavior;
    [SerializeField] private NPCScriptableObject npcInfo;
    [SerializeField] private int npcId;
    public bool isShopkeeper;
    public Transform npcHouse;
    public NPCMovement.SpotType selectedSpot = NPCMovement.SpotType.None;

    
    private CameraController playerCam;
    private TimeManager timeManager;
    private Transform currentWaypoint;
    public NPCBehavior AfterDialogueBehavior {get; set;}
    
    private TextureAnimation textureAnimation;
    private float eliotBehaviorTimer = 0f;

    
    private void Awake()
    {
        state = GetComponent<NPCState>();
        movement = GetComponent<NPCMovement>();
        playerCam = FindFirstObjectByType<CameraController>();
        timeManager = FindAnyObjectByType<TimeManager>();

        textureAnimation = GetComponent<TextureAnimation>();

        if (npcId is 3 or 8 or 9 or 4)
        {
            isShopkeeper = true;
        }
    }

    private void Start()
    {
        NPCManager.Instance.RegisterNPC(gameObject);
    }

    private void Update()
    {
        if (state.IsOverriden) return;
        UpdateBehavior();
        NPCCycles();
    }

    public int GetNPCID()
    {
        return npcId;
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
            case NPCBehavior.GoToStore:
                movement.HandleGoToStore(selectedSpot);
                break;
            case NPCBehavior.GoToMarket:
                movement.HandleGoToStore(NPCMovement.SpotType.Market);
                break;
            case NPCBehavior.GoToBodyMod:
                movement.HandleGoToStore(NPCMovement.SpotType.BodyMod);
                break;
            case NPCBehavior.GoToMedical:
                movement.HandleGoToStore(NPCMovement.SpotType.Medical);
                break;
            case NPCBehavior.GoToHole:
                movement.HandleGoTo(movement.holePos.position);
                break;
            case NPCBehavior.ExitStore:
                movement.HandleExitStore();
                break;
        }
    }
    
    private void NPCCycles()
    {
        float currentHour = timeManager.GetWorldTime()/60f;
        float activeValue = npcInfo.NPCActiveTimeCurve.Evaluate(currentHour);
        
        if (npcInfo.storyImportant)
        {
            if (npcInfo.NPCBehaviourType == NPCScriptableObject.NPCBehaviourTypes.Quan)
            {
                SetBehavior(NPCBehavior.Sit);
                return;
            }

            if (npcInfo.NPCBehaviourType == NPCScriptableObject.NPCBehaviourTypes.Elliot && npcInfo.hasDailySchedule)
            {
                if (npcInfo.NPCWayPointNames.Count > 0)
                {
                    currentWaypoint = WaypointManager.Instance.GetWaypoint(npcInfo.NPCWayPointNames[0]);
                }

                if (currentHour >= npcInfo.activeHourStart && currentHour <= npcInfo.activeHourEnd)
                {
                    if (!gameObject.activeSelf)
                    {
                        NPCManager.Instance.EnableNPC(gameObject);
                        SetBehavior(NPCBehavior.Wander);
                        eliotBehaviorTimer = 0f; // Reset timer on activation
                    }

                    eliotBehaviorTimer += Time.deltaTime;

                    if (eliotBehaviorTimer >= 15f)
                    {
                        eliotBehaviorTimer = 0f;

                        List<(NPCBehavior behavior, float weight)> behaviors = new List<(NPCBehavior, float)>
                        {
                            (NPCBehavior.Idle, 0.3f),
                            (NPCBehavior.Wander, 0.1f),
                            (NPCBehavior.Sit, 0.6f)
                        };

                        float behaviorWeight = behaviors.Sum(b => b.weight);
                        float behaviorRandValue = Random.Range(0, behaviorWeight);

                        foreach (var behavior in behaviors)
                        {
                            if (behaviorRandValue < behavior.weight)
                            {
                                SetBehavior(behavior.behavior);
                                break;
                            }
                            behaviorRandValue -= behavior.weight;
                        }
                    }
                }
                else
                {
                    SetBehavior(NPCBehavior.GoHome);
                }

                return;
            }
            return;
        }

        
        if (Random.value < npcInfo.NPCRandomness * (activeValue / 2) * Time.deltaTime)
        {
            if (currentHour >= 18f || currentHour < 6f)
            {
                if (!npcHouse) return;
                
                if (currentBehavior != NPCBehavior.GoHome)
                {
                    SetBehavior(NPCBehavior.GoHome);
                }
                return;
            }

            
            if (npcInfo.NPCBehaviourType != NPCScriptableObject.NPCBehaviourTypes.Stationary)
            {
                SetBehavior(GetRandomBehavior());
            }
            else
            {
                switch (npcId)
                {
                    case 8:     //market
                        SetBehavior(NPCBehavior.GoToMarket);
                        break;
                    case 9:     //medical
                        SetBehavior(NPCBehavior.GoToMedical);
                        break;
                    case 3:     //bodymods
                        SetBehavior(NPCBehavior.GoToBodyMod);
                        break;
                    case 4:     //policie
                        SetBehavior(NPCBehavior.GoToHole);
                        break;
                }
            }
        }
    }
    
    private NPCBehavior GetRandomBehavior()
    {
        if (movement.currentSpot != NPCMovement.SpotType.None)
        {
            List<(NPCBehavior behavior, float weight)> inStoreBehaviors = new List<(NPCBehavior, float)>
            {
                (NPCBehavior.Idle, 0.3f),
                (NPCBehavior.Wander, 0.4f),
                (NPCBehavior.ExitStore, 0.3f)
            };
            
            float inStoreTotalWeight = inStoreBehaviors.Sum(b => b.weight);
            float inStoreRandomValue = Random.Range(0, inStoreTotalWeight);

            foreach (var behavior in inStoreBehaviors)
            {
                if (inStoreRandomValue < behavior.weight)
                {
                    return behavior.behavior;
                }
                inStoreRandomValue -= behavior.weight;
            }

            return NPCBehavior.Idle;
        }

        List<(NPCBehavior behavior, float weight)> behaviors = new List<(NPCBehavior, float)>
        {
            (NPCBehavior.Idle, 0.2f),
            (NPCBehavior.Wander, 0.5f),
            (NPCBehavior.Sit, 0.2f),
            (NPCBehavior.GoToStore, 0.1f)
        };

        float totalWeight = behaviors.Sum(b => b.weight);
        float randomValue = Random.Range(0, totalWeight);
        
        foreach (var behavior in behaviors)
        {
            if (randomValue < behavior.weight)
            {
                if (behavior.behavior == NPCBehavior.GoToStore)
                {
                    selectedSpot = GetRandomStore();
                }
                return behavior.behavior;
            }
            randomValue -= behavior.weight;
        }

        return NPCBehavior.Idle;
    }
    
    private NPCMovement.SpotType GetRandomStore()
    {
        return new List<NPCMovement.SpotType>
        {
            NPCMovement.SpotType.Market,
            NPCMovement.SpotType.Medical,
            NPCMovement.SpotType.BodyMod
        }[Random.Range(0, 3)];
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
         playerCam.isInConvo = true;
         textureAnimation.PlayAnimation(TextureAnimation.AnimationType.Speaking);
    }
    
    public void EndConversation()
    {
        currentBehavior = AfterDialogueBehavior;
        playerCam.isInConvo = false;
        textureAnimation.PlayAnimation(TextureAnimation.AnimationType.Blinking);
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
        GoHome,
        GoToMarket,
        GoToMedical,
        GoToBodyMod,
        GoToStore,
        GoToHole,
        ExitStore
    }
}