using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    private QuestManager questManager;
    private List<ParsedQuestModel> questList;
    
    private Firebase firebase;
    
    void Start()
    {
        firebase = FindFirstObjectByType<Firebase>();
        
        questManager = FindFirstObjectByType<QuestManager>();
        questList = questManager.GetQuestList();
    }

    public void TriggerQuest(ParsedQuestModel questData)
    {
        if (CheckConditions())
        {
            firebase.AddQuest(questData.GUID,questData.QuestName, questData.QuestDescription, questData.Objectives, questData.Rewards, questData.isActive);
        }
    }

    private bool CheckConditions()
    {
        return true;
    }
    
}
