using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteract : InteractAction
{
    private QuestManager questManager;
    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();
    }

    public override void OnInteract()
    {
        if (questManager.GetIsQuestActiveId() != null)
        {
            OnObjectiveInteract();
        }
        
        PlayerManager.Instance.PickUpItem(this.gameObject);
    }
    public override void OnObjectiveInteract()
    {
        var objectiveList = questManager.GetQuestObjectivesByQuestID(questManager.GetIsQuestActiveId());

        foreach (var objective in objectiveList)
        {
            if (objective.ObjectiveType == "PickUp")
            {
                
            }
        }
        
    }
}
