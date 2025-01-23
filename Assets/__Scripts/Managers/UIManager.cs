using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.Playables;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private QuestItem questPrefab;
    
    [SerializeField]
    private NotificationManager notificationManager;

    private List<string> notifList = new List<string>();
    private int lastProcessedQuestIndex = 0;
    private int lastProcessedNotifIndex = 0;

    private bool isRunningQuests = false;
    private bool isPinned = false;
    private bool isRunningNotifs = false;
    
    private QuestManager questManager;
    private List<ParsedQuestModel> questList;

    void Start()
    {
        //AddQuest("Promluv si s kamarádem");
        
        AddNotification("Vítej");
        
        questManager = FindFirstObjectByType<QuestManager>();
        questList = questManager.GetQuestList();
    }

    public void AddQuest(string questId)
    {
        string questName = "";
        foreach (var quest in questList)
        {
            if(quest.GUID == questId)
            {
                questName = quest.QuestName;
            }
        }
        
        if (!isRunningQuests && !isPinned)
        {
            StartCoroutine(RunQuests(questName));
        }
    }

    private IEnumerator RunQuests(string questName)
    {
        isRunningQuests = true;
    
        while (lastProcessedQuestIndex < questList.Count)
        {
            lastProcessedQuestIndex++;
            Debug.Log(questList.Count + " " + lastProcessedNotifIndex);
            
            questPrefab.defaultState = QuestItem.DefaultState.Expanded;
            questPrefab.questText = questName;
            questPrefab.UpdateUI();
            
            questPrefab.AnimateQuest();
            questPrefab.ExpandQuest();
            yield return new WaitForSeconds(3);

            questPrefab.MinimizeQuest();
            yield return new WaitForSeconds(1);
        }

        isRunningQuests = false;
    }
    
    public void PinQuest(string questId)
    {
        isPinned = true;
        string questName = "";
        foreach (var quest in questList)
        {
            if(quest.GUID == questId)
            {
                questName = quest.QuestName;
                break;
            }
        }
        
        questPrefab.defaultState = QuestItem.DefaultState.Expanded;
        questPrefab.questText = questName;
        questPrefab.UpdateUI();
        questPrefab.AnimateQuest();
        questPrefab.ExpandQuest();
    }

    public void UnPinQuest()
    {
        questPrefab.MinimizeQuest();
        isPinned = false;
    }
    
    public void AddNotification(string notifText)
    {
        notifList.Add(notifText);
        Debug.Log(notifText);
        //if (!isRunningNotifs)
        //{
            StartCoroutine(RunNotifications());
        //}
    }

    private IEnumerator RunNotifications()
    {
        isRunningNotifs = true;

        while (lastProcessedNotifIndex < notifList.Count)
        {
            string notif = notifList[lastProcessedNotifIndex];
            lastProcessedNotifIndex++;

            notificationManager.defaultState = NotificationManager.DefaultState.Expanded;
            notificationManager.notificationText = notif;
            notificationManager.UpdateUI();
            
            notificationManager.ExpandNotification();
            yield return new WaitForSeconds(3);
            
            notificationManager.MinimizeNotification();
            yield return new WaitForSeconds(1);
        }

        isRunningNotifs = false;
        Debug.Log(isRunningNotifs);
    }
}