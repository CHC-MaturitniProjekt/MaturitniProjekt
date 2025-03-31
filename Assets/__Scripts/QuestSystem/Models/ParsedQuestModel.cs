using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static QuestNode;

[Serializable]
public class ParsedQuestModel
{
    public string GUID;
    public Nullable<int> QuestID;
    public string QuestName;
    public string QuestDescription;
    public List<ObjectiveNodeModel> Objectives = new List<ObjectiveNodeModel>();
    public List<RewardNodeModel> Rewards = new List<RewardNodeModel>();
    public List<string> nextQuests = new List<string>();
    public bool isCompleted;
    public bool isActive;
    public bool isObtained;

    public List<DialogueNodeModel> dialogues = new List<DialogueNodeModel>();
}