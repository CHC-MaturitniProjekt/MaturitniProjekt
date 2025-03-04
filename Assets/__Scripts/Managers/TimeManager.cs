using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float worldTime;
    [SerializeField] private float timeMultiplier = 0.5f;
    
    private GameObject timeManager;
    
    private void Awake()
    {
        timeManager = this.gameObject;
    }

    private void Start()
    {
        DontDestroyOnLoad(timeManager);
        
        SetWorldTime(480f);
    }

    public void SetWorldTime(float time)
    {
        worldTime = time;
    }

    public int GetWorldTime()
    {
        return (int)worldTime;
    }

    public string GetDisplayTime(float time)
    {
        int hours;
        int minutes;
        string displayTime = "";

        hours = (int)time / 60;
        minutes = (int)time % 60;

        displayTime = hours.ToString("D2") + ":" + minutes.ToString("D2");
        
        return displayTime;
    }

    void Update()
    {
        worldTime += Time.deltaTime * timeMultiplier;

        if (worldTime >= 1440f) worldTime = 0;
    }
}
