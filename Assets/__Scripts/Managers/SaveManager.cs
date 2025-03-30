using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class SaveManager : MonoBehaviour
{
    private VolumeManager volumeManager;
    private PlayerManager playerManager;
    private Movement playerMovement;
    
    private float playerSprintTime;
    private float playerSprintRecoveryT;
    private PickUp pickUpScript;
    private TimeManager time;
    
    private void Awake()
    {
        pickUpScript = FindFirstObjectByType<PickUp>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        playerMovement = FindFirstObjectByType<Movement>();
        time = FindFirstObjectByType<TimeManager>();
        
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager not found!");
            return;
        }
        
        LoadGame();
    }

    public void SaveGame()
    {
        SaveSystem.SavePlayer(playerMovement, pickUpScript, time);
    }

    public void LoadGame()
    {
        PlayerData data = SaveSystem.LoadPlayer();

        if (data == null)
        {
            Debug.LogError("Failed to load player data.");
            return;
        }

        if (!string.IsNullOrEmpty(data.heldObjectName))
        {
            GameObject heldObject = GameObject.Find(data.heldObjectName);
            if (heldObject != null)
            {
                pickUpScript.CarryItem(heldObject);
            }
            else
            {
                Debug.LogWarning("Held object not found: " + data.heldObjectName);
            }
        }

        time.SetWorldTime(data.timeOfDay);
        playerManager.TeleportPlayer(data.playerPosition);

        var allInteractables = FindObjectsOfType<ItemInteract>(); 
        foreach (var itemData in data.interactableItems)
        {
            var item = allInteractables.FirstOrDefault(i => i.GetItemId() == itemData.itemID);
            if (item != null && item.gameObject != pickUpScript.currentItem) 
            {
                item.transform.position = itemData.position;
                item.transform.rotation = itemData.rotation;
            }
        }
    }

    public void ResetPlayer()
    {
        SaveSystem.ResetPlayer();
    }
    
}