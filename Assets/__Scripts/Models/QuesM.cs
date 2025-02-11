using System.Collections.Generic;

[System.Serializable]
public class QuestM
{
    public string title;
    public string description;
    public string reward;
}



public class Position
{
    public float x { get; set; }
    public float y { get; set; }
    public float magnitude { get; set; }
    public float sqrMagnitude { get; set; }
    public Normalized normalized { get; set; }
}

public class Normalized
{
    public float x { get; set; }
    public float y { get; set; }
    public float magnitude { get; set; }
    public float sqrMagnitude { get; set; }
}

public class Objective
{
    public string GUID { get; set; }
    public string ObjectiveDescription { get; set; }
    public string ObjectiveType { get; set; }
    public int QuestType { get; set; }
    public bool isCompleted { get; set; }
    public bool isOptional { get; set; }
    public Position position { get; set; }
}

public class Reward
{
    public string GUID { get; set; }
    public int QuestType { get; set; }
    public string RewardType { get; set; }
    public int RewardValue { get; set; }
    public Position position { get; set; }
}

public class Quest
{
    public string title { get; set; }
    public string description { get; set; }
    public bool isActive { get; set; }
    public List<Objective> objectives { get; set; }
    public List<Reward> rewards { get; set; }
}

public class QuestData
{
    public Dictionary<string, Quest> data { get; set; }
}