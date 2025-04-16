using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class PcInteract : InteractAction
{
    private QuestManager questManager;
    
    [Header("Input")]
    [SerializeField] private InputReader input;
    [SerializeField] private Pc pc;
    [SerializeField] private int itemID;
    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();
    }

    public override async void OnInteract()
    {
        CameraManager.Instance.EnterPcCamera();
        PlayerManager.Instance.isDisabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        pc.StartInteracting();
        input.PcInputEnable();
        
        if (questManager.GetActiveQuestID() != null)
        {
            await OnObjectiveInteract();
        }
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