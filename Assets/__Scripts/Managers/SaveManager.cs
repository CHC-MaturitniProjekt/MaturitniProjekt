using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    private VolumeManager volumeManager;
    private Movement playerMovement;
    private CameraController camController;
    private NPCManager npcManager;
    
    private float playerSprintTime;
    private float playerSprintRecoveryT;
    private PickUp pickUpScript;
    private TimeManager time;

    [Header("Settings")] 
    [SerializeField] private Slider sensitivitySlider;
    
    
    private void Awake()
    {
        camController = FindFirstObjectByType<CameraController>();
        pickUpScript = FindFirstObjectByType<PickUp>();
        playerMovement = FindFirstObjectByType<Movement>();
        time = FindFirstObjectByType<TimeManager>();
        npcManager = FindFirstObjectByType<NPCManager>();
        
        //LoadGame();
    }

    public void SaveGame()
    {
        SaveSystem.SaveData(playerMovement, pickUpScript, time, camController, npcManager);
    }

    public void LoadGame()
    {
        SaveData data = SaveSystem.LoadData();

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
        PlayerManager.Instance.TeleportPlayer(data.playerPosition);

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
        
        foreach (var npcData in data.npcs)
        {
            var npc = npcManager.GetNPCByID(npcData.npcID);
            if (npc != null)
            {
                npc.transform.position = npcData.position;
                npc.transform.rotation = npcData.rotation;
                npc.GetComponent<NPCBrain>().SetBehavior(npcData.npcBehavior);
                if (npcData.isActive)
                {
                    npcManager.EnableNPC(npc);
                }
                else
                {
                    npcManager.DisableNPC(npc);
                }
            }
            else
            {
                Debug.LogWarning("NPC not found: " + npcData.npcID);
            }
        }        
    }

    public void ResetData()
    {
        SaveSystem.ResetData();
    }

    public void SetMouseSensitivity()
    {
        camController.mouseSensitivity = sensitivitySlider.value;
    }
    
}