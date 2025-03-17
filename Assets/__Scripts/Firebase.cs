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
    private UIManager uiManager;
    private QuestManager questManager;
    FirebaseConfig config;
    private FirebaseClient client;

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

        try
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
            if (jsonData != null && jsonData.ContainsKey("data"))
            {
                var dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData["data"].ToString());
                foreach (var item in dataDict)
                {
                    string[] parts = item.Key.Split('/');
                    if (parts.Length == 2 && parts[1] == "isActive")
                    {
                        Debug.Log("aaaaaaaaaaaaaaaaaaa");
                        string questId = parts[0];
                        bool isActive = Convert.ToBoolean(item.Value);
                        
                        questManager.SetQuestAsActive(questId);
                        
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
        catch (JsonReaderException ex)
        {
            Debug.LogError("JSON parsing error: " + ex.Message);
        }
        
    }       //TODO: pridat check jestli jsou vsechny objectives splnene
    // TODO: pridat parametr isObtained, podle toho se bude zobrazovat na UI
    //TODO: v custom funkcich zmenit z pridani questu na nastaveni isObtained
    //TODO: predelat updateObjective at je dynamicky - pokusit se
    
    public async void AddQuest(string guid, string title, string description, List<ObjectiveNodeModel> objectives, List<RewardNodeModel> rewards, List<string?> nextQuests, bool isActive, bool isCompleted, bool isObtained)
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
    
    public async void UpdateObjectiveCompletionStatus(string questGUID, int objectiveIndex, bool isCompleted)
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

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        string jsonUpdate = JsonConvert.SerializeObject(objective, settings);

        client.PutSync($"quests/{questGUID}/Objectives/{objectiveIndex}", jsonUpdate);
        CheckQuestsObjectivesCompletion(questGUID);
    }
    
    public async void QuestObtain(string questGUID)
    {
        FirebaseResponse response = client.GetSync($"quests/{questGUID}/");
        if (response == null || string.IsNullOrEmpty(response.RawJson))
        {
            Debug.LogError("Failed to retrieve existing objective data.");
            return;
        }

        var objective = JsonConvert.DeserializeObject<ParsedQuestModel>(response.RawJson);
        if (objective == null)
        {
            Debug.LogError("Failed to deserialize existing objective data.");
            return;
        }

        objective.isObtained = true;

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        string jsonUpdate = JsonConvert.SerializeObject(objective, settings);

        client.PutSync($"quests/{questGUID}/", jsonUpdate);
        CheckQuestsObjectivesCompletion(questGUID);
    }

    public async void CheckQuestsObjectivesCompletion(string questGUID)
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

        if (quest.Objectives.All(obj => obj.isCompleted))
        {
            quest.isCompleted = true;
            quest.isActive = false;

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