using UnityEngine;

public class QuestObtainHandlerer : MonoBehaviour
{
    public int questID;
    public bool isTriggerEnabled = true;
    private QuestManager questManager;

    void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        if (questManager == null)
        {
            Debug.LogError("QuestManager not found. Please ensure it is added to the scene.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggerEnabled && other.CompareTag("Player"))
        {
            SetQuestAsObtained();
        }
    }

    public async void SetQuestAsObtained()
    {
        if (questManager != null)
        {
            await questManager.ObtainQuest(questID);
            Debug.Log($"Quest {questID} set as obtained.");
        }
    }
}
