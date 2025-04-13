using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    private Dictionary<Transform, GameObject> reservedSeats = new Dictionary<Transform, GameObject>();

    private TimeManager timeManager;
    private List<GameObject> allNPCs = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        timeManager = FindFirstObjectByType<TimeManager>();
    }

    private float lastCheckedHour = -1f;

    private void Update()
    {
        float currentHour = timeManager.GetWorldTime() / 60f;

        if (Mathf.Floor(currentHour) != Mathf.Floor(lastCheckedHour))
        {
            lastCheckedHour = currentHour;
            EnableStandardNPCs(currentHour);
            EnableScheduledNPCs(currentHour);
        }
    }
    
    public List<GameObject> GetAllNPCs()
    {
        return allNPCs;
    }

    public GameObject GetNPCByID(int npcID)
    {
        foreach (var npc in allNPCs)
        {
            if (npc != null && npc.TryGetComponent(out NPCBrain brain))
            {
                if (brain.GetNPCID() == npcID)
                {
                    return npc;
                }
            }
        }
        return null;
    }
    
    public void RegisterNPC(GameObject npc)
    {
        if (!allNPCs.Contains(npc))
            allNPCs.Add(npc);
    }

    public bool IsSeatReserved(Transform seat)
    {
        return reservedSeats.ContainsKey(seat);
    }

    public bool ReserveSeat(Transform seat, GameObject npc)
    {
        if (IsSeatReserved(seat)) return false;
        
        reservedSeats.Add(seat, npc);
        return true;
    }

    public void ReleaseSeat(Transform seat)
    {
        if (reservedSeats.ContainsKey(seat))
        {
            reservedSeats.Remove(seat);
        }
    }

    private void EnableStandardNPCs(float currentHour)
    {
        if (currentHour < 6f || currentHour >= 18f) return;

        foreach (var npc in allNPCs)
        {
            if (npc == null || npc.activeSelf) continue;

            var brain = npc.GetComponent<NPCBrain>();
            if (brain == null) continue;

            var so = brain.GetNPCSO();
            if (so != null && so.storyImportant && so.hasDailySchedule)
            {
                continue;
            }

            npc.SetActive(true);
            if (npc.TryGetComponent(out NavMeshAgent agent))
                agent.enabled = true;

            brain.SetBehavior(NPCBrain.NPCBehavior.Wander);
        }
    }

    private void EnableScheduledNPCs(float currentHour)
    {
        foreach (var npc in allNPCs)
        {
            if (npc == null) continue;

            var brain = npc.GetComponent<NPCBrain>();
            if (brain == null) continue;

            var so = brain.GetNPCSO();
            if (so == null || !so.storyImportant || !so.hasDailySchedule) continue;

            bool shouldBeActive = currentHour >= so.activeHourStart && currentHour <= so.activeHourEnd;

            if (shouldBeActive && !npc.activeSelf)
            {
                brain.SetBehavior(NPCBrain.NPCBehavior.Sit);
                EnableNPC(npc);
            }
        }
    }

    public void DisableNPC(GameObject npc)
    {
        if (npc != null && npc.activeSelf)
        {
            npc.GetComponent<NavMeshAgent>().enabled = false;
            npc.SetActive(false);
        }
    }
    
    public void EnableNPC(GameObject npc)
    {
        if (npc != null && !npc.activeSelf)
        {
            npc.GetComponent<NavMeshAgent>().enabled = true;
            npc.SetActive(true);
        }
    }
}