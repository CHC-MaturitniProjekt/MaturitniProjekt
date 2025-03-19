using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject objToSpawn;

    public void SpawnObject()
    {
        Vector3 randomVector = new Vector3(0, 0, Random.Range(-0.1f, 0.1f));
        
        Instantiate(objToSpawn, spawnPoint.position + randomVector, spawnPoint.rotation);
        Debug.Log("spawned");
    }
}
