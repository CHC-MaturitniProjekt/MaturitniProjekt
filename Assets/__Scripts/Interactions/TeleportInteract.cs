using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TeleportInteract : InteractAction
{
    [SerializeField] private Transform teleportDestination;
    private PlayerManager playerManager;
    private void Start()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    public override void OnInteract()
    {
        StartCoroutine(playerManager.TeleportPlayer(teleportDestination.position, InteractionText));
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }

}