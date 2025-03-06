using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCStuff", menuName = "ScriptableObjects/NPCDialogueSet", order = 1)]
[Serializable]
public class NPCDialogueSO : ScriptableObject
{
    public List<string> dialogues = new List<string>();

}
