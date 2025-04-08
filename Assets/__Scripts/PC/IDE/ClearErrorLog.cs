using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearErrorLog : MonoBehaviour
{
    [SerializeField] private GameObject logParent;
    public void clearConsoleLogs()
    {
        foreach (Transform child in logParent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
