using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LockedTeleportInteract : InteractAction
{
    [SerializeField] private string InteractionOpenText;
    [SerializeField] private string InteractionCloseText;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private TimeManager timeManager;
    public bool hacked = false;
    private PlayerManager playerManager;
    private bool canEnter = true;
    private void Start()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    public void setHacked()
    {
        hacked = true;
    }

    private void Update()
    {
        if (!hacked)
        {
            if (timeManager.GetWorldTime() < 1080 && timeManager.GetWorldTime() > 480)
            {
                canEnter = true;
                InteractionText = InteractionOpenText;
            }
            else
            {
                InteractionText = InteractionCloseText;
                canEnter = false;
            }
        }
        else
        {
            canEnter = true;
            InteractionText = "Hacked";
        }
    }

    public override void OnInteract()
    {
        if (canEnter)
            StartCoroutine(playerManager.TeleportPlayerWithTransition(teleportDestination.position, InteractionText));
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }

}