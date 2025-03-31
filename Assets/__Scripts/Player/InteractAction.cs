using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class InteractAction : MonoBehaviour
{
    [SerializeField] public Sprite InteractionIcon;
    [SerializeField] public string InteractionText;
    public abstract void OnInteract();
    public abstract Task OnObjectiveInteract();
}
