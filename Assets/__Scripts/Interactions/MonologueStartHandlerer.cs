using PixelCrushers.DialogueSystem;
using UnityEngine;

public class MonologueStartHandlerer : MonoBehaviour
{
    public string dialogueName;
    public bool isTriggerEnabled = true;
    void OnTriggerEnter(Collider other)
    {
        if (isTriggerEnabled && other.CompareTag("Player"))
        {
            RunMonologue();
        }
    }

    public void RunMonologue()
    {
        DialogueManager.StartConversation(dialogueName, gameObject.transform);
    }
}
