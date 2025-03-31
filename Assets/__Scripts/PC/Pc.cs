using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pc : MonoBehaviour
{
    [SerializeField] private InputReader input;
    public bool isInteracting = false;
    public static Pc Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        input.InteractEvent += OnInteractExit;
    }

    public void StartInteracting()
    {
        StartCoroutine(StartInteractingOnDelay());
    }

    private IEnumerator StartInteractingOnDelay()
    {
        yield return new WaitForSeconds(0.5f);
        isInteracting = true;
    }

    private void OnInteractExit()
    {
        if (isInteracting)
        {
            isInteracting = false;
            Cursor.lockState = CursorLockMode.Locked;
            CameraManager.Instance.EnterPlayerCamera();
            PlayerManager.Instance.EnablePlayerWithDelay();
        }
    }
}
