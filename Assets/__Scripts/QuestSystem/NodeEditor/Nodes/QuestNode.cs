using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestNode : Node
{
    public enum NodeTypes
    {
        Start,
        MainQuestNode,
        ObjectiveNode,
        RewardNode
    }

    public Color TitleColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
    public NodeTypes QuestType;
    public string GUID;

    public virtual void DrawNode()
    {
        RefreshExpandedState();
        RefreshPorts();
    }
}