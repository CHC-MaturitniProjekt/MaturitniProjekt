using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;
    
    private Dictionary<string, Transform> waypoints = new Dictionary<string, Transform>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void RegisterWaypoint(string waypointName, Transform waypointTransform)
    {
        if (!waypoints.ContainsKey(waypointName))
        {
            waypoints.Add(waypointName, waypointTransform);
        }
    }

    public Transform GetWaypoint(string waypointName)
    {
        waypoints.TryGetValue(waypointName, out Transform waypoint);
        return waypoint;
    }
}