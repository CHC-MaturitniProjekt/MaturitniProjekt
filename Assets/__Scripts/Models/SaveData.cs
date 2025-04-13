using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string heldObjectName;
    public float timeOfDay;
    public List<ItemData> interactableItems;
    public List<NPCData> npcs;

    public SaveData(Movement player, PickUp pickUp, TimeManager time, List<ItemInteract> items, CameraController cameraController, List<NPCData> npcData)
    {
        playerPosition = player.transform.position;
        heldObjectName = pickUp.isHoldingItem ? pickUp.currentItem.name : "";
        timeOfDay = time.GetWorldTime();
        interactableItems = items.Select(item => new ItemData(item.GetItemId(), item.transform.position, item.transform.rotation)).ToList();
        var allNPCs = NPCManager.Instance.GetAllNPCs();
        npcs = allNPCs
            .Where(npc => npc != null)
            .Select(npc =>
            {
                var npcBrain = npc.GetComponent<NPCBrain>();
                return new NPCData(npcBrain.GetNPCID(), npc.transform.position, npc.transform.rotation, npcBrain.GetCurrentBehavior(), npcBrain.isActiveAndEnabled);
            })
            .ToList();
    }

    public SaveData()
    {
        playerPosition = Vector3.zero;
        heldObjectName = "";
        timeOfDay = 480f;
        interactableItems = new List<ItemData>();
        npcs = new List<NPCData>();
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

[Serializable]
public class NPCData
{
    public int npcID;
    public Vector3 position;
    public Quaternion rotation;
    public NPCBrain.NPCBehavior npcBehavior;
    public bool isActive;
    
    public NPCData(int id, Vector3 pos, Quaternion rot, NPCBrain.NPCBehavior npcBehaviorType, bool isActiveCheck)
    {
        npcID = id;
        position = pos;
        rotation = rot;
        npcBehavior = npcBehaviorType;
        isActive = isActiveCheck;
    }
}