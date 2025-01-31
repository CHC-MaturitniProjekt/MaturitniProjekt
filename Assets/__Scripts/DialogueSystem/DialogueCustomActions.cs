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
    
    private QuestManager questManager;
    private List<ParsedQuestModel> questList;
    
    private Firebase firebase;
    
    void Start()
    {
        firebase = FindFirstObjectByType<Firebase>();
        
        questManager = FindFirstObjectByType<QuestManager>();
    }
    
    private void RegisterLuaFunctions()
    {
        Lua.RegisterFunction("SetNPCBehaviour", this, SymbolExtensions.GetMethodInfo(() => SetNPCBehaviour("")));
        Lua.RegisterFunction("SetNPCEmotion", this, SymbolExtensions.GetMethodInfo(() => SetNPCEmotion("")));
        Lua.RegisterFunction("TriggerQuest", this, SymbolExtensions.GetMethodInfo(() => TriggerQuest(5)));
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
    
    public void TriggerQuest(double questID)
    {
        questList = questManager.GetQuestList();
        ParsedQuestModel questData = questList.Find(quest => quest.QuestID == (int)questID);
        Debug.Log("Quest data: " + questData);
        if (questData != null && CheckConditions())
        {
            firebase.AddQuest(questData.GUID, questData.QuestName, questData.QuestDescription, questData.Objectives, questData.Rewards, questData.isActive);
        }
    }
    
    private bool CheckConditions()
    {
        // aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        return true;
    }
}