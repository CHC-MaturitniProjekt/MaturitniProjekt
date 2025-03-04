using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCStuff", menuName = "ScriptableObjects/NPCInfo", order = 1)]
[Serializable]
public class NPCScriptableObject : ScriptableObject
{
    public enum NPCTypes
    {
        Homeless,
        Fancy,
        Basic,
        Stationary
    }
    public string NPCName;
    public NPCTypes NPCType;
    public List<string> NPCWayPointNames = new List<string>();
    public int NPCId;

}
