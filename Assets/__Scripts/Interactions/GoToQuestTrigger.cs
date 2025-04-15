using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;

public class GoToQuestTrigger : InteractAction
{
    [SerializeField] private int questID;
    private QuestManager questManager;

    private void Awake()
    {
        questManager = FindFirstObjectByType<QuestManager>();
    }

    void OnTriggerEnter(Collider col)
    {
        
        Debug.Log(questID == questManager.GetActiveQuestID());
        Debug.Log( "Active questID: "+ questManager.GetActiveQuestID() + " QuestID: " + questID);
        if (col.CompareTag("Player"))
        {
            if (questID == questManager.GetActiveQuestID())
            {
                Debug.Log("aa");
                OnObjectiveInteract();
            }
        }
    }

    public override void OnInteract()
    {
        throw new System.NotImplementedException();
    }

    public override Task OnObjectiveInteract()
    {
        var objectiveList = questManager.GetQuestObjectivesByQuestID(questManager.GetActiveQuestID());

        foreach (var objective in objectiveList)
        {
            if (objective.ObjectiveType == "GoTo" && objective.CompletionCriteria != null && objective.CompletionCriteria.Count > 0)
            {
                var completionData = CompletionCriteriaSerializer.Deserialize(objective.CompletionCriteria);
                var itemCompletionData = completionData.OfType<GoToTriggerCriteria>().FirstOrDefault();
                if (itemCompletionData != null)
                {
                    questManager.SetObjectiveAsComplete((int)questManager.GetActiveQuestID(), objective.ObjectiveType);
                }
            }
        }

        return null;
    }
}