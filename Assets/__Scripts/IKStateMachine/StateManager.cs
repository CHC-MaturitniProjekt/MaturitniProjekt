using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    protected BaseState<EState> CurrentState;
    
    private bool IsTransitioningState = false;
    
    private void Start()
    {
        CurrentState.EnterState();    
    }

    private void Update()
    {
        EState nextState = CurrentState.GetNextState();

        if (!IsTransitioningState && nextState.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }
        else if (!IsTransitioningState)
        {
            TransitionToState(nextState);
        }
    }

    public void TransitionToState(EState stateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = States[stateKey];
        CurrentState.EnterState();
        IsTransitioningState = false;
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
