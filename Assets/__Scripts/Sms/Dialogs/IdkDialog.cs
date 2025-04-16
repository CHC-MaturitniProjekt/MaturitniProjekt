using PixelCrushers.DialogueSystem;
using System.Collections.Generic;

class IdkDialog : SmsSystem
{
    private QuestManager _questManager;
    
    private void Start()
    {
        _questManager = FindFirstObjectByType<QuestManager>();

        
        DialogNode fifthNode = new DialogNode
        {
            NpcText = new List<string>() {"Don’t finish that sentence unless it ends in numbers.", "Clock’s ticking, Solace. Don't make us come off silent."},
            OnNodeEnter = () =>
            {
                _questManager.ObtainQuest(3);
            }
        };
        DialogNode forthNode = new DialogNode
        {
            NpcText = new List<string>() {"You used to reply faster. You used to sound more grateful."},
            Responses = new List<Response>
            {
                new Response { Text = "I’m not trying to disrespect. I’m just—", NextNode = fifthNode },
            }
        };
        DialogNode thirdNode = new DialogNode
        {
            NpcText = new List<string>() {"Then why do you act like we won’t collect?"},
            Responses = new List<Response>
            {
                new Response { Text = "I’m not. I just don’t have it. Yet.", NextNode = forthNode },
            }
        };
        DialogNode secondNode = new DialogNode
        {
            NpcText = new List<string>() {"Not enough. We’ve covered your cold nights and clean water.", "This is not charity."},
            Responses = new List<Response>
            {
                new Response { Text = "I know.", NextNode = thirdNode },
            }
        };

        DialogNode firstNode = new DialogNode
        {
            NpcText = new List<string>() {"Payment window closes in 48 hours.", "No sign of movement on your end."},
            Responses = new List<Response>
            {
                new Response { Text = "Still working on it.", NextNode = secondNode },
            }
        };
        
        dialogManager.StartDialogue(firstNode);
    }
}