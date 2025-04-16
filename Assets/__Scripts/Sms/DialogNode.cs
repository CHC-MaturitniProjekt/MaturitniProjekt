using System.Collections.Generic;

[System.Serializable]
public class DialogNode
{
    public List<string> NpcText;
    public List<Response> Responses;
    public bool AutoAdvance = false;
    
    public System.Action OnNodeEnter;

    public List<string> GetResponseTexts()
    {
        List<string> texts = new List<string>();
        foreach (var r in Responses)
        {
            texts.Add(r.Text);
        }
        return texts;
    }
}

[System.Serializable]
public class Response
{
    public string Text;
    public DialogNode NextNode;
}
