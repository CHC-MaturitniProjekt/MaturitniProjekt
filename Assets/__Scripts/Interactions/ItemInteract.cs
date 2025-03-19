using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public override void OnInteract()
    {
        if (questManager.GetActiveQuestID() != null)
        {
            OnObjectiveInteract();
        }
        
        var questObtainHandler = GetComponent<QuestObtainHandlerer>();
        if (questObtainHandler != null)
        {
            questObtainHandler.SetQuestAsObtained();
        }
        
        PlayerManager.Instance.PickUpItem(this.gameObject);
    }
    public override void OnObjectiveInteract()
    {
        var objectiveList = questManager.GetQuestObjectivesByQuestID(questManager.GetActiveQuestID());

        foreach (var objective in objectiveList)
        {
            if (objective.ObjectiveType == "PickUp" && objective.CompletionCriteria[0] != null)
            {
                var completionData = CompletionCriteriaSerializer.Deserialize(objective.CompletionCriteria);
                var itemCompletionData = completionData.OfType<ItemCollectionCriteria>().FirstOrDefault();
                if (itemCompletionData != null && itemCompletionData.RequiredItemCount == itemID)
                {
                    questManager.SetObjectiveAsComplete((int)questManager.GetActiveQuestID(), objective.ObjectiveType);
                }
            }
        }
        
    }
}
