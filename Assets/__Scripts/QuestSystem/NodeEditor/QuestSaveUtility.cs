using Assets.__Scripts.QuestSystem.NodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

public class QuestSaveUtility
{
    private QuestGraphView _targetGraphView;
    private QuestContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<QuestNode> Nodes => _targetGraphView.nodes.ToList().Cast<QuestNode>().ToList();
    
    public static QuestSaveUtility GetInstance(QuestGraphView targetGraphView)
    {
        return new QuestSaveUtility
        {
            _targetGraphView = targetGraphView,
        };
    
    }

    public void SaveGraph()
    {
        var questContainer = ScriptableObject.CreateInstance<QuestContainer>();

        foreach (var edge in Edges)
        {
            var outputNode = edge.output.node as QuestNode;
            var inputNode = edge.input.node as QuestNode;

            if (outputNode == null || inputNode == null)
            {
                Debug.LogError("Edge has null nodes.");
                continue;
            }

            questContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGUID = outputNode.GUID,
                portName = edge.output.portName,
                targetNodeGUID = inputNode.GUID
            });
        }

        foreach (var questNode in Nodes)
        {
            QuestNodeModel nodeModel = CreateNodeModel(questNode);
            if (nodeModel != null)
            {
                var serializableModel = SerializableQuestNodeModel.SerializeNodeModel(nodeModel);
                questContainer.questNodeData.Add(serializableModel);
            }
        }

        AssetDatabase.CreateAsset(questContainer, "Assets/Resources/questGraph.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadGraph()
    {
        _containerCache = Resources.Load<QuestContainer>("questGraph");
        if (_containerCache == null)
        {
            Debug.Log("Incorrect path to data");
            return;
        }

        ClearGraph();
        CreateNodes();
        if (Nodes.FirstOrDefault(node => node.QuestType == QuestNode.NodeTypes.Start) == null)
        {
            _targetGraphView.CreateNode(QuestNode.NodeTypes.Start);
        }
        ConnectNodes();
    }

    public void ConnectNodes()
    {
        foreach (var node in Nodes)
        {
            var connections = _containerCache.nodeLinks.Where(x => x.baseNodeGUID == node.GUID).ToList();
            foreach (var connection in connections)
            {
                var targetNodeGUID = connection.targetNodeGUID;
                var targetNode = Nodes.FirstOrDefault(x => x.GUID == targetNodeGUID);
                if (targetNode == null)
                {
                    Debug.LogError($"Target node with GUID {targetNodeGUID} not found.");
                    continue;
                }

                var outputPorts = node.outputContainer.Children().OfType<Port>().ToList();
                if (!outputPorts.Any())
                {
                    Debug.LogError($"No output ports found on node {node.GUID}");
                }

                var outputPort = outputPorts.FirstOrDefault(port => port.portName == connection.portName);
                if (outputPort == null)
                {
                    Debug.LogError($"Output port with name {connection.portName} not found on node {node.GUID}");
                }

                var inputPort = targetNode.inputContainer.Children().OfType<Port>().FirstOrDefault();
                if (inputPort == null)
                {
                    Debug.LogError($"Input port not found on node {targetNode.GUID}.");
                    continue;
                }

                var targetNodeData = _containerCache.questNodeData.FirstOrDefault(x => SerializableQuestNodeModel.DeserializeNodeModel(x).GUID == targetNodeGUID);
                if (targetNodeData != null)
                {
                    var targetNodeModel = SerializableQuestNodeModel.DeserializeNodeModel(targetNodeData);
                    if (targetNodeModel != null)
                    {
                        targetNode.SetPosition(new Rect(targetNodeModel.position, _targetGraphView.defNodeSize));
                        LinkNodes(outputPort, inputPort);
                    }
                    else
                    {
                        Debug.LogError($"Unable to deserialize target node with GUID {targetNodeGUID}.");
                    }
                }
                else
                {
                    Debug.LogError($"Node data for GUID {targetNodeGUID} not found in container.");
                }
            }
        }
    }
    private QuestNodeModel CreateNodeModel(QuestNode node)
    {
        switch (node.QuestType)
        {
            case QuestNode.NodeTypes.Start:
                return new StartNodeModel
                {
                    GUID = node.GUID,
                    QuestType = node.QuestType,
                    position = node.GetPosition().position,
                };
            case QuestNode.NodeTypes.MainQuestNode:
                return new MainQuestNodeModel
                {
                    GUID = node.GUID,
                    QuestType = node.QuestType,
                    position = node.GetPosition().position,
                    QuestName = ((MainQuestNode)node).QuestName,
                    QuestDescription = ((MainQuestNode)node).QuestDescription
                };
            case QuestNode.NodeTypes.ObjectiveNode:
                return new ObjectiveNodeModel
                {
                    GUID = node.GUID,
                    QuestType = node.QuestType,
                    position = node.GetPosition().position,
                    ObjectiveType = ((ObjectiveNode)node).ObjectiveType,
                    ObjectiveDescription = ((ObjectiveNode)node).ObjectiveDescription,
                    isOptional = ((ObjectiveNode)node).isOptional,
                    CompletionCriteria = ((ObjectiveNode)node).CompletionCriteria
                };
            case QuestNode.NodeTypes.RewardNode:
                return new RewardNodeModel
                {
                    GUID = node.GUID,
                    QuestType = node.QuestType,
                    position = node.GetPosition().position,
                    RewardType = ((RewardNode)node).RewardType,
                    RewardValue = ((RewardNode)node).RewardValue
                };
            default:
                Debug.LogError($"Unsupported node type: {node.QuestType}");
                return null;
        }
    }


    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var serializableNode in _containerCache.questNodeData)
        {
            var nodeModel = SerializableQuestNodeModel.DeserializeNodeModel(serializableNode);
            if (nodeModel != null)
            {
                var createdNode = _targetGraphView.CreateNode(nodeModel.QuestType, nodeModel);
                _targetGraphView.AddElement(createdNode);
            }
        }
    }



    private void ClearGraph()
    {
        if (!Nodes.Any()) return;

        foreach (var node in Nodes)
        {
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            _targetGraphView.RemoveElement(node);
        }
    }
}


public static class CompletionCriteriaSerializer
{
    public static List<SerializableCompletionCriteria> Serialize(List<ICompletionCriteria> criteria)
    {
        var serializedCriteria = new List<SerializableCompletionCriteria>();
        foreach (var criterion in criteria)
        {
            var criteriaType = criterion.GetType().AssemblyQualifiedName;
            var jsonData = JsonUtility.ToJson(criterion);
            serializedCriteria.Add(new SerializableCompletionCriteria { CriteriaType = criteriaType, JsonData = jsonData });
        }
        return serializedCriteria;
    }

    public static List<ICompletionCriteria> Deserialize(List<SerializableCompletionCriteria> serializedCriteria)
    {
        var criteria = new List<ICompletionCriteria>();
        foreach (var serializedCriterion in serializedCriteria)
        {
            var type = Type.GetType(serializedCriterion.CriteriaType);
            var criterion = (ICompletionCriteria)JsonUtility.FromJson(serializedCriterion.JsonData, type);
            criteria.Add(criterion);
        }
        return criteria;
    }
}