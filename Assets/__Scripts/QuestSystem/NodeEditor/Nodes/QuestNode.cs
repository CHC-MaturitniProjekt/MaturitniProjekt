
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif
public enum NodeTypes
{
    Start,
    MainQuestNode,
    ObjectiveNode,
    RewardNode,
    DialogueNode
}

#if UNITY_EDITOR

public class QuestNode : Node
{
    
    public Color TitleColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
    public NodeTypes QuestType;
    public string GUID;

    public virtual void DrawNode()
    {
        RefreshExpandedState();
        RefreshPorts();
    }
}
#endif