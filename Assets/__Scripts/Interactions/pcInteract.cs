using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PcInteract : InteractAction
{    public override void OnInteract()
    {
        Debug.Log("Start");
        CameraManager.Instance.EnterPcCamera();
        PlayerManager.Instance.isDisabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        GetComponent<Pc>().StartInteracting();
      
    }
}