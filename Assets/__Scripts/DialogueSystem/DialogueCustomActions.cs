using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Reflection;
using System.Collections.Generic;

public class DialogueActionsLibrary : MonoBehaviour
{
    private static DialogueActionsLibrary instance;
    private QuestTrigger questTrigger;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            RegisterLuaFunctions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        questTrigger = FindObjectOfType<QuestTrigger>();
    }
    
    private void RegisterLuaFunctions()
    {
        Lua.RegisterFunction("SetNPCBehaviour", this, SymbolExtensions.GetMethodInfo(() => SetNPCBehaviour("")));
        Lua.RegisterFunction("SetNPCEmotion", this, SymbolExtensions.GetMethodInfo(() => SetNPCEmotion("")));
        Lua.RegisterFunction("TriggerQuest", this, SymbolExtensions.GetMethodInfo(() => questTrigger.TriggerQuest(null)));
    }

    public void SetNPCBehaviour(string behaviourString)
    {
        NPCBrain npcBrain = FindFirstObjectByType<NPCBrain>();
        if (npcBrain == null)
        {
            Debug.LogError("NPCBrain component not found");
            return;
        }

        if (System.Enum.TryParse(behaviourString, out NPCBrain.NPCBehaviour behaviour))
        {
            npcBrain.currentBehaviour = behaviour;
            Debug.Log("Behaviour set to " + behaviour);
        }
        else
        {
            Debug.LogError("Invalid behaviour: " + behaviourString);
        }
    }
    
    public void SetNPCEmotion(string emotionString)
    {
        NPCBrain npcBrain = FindFirstObjectByType<NPCBrain>();
        if (npcBrain == null)
        {
            Debug.LogError("NPCBrain component not found");
            return;
        }
        
        if (System.Enum.TryParse(emotionString, out NPCBrain.NPCEmotion emotion))
        {
            npcBrain.currentEmotion = emotion;
            Debug.Log("Emotion set to " + emotion);
        }
        else
        {
            Debug.LogError("Invalid emotion: " + emotionString);
        }
    }
}