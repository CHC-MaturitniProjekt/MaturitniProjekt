using System;
using System.Collections.Generic;

[Serializable]
public class ObjectiveNodeModel : QuestNodeModel
{
    public string ObjectiveDescription;
    public string ObjectiveType;
    public bool isOptional;
    public List<ICompletionCriteria> CompletionCriteria = new List<ICompletionCriteria>();
}
