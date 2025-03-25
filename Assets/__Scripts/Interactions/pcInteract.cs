using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class PcInteract : InteractAction
{    public override void OnInteract()
    {
        Debug.Log("Start");
        CameraManager.Instance.EnterPcCamera();
        PlayerManager.Instance.isDisabled = true;
        Cursor.lockState = CursorLockMode.None;
        GetComponent<Pc>().StartInteracting();
      
    }

    public override Task OnObjectiveInteract()
    {
        return null;
    }

}