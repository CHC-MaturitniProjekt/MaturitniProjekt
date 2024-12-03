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
        ConnectQuests();
        DisplayQuestInfo();
    }

    void Start()
    {

    }

    void Update()
    {

    }

    private void LoadQuests()
    {
        foreach (var node in questContainer.questNodeData)
        {
            questList.Add(node);
        }
    }

    private void ConnectQuests()
    {
        foreach (var link in questContainer.nodeLinks)
        {
            List<string> connection = new List<string>();
            connection.Add(link.baseNodeGUID);
            Debug.Log("Base GUID: " + link.baseNodeGUID);

            foreach (var node in questContainer.questNodeData)
            {
                if (node.GUID == link.targetNodeGUID)
                {
                    connection.Add(node.GUID);
                    Debug.Log("GUID: " + node.GUID);
                }
            }

            questConnections.Add(connection.ToArray());

            Debug.Log("Connection: ");
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
            var mainQuest = questList[0]; // Assuming the first quest is the main quest
            questTitleText.text = mainQuest.QuestName;

            string objectives = "";
            questObjectivesText.text = mainQuest.ObjectiveDescription;

            string rewards = "";
            questRewardsText.text = mainQuest.RewardValue.ToString();
        }
    }
}