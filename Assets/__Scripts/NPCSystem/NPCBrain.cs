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
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
