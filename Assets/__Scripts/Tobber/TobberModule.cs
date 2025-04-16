using System;
using System.Collections.Generic;
using UnityEngine;

public class TobberModule : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDistance;

    private Dictionary<string, List<Type>> moduleTypes = new()
    {
        { "KeyPad", new List<Type> { typeof(IndexKeypadModule), typeof(BruteforceKeypadModule) } },
        { "NFC", new List<Type> { typeof(ReverseNfcModule), typeof(EncryptedNfcModule) } },
        { "Camera", new List<Type> { typeof(ReactionCameraModule), typeof(FibonacciCameraModule) } },
    };

    private PuzzleModule currentModule;

    public void setInput(int[] input)
    {
        currentModule?.setInput(input);
    }

    public int[] getOutput()
    {
        return currentModule?.outputPorts ?? new int[10];
    }

    public bool FindModule(string moduleName)
    {
        if (!moduleTypes.ContainsKey(moduleName))
        {
            Debug.LogWarning("Unknown module type: " + moduleName);
            return false;
        }

        List<Type> types = moduleTypes[moduleName];
        PuzzleModule closest = null;
        float closestDistance = maxDistance;

        foreach (Type type in types)
        {
            PuzzleModule found = FindClosestComponent(type, transform.position, closestDistance);
            if (found != null)
            {
                float dist = Vector3.Distance(transform.position, found.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = found;
                }
            }
        }

        if (closest != null)
        {
            currentModule = closest;
            return true;
        }

        return false;
    }

    public static PuzzleModule FindClosestComponent(Type componentType, Vector3 position, float maxDistance)
    {
        PuzzleModule[] all = GameObject.FindObjectsOfType(componentType) as PuzzleModule[];
        PuzzleModule closest = null;
        float minDistance = maxDistance;

        foreach (var module in all)
        {
            float dist = Vector3.Distance(position, module.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = module;
            }
        }

        return closest;
    }
}
