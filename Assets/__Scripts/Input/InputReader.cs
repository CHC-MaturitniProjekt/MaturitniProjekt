using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, Inputs.IMainActions, Inputs.IPCActions, Inputs.ITobberActions, Inputs.IPlayerActions
{
    Inputs _inputs;

    private void OnEnable()
    {
        if (_inputs == null)
        {
            _inputs = new Inputs();
            PlayerInputEnable();
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

    public void TobberInputEnable()
    {
        _inputs.Tobber.SetCallbacks(this);
        _inputs.Tobber.Enable();
    }

    public void TobberInputDisable()
    {
        _inputs.Tobber.Disable();
    }

    public void PlayerInputEnable()
    {
        _inputs.Player.SetCallbacks(this);
        _inputs.Player.Enable();
    }

    public void PlayerInputDisable()
    {
        _inputs.Player.Disable();
    }

    private void OnDisable()
    {
        MainInputDisable();
        PcInputDisable();
        TobberInputDisable();
        PlayerInputDisable();
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

    //public event Action CamModeEvent;
    //public void OnCamMode(InputAction.CallbackContext context)
    //{
    //    CamModeEvent?.Invoke();
    //}

    //public event Action CamIndexIncrement;
    //public void OnCamIndexIncrement(InputAction.CallbackContext context)
    //{
    //    if (context.phase == InputActionPhase.Performed)
    //    {
    //        CamIndexIncrement.Invoke();
    //    }
    //}

    //public event Action CamIndexDecrement;
    //public void OnCamIndexDecrement(InputAction.CallbackContext context)
    //{
    //    if(context.phase == InputActionPhase.Performed)
    //    {
    //        CamIndexDecrement.Invoke();
    //    }
    //}

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

    public event Action PcRightClickStart;
    public event Action PcRightClickEnd;
    public void OnPcRightClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PcRightClickStart?.Invoke();
        }
        if (context.canceled)
        {
            PcRightClickEnd?.Invoke();
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

    public event Action PcOnUndo;
    public void OnUndo(InputAction.CallbackContext context)
    {
        PcOnUndo?.Invoke();
    }

    public event Action PcOnRedo;
    public void OnRedo(InputAction.CallbackContext context)
    {
        PcOnRedo?.Invoke();
    }

    public event Action TobberOnUp;
    public void OnUp(InputAction.CallbackContext context)
    {
        if (context.started)
            TobberOnUp?.Invoke();
    }

    public event Action TobberOnDown;
    public void OnDown(InputAction.CallbackContext context)
    {
        if (context.started)
            TobberOnDown?.Invoke();
    }

    public event Action TobberOnEnter;
    public void OnEnter(InputAction.CallbackContext context)
    {
        if (context.started)
            TobberOnEnter?.Invoke();
    }

    public event Action TobberOnBack;
    public void OnBack(InputAction.CallbackContext context)
    {
        if (context.started)
            TobberOnBack?.Invoke();
    }

    public event Action onTobber;

    public void OnTobberDisplay(InputAction.CallbackContext context)
    {
        if (context.started)
            onTobber?.Invoke();
    }
}
