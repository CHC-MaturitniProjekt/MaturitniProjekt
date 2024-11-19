using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

public class GraphSaveUtility
{
    private QuestGraphView _targetGraphView;
    private QuestContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<QuestNode> Nodes => _targetGraphView.nodes.ToList().Cast<QuestNode>().ToList();
    
    public static GraphSaveUtility GetInstance(QuestGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView,
        };
    
    }

    public void SaveGraph()
    {
        if(!Edges.Any()) return;

        var questContainer = ScriptableObject.CreateInstance<QuestContainer>();

        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as QuestNode;
            var inputNode = connectedPorts[i].input.node as QuestNode;

            questContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGUID = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeGUID = inputNode.GUID
            });
        }

        foreach (var questNode in Nodes.Where(node=>!node.EntryPoint))
        {
            questContainer.questNodeData.Add(new QuestNodeData()
            {
                GUID = questNode.GUID,
                QuestName = questNode.QuestName,
                QuestDescription = questNode.QuestDescription,
                Position = questNode.GetPosition().position
            });
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
        ConnectNodes();

    }

    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count(); i++)
        {
            var connections = _containerCache.nodeLinks.Where(x => x.baseNodeGUID == Nodes[i].GUID).ToList();
            for (int h = 0; h < connections.Count; h++)
            {
                var targetNodeGUID = connections[h].targetNodeGUID;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                LinkNodes(Nodes[i].outputContainer[h].Q<Port>(), (Port)targetNode.inputContainer[0]);

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

    private void CreateNodes()
    {
        foreach (var nodeData in _containerCache.questNodeData)
        {
            var tempNode = _targetGraphView.CreateQuestNode(nodeData.QuestName);
            tempNode.GUID = nodeData.GUID;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGUID == nodeData.GUID).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        if (Nodes.Count > 0) return;
        Nodes.Find(x => x.EntryPoint).GUID = _containerCache.nodeLinks[0].baseNodeGUID;

        foreach (var node in Nodes)
        {
            if (node.EntryPoint) continue;

            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            _targetGraphView.RemoveElement(node);
        }
    }
}
