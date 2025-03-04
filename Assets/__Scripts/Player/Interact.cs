using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;

public class Interact : MonoBehaviour
{
    [Header("Interact System")]
    [SerializeField] private InputReader input;
    [SerializeField] private float reachLength;

    [Header("Highlight")]
    [SerializeField] private string NPCTag;
    [SerializeField] private string ItemTag;
    //[SerializeField] private string PCTag;

    [Header("Interact Icon")]
    [SerializeField] private GameObject interGameObject;
    [SerializeField] private TextMeshPro interText;
    [SerializeField] private SpriteRenderer interIcon;
    [SerializeField] private Sprite interIconTalk;
    [SerializeField] private Sprite interIconPickup;

    private Outline currentOutline;
    private string currentHitTag;
    private GameObject selectedObj;
    private GameObject itemHeld;

    private CameraController camController;
    private QuestTrigger questTrigger;

    private QuestManager questManager;
    private List<ParsedQuestModel> questList;

    private Camera cam;
    
    void Start()
    {
        camController = FindFirstObjectByType<CameraController>();
        questTrigger = FindFirstObjectByType<QuestTrigger>();
        questManager = FindFirstObjectByType<QuestManager>();

        input.InteractEvent += OnInteract;

        interGameObject.SetActive(false);

        cam = Camera.main;
    }

    void FixedUpdate()
    {
        Scan();
    }

    public void OnInteract()
    {
        #region TobankoInteract
        //Tobanko
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * reachLength, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reachLength))
        {
            InteractAction interactable = hit.collider.GetComponent<InteractAction>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }
        }
        #endregion

        switch (currentHitTag)
        {
            case var value when value == NPCTag:
                InteractWithNPC();
                break;
            case var value when value == ItemTag:
                InteractWithItem();
                break;
            //case var value when value == PCTag:
            //    InteractWithPC();
            //    break;
            default:
                break;
        }
    }

    private void Scan()
    {
        if (cam == null)
        {
            Debug.LogWarning("no active cam");
            return;
        }
        
        if (camController.isUsingPC)
        {
            HideInteractThing();
            return;
        }

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * reachLength, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reachLength))
        {
            bool interacted = false;

            currentHitTag = hit.collider.tag;
            #region TobankoInteract
            //Tobanko Chujovina
            InteractAction interactable = hit.collider.GetComponent<InteractAction>();
            if (interactable != null)
            {
                interacted = true;
                Outline outline = hit.collider.GetComponent<Outline>();
                selectedObj = hit.collider.gameObject;
                Highlight(outline);
                DisplayInteractThing(interactable.InteractionText, interactable.InteractionIcon, hit);
            }
            #endregion

            if (currentHitTag == NPCTag || currentHitTag == ItemTag )//|| currentHitTag == PCTag)
            {
                interacted = true;
                Outline outline = hit.collider.GetComponent<Outline>();
                selectedObj = hit.collider.gameObject;
                Highlight(outline);

                switch (currentHitTag)
                {
                    case var tag when tag == NPCTag:
                        DisplayInteractThing("Talk", interIconTalk, hit);
                        break;
                    case var tag when tag == ItemTag:
                        DisplayInteractThing("Pick Up", interIconPickup, hit);
                        break;
                    //case var tag when tag == PCTag:
                    //    DisplayInteractThing("Use PC", interIconTalk, hit);
                    //    break;
                    default:
                        break;
                }
            }

            if(!interacted)
            {
                currentHitTag = null;
                ClearHighlight();
                HideInteractThing();
            }
        }
        else
        {
            currentHitTag = null;
            ClearHighlight();
            HideInteractThing();
        }
    }

    void Highlight(Outline outline)
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

        DialogueSystemTrigger dialogueTrigger = selectedObj.GetComponent<DialogueSystemTrigger>();
        if (dialogueTrigger)
        {
            dialogueTrigger.OnUse();
        }
        else
        {
            DialogueManager.StartConversation("New Conversation 1", selectedObj.transform);
        }

        NPCBrain npcBrain = selectedObj.GetComponent<NPCBrain>();
        if (npcBrain != null)
        {
            npcBrain.SetBehavior(NPCBrain.NPCBehavior.LookAtPlayer);
        }
    }

    private void InteractWithItem()
    {
        if (selectedObj)
        {
            itemHeld = selectedObj;
            PlayerManager.Instance.PickUpItem(selectedObj);
        }
    }

    private void InteractWithPC()
    {
        camController.isUsingPC = !camController.isUsingPC;
    }
    
    private void DisplayInteractThing(string text, Sprite icon, RaycastHit hit)
    {
        interGameObject.SetActive(true);
        interText.text = text;
        interIcon.sprite = icon;
        
        interGameObject.transform.position = transform.position + (hit.point - transform.position) * 0.8f + new Vector3(0f, 0, 0);
        interGameObject.transform.rotation = Quaternion.LookRotation(hit.point - transform.position) * Quaternion.Euler(0, -90, -50);
    }

    private void HideInteractThing()
    {
        interGameObject.SetActive(false);
    }

    public GameObject GetCurrentItem()
    {
        return itemHeld;
    }
}
