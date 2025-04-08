using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCStuff", menuName = "ScriptableObjects/NPCInfo", order = 1)]
[Serializable]
public class NPCScriptableObject : ScriptableObject
{
    public enum NPCBehaviourTypes
    {
        Homeless,
        Fancy,
        Basic,
        Stationary
    }
    public enum NPCDialogueTypes
    {
        Nyx,
        Eliot,
        Quan,
        Pharmacy,
        Bank,
        Electronics,
        Market,
        Homeless,
        Generic
    }
    public string NPCName;
    public NPCBehaviourTypes NPCBehaviourType;
    public NPCDialogueTypes NPCDialogueType;
    public List<string> NPCWayPointNames = new List<string>();
    public List<string> NPCCompletedDialogues = new List<string>();
    public AnimationCurve NPCActiveTimeCurve;
    public float NPCRandomness; 

}
