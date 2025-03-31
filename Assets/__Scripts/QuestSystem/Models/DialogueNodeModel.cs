using System;
using System.Collections.Generic;

[Serializable]
public class DialogueNodeModel : QuestNodeModel
{
    public string DialogueName;
    public int NPCID;
    public int order;
    public bool isCompleted;
    public bool isSMS;
}