using Assets.__Scripts.QuestSystem.NodeEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StartQuestNode : QuestNode
{
    public StartQuestNode()
    {
        TitleColor = new Color(0f / 255f, 50f / 255f, 0f / 255f);
    }

    public override void DrawNode()
    {
        var inputPort = new CustomPort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, ConnectionType.Quest);
        inputPort.portName = "Start";
        inputContainer.Add(inputPort);

        RefreshExpandedState();
        RefreshPorts();
    }
}