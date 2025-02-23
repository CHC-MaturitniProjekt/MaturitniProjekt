using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    public float _approachDistanceTreashold = 5f;
    
    public SearchState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState() {}
    public override void ExitState() {}

    public override void UpdateState() {}
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }
        
        bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position) < _approachDistanceTreashold;
        bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;
        
        if (isClosestPointOnColliderValid && isCloseToTarget)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Approach;
        }
        
        return StateKey;
    }

    public override void OnTriggerEnter(Collider other)
    {
        StartIkTargetPositionTracking(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        UpdateIkTargetPosition(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        ResetIkTargetPositionTracking(other);
    }
    
}