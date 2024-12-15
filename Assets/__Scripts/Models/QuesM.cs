[System.Serializable]
public class QuestM
{
    public string title;
    public string description;
    public string reward;

    public QuestM(string title, string description, string reward)
    {
        this.title = title;
        this.description = description;
        this.reward = reward;
    }
}