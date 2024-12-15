using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Firebase : MonoBehaviour
{
    UIManager uiManager;
    FirebaseConfig config;
    private FirebaseClient client;

    void Awake()
    {
        uiManager = FindAnyObjectByType<UIManager>();
    }
    
    void Start()
    {
        config = new FirebaseConfig("https://augumentum-default-rtdb.europe-west1.firebasedatabase.app/");
        client = new FirebaseClient(config);

        FirebaseResponse response = client.GetSync("quests");

        Dictionary<string, QuestM> quests = response.ResultAs<Dictionary<string, QuestM>>();
        if (quests != null)
        {
            foreach (var quest in quests) 
            {
                Debug.Log($"Title: {quest.Value.title}, Description: {quest.Value.description}, Reward: {quest.Value.reward}"); 
            }
        }
        
       //client.StartListening("quests", OnDataChanged);
        client.StartListening("upgrades", OnStatsChange);
        //client.StartListening("quests", OnQuestsChange);
    }

    void OnDataChanged(string eventType, string data)
    {
        Debug.Log($"Event: {eventType}, Data: {data}");
    }

    void OnStatsChange(string eventType, string data)
    {
        Debug.Log($"Event: {eventType}, Data: {data}");
        uiManager.AddNotification("Stats changed");
       
    }
    
    void OnQuestsChange(string eventType, string data)
    {
        Debug.Log($"Event: {eventType}, Data: {data}");
        data.Split('"');
        string title = data.Split('"')[3];
        uiManager.AddQuest(title);
       
    }
    
    public async void AddQuest(string title, string description, string reward)
    {
        string jsonQuest = $@"{{
            ""title"": ""{title}"",
            ""description"": ""{description}"",
            ""reward"": ""{reward}"",
            ""criteria"": {{
                ""name"": {{
                    ""0"": 0,
                    ""5"": 5
                }}
            }}
        }}";
        FirebaseResponse response = await client.PostAsync("quests", jsonQuest);
        uiManager.AddQuest(title);
        if (response != null && !string.IsNullOrEmpty(response.RawJson))
        {
            Debug.Log($"Quest added successfully: {response.RawJson}");
        }
        else
        {
            Debug.LogError("Failed to add quest. Response is null or empty.");
        }
    }


    void Update()
    {

    }
}