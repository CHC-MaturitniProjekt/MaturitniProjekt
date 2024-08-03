using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, Inputs.IMainActions
{
    Inputs _inputs;

    private void OnEnable()
    {
        if (_inputs == null)
        {
            _inputs = new Inputs();
            _inputs.Main.SetCallbacks(this);
            _inputs.Main.Enable();
        }
    }

    private void OnDisable()
    {
        _inputs.Main.Disable();
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
        if (context.started)
            InteractEvent?.Invoke();
    }

    public event Action JumpEvent;
    public void OnJump(InputAction.CallbackContext context)
    {
        JumpEvent?.Invoke();
    }
}
