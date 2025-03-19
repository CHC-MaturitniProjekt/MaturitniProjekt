using Assets.__Scripts.QuestSystem.NodeEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using Language.Lua;
using Unity.VisualScripting;
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
        
        this.AddManipulator(new ContextualMenuManipulator(evt => ShowNodeCreationDropdown(evt.menu, evt.mousePosition / this.scale)));
    }

    public void ShowNodeCreationDropdown(DropdownMenu menu, Vector2 mousePosition)
    {
        menu.AppendAction("Quest Node", action =>
        {
            CreateNode(QuestNode.NodeTypes.MainQuestNode, mousePosition);
        });

        menu.AppendAction("Objective Node", action =>
        {
            CreateNode(QuestNode.NodeTypes.ObjectiveNode, mousePosition);
        });

        menu.AppendAction("Reward Node", action =>
        {
            CreateNode(QuestNode.NodeTypes.RewardNode, mousePosition);
        });

        menu.AppendAction("Dialogue Node", action =>
        {
            CreateNode(QuestNode.NodeTypes.DialogueNode, mousePosition);
        });
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
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateAsset(_containerCache, "Assets/Resources/questGraph.asset");
            AssetDatabase.SaveAssets();
        }

        if (!NodeExists(QuestNode.NodeTypes.Start))
        {
            CreateNode(QuestNode.NodeTypes.Start);
        }    
    }
    
    private bool NodeExists(QuestNode.NodeTypes nodeType)
    {
        foreach (var node in nodes.ToList())
        {
            if (node is QuestNode questNode && questNode.QuestType == nodeType)
            {
                return true;
            }
        }
        return false;
    }


    public void CreateNode(QuestNode.NodeTypes nodeType, Vector2 position = default)
    {
        QuestNode node;

        switch (nodeType)
        {
            case QuestNode.NodeTypes.MainQuestNode:
                node = new MainQuestNode
                {
                    QuestID = 0,
                    title = "Quest Node",
                    QuestName = "New Quest",
                    QuestDescription = "Describe the quest here",
                    isActive = false,
                    isCompleted = false,
                    isObtained = false
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
            case QuestNode.NodeTypes.DialogueNode:
                node = new DialogueNode
                {
                    DialogueName = "Dialogue",
                    NPCID = 0,
                    order = 0,
                    isCompleted = false
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
        node.SetPosition(new Rect(position, new Vector2(500, 450)));
        AddElement(node);
    }
    
    public QuestNode CreateNode(QuestNode.NodeTypes nodeType, QuestNodeModel nodeData)
    {
        QuestNode node;
        
        switch (nodeType)
        {
            case QuestNode.NodeTypes.Start:
                node = new StartQuestNode
                {
                    title = nodeType.ToString(),
                };
                break;
            case QuestNode.NodeTypes.MainQuestNode:
                node = new MainQuestNode
                {
                    title = nodeType.ToString(),
                    QuestID = (nodeData as MainQuestNodeModel).QuestID,
                    QuestName = (nodeData as MainQuestNodeModel).QuestName,
                    QuestDescription = (nodeData as MainQuestNodeModel).QuestDescription,
                    isActive = (nodeData as MainQuestNodeModel).isActive,
                    isObtained = (nodeData as MainQuestNodeModel).isObtained,
                    isCompleted = (nodeData as MainQuestNodeModel).isCompleted,
                    
                };
                break;

            case QuestNode.NodeTypes.ObjectiveNode:
                node = new ObjectiveNode
                {
                    title = nodeType.ToString(),
                    ObjectiveDescription = (nodeData as ObjectiveNodeModel).ObjectiveDescription,
                    ObjectiveType = (nodeData as ObjectiveNodeModel).ObjectiveType,
                    isOptional = (nodeData as ObjectiveNodeModel).isOptional,
                    CompletionCriteria = CompletionCriteriaSerializer.Deserialize((nodeData as ObjectiveNodeModel).CompletionCriteria)
                };
                break;
            case QuestNode.NodeTypes.RewardNode:
                node = new RewardNode
                {
                    title = nodeType.ToString(),
                    RewardType = (nodeData as RewardNodeModel).RewardType,
                    RewardValue = (nodeData as RewardNodeModel).RewardValue
                };
                break;
            case NodeTypes.DialogueNode:
                node = new DialogueNode
                {
                    title = nodeType.ToString(),
                    DialogueName = (nodeData as DialogueNodeModel).DialogueName,
                    NPCID = (nodeData as DialogueNodeModel).NPCID,
                    order = (nodeData as DialogueNodeModel).order,
                    isCompleted = (nodeData as DialogueNodeModel).isCompleted
                };
                break;

            default:
                node = new StartQuestNode
                {
                    title = "Start",
                };
                Debug.LogError($"Unknown node type: {nodeType}");
                return node;
        }

        node.DrawNode();
        node.style.backgroundColor = UnityEngine.Color.black;

        node.GUID = nodeData.GUID;
        node.SetPosition(new Rect(nodeData.position, new Vector2(500, 450)));
        AddElement(node);
        return node;
    }
}


