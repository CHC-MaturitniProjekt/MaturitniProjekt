using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NyxInteract : InteractAction
{
    private NPCBrain npcBrain;
    private QuestManager questManager;
    
    private void Start()
    {
        questManager = FindAnyObjectByType<QuestManager>();
        npcBrain = GetComponent<NPCBrain>();
    }

    public override void OnInteract()
    {
        string questName = "";
        ParsedQuestModel quest = null;
        foreach (var tempQuest in questManager.GetQuestList())
        {
            if (tempQuest.isActive && tempQuest.dialogues != null)
            {
                quest = tempQuest;
                Debug.Log(quest);
                break;
            }
        }
        
        if (quest != null)
        {
            var availableDialogues = quest.dialogues;
            Debug.Log(availableDialogues);
            foreach (var dialogue in availableDialogues.OrderBy(d => d.order))
            {
                if (!dialogue.isCompleted)
                {
                    questName = dialogue.DialogueName;
                    dialogue.isCompleted = true;
                    Debug.Log(questName);
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
            questName = SelectGenericDialogue(this.npcBrain.GetNPCSO().GenericDialogues);
        }
        
        DialogueManager.StartConversation(questName, gameObject.transform);
        
        NPCBrain npcBrain = this.GetComponent<NPCBrain>();
        if (npcBrain != null && npcBrain.GetCurrentBehavior() != NPCBrain.NPCBehavior.Sit)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.LookAtPlayer);
        }
    }

    public override Task OnObjectiveInteract()
    {
        var objectiveList = questManager.GetQuestObjectivesByQuestID(questManager.GetActiveQuestID());
        Debug.Log(questManager.GetActiveQuestID());
        foreach (var objective in objectiveList)
        {
            Debug.Log("afds: " + objective.ObjectiveType);
            if (objective.ObjectiveType == "Interact")
            {
                Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var completionData = CompletionCriteriaSerializer.Deserialize(objective.CompletionCriteria);
                questManager.SetObjectiveAsComplete((int)questManager.GetActiveQuestID(), objective.ObjectiveType);
                /*var itemCompletionData = completionData.OfType<NpcInteractionCriteria>().FirstOrDefault();
                if (itemCompletionData != null && itemCompletionData.NpcName == npcBrain.GetNPCSO().NPCName)
                {
                    Debug.Log("fkasdjflasdjlfasdk");
                    
                }*/
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