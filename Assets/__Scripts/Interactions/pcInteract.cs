using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pcInteract : InteractAction
{
    public override void OnInteract()
    {
        Debug.Log("PC");
    }
    
    public override void OnObjectiveInteract() {}

}