using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, Inputs.IMainActions, Inputs.IPCActions
{
    Inputs _inputs;

    private void OnEnable()
    {
        if (_inputs == null)
        {
            _inputs = new Inputs();
            MainInputEnable();
        }
    }

    public void MainInputEnable()
    {
        _inputs.Main.SetCallbacks(this);
        _inputs.Main.Enable();
    }

    public void MainInputDisable()
    {
        _inputs.Main.Disable();
    }

    public void PcInputEnable()
    {
        _inputs.PC.SetCallbacks(this);
        _inputs.PC.Enable();
    }

    public void PcInputDisable()
    {
        _inputs.PC.Disable();
    }

    private void OnDisable()
    {
        MainInputDisable();
    }

    public event Action<Vector2> MoveEvent;

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public event Action PauseEvent;

    public void OnPause(InputAction.CallbackContext context)
    {
        PauseEvent?.Invoke();
    }

    public event Action InteractEvent;
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            InteractEvent?.Invoke();
        }
    }

    public event Action JumpEvent;
    public void OnJump(InputAction.CallbackContext context)
    {
        JumpEvent?.Invoke();
    }

    public event Action<Vector2> LookEvent;
    public void OnLook(InputAction.CallbackContext context)
    {
        LookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public event Action SprintStart;
    public event Action SprintEnd;
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SprintStart?.Invoke();
        }
        if (context.canceled)
        {
            SprintEnd?.Invoke();
        }
    }
    public event Action CrouchEvent;
    public void OnCrouch(InputAction.CallbackContext context)
    {
        CrouchEvent?.Invoke();
    }

    public event Action CamModeEvent;
    public void OnCamMode(InputAction.CallbackContext context)
    {
        CamModeEvent?.Invoke();
    }

    public event Action CamIndexIncrement;
    public void OnCamIndexIncrement(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            CamIndexIncrement.Invoke();
        }
    }

    public event Action CamIndexDecrement;
    public void OnCamIndexDecrement(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            CamIndexDecrement.Invoke();
        }
    }

    public event Action DropEvent;
    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            DropEvent?.Invoke();
        }
    }

    public event Action PcLeftClickStart;
    public event Action PcLeftClickEnd;
    public void OnPcLeftClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PcLeftClickStart?.Invoke();
        }
        if (context.canceled)
        {
            PcLeftClickEnd?.Invoke();
        }
    }

    public event Action PcOnExit;
    public void OnExit(InputAction.CallbackContext context)
    {
        PcOnExit?.Invoke();
    }

    public event Action PcOnStep;
    public void OnStep(InputAction.CallbackContext context)
    {
        if (context.started)
            PcOnStep?.Invoke();
    }
}
