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
    private UIManager uiManager;
    
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
        uiManager = FindFirstObjectByType<UIManager>();
        
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

        var quests = firebase.GetQuests();
        foreach (var quest in quests)
        {
            quest.Value.GUID = quest.Key;
            questList.Add(quest.Value);
        }

        foreach (var node in questContainer.questNodeData)
        {
            var parsedQuest = ParseQuestData(node);
            if (parsedQuest != null)
            {
                var existingQuest = questList.FirstOrDefault(q => q.GUID == parsedQuest.GUID);
                if (existingQuest == null) continue;
                existingQuest.QuestID = parsedQuest.QuestID;
                existingQuest.dialogues = parsedQuest.dialogues;
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
        uiManager.AddQuest("New Quest: " + quest.QuestName);
        firebase.QuestObtain(quest.GUID);

    }

    public bool IsQuestObtained(int questID)
    {
        var quest = questList.FirstOrDefault(q => q.QuestID == questID);
        if (quest == null || !quest.isObtained) return false;

        return true;
    }

    public int? GetActiveQuestID()
    {
        foreach (var quest in questList)
        {
            if (quest.isActive) return quest.QuestID;
        }
        return null;
    }

    public void SetQuestAsActive(string questGUID)
    {
        foreach (var quest in questList)
        {
            if (quest.GUID == questGUID)
            {
                quest.isActive = !quest.isActive;
            }
            else
            {
                quest.isActive = false;
            }
        }
    }
    
    public void SetQuestAsCompleted(string questGUID)
    {
        foreach (var quest in questList)
        {
            quest.isCompleted = quest.GUID == questGUID;
        }
    }
    public void SetQuestAsObtained(string questGUID)
    {
        foreach (var quest in questList)
        {
            quest.isObtained = quest.GUID == questGUID;
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
                AddQuestRewards(questID);
            }
        }
    }

    private async void AddQuestRewards(int? questID)
    {
        var quest = questList.FirstOrDefault(q => q.QuestID == questID);
        if (quest?.Rewards[0] == null) return;
        
        string rewardType = quest.Rewards[0].RewardType;
        int rewardValue = quest.Rewards[0].RewardValue;

        switch (rewardType)
        {
            case "Money":
                await firebase.AddPlayerMoney(rewardValue);
                break;
            case "PerkPoints":
                Debug.Log("perk points added");
                break;
            default:
                Debug.LogError("Incorrect reward type on quest" + questID);
                break;
        }
    }

    private void CheckObjectiveCompletion(int questID)
    {
        var quest = questList.FirstOrDefault(q => q.QuestID == questID);
        if (quest != null)
        {
            bool allObjectivesCompleted = quest.Objectives.All(obj => obj.isCompleted);
            if (allObjectivesCompleted)
            {
                SetQuestAsComplete(questID);
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
        if (questList.Count == 0)
        {
            foreach (var node in questContainer.questNodeData)
            {
                var parsedQuest = ParseQuestData(node);
                if (parsedQuest != null)
                {
                    questList.Add(parsedQuest);
                }
            }
        }
        
        foreach (var questData in questList)
        {
            bool questExists = await firebase.CheckQuest(questData.GUID);
            if (!questExists)
            {
                Debug.Log(questData.QuestName);
                firebase.AddQuest(questData.GUID, questData.QuestName, questData.QuestDescription, questData.Objectives, questData.Rewards, questData.nextQuests, questData.isActive, questData.isCompleted, questData.isObtained);
            }
        }
        
        LoadQuests();
    }

    private ParsedQuestModel ParseQuestData(SerializableQuestNodeModel node)
    {
        var questNodeModel = SerializableQuestNodeModel.DeserializeNodeModel(node);
        if (questNodeModel.QuestType == QuestNode.NodeTypes.MainQuestNode)
        {
            var mainQuestNodeModel = questNodeModel as MainQuestNodeModel;
            var parsedQuestModel = new ParsedQuestModel
            {
                GUID = mainQuestNodeModel.GUID,
                QuestID = mainQuestNodeModel.QuestID,
                QuestName = mainQuestNodeModel.QuestName,
                QuestDescription = mainQuestNodeModel.QuestDescription,
                Objectives = new List<ObjectiveNodeModel>(),
                Rewards = new List<RewardNodeModel>(),
                nextQuests = new List<string>(),
                isCompleted = mainQuestNodeModel.isCompleted,
                isActive = mainQuestNodeModel.isActive,
                isObtained = mainQuestNodeModel.isObtained,
                dialogues = new List<DialogueNodeModel>()
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
                                case QuestNode.NodeTypes.DialogueNode:
                                    parsedQuestModel.dialogues.Add(objectiveModel as DialogueNodeModel);
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