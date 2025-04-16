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
        //Debug.Log("Objective list retrieved: " + (objectiveList != null ? objectiveList.Count.ToString() : "null"));

        foreach (var objective in objectiveList)
        {
            //Debug.Log("Processing objective: " + (objective != null ? objective.ObjectiveType : "null"));

            if (objective.ObjectiveType == "GoTo")
            {
                questManager.SetObjectiveAsComplete((int)questManager.GetActiveQuestID(), objective.ObjectiveType);
            }
            else
            {
                //Debug.Log("Objective does not meet criteria.");
            }
        }

        return null;
    }
}