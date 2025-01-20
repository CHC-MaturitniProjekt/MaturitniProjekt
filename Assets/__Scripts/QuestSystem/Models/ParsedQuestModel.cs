using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static QuestNode;

[Serializable]
public class ParsedQuestModel
{
    public string GUID;
    public string QuestName;
    public string QuestDescription;
    public List<string> Objectives = new List<string>();
    public List<string> Rewards = new List<string>();
    public bool isOptional;
    public bool isCompleted;
}
