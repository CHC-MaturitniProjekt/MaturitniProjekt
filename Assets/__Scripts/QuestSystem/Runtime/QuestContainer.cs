using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestContainer : ScriptableObject
{
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<SerializableQuestNodeModel> questNodeData = new List<SerializableQuestNodeModel>();
}
