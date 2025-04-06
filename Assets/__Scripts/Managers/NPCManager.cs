using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    private HashSet<GameObject> registeredNPCs = new HashSet<GameObject>();
    private Dictionary<Transform, GameObject> reservedSeats = new Dictionary<Transform, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
}