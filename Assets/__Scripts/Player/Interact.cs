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

    private Outline currentOutline;
    private string currentHitTag;
    private GameObject selectedObj;

    void Start()
    {
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
            default:
                Debug.Log("nn");
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

            if (currentHitTag == NPCTag || currentHitTag == ItemTag)
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
        if (currentHitTag == NPCTag || currentHitTag == ItemTag)
        {
            if (currentOutline != outline)
            {
                ClearHighlight();
                
                currentOutline = outline;
                if (currentOutline != null)
                {
                    currentOutline.OutlineWidth = 5f;
                }
                else
                {
                    Debug.Log(currentOutline);
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
    }

    private void InteractWithItem()
    {
        //ziskat item z raycastu
        if (selectedObj)
        {
            PlayerManager.Instance.PickUpItem(selectedObj);
        }
    }
}
