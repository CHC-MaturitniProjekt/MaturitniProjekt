using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Label = UnityEngine.UIElements.Label;

public class QuestGraphView : GraphView
{
    public readonly Vector2 defNodeSize = new Vector2(150, 200);
    private readonly Vector2 defNodePosition = new Vector2(350, 350);
    private QuestContainer _containerCache;

    
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

    public QuestNode GenerateEntryNode()
    {
        _containerCache = Resources.Load<QuestContainer>("questGraph");

        if (_containerCache == null)
        {
            _containerCache = ScriptableObject.CreateInstance<QuestContainer>();
            _containerCache.entryNodeGUID = Guid.NewGuid().ToString();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateAsset(_containerCache, "Assets/Resources/questGraph.asset");
            AssetDatabase.SaveAssets();
        }

        var node = new QuestNode
        {
            QuestName = "test",
            title = "Start",
            GUID = _containerCache.entryNodeGUID,
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


    public void CreateNode(QuestNode.NodeTypes nodeType, Vector2 position = default)
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
                    RewardType = "PerkPoints",
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
    
    public QuestNode CreateNode(QuestNode.NodeTypes nodeType, QuestNodeData nodeData)
    {
        QuestNode node;
        
        switch (nodeType)
        {
            case QuestNode.NodeTypes.MainQuestNode:
                node = new MainQuestNode
                {
                    title = nodeType.ToString(),
                    QuestName = nodeData.QuestName,
                    QuestDescription = nodeData.QuestDescription
                };
                break;

            case QuestNode.NodeTypes.ObjectiveNode:
                node = new ObjectiveNode
                {
                    title = nodeType.ToString(),
                    QuestName = nodeData.QuestName,
                    QuestDescription = nodeData.QuestDescription,
                    ObjectiveDescription = nodeData.ObjectiveDescription,
                    ObjectiveType = nodeData.ObjectiveType,
                    /*
                    isOptional = nodeData.isOptional
                    */

                };
                break;

            case QuestNode.NodeTypes.RewardNode:
                node = new RewardNode
                {
                    title = nodeType.ToString(),
                    QuestName = nodeData.QuestName,
                    QuestDescription = nodeData.QuestDescription,
                    RewardType = nodeData.RewardType,
                    RewardValue = nodeData.RewardValue
                };
                break;

            default:
                node = new RewardNode
                {
                    title = "",
                    QuestName = "",
                    QuestDescription = "",
                    RewardType = "",
                    RewardValue = 0
                };
                Debug.LogError($"Unknown node type: {nodeType}");
                return node;
        }

        node.DrawNode();
        node.style.backgroundColor = UnityEngine.Color.black;

        node.GUID = Guid.NewGuid().ToString();
        node.SetPosition(new Rect(nodeData.Position, new Vector2(500, 450)));
        AddElement(node);
        return node;
    }
}


