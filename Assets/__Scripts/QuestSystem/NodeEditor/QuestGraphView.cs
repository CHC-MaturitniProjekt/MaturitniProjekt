using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Label = UnityEngine.UIElements.Label;

public class QuestGraphView : GraphView
{
    public readonly Vector2 defNodeSize = new Vector2(150, 200);
    private readonly Vector2 defNodePosition = new Vector2(150, 150);

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
            QuestDescription = "testuju",
            QuestType = "Swag",
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


    public void CreateNode(string nodeName)
    {
        AddElement(CreateQuestNode(nodeName));
    }


    public QuestNode CreateQuestNode(string nodeName)
    {
        var questNode = new QuestNode
        {
            title = nodeName,
            QuestDescription = nodeName,
            GUID = Guid.NewGuid().ToString()

        };

        var inputPort = GeneratePort(questNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        questNode.inputContainer.Add(inputPort);

        var btn = new Button(() =>
        {
            AddChoicePort(questNode);
        });
        btn.text = "New choice";
        questNode.titleContainer.Add(btn);

        questNode.RefreshExpandedState();
        questNode.RefreshPorts();

        questNode.SetPosition(new Rect(defNodePosition, defNodeSize));


        return questNode;
    }

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
