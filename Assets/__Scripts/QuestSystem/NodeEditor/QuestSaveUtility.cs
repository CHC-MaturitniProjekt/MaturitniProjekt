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

    //public void SaveGraph()
    //{
    //    if (!Edges.Any()) return;

    //    var questContainer = ScriptableObject.CreateInstance<QuestContainer>();

    //    var entryNode = Nodes.Find(x => x.EntryPoint);
    //    if (entryNode != null)
    //    {
    //        questContainer.entryNodeGUID = entryNode.GUID;
    //    }

    //    var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
    //    for (int i = 0; i < connectedPorts.Length; i++)
    //    {
    //        var outputNode = connectedPorts[i].output.node as QuestNode;
    //        var inputNode = connectedPorts[i].input.node as QuestNode;

    //        questContainer.nodeLinks.Add(new NodeLinkData
    //        {
    //            baseNodeGUID = outputNode.GUID,
    //            portName = connectedPorts[i].output.portName,
    //            targetNodeGUID = inputNode.GUID
    //        });
    //    }

    //    foreach (var questNode in Nodes.Where(node => !node.EntryPoint))
    //    {
    //        questContainer.questNodeData.Add(new QuestNodeData()
    //        {
    //            GUID = questNode.GUID,
    //            NodeType = questNode.QuestType,
    //            QuestName = questNode.QuestName,
    //            QuestDescription = questNode.QuestDescription,
    //            ObjectiveDescription = (questNode as ObjectiveNode)?.ObjectiveDescription,
    //            ObjectiveType = (questNode as ObjectiveNode)?.ObjectiveType,
    //            CompletionCriteria = CompletionCriteriaSerializer.Serialize(questNode.CompletionCriteria),
    //            RewardType = (questNode as RewardNode)?.RewardTypes.ToString(),
    //            RewardValue = (questNode as RewardNode)?.RewardValue ?? 0,
    //            Position = questNode.GetPosition().position,
    //            /*
    //            isOptional = (questNode as ObjectiveNode)?.isOptional ?? false
    //        */
    //        });
    //    }

    //    AssetDatabase.CreateAsset(questContainer, "Assets/Resources/questGraph.asset");
    //    AssetDatabase.SaveAssets();
    //}

    public void LoadGraph()
    {
        _containerCache = Resources.Load<QuestContainer>("questGraph");
        if (_containerCache == null)
        {
            Debug.Log("Incorrect path to data");
            return;
        }

        ClearGraph();
       // CreateNodes();
        ConnectNodes();

        //var entryNode = Nodes.Find(x => x.EntryPoint);
        //if (entryNode != null)
        //{
        //    entryNode.GUID = _containerCache.entryNodeGUID;
        //}
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

                LinkNodes(outputPort, inputPort);

                targetNode.SetPosition(new Rect(
                    _containerCache.questNodeData.First(x => x.GUID == targetNodeGUID).Position, _targetGraphView.defNodeSize
                ));
            }
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

    //private void CreateNodes()
    //{
    //    foreach (var nodeData in _containerCache.questNodeData)
    //    {
    //        var tempNode = _targetGraphView.CreateNode(nodeData.NodeType, nodeData);
    //        tempNode.GUID = nodeData.GUID;
    //        tempNode.QuestName = nodeData.QuestName;
    //        tempNode.QuestType = nodeData.NodeType;
    //        tempNode.CompletionCriteria = CompletionCriteriaSerializer.Deserialize(nodeData.CompletionCriteria);
    //        _targetGraphView.AddElement(tempNode);
            
    //        var nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGUID == nodeData.GUID).ToList();
    //    }
    //}
    
    private void ClearGraph()
    {
        if (!Nodes.Any()) return;

        //var entryNode = Nodes.Find(x => x.EntryPoint);
        //if (entryNode != null)
        //{
        //    entryNode.GUID = _containerCache.entryNodeGUID;
        //}

        //foreach (var node in Nodes)
        //{
        //    if (node.EntryPoint) continue;

        //    Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
        //    _targetGraphView.RemoveElement(node);
        //}
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