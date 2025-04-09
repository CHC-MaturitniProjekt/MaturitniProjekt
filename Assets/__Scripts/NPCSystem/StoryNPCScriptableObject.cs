using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCStuff", menuName = "ScriptableObjects/StoryNPCInfo", order = 1)]
[Serializable]
public class StoryNPCScriptableObject : ScriptableObject
{
    public enum NPCBehaviourTypes
    {
        Homeless,
        Fancy,
        Stationary
    }
    public enum NPCDialogueTypes
    {
        Nyx,
        Eliot,
        Quan
    }
    public string NPCName;
    public NPCBehaviourTypes NPCBehaviourType;
    public NPCDialogueTypes NPCDialogueType;
    public List<string> NPCWayPointNames = new List<string>();
    public int NPCId;

    public List<string> GenericDialogues = new List<string>();
}
