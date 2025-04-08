using System;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using DialogueActor = PixelCrushers.DialogueSystem.Wrappers.DialogueActor;

public class DialogueManagerExtension : MonoBehaviour
{
    private void Update()
    {
        if (DialogueManager.instance.isConversationActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnEnable()
    {
        DialogueManager.instance.conversationStarted += OnConversationStart;
        DialogueManager.instance.conversationEnded += OnConversationEnd;
    }

    private void OnDisable()
    {
        if (DialogueManager.instance == null) return;
        DialogueManager.instance.conversationStarted -= OnConversationStart;
        DialogueManager.instance.conversationEnded -= OnConversationEnd;
    }
    
    private void OnConversationStart(Transform actor)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        var actorBrain = actor.GetComponent<NPCBrain>();
        if (actorBrain != null)
        {
            actorBrain.StartConversation();
        }
        else
        {
            Debug.LogWarning("NPCBrain not found on actor: " + actor.name);
        }
    }

    private void OnConversationEnd(Transform actor)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        var actorBrain = actor.GetComponent<NPCBrain>();
        if (actorBrain != null)
        {
            actorBrain.EndConversation();
        }
    }
    
}