using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [SerializeField] private NPCState state;
    [SerializeField] private NPCMovement movement;
    [SerializeField] private NPCBehavior currentBehavior;
    
    public NPCBehavior AfterDialogueBehavior {get; set;}

    private NPCBehavior tempBehaviour;
    private void Awake()
    {
        state = GetComponent<NPCState>();
        movement = GetComponent<NPCMovement>(); 
    }

    private void Update()
    {
        if (state.IsOverriden) return;
        
        UpdateBehavior();
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
    }
    
    public void EndConversation()
    {
        //SetBehavior(AfterDialogueBehavior, 5f);
        currentBehavior = AfterDialogueBehavior;        // <-------- nejak ukoncit po urcite dobe
    }

    public void SetBehavior(NPCBehavior newBehavior, float duration = 0)
    {
        if (duration > 0)
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
        LookAtPlayer
    }
}