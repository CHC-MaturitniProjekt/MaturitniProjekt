using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject objToSpawn;

    private Firebase firebase;

    private void Start()
    {
        firebase = FindFirstObjectByType<Firebase>();
    }

    public async void SpawnObject()
    {
        if ((await firebase.GetPlayerMoney() - 10) >= 0)
        {
            await firebase.SubtractPlayerMoney(10);
        }        

        Vector3 randomVector = new Vector3(0, 0, Random.Range(-0.1f, 0.1f));
        
        Instantiate(objToSpawn, spawnPoint.position + randomVector, spawnPoint.rotation);
    }
}
