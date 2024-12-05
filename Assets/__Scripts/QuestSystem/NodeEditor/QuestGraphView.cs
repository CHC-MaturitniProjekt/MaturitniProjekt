using Assets.__Scripts.QuestSystem.NodeEditor;
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
using static QuestNode;
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

        EditorApplication.delayCall += afterGraphInicialization;
    }

    public void afterGraphInicialization()
    {
        GenerateEntryNode();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                if (startPort is CustomPort startCustomPort && port is CustomPort targetCustomPort)
                {
                    if (startCustomPort.connectionType == targetCustomPort.connectionType)
                    {
                        if (startCustomPort.direction != targetCustomPort.direction)
                        {
                            compatiblePorts.Add(port);
                        }
                    }
                }
            }
        });

        return compatiblePorts;
    }

    public void GenerateEntryNode()
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

        CreateNode(NodeTypes.Start);
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
            case QuestNode.NodeTypes.Start:
                node = new StartQuestNode
                {
                    title = "Start"
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
                    RewardType = nodeData.RewardType,
                    RewardValue = nodeData.RewardValue
                };
                break;

            default:
                node = new RewardNode
                {
                    title = "",
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


