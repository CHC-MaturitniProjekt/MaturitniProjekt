using System;

[Serializable]
public class MainQuestNodeModel : QuestNodeModel
{
    public string QuestName;
    public string QuestDescription;
    public int QuestID;
    public bool isCompleted;
    public bool isActive;
    public bool isObtained;
}
