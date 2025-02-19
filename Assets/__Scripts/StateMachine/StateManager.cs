using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();

    private BaseState<EState> CurrentState;
    private bool IsTransitioningState = false;
    
    private void Start()
    {
        CurrentState.EnterState();    
    }

    private void Update()
    {
        EState nextStateKey = CurrentState.GetNextState();

        if (nextStateKey.Equals(CurrentState.StateKey) && !IsTransitioningState)
        {
            CurrentState.UpdateState();
        }
        else if (!IsTransitioningState)
        {
            TransitionToState(nextStateKey);
        }
    }

    public void TransitionToState(EState stateKey)
    {
        CurrentState.ExitState();
        CurrentState = States[stateKey];
        CurrentState.EnterState();
    }
    
    void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }
    
    void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);
    }
    
    void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }
    
}
