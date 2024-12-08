using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Heat;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private QuestItem questPrefab;

    private List<string> questList = new List<string>();
    private int lastProcessedQuestIndex = 0;

    private bool isRunningQuests = false;

    void Start()
    {
        AddQuest("Analne potesit tobana");
        AddQuest("Vypij 5 piv");
        AddQuest("Rimming s babickou");
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

            questPrefab.questText = quest;
            questPrefab.UpdateUI();
            questPrefab.AnimateQuest();
            yield return new WaitForSeconds(3);

            questPrefab.MinimizeQuest();
            yield return new WaitForSeconds(1);
        }

        isRunningQuests = false; // All quests are processed
    }
}