using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    
    [SerializeField]
    private GameObject questPrefab;
    
    [SerializeField]
    private GameObject questObjectivePrefab;
    
    [SerializeField]
    private Transform questPosition;
    
    void Start()
    {
        AddQuest();
    }

    private void AddQuest()
    {
        questPrefab = Instantiate(questPrefab, questPosition);
        questPrefab.transform.SetParent(canvas.transform);
        
    }
}
