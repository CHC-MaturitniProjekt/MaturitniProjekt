using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NyxInteract : InteractAction
{
    [SerializeField] private StoryNPCScriptableObject NPCSO;
    private QuestManager questManager;
    
    private void Start()
    {
        questManager = FindAnyObjectByType<QuestManager>();
    }

    public override void OnInteract()
    {
        string questName = "";
        
        if (CheckQuestIsActive(questManager.GetIsQuestActiveId()))
        {
            questName = SelectStoryDialogue(NPCSO.StoryDialogues, "DHGeneric2");  // TODO: dynamicky doplnit nazev
        }
        else if (CheckQuestIsCompleted(questManager.GetIsQuestCompletedId()))
        {
            questName = SelectStoryDialogue(NPCSO.StoryDialogues, "test");
        }
        else
        {
            questName = SelectGenericDialogue(NPCSO.GenericDialogues);
        }

        DialogueManager.StartConversation(questName, this.gameObject.transform);
        
        NPCBrain npcBrain = this.GetComponent<NPCBrain>();
        if (npcBrain != null)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.LookAtPlayer);
        }
    }
    
    public override void OnObjectiveInteract() {}


    private bool CheckQuestIsCompleted(int? requestedQuestId)
    {
        var quests = questManager.GetQuestList();
        foreach (var quest in quests)
        {
            if (quest.QuestID == requestedQuestId)
            {
                if (quest.isCompleted) return true;
            }
        }
        return false;
    }
    
    private bool CheckQuestIsActive(int? requestedQuestId)
    {
        var quests = questManager.GetQuestList();
        foreach (var quest in quests)
        {
            if (quest.QuestID == requestedQuestId)
            {
                if (quest.isActive) return true;
            }
        }
        return false;
    }

    private string SelectStoryDialogue(List<string> dialogues, string storyConvoName)
    {
        string conversation = "";
        foreach (var dial in dialogues)
        {
            if (dial == storyConvoName)
            {
                conversation = dial;
                break;
            }
        }
        
        return conversation;
    }

    private string SelectGenericDialogue(List<string> dialogues)
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogError("No dialogues available");
            return string.Empty;
        }

        int randomDialogueIndex = UnityEngine.Random.Range(0, dialogues.Count);
        return dialogues[randomDialogueIndex];
    }
}