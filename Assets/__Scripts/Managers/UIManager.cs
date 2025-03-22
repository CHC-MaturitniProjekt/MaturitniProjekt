using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Heat;
using PixelCrushers.DialogueSystem;
using TMPro;
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
    private List<string> questList = new List<string>();
    private int lastProcessedQuestIndex = 0;
    private int lastProcessedNotifIndex = 0;

    private bool isRunningQuests = false;
    private bool isRunningNotifs = false;
    
    [SerializeField] private TextMeshProUGUI timeDisplay;
    private TimeManager timeManager;
    [SerializeField] private TextMeshProUGUI moneyDisplay;
    private Firebase firebase;

    void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
        if (timeManager != null) timeDisplay.gameObject.SetActive(true);
        
        firebase = FindFirstObjectByType<Firebase>();
        if (firebase != null)
        {
            moneyDisplay.gameObject.SetActive(true);
            DisplayGameMoney();
        }

        lastProcessedNotifIndex = 0;
        lastProcessedQuestIndex = 0;
    }

    private void Update()
    {
        DisplayGameTime();
    }

    public void AddQuest(string questText)
    {
        if (!isRunningQuests && questText != null)
        {
            questList.Add(questText);
            StartCoroutine(RunQuests());
        }
    }

    private IEnumerator RunQuests()
    {
        isRunningQuests = true;
        while (lastProcessedQuestIndex < questList.Count)
        {
            string questName = questList[lastProcessedQuestIndex];
            lastProcessedQuestIndex++;
            
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
    
    public void AddNotification(string notifText)
    {
        if (notifText == null) return;
        
        notifList.Add(notifText);
        StartCoroutine(RunNotifications());
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
    }

    private void DisplayGameTime()
    {
        timeDisplay.text = timeManager.GetDisplayTime(timeManager.GetWorldTime());
    }

    private async void DisplayGameMoney()
    {
        int playerMoney = await firebase.GetPlayerMoney();
        moneyDisplay.text = string.Format("{0:N0} Ħ", playerMoney);
    }

    public void UpdateGameMoney(int amount)
    {
        moneyDisplay.text = string.Format("{0:N0} Ħ", amount);
    }
}