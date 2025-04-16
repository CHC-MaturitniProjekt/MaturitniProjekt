using UnityEngine;

public class SmSManager : MonoBehaviour
{
    [SerializeField] private SmsSystem smsUI;
    private DialogNode currentNode;

    public void StartDialogue(DialogNode startNode)
    {
        currentNode = startNode;
        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        currentNode.OnNodeEnter?.Invoke();
        
        if (currentNode.NpcText != null && currentNode.NpcText.Count > 0)
        {
            smsUI.npcType(currentNode.NpcText, () =>
            {
                if (currentNode.Responses != null && currentNode.Responses.Count > 0)
                {
                    smsUI.showResponses(currentNode.GetResponseTexts());
                }
            });
        }
        else
        {
            if (currentNode.Responses != null && currentNode.Responses.Count > 0)
            {
                smsUI.showResponses(currentNode.GetResponseTexts());
            }
        }
    }

    public void OnResponseSelected(int index)
    {
        var selected = currentNode.Responses[index];
        smsUI.pcType(selected.Text);
        currentNode = selected.NextNode;

        if (currentNode.AutoAdvance)
        {
            ContinueDialogue();
        }
        else
        {
            ShowCurrentNode();
        }
    }

    public void ContinueDialogue()
    {
        if (currentNode.NpcText != null && currentNode.NpcText.Count > 0)
        { 
            smsUI.npcType(currentNode.NpcText);
        }

        if (currentNode.Responses != null && currentNode.Responses.Count > 0)
        {
            smsUI.showResponses(currentNode.GetResponseTexts());
        }
    }
}
