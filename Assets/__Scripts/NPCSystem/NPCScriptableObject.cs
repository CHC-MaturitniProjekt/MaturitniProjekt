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
        Stationary,
        Quan,
        Elliot
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
    public bool storyImportant;
    public NPCBehaviourTypes NPCBehaviourType;
    public NPCDialogueTypes NPCDialogueType;
    public List<string> NPCWayPointNames = new List<string>();
    public List<string> NPCCompletedDialogues = new List<string>();
    public AnimationCurve NPCActiveTimeCurve;
    [Range(0f, 1f)] public float NPCRandomness;

    [Header("Quan and Elliot settings")] 
    public bool isObtained;
    public bool hasDailySchedule;
    [Range(0f, 24f)] public float activeHourStart = 10f;
    [Range(0f, 24f)] public float activeHourEnd = 18f;

}
