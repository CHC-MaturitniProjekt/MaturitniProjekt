using PixelCrushers.DialogueSystem;
using System.Collections.Generic;

class NyxDialog : SmsSystem {
    private void Start()
    {
        DialogNode endNode = new DialogNode { NpcText = new List<string>() {"Díky, měj se!"}, Responses = new List<Response>() };

        DialogNode secondNode = new DialogNode
        {
            NpcText = new List<string>() {"To je zajímavé. Co bys udělal dál?"},
            Responses = new List<Response>
        {
            new Response { Text = "Zamyslím se nad tím", NextNode = endNode },
            new Response { Text = "To není moje starost", NextNode = endNode }
        }
        };

        DialogNode firstNode = new DialogNode
        {
            NpcText = new List<string>() {"Ahoj, jak se máš?"},
            Responses = new List<Response>
        {
            new Response { Text = "Dobře, co ty?", NextNode = secondNode },
            new Response { Text = "Nic moc", NextNode = secondNode }
        }
        };

        dialogManager.StartDialogue(firstNode);
    }
}
