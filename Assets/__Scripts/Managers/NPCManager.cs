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

    private void Update()
    {
        float currentHour = timeManager.GetWorldTime()/60f;
        
        if (currentHour >= 6f && currentHour < 21f)
        {
            EnableNPCS();
        }
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

    private void EnableNPCS()
    {
        foreach (var npc in allNPCs)
        {
            if (npc != null && !npc.activeSelf)
            {
                npc.SetActive(true);
                npc.GetComponent<NavMeshAgent>().enabled = true;
                npc.GetComponent<NPCBrain>().SetBehavior(NPCBrain.NPCBehavior.Wander);
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
}