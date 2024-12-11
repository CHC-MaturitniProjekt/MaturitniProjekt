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

    private List<string> questList = new List<string>();
    private List<string> notifList = new List<string>();
    private int lastProcessedQuestIndex = 0;
    private int lastProcessedNotifIndex = 0;

    private bool isRunningQuests = false;
    private bool isRunningNotifs = false;

    void Start()
    {
        AddQuest("Analne potesit tobana");
        AddQuest("Vypij 5 piv");
        AddQuest("Rimming s babickou");
        
        AddNotification("Prijata nova quest");
        AddNotification("Prijata nova quest");
    }

    public void AddQuest(string newQuest)
    {
        questList.Add(newQuest);

        if (!isRunningQuests)
        {
            StartCoroutine(RunQuests());
        }
    }

    private IEnumerator RunQuests()
    {
        isRunningQuests = true;
    
        while (lastProcessedQuestIndex < questList.Count)
        {
            string quest = questList[lastProcessedQuestIndex];
            lastProcessedQuestIndex++;

            questPrefab.defaultState = QuestItem.DefaultState.Expanded;
            questPrefab.questText = quest;
            questPrefab.UpdateUI();
            
            questPrefab.AnimateQuest();
            questPrefab.ExpandQuest();
            yield return new WaitForSeconds(3);

            questPrefab.MinimizeQuest();
            yield return new WaitForSeconds(1);
        }

        isRunningQuests = false; // All quests are processed
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