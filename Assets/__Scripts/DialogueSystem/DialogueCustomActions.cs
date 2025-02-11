using UnityEngine;
using PixelCrushers.DialogueSystem;
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
    private Interact interact;
    
    private Firebase firebase;
    
    void Start()
    {
        firebase = FindFirstObjectByType<Firebase>();
        
        questManager = FindFirstObjectByType<QuestManager>();
        interact = FindFirstObjectByType<Interact>();

    }
    
    private void RegisterLuaFunctions()
    {
        Lua.RegisterFunction("SetNPCBehaviour", this, SymbolExtensions.GetMethodInfo(() => SetNPCBehaviour("", 0)));
        Lua.RegisterFunction("TriggerQuest", this, SymbolExtensions.GetMethodInfo(() => TriggerQuest(0)));
        Lua.RegisterFunction("CheckQuestHaving", this, SymbolExtensions.GetMethodInfo(() => CheckQuestHaving(0)));
        Lua.RegisterFunction("SwitchConversation", this, SymbolExtensions.GetMethodInfo(() => SwitchConversation("")));
        Lua.RegisterFunction("IsHoldingItem", this, SymbolExtensions.GetMethodInfo(() => IsHoldingItem("")));
    }

    public void SetNPCBehaviour(string behaviourString, double overrideTime)
    {
        NPCBrain npcBrain = FindFirstObjectByType<NPCBrain>();
        if (npcBrain == null)
        {
            Debug.LogError("NPCBrain component not found");
            return;
        }

        if (System.Enum.TryParse<NPCBrain.NPCBehavior>(behaviourString, out var behaviour))
        {
            npcBrain.AfterDialogueBehavior = behaviour;
        }
        else
        {
            Debug.LogError("Invalid behaviour: " + behaviourString);
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

    public bool CheckQuestHaving(double questID)
    {
        questList = questManager.GetQuestList();
        ParsedQuestModel questData = questList.Find(quest => quest.QuestID == (int)questID);

        bool res = firebase.CheckQuest(questData.GUID).Result;

        return res;
    }

    private void SwitchConversation(string conversationName)
    {
        DialogueManager.StopConversation();
        DialogueManager.StartConversation(conversationName, transform);
    }

    public bool IsHoldingItem(string itemName)
    {
        var currentItem = interact.GetCurrentItem();
        if (currentItem == null)
        {
            return false;
        }

        if (currentItem.CompareTag(itemName))
        {
            return true;
        }
        return false;
    }
}