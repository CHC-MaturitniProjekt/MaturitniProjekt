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

    void Awake()
    {
        LoadQuests();
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
}