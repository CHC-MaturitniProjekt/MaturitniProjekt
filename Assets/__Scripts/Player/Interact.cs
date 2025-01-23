using System.Collections.Generic;
using Cinemachine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

public class Interact : MonoBehaviour
{
    [Header("Interact System")]
    [SerializeField] private InputReader input;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private float reachLength;

    [Header("Highlight")]
    [SerializeField] private string NPCTag;
    [SerializeField] private string ItemTag;
    [SerializeField] private string PCTag;

    private Outline currentOutline;
    private string currentHitTag;
    private GameObject selectedObj;
    
    private CameraController camController;
    private QuestTrigger questTrigger;
    
    private QuestManager questManager;
    private List<ParsedQuestModel> questList;

    void Start()
    {
        camController = FindFirstObjectByType<CameraController>();
        
        questTrigger = FindFirstObjectByType<QuestTrigger>();
        questManager = FindFirstObjectByType<QuestManager>();

        input.InteractEvent += OnInteract;
    }

    void Update()
    {
        Scan();
    }

    public void OnInteract()
    {
        switch (currentHitTag)
        {
            case var value when value == NPCTag:
                InteractWithNPC();
                break;
            case var value when value == ItemTag:
                InteractWithItem();
                break;
            case var value when value == PCTag:
                InteractWithPC();
                break;
            default:
                break;
        }
    }

    private void Scan()
    {
        Ray ray = new Ray(vcam.transform.position, vcam.transform.forward);
        Vector3 rayPos = new Vector3(vcam.transform.position.x, vcam.transform.position.y, vcam.transform .position.z);
        Debug.DrawRay(rayPos, vcam.transform.forward, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reachLength))
        {
            currentHitTag = hit.collider.tag;

            if (currentHitTag == NPCTag || currentHitTag == ItemTag || currentHitTag == PCTag)
            {
                Outline outline = hit.collider.GetComponent<Outline>();
                selectedObj = hit.collider.gameObject;
                Highlight(outline); 
            }
            else
            {
                currentHitTag = null;
                ClearHighlight();
            }
        }
        else
        {
            currentHitTag = null;
            ClearHighlight();
        }
    }

    void Highlight(Outline outline)
    {
        if (currentHitTag == NPCTag || currentHitTag == ItemTag || currentHitTag == PCTag)
        {
            if (currentOutline != outline)
            {
                ClearHighlight();
                
                currentOutline = outline;
                if (currentOutline != null)
                {
                    currentOutline.OutlineWidth = 5f;
                }
            }
        }
    }

    void ClearHighlight()
    {
        if (currentOutline != null)
        {
            currentOutline.OutlineWidth = 0f; 
            currentOutline = null;
        }
    }

    private void InteractWithNPC()
    {
        Debug.Log("Interacting with NPC");
        questList = questManager.GetQuestList();
        questTrigger.TriggerQuest(questList[0]);
    }

    private void InteractWithItem()
    {
        if (selectedObj)
        {
            PlayerManager.Instance.PickUpItem(selectedObj);
        }
    }

    private void InteractWithPC()
    {
        camController.isUsingPC = !camController.isUsingPC;
    }
}
