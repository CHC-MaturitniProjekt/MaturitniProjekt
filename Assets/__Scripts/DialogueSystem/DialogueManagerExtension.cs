using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueManagerExtension : MonoBehaviour
{
    private CameraManager cameraManager;
    
    private void Start()
    {
        cameraManager = FindObjectOfType<CameraManager>();
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
        
    }

    private void OnConversationEnd(Transform actor)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}