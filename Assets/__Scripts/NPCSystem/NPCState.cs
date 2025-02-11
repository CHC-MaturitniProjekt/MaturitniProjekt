using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState : MonoBehaviour
{
    public enum Emotion { Happy, Sad, Angry, Neutral }
    public Emotion CurrentEmotion { get; private set; }

    public bool IsLookingAtPlayer { get; set; }
    public bool IsRunningAway { get; set; }
    public bool IsOverriden { get; set; }

    public void SetEmotion(Emotion newEmotion) => CurrentEmotion = newEmotion;
}
