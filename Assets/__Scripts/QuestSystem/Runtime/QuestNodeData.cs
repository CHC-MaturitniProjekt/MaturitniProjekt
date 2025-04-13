#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestNodeData
{
    public string GUID;
    public NodeTypes NodeType;
    public string QuestName;
    public string QuestDescription;
    public string ObjectiveDescription;
    public string ObjectiveType;
    public List<SerializableCompletionCriteria> CompletionCriteria = new List<SerializableCompletionCriteria>();
    public string RewardType;
    public int RewardValue;
    public Vector2 Position;
    public bool isOptional;
}
#endif

