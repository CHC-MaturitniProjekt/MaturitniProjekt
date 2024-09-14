using Cinemachine;
using UnityEngine;

public class Interact : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private InputReader input;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private float reachLength;

    private string tag = "Interactable";
    
    void Start()
    {
        input.InteractEvent += OnInteract;
    }

    public void OnInteract()
    {
        Interacting();
    }

    void Update()
    {

    }

    private void Interacting()
    {
        Ray ray = new Ray(vcam.transform.position, vcam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reachLength) && hit.collider.CompareTag(tag))
        {
            Debug.Log(gameObject);
        }
    }
}
