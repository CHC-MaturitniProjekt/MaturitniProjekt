using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            foreach (var dialogue in availableDialogues.OrderBy(d => d.order))
            {
                if (!dialogue.isCompleted && !dialogue.isSMS)
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

    public override Task OnObjectiveInteract()
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

        return null;
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