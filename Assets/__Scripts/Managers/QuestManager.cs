using Assets.__Scripts.QuestSystem.NodeEditor;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TreeEditor.TreeEditorHelper;

public class QuestManager : MonoBehaviour
{
    public QuestContainer questContainer;
   
    [SerializeField]
    private List<ParsedQuestModel> questList = new List<ParsedQuestModel>();

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
            var parsedQuest = ParseQuestData(node);
            if (parsedQuest != null)
            {
                questList.Add(parsedQuest);
            }
        }
        
        foreach (var link in questContainer.nodeLinks)  //uklada spojeni mezi questy
        {
            List<string> connection = new List<string>();
            connection.Add(link.baseNodeGUID);

            foreach (var node in questContainer.questNodeData)
            {
                if (SerializableQuestNodeModel.DeserializeNodeModel(node).GUID == link.targetNodeGUID)
                {
                    connection.Add(SerializableQuestNodeModel.DeserializeNodeModel(node).GUID);
                }
            }
            questConnections.Add(connection.ToArray());

            foreach (var tmp in questConnections)
            {
                Debug.Log(tmp);
            }
        }
    }

    private void TobankuvParserv()
    {
        var idk = Resources.Load<QuestContainer>("questGraph");
        foreach (var serializableNode in idk.questNodeData)
        {
            var nodeModel = SerializableQuestNodeModel.DeserializeNodeModel(serializableNode);
            QuestNode node;

            switch (nodeModel.QuestType)
            {
                case QuestNode.NodeTypes.Start:
                    node = new StartQuestNode
                    {
                        title = nodeModel.QuestType.ToString(),
                    };
                    break;
                case QuestNode.NodeTypes.MainQuestNode:
                    node = new MainQuestNode
                    {
                        title = nodeModel.QuestType.ToString(),
                        QuestName = (nodeModel as MainQuestNodeModel).QuestName,
                        QuestDescription = (nodeModel as MainQuestNodeModel).QuestDescription
                    };
                    break;

                case QuestNode.NodeTypes.ObjectiveNode:
                    node = new ObjectiveNode
                    {
                        title = nodeModel.QuestType.ToString(),
                        ObjectiveDescription = (nodeModel as ObjectiveNodeModel).ObjectiveDescription,
                        ObjectiveType = (nodeModel as ObjectiveNodeModel).ObjectiveType,
                        isOptional = (nodeModel as ObjectiveNodeModel).isOptional,
                        CompletionCriteria = (nodeModel as ObjectiveNodeModel).CompletionCriteria
                    };
                    break;
                case QuestNode.NodeTypes.RewardNode:
                    node = new RewardNode
                    {
                        title = nodeModel.QuestType.ToString(),
                        RewardType = (nodeModel as RewardNodeModel).RewardType,
                        RewardValue = (nodeModel as RewardNodeModel).RewardValue
                    };
                    break;
                 
            }
        }
    }

        private ParsedQuestModel ParseQuestData(SerializableQuestNodeModel node)
    {
        var questNodeModel = SerializableQuestNodeModel.DeserializeNodeModel(node);
        if (questNodeModel.QuestType == QuestNode.NodeTypes.MainQuestNode)
        {
            var parsedQuestModel = new ParsedQuestModel
            {
                GUID = questNodeModel.GUID,
                QuestName = questNodeModel.QuestType.ToString(),
                QuestDescription = "Description",
                Objectives = new List<string>(),
                Rewards = new List<string>(),
                isOptional = false,
                isCompleted = false
            };

            foreach (var objectiveNode in questContainer.questNodeData)
            {
                var objectiveModel = SerializableQuestNodeModel.DeserializeNodeModel(objectiveNode);
                if (objectiveModel.QuestType == QuestNode.NodeTypes.ObjectiveNode)
                {
                    parsedQuestModel.Objectives.Add(JsonUtility.ToJson(objectiveModel));
                }
                else if (objectiveModel.QuestType == QuestNode.NodeTypes.RewardNode)
                {
                    parsedQuestModel.Rewards.Add(JsonUtility.ToJson(objectiveModel));
                }
            }

            return parsedQuestModel;
        }

        return null;
    }
}