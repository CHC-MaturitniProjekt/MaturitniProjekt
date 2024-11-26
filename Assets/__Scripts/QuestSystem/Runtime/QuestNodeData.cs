using System;
using UnityEngine;

[Serializable]
public class QuestNodeData
{
    public string GUID;
    public QuestNode.NodeTypes NodeType; 
    public string QuestName;
    public string QuestDescription;
    public string ObjectiveDescription;
    public string ObjectiveType;
    public string RewardType;
    public int RewardValue;
    public Vector2 Position;
}