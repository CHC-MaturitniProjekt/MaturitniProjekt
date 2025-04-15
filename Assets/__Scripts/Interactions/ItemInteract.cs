using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework.Internal.Execution;
using UnityEngine;

public class ItemInteract : InteractAction
{
    private QuestManager questManager;
    [SerializeField] private int itemID;
    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();
    }

    public int GetItemId()
    {
        return itemID;
    }
    
    public override async void OnInteract()
    {
        if (questManager.GetActiveQuestID() != null)
        {
            await OnObjectiveInteract();
        }
        
        var questObtainHandler = GetComponent<QuestObtainHandlerer>();
        if (questObtainHandler != null)
        {
            questObtainHandler.SetQuestAsObtained();
        }

        var monologueTriggerHandlerer = GetComponent<MonologueStartHandlerer>();
        if (monologueTriggerHandlerer != null)
        {
            monologueTriggerHandlerer.RunMonologue();
        }
        
        PlayerManager.Instance.PickUpItem(this.gameObject);
    }
    public override async Task OnObjectiveInteract()
    {
        var objectiveList = questManager.GetQuestObjectivesByQuestID(questManager.GetActiveQuestID());

        foreach (var objective in objectiveList)
        {
            if (objective.ObjectiveType == "PickUp" && objective.CompletionCriteria != null)
            {
                var completionData = CompletionCriteriaSerializer.Deserialize(objective.CompletionCriteria);
                if (completionData != null)
                {
                    var itemCompletionData = completionData.OfType<ItemCollectionCriteria>().FirstOrDefault();
                    if (itemCompletionData != null && itemCompletionData.RequiredItemCount == itemID)
                    {
                        await questManager.SetObjectiveAsComplete((int)questManager.GetActiveQuestID(), objective.ObjectiveType);
                    }
                }
            }
        }
        
    }
}
