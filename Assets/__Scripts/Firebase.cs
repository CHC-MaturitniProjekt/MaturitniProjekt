using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Firebase : MonoBehaviour
{


    void Start()
    {
        FirebaseConfig config = new FirebaseConfig("https://augumentum-default-rtdb.europe-west1.firebasedatabase.app/");
        FirebaseClient client = new FirebaseClient(config);

        FirebaseResponse response = client.GetSync("quests");

        Dictionary<string, QuestM> quests = response.ResultAs<Dictionary<string, QuestM>>();
        foreach (var quest in quests)
        {
            Debug.Log($"Title: {quest.Value.title}, Description: {quest.Value.description}, Reward: {quest.Value.reward}");
        }


        client.StartListening("quests", OnDataChanged);
    }

    void OnDataChanged(string eventType, string data)
    {
        Debug.Log($"Event: {eventType}, Data: {data}");
    }

    void Update()
    {

    }
}