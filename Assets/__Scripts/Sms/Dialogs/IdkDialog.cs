using PixelCrushers.DialogueSystem;
using System.Collections.Generic;

class IdkDialog : SmsSystem {
    private void Start()
    {
        DialogNode endNode = new DialogNode { NpcText = "Ne", Responses = new List<Response>() };

        DialogNode secondNode = new DialogNode
        {
            NpcText = "Test",
            Responses = new List<Response>
        {
            new Response { Text = "gay", NextNode = endNode },
            new Response { Text = "To není moje starost", NextNode = endNode }
        }
        };

        DialogNode firstNode = new DialogNode
        {
            NpcText = "Ahoj, jak se máš?",
            Responses = new List<Response>
        {
            new Response { Text = "Dobře, co ty?", NextNode = secondNode },
            new Response { Text = "Nic moc", NextNode = secondNode }
        }
        };

        dialogManager.StartDialogue(firstNode);
    }
}
