using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueCustomActions : MonoBehaviour
{
    void Start()
    {
        Lua.RegisterFunction("GivePlayerMoney", this, SymbolExtensions.GetMethodInfo(() => SetNPCBehaviour(NPCBrain.NPCBehaviour.FollowPlayer)));
    }

    public void SetNPCBehaviour(NPCBrain.NPCBehaviour behaviour)
    {
        Debug.Log("NPC behaviour set to " + behaviour); 
        NPCBrain npcBrain = GetComponent<NPCBrain>();
        npcBrain.currentBehaviour = behaviour;
    }
}