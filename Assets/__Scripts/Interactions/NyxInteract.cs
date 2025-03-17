using System;
using System.Collections.Generic;
using System.Linq;
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

    public override void OnInteract()           //TODO: neco to dela, ted jen zjistit co a proc
    {
        string questName = "";
        ParsedQuestModel quest = null;
        foreach (var tempQuest in questManager.GetQuestList())
        {
            if (tempQuest.isActive && tempQuest.dialogues != null && tempQuest.dialogues.Any())
            {
                quest = tempQuest;
                break;
            }
        }
        
        if (quest != null)
        {
            var availableDialogues = quest.dialogues;
            foreach (var dialogue in availableDialogues.OrderBy<DialogueNodeModel, int>(d => d.order))
            {
                if (!dialogue.isCompleted)
                {
                    questName = dialogue.DialogueName;
                    dialogue.isCompleted = true;
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(questName) && questManager.GetActiveQuestID() != null)
            {
                OnObjectiveInteract();
            }
        }
        else
        {
            questName = SelectGenericDialogue(NPCSO.GenericDialogues);
        }

        DialogueManager.StartConversation(questName, gameObject.transform);
        
        NPCBrain npcBrain = this.GetComponent<NPCBrain>();
        if (npcBrain != null)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.LookAtPlayer);
        }
    }

    public override void OnObjectiveInteract()
    {
        var objectiveList = questManager.GetQuestObjectivesByQuestID(questManager.GetActiveQuestID());

        foreach (var objective in objectiveList)
        {
            if (objective.ObjectiveType == "Interact" && objective.CompletionCriteria[0] != null)
            {
                var completionData = CompletionCriteriaSerializer.Deserialize(objective.CompletionCriteria);
                var itemCompletionData = completionData.OfType<NpcInteractionCriteria>().FirstOrDefault();
                if (itemCompletionData != null && itemCompletionData.NpcName == NPCSO.NPCName)
                {
                    questManager.SetObjectiveAsComplete((int)questManager.GetActiveQuestID(), objective.ObjectiveType);
                }
            }
        }
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