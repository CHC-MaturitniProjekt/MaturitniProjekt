using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class PcInteract : InteractAction
{
    [Header("Input")]
    [SerializeField] private InputReader input;
    public override void OnInteract()
    {
        Debug.Log("Start");
        CameraManager.Instance.EnterPcCamera();
        PlayerManager.Instance.isDisabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        GetComponent<Pc>().StartInteracting();
        input.PcInputEnable();
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }

}