using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class NPCInteract : InteractAction
{
    [SerializeField] private List<NPCDialogueSO> dialogueSets;
    private NPCBrain npcBrain;
    private NPCScriptableObject NPCSO;

    private void Start()
    {
        npcBrain = GetComponent<NPCBrain>();
        NPCSO = npcBrain.GetNPCSO();
    }

    public override void OnInteract()
    {
        Debug.Log("Interacting with NPC");
        SelectDialogueType();

        NPCBrain npcBrain = this.GetComponent<NPCBrain>();
        if (npcBrain != null)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.LookAtPlayer);
        }
    }

    public override void OnObjectiveInteract() {}

    private void SelectDialogueType()
    {
        NPCDialogueSO selectedDialogueSet = null;

        switch (NPCSO.NPCDialogueType)
        {
            case NPCScriptableObject.NPCDialogueTypes.Homeless:
                selectedDialogueSet = dialogueSets[0];
                break;
            case NPCScriptableObject.NPCDialogueTypes.Nyx:
                selectedDialogueSet = dialogueSets[1];
                break;
            case NPCScriptableObject.NPCDialogueTypes.Quan:
                selectedDialogueSet = dialogueSets[2];
                break;
            case NPCScriptableObject.NPCDialogueTypes.Eliot:
                selectedDialogueSet = dialogueSets[3];
                break;
            //doplnit zbytek        - mozna upravit rozrazovani
            default:
                Debug.LogError("Unknown NPC type");
                return;
        }

        if (selectedDialogueSet != null)
        {
            string selectedDialogue = SelectDialogue(selectedDialogueSet.dialogues);
            DialogueManager.StartConversation(selectedDialogue, this.gameObject.transform);
        }
    }

    private string SelectDialogue(List<string> dialogues)
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogError("No dialogues available");
            return string.Empty;
        }

        int randomDialogueIndex = UnityEngine.Random.Range(0, dialogues.Count);
        return dialogues[randomDialogueIndex];
    }
}