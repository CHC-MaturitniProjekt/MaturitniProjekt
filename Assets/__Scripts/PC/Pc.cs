using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pc : MonoBehaviour
{
    [SerializeField] private InputReader input;
    public bool isInteracting = false;
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
