using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingInteract : InteractAction
{
    private VendingMachine vendingMachine;

    private void Start()
    {
        vendingMachine = FindFirstObjectByType<VendingMachine>();
    }

    public override void OnInteract()
    {
        vendingMachine.SpawnObject();
        Debug.Log("spawn");
    }
    
    public override void OnObjectiveInteract() {}

}