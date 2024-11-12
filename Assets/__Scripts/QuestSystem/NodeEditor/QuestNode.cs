using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class QuestNode : Node
{
    public string GUID;

    public string QuestName;
    public string QuestDescription;
    public string QuestType;

    public bool EntryPoint = false;
}
