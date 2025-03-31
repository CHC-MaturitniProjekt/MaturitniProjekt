using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PlayerData
{
    public Vector3 playerPosition;
    public string heldObjectName;
    public float timeOfDay;
    public List<ItemData> interactableItems;
    

    public PlayerData(Movement player, PickUp pickUp, TimeManager time, List<ItemInteract> items, CameraController cameraController)
    {
        playerPosition = player.transform.position;
        heldObjectName = pickUp.isHoldingItem ? pickUp.currentItem.name : "";
        timeOfDay = time.GetWorldTime();
        interactableItems = items.Select(item => new ItemData(item.GetItemId(), item.transform.position, item.transform.rotation)).ToList();
    }

    public PlayerData()
    {
        playerPosition = Vector3.zero;
        heldObjectName = "";
        timeOfDay = 480f;
        interactableItems = new List<ItemData>();
    }
}

[Serializable]
public class ItemData
{
    public int itemID;
    public Vector3 position;
    public Quaternion rotation;

    public ItemData(int id, Vector3 pos, Quaternion rot)
    {
        itemID = id;
        position = pos;
        rotation = rot;
    }
}