using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }

}