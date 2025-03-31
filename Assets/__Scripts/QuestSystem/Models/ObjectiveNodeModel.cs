using System;
using System.Collections.Generic;

[Serializable]
public class ObjectiveNodeModel : QuestNodeModel
{
    public string ObjectiveDescription;
    public string ObjectiveType;
    public bool isOptional;
    public bool isCompleted;
    public List<SerializableCompletionCriteria> CompletionCriteria = new List<SerializableCompletionCriteria>();
}

[Serializable]
public class SerializableCompletionCriteria
{
    public string CriteriaType;
    public string JsonData;
}


//[Serializable]
//public class CollectObjectiveNodeModel : ObjectiveNodeModel
//{
//    public string ItemToCollect;
//    public int Quantity;
//}

//[Serializable]
//public class InteractObjectiveNodeModel : ObjectiveNodeModel
//{
//    public string ObjectToInteractWith;
//}

//[Serializable]
//public class FetchObjectiveNodeModel : ObjectiveNodeModel
//{
//    public string ItemToFetch;
//    public string FetchLocation;
//}