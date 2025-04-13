using System;
using System.Collections.Generic;
using UnityEngine;

public class TobberModule : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDistance;

    private Dictionary<string, Type> moduleTypes = new Dictionary<string, Type>
{
    { "KeyPad", typeof(BasicKeypadModule) }
};

    private PuzzleModule currentModule;

    public void setInput(int[] input)
    {
        currentModule.setInput(input);
    }

    public int[] getOutput()
    {
        return currentModule.outputPorts;
    }

    public bool FindModule(string moduleName)
    {
        if (!moduleTypes.ContainsKey(moduleName))
        {
            Debug.LogWarning("Unknown module type: " + moduleName);
            return false;
        }

        Type moduleType = moduleTypes[moduleName];
        PuzzleModule closest = FindClosestComponent(moduleType, transform.position, maxDistance);

        if (closest != null)
        {
            currentModule = closest;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static PuzzleModule FindClosestComponent(Type componentType, Vector3 position, float maxDistance)
    {
        Component[] allComponents = GameObject.FindObjectsByType(componentType, FindObjectsSortMode.None) as Component[];
        PuzzleModule closest = null;
        float minDistance = maxDistance;

        foreach (Component component in allComponents)
        {
            PuzzleModule module = component as PuzzleModule;
            if (module == null) continue;

            float distance = Vector3.Distance(position, module.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = module;
            }
        }

        return closest;
    }


}