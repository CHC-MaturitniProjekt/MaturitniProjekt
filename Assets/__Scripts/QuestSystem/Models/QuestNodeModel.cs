using System;
using System.Collections.Generic;
using UnityEngine;
using static QuestNode;

[Serializable]
public class QuestNodeModel
{
    public NodeTypes QuestType;
    public string GUID;
    public Vector2 position;
}

[Serializable]
public class SerializableQuestNodeModel
{
    public string NodeType; // Typ uzlu (nap�. MainQuestNode, ObjectiveNode, ...)
    public string JsonData; // Serializovan� data uzlu

    public SerializableQuestNodeModel(string nodeType, string jsonData)
    {
        NodeType = nodeType;
        JsonData = jsonData;
    }

    public static SerializableQuestNodeModel SerializeNodeModel(QuestNodeModel nodeModel)
    {
        var nodeType = nodeModel.GetType().AssemblyQualifiedName; // Z�sk� pln� typ t��dy
        var jsonData = JsonUtility.ToJson(nodeModel);             // Serializuje do JSON
        return new SerializableQuestNodeModel(nodeType, jsonData);
    }

    public static QuestNodeModel DeserializeNodeModel(SerializableQuestNodeModel serializableModel)
    {
        var nodeType = Type.GetType(serializableModel.NodeType); // Najde typ uzlu
        return (QuestNodeModel)JsonUtility.FromJson(serializableModel.JsonData, nodeType);
    }

}
