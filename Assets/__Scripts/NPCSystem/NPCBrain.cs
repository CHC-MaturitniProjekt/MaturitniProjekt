using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    public string NPCName;
    public enum NPCEmotion
    {
        Happy,
        Sad,
        Angry,
        Neutral
    }
    public NPCEmotion currentEmotion;
    
    public enum NPCBehaviour
    {
        Wander,
        FollowPlayer,
        RunAway,
        Idle,
        GoToDestination,
        LookAtPlayer
    }
    public NPCBehaviour currentBehaviour;
    public bool isOverriden;
    
    private NPCMovement npcMovement;

    private void Start()
    {
        npcMovement = GetComponent<NPCMovement>();
    }

    private void Update()
    {
        if (isOverriden) return;
        
        switch (currentBehaviour)
        {
            case NPCBehaviour.Wander:
                npcMovement.HandleWander();
                break;
            case NPCBehaviour.FollowPlayer:
                npcMovement.HandleFollowPlayer();
                break;
            case NPCBehaviour.RunAway:
                npcMovement.HandleRunAway();
                break;
            case NPCBehaviour.LookAtPlayer:
                npcMovement.HandleLookAt();
                break;
        }
    }

    public void SetEmotion(NPCEmotion emotion)
    {
        currentEmotion = emotion;
    }

    public void SetBehaviour(NPCBehaviour behaviour)
    {
        currentBehaviour = behaviour;
    }
    
    public void SetBehaviour(NPCBehaviour behaviour, double overrideTime)
    {
        StartCoroutine(OverrideBehaviour(behaviour, overrideTime));
    }
    
    public IEnumerator OverrideBehaviour(NPCBehaviour behaviour, double time)
    {
        isOverriden = true;
        currentBehaviour = behaviour;
        
        Debug.Log("Overriding behaviour for " + time + " seconds");
        yield return new WaitForSeconds((float) time);
        
        npcMovement.isRunningAway = false;
        
        isOverriden = false;
    }
}