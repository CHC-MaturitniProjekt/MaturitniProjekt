using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found. Please ensure it is added to the scene.");
        }
    }
    
    void Start()
    {
        config = new FirebaseConfig("https://augumentum-default-rtdb.europe-west1.firebasedatabase.app/");
        client = new FirebaseClient(config);

        FirebaseResponse response = client.GetSync("quests");

        Dictionary<string, ParsedQuestModel> quests = response.ResultAs<Dictionary<string, ParsedQuestModel>>();
        
        client.StartListening("upgrades", OnStatsChange);
        client.StartListening("quests", OnQuestsChange);
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

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
      
        FirebaseResponse response = client.GetSync("quests");
        Dictionary<string, ParsedQuestModel> quests = response.ResultAs<Dictionary<string, ParsedQuestModel>>();
        
        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
        if (jsonData != null && jsonData.ContainsKey("data"))
        {
            var dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData["data"].ToString());
            foreach (var item in dataDict)
            {
                string[] parts = item.Key.Split('/');
                if (parts.Length == 2 && parts[1] == "isActive")
                {
                    string questId = parts[0];
                    bool isActive = Convert.ToBoolean(item.Value);
                    
                    if (isActive)
                    {
                        uiManager.PinQuest(questId);
                    }
                    else if (!isActive)
                    {
                        uiManager.UnPinQuest();
                    }
                }
                else
                {
                    string questId = parts[0];
                    uiManager.AddQuest(questId);
                }
            }
        }
    }
    
    public async void AddQuest(string guid, string title, string description, List<ObjectiveNodeModel> objectives, List<RewardNodeModel> rewards, bool isActive)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        string objectivesJson = JsonConvert.SerializeObject(objectives, settings);
        string rewardsJson = JsonConvert.SerializeObject(rewards, settings);

        
        string jsonQuest = $@"{{
            ""QuestName"": ""{title}"",
            ""QuestDescription"": ""{description}"",           
            ""Objectives"": {objectivesJson},
            ""Rewards"": {rewardsJson},
            ""isActive"": {isActive.ToString().ToLower()}
        }}";
        
        
        FirebaseResponse response = client.PutSync($"quests/{guid}", jsonQuest);
        Debug.Log(response); 
    }

    public async Task<bool> CheckQuest(string questGUID)
    {
        bool isAdded = false;
        FirebaseResponse response = client.GetSync("quests");
        Dictionary<string, ParsedQuestModel> quests = response.ResultAs<Dictionary<string, ParsedQuestModel>>();

        if (quests == null)
        {
            Debug.LogError("Quests dictionary is null.");
            return isAdded;
        }

        foreach (var quest in quests)
        {
            if (quest.Key == questGUID)
            {
                isAdded = true;
                break;
            }
        }
        return isAdded;
    }
    
    /*public async Task<bool> CheckQuest(string questGUID)
    {
        FirebaseResponse response = client.GetSync($"quests/{questGUID}");
        Debug.Log(response);
        return false;
    }*/
}