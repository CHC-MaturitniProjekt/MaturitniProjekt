using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public QuestContainer questContainer;
    [SerializeField]
    private List<QuestNodeData> questList = new List<QuestNodeData>();

    [SerializeField]
    private List<string[]> questConnections = new List<string[]>();
    void Awake()
    {
        LoadQuests();
        ConnectQuests();
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
}