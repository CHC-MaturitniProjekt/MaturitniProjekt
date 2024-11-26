using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Label = UnityEngine.UIElements.Label;

public class QuestGraphView : GraphView
{
    public readonly Vector2 defNodeSize = new Vector2(150, 200);
    private readonly Vector2 defNodePosition = new Vector2(350, 350);

   public QuestGraphView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("NodeEditor"));

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryNode());
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    private Port GeneratePort(QuestNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    private QuestNode GenerateEntryNode()
    {
        var node = new QuestNode
        {
            QuestName = "test",
            title = "Start",
            GUID = Guid.NewGuid().ToString(),
            EntryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 100, 100, 150));

        return node;
    }


    public void CreateNode(QuestNode.NodeTypes nodeType)
    {
        QuestNode node;

        switch (nodeType)
        {
            case QuestNode.NodeTypes.MainQuestNode:
                node = new MainQuestNode
                {
                    title = "Quest Node",
                    QuestName = "New Quest",
                    QuestDescription = "Describe the quest here"
                };
                break;

            case QuestNode.NodeTypes.ObjectiveNode:
                node = new ObjectiveNode
                {
                    title = "Objective Node",
                    ObjectiveDescription = "Describe the objective here",
                    ObjectiveType = "Fetch"
                };
                break;

            case QuestNode.NodeTypes.RewardNode:
                node = new RewardNode
                {
                    title = "Reward Node",
                    RewardType = "XP",
                    RewardValue = 100
                };
                break;

            default:
                Debug.LogError($"Unknown node type: {nodeType}");
                return;
        }

        node.DrawNode();
        node.style.backgroundColor = UnityEngine.Color.black;

        node.GUID = Guid.NewGuid().ToString();
        node.SetPosition(new Rect(Vector2.zero, new Vector2(500, 450)));
        AddElement(node);
    }
    
    /*public QuestNode CreateQuestNode(string nodeName)
    {
        var questNode = new QuestNode
        {
            title = nodeName,
            QuestName = nodeName,
            QuestDescription = "Default quest description",
            GUID = Guid.NewGuid().ToString()
        };

        questNode.DrawNode();

        questNode.SetPosition(new Rect(defNodePosition, defNodeSize));

        AddElement(questNode);

        return questNode;
    }*/

    public void AddChoicePort(QuestNode questNode, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(questNode, Direction.Output);

        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        var outputPortCount = questNode.outputContainer.Query("connector").ToList().Count;
        var choicePortName = string.IsNullOrEmpty(overriddenPortName) ? $"Choice {outputPortCount}" : overriddenPortName;
        
        var nameTextField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        nameTextField.RegisterValueChangedCallback(e => generatedPort.portName = e.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(nameTextField);

        var deleteButton = new Button(() => RemovePort(questNode, generatedPort)) {
            text = "X"
        };
        generatedPort.contentContainer.Add(deleteButton);


        generatedPort.portName = choicePortName;

        questNode.outputContainer.Add(generatedPort);
        questNode.RefreshPorts();
        questNode.RefreshExpandedState();
    }

    private void RemovePort(QuestNode questNode, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (!targetEdge.Any()) return;

        var edge = targetEdge.First();
        edge.input.Disconnect(edge);
        RemoveElement(targetEdge.First());

        questNode.outputContainer.Remove(generatedPort);
        questNode.RefreshPorts();
        questNode.RefreshExpandedState();
    }
}
