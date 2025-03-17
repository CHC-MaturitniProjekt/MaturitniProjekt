using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public string waypointName;

    private void Start()
    {
        WaypointManager.Instance.RegisterWaypoint(waypointName, transform);
    }
}
