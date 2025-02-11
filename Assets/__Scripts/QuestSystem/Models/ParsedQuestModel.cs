using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static QuestNode;

[Serializable]
public class ParsedQuestModel
{
    public string GUID;
    public int QuestID;
    public string QuestName;
    public string QuestDescription;
    public List<ObjectiveNodeModel> Objectives = new List<ObjectiveNodeModel>();
    public List<RewardNodeModel> Rewards = new List<RewardNodeModel>();
    public bool isOptional;
    public bool isCompleted;
    public bool isActive;
}