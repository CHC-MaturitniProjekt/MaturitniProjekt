using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StripLockedTeleport : InteractAction
{
    [SerializeField] private Transform teleportDestination;
    public string startInteractionText;
    private PlayerManager playerManager;
    public bool locked = true;

    public void Unlock()
    {
        locked = false;
        InteractionText = startInteractionText;
    }
    private void Start()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    public override void OnInteract()
    {
        if (!locked)
            StartCoroutine(playerManager.TeleportPlayerWithTransition(teleportDestination.position, InteractionText));
        else
            InteractionText = "Locked";
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }

}