using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class PcInteract : InteractAction
{
    [Header("Input")]
    [SerializeField] private InputReader input;
    [SerializeField] private Pc pc;
    public override void OnInteract()
    {
        CameraManager.Instance.EnterPcCamera();
        PlayerManager.Instance.isDisabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        pc.StartInteracting();
        input.PcInputEnable();
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }

}