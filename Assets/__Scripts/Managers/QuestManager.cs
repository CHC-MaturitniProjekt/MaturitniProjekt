using System;
using System.Collections;
using Assets.__Scripts.QuestSystem.NodeEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private QuestContainer questContainer;
    private Firebase firebase;
    
    [SerializeField]
    private List<ParsedQuestModel> questList = new List<ParsedQuestModel>();
    
    public List<ParsedQuestModel> GetQuestList()
    {
        return questList;
    }
    
    [SerializeField]
    private List<string[]> questConnections = new List<string[]>();

    void Awake()
    {
        questContainer = Resources.Load<QuestContainer>("questGraph");
        firebase = FindFirstObjectByType<Firebase>();
        LoadQuests();
        StartCoroutine(DelayedPushQuests());
        
    }
    private IEnumerator DelayedPushQuests()
    {
        yield return new WaitForSeconds(1.0f); 
        PushQuests();
    }

    private void LoadQuests()
    {
        questList.Clear();
        foreach (var node in questContainer.questNodeData)
        {
            var parsedQuest = ParseQuestData(node);
            if (parsedQuest != null)
            {
                questList.Add(parsedQuest);
            }
        }

        questConnections.Clear();
        foreach (var link in questContainer.nodeLinks)
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
        }
    }

    public void ObtainQuest(int questID)
    {
        var quest = questList.FirstOrDefault(q => q.QuestID == questID);
        if (quest == null)
        {
            Debug.LogError("Quest ID is incorrect.");
            return;
        }

        if (quest.isObtained)
        {
            Debug.LogWarning("Quest is already obtained.");
            return;
        }
        quest.isObtained = true;

    }

    public bool IsQuestObtained(int questID)
    {
        var quest = questList.FirstOrDefault(q => q.QuestID == questID);
        if (quest == null || !quest.isObtained) return false;

        return true;
    }

    public int? GetIsQuestActiveId()
    {
        foreach (var quest in questList)
        {
            if (quest.isActive) return quest.QuestID;
        }
        return null;
    }
    
    public int? GetIsQuestCompletedId()
    {
        foreach (var quest in questList)
        {
            if (quest.isCompleted) return quest.QuestID;
        }
        return null; 
    }

    public void SetQuestAsActive(string QuestGUID)
    {
        foreach (var quest in questList)
        {
            quest.isActive = quest.GUID == QuestGUID;
        }
    }

    public List<ObjectiveNodeModel> GetQuestObjectivesByQuestID(int? questID)
    {
        foreach (var quest in questList)
        {
            if (quest.QuestID == questID && quest.Objectives != null) return quest.Objectives;
        }
        return null;
    }

    public void SetQuestAsComplete(int? questID)
    {
        foreach (var quest in questList)
        {
            if (quest.QuestID == questID)
            {
                quest.isCompleted = true;
            }
        }
    }

    public void CheckObjectiveCompletion(int questID)
    {
        var quest = questList.FirstOrDefault(q => q.QuestID == questID);
        if (quest != null)
        {
            bool allObjectivesCompleted = quest.Objectives.All(obj => obj.isCompleted);
            if (allObjectivesCompleted)
            {
                quest.isCompleted = true;
            }
        }
    }
    
    
    public void SetObjectiveAsComplete(int questID, string objectiveType)
    {
        var objectives = GetQuestObjectivesByQuestID(questID);
        var quest = questList.FirstOrDefault(q => q.QuestID == questID);
        if (objectives != null && quest != null)
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectiveType == objectives[i].ObjectiveType)
                {
                    objectives[i].isCompleted = true;
                    CheckObjectiveCompletion(questID);
                    firebase.UpdateObjectiveCompletionStatus(quest.GUID, i, true);
                }
            }
        }
    }
    
    public async void PushQuests()
    {
        foreach (var questData in questList)
        {
            bool questExists = await firebase.CheckQuest(questData.GUID);
            if (!questExists)
            {
                Debug.Log(questData.QuestName);
                firebase.AddQuest(questData.GUID, questData.QuestName, questData.QuestDescription, questData.Objectives, questData.Rewards, questData.nextQuests, questData.isActive, questData.isCompleted, questData.isObtained);
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
                QuestID = (questNodeModel as MainQuestNodeModel).QuestID,
                QuestName = (questNodeModel as MainQuestNodeModel).QuestName,
                QuestDescription = (questNodeModel as MainQuestNodeModel).QuestDescription,
                Objectives = new List<ObjectiveNodeModel>(),
                Rewards = new List<RewardNodeModel>(),
                nextQuests = new List<string>(),
                isCompleted = false,
                isActive = false,
                isObtained = false
            };

            foreach (var link in questContainer.nodeLinks)
            {
                if (link.baseNodeGUID == questNodeModel.GUID)
                {
                    foreach (var objectiveNode in questContainer.questNodeData)
                    {
                        var objectiveModel = SerializableQuestNodeModel.DeserializeNodeModel(objectiveNode);
                        if (objectiveModel.GUID == link.targetNodeGUID)
                        {
                            switch (objectiveModel.QuestType)
                            {
                                case QuestNode.NodeTypes.ObjectiveNode: 
                                    parsedQuestModel.Objectives.Add(objectiveModel as ObjectiveNodeModel);
                                    break;
                                case QuestNode.NodeTypes.RewardNode:
                                    parsedQuestModel.Rewards.Add(objectiveModel as RewardNodeModel);
                                    break;
                                case QuestNode.NodeTypes.MainQuestNode:
                                    parsedQuestModel.nextQuests.Add(objectiveModel.GUID);
                                    break;
                                default:
                                    Debug.LogError("Error parsing quests: Quest type mismatch");
                                    break;
                            }
                        }
                    }
                }
            }

            return parsedQuestModel;
        }
        return null;
    }
    
}