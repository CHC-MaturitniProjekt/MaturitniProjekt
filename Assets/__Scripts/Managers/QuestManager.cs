using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public QuestContainer questContainer;
   
    [SerializeField]
    private List<QuestNodeData> questList = new List<QuestNodeData>();

    [SerializeField]
    private List<string[]> questConnections = new List<string[]>();

    [SerializeField]
    private TMP_Text questTitleText;
    [SerializeField]
    private TMP_Text questObjectivesText;
    [SerializeField]
    private TMP_Text questRewardsText;

    void Awake()
    {
        LoadQuests();
        DisplayQuestInfo();
    }

    void Start()
    {

    }

    private void LoadQuests()
    {
        foreach (var node in questContainer.questNodeData) //uklada data o questech
        {
            questList.Add(node);
        }
        
        foreach (var link in questContainer.nodeLinks)  //uklada spojeni mezi questy
        {
            List<string> connection = new List<string>();
            connection.Add(link.baseNodeGUID);

            foreach (var node in questContainer.questNodeData)
            {
                if (node.GUID == link.targetNodeGUID)
                {
                    connection.Add(node.GUID);
                }
            }
            questConnections.Add(connection.ToArray());

            foreach (var tmp in questConnections)
            {
                Debug.Log(tmp);
            }
        }
    }

    private void DisplayQuestInfo()
    {
        if (questList.Count > 0)
        {
            var mainQuest = questList[0];
            questTitleText.text = mainQuest.QuestName;

            string objectives = "";
            questObjectivesText.text = mainQuest.ObjectiveDescription;

            string rewards = "";
            questRewardsText.text = mainQuest.RewardValue.ToString();
        }
    }
}