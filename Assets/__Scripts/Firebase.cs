using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Firebase : MonoBehaviour
{
    private UIManager uiManager;
    private QuestManager questManager;
    
    FirebaseConfig config;
    private FirebaseClient client;

    public event Action OnDatabaseInitialized;
    
    void Awake()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        questManager = FindFirstObjectByType<QuestManager>();
        
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found. Please ensure it is added to the scene.");
        }
        if (questManager == null)
        {
            Debug.LogError("UIManager not found. Please ensure it is added to the scene.");
        }
    }
    
    async void Start()
    {
        config = new FirebaseConfig("https://augumentum-default-rtdb.europe-west1.firebasedatabase.app/", "AIzaSyBkWcIDRsasLWlVmu2ZLHIsbu5LVT2-y3U");
        client = new FirebaseClient(config);
        string cachedId = client.LoadCachedLocalId();
        client.setUserId(cachedId);

        OnDatabaseInitialized?.Invoke();
        client.StartListening("/", OnDataChanged);
    }

    public Dictionary<string, ParsedQuestModel> GetQuests()
    {
        FirebaseResponse response = client.GetSync("quests");
        
        return response.ResultAs<Dictionary<string, ParsedQuestModel>>();
    }

    void OnDataChanged(string eventType, string data)
    {
        Debug.Log($"Event: {eventType}, Data: {data}");

        try
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            if (jsonData != null && jsonData.ContainsKey("path"))
            {
                string path = jsonData["path"].ToString();
                if (path.Contains("/quests"))
                {
                    ProcessDataChange(data);
                } 
                else if (path.Contains("/userMoney"))
                {
                    ProcessMoneyChange(data);
                }
            }
            else
            {
                Debug.LogError("Key missing or null in JSON data");
            }
        }
        catch (JsonReaderException ex)
        {
            Debug.LogError("JSON parsing error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error: " + ex.Message);
        }    
    }

    void ProcessMoneyChange(string data)
    {
        try
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            if (jsonData != null && jsonData.ContainsKey("data"))
            {
                object rawData = jsonData["data"];
                int moneyValue = Convert.ToInt32(rawData);
                Debug.Log(moneyValue);
                
            }
        }
        catch (JsonReaderException ex)
        {
            Debug.LogError("JSON parsing error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error: " + ex.Message);
        }
    }
    
    public async Task<int> GetPlayerMoney()
    {
        FirebaseResponse response = await client.GetAsync("userMoney");
        if (response == null || string.IsNullOrEmpty(response.RawJson))
        {
            Debug.LogError("Failed to retrieve player money.");
            return 0;
        }

        return JsonConvert.DeserializeObject<int>(response.RawJson);
    }

    public async Task AddPlayerMoney(int amount)
    {
        int currentMoney = await GetPlayerMoney();
        int newMoney = currentMoney + amount;
        
        uiManager.UpdateGameMoney(newMoney);
        string jsonUpdate = JsonConvert.SerializeObject(newMoney);
        await client.PutAsync("userMoney", jsonUpdate);
    }

    public async Task SubtractPlayerMoney(int amount)
    {
        int currentMoney = await GetPlayerMoney();
        int newMoney = currentMoney - amount;

        if (newMoney < 0)
        {
            Debug.LogError("Not enough money.");
            return;
        }

        uiManager.UpdateGameMoney(newMoney);
        string jsonUpdate = JsonConvert.SerializeObject(newMoney);
        await client.PutAsync("userMoney", jsonUpdate);
    }
    
    void ProcessDataChange(string data)
    {
        try
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            if (jsonData != null && jsonData.ContainsKey("data"))
            {
                object rawData = jsonData["data"];
                if (rawData is JObject dataObject)
                {
                    var dataDict = dataObject.ToObject<Dictionary<string, object>>();
                    ProcessQuestData(dataDict);
                }
                else
                {
                    string path = jsonData.ContainsKey("path") ? jsonData["path"].ToString() : "";
                    ProcessBooleanData(path);
                }
            }
            else
            {
                Debug.LogError("Key missing or null in JSON data");
            }
        }
        catch (JsonReaderException ex)
        {
            Debug.LogError("JSON parsing error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error: " + ex.Message);
        }
    }

    void ProcessQuestData(Dictionary<string, object> dataDict)
    {
        foreach (var item in dataDict)
        {
            string[] parts = item.Key.Split('/');
            if (parts.Length == 2 && parts[1] == "isActive")
            {
                string questId = parts[0];
                questManager.SetQuestAsActive(questId);
                Debug.Log($"Quest {questId} set as active.");
            }
        }
    }

    void ProcessBooleanData(string path)
    {
        string[] parts = path.Split('/');
        string questId = parts[2];
        switch (parts[3])
        {
            case "isActive":
                questManager.SetQuestAsActive(questId);
                Debug.Log($"Quest {questId} set as active.");
                break;
            case "isCompleted":
                questManager.SetQuestAsCompleted(questId);
                Debug.Log($"Quest {questId} set as completed.");
                break;
            case "isObtained":
                questManager.SetQuestAsObtained(questId);
                Debug.Log($"Quest {questId} set as obtained.");
                break;
            default:
                Debug.LogError("Invalid DB query.");
                break;
        }
    }

    public void AddQuest(string guid, string title, string description, List<ObjectiveNodeModel> objectives, List<RewardNodeModel> rewards, List<string?> nextQuests, bool isActive, bool isCompleted, bool isObtained)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        string objectivesJson = JsonConvert.SerializeObject(objectives, settings);
        string rewardsJson = JsonConvert.SerializeObject(rewards, settings);
        string nextQuestsJson = JsonConvert.SerializeObject(nextQuests, settings);
        
        string jsonQuest = $@"{{
            ""QuestName"": ""{title}"",
            ""QuestDescription"": ""{description}"",           
            ""Objectives"": {objectivesJson},
            ""Rewards"": {rewardsJson},
            ""NextQuests"": {nextQuestsJson},
            ""isActive"": {isActive.ToString().ToLower()},
            ""isCompleted"": {isCompleted.ToString().ToLower()},
            ""isObtained"": {isObtained.ToString().ToLower()}
        }}";
        
        client.PutSync($"quests/{guid}", jsonQuest);
    }

    public async Task UpdateObjectiveCompletionStatus(string questGUID, int objectiveIndex, bool isCompleted)
    {
        FirebaseResponse response = client.GetSync($"quests/{questGUID}/Objectives/{objectiveIndex}");
        if (response == null || string.IsNullOrEmpty(response.RawJson))
        {
            Debug.LogError("Failed to retrieve existing objective data.");
            return;
        }

        var objective = JsonConvert.DeserializeObject<ObjectiveNodeModel>(response.RawJson);
        if (objective == null)
        {
            Debug.LogError("Failed to deserialize existing objective data.");
            return;
        }

        objective.isCompleted = isCompleted;
        uiManager.AddNotification("Objective completed: " + objective.ObjectiveDescription);


        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        string jsonUpdate = JsonConvert.SerializeObject(objective, settings);

        client.PutSync($"quests/{questGUID}/Objectives/{objectiveIndex}", jsonUpdate);
        CheckQuestsObjectivesCompletion(questGUID);
    }
    
    public async Task QuestObtain(string questGUID)
    {
        FirebaseResponse response = await client.GetAsync($"quests/{questGUID}/");
        if (response == null || string.IsNullOrEmpty(response.RawJson))
        {
            Debug.LogError("Failed to retrieve existing objective data.");
            return;
        }

        var quest = JsonConvert.DeserializeObject<ParsedQuestModel>(response.RawJson);
        if (quest == null)
        {
            Debug.LogError("Failed to deserialize existing objective data.");
            return;
        }

        quest.isObtained = true;
        uiManager.AddQuest("New quest: " + quest.QuestName);

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        string jsonUpdate = JsonConvert.SerializeObject(quest, settings);

        await client.PutAsync($"quests/{questGUID}/", jsonUpdate);
        await CheckQuestsObjectivesCompletion(questGUID);
    }
    
    public async Task CheckQuestsObjectivesCompletion(string questGUID)
    {
        FirebaseResponse response = client.GetSync($"quests/{questGUID}");
        if (response == null || string.IsNullOrEmpty(response.RawJson))
        {
            Debug.LogError("Failed to retrieve existing quest data.");
            return;
        }

        var quest = JsonConvert.DeserializeObject<ParsedQuestModel>(response.RawJson);
        if (quest == null)
        {
            Debug.LogError("Failed to deserialize existing quest data.");
            return;
        }

        if (quest.Objectives.All(obj => obj.isCompleted) && !quest.isCompleted)
        {
            quest.isCompleted = true;
            quest.isActive = false;
            
            uiManager.AddNotification("Quest completed: " + quest.QuestName);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonUpdate = JsonConvert.SerializeObject(quest, settings);

            client.PutSync($"quests/{questGUID}", jsonUpdate);
        }
    }

    public async Task<bool> CheckQuest(string questGUID)
    {
        bool isAdded = false;

        FirebaseResponse response = client.GetSync("quests");
        if (response == null || string.IsNullOrEmpty(response.RawJson))
        {
            return isAdded;
        }

        Dictionary<string, ParsedQuestModel> quests = response.ResultAs<Dictionary<string, ParsedQuestModel>>();
        if (quests == null)
        {
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
}