using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueManagerExtension : MonoBehaviour
{
    private CameraManager cameraManager;
    
    private NPCBrain npcBrain;
    
    private void Start()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        npcBrain = FindObjectOfType<NPCBrain>();
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
        npcBrain.StartConversation();
        
    }

    private void OnConversationEnd(Transform actor)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        npcBrain.EndConversation();
    }
}