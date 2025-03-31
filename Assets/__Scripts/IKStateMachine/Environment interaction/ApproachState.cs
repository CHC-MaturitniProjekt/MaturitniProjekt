using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachState : EnvironmentInteractionState
{
    float _elapsedTime;
    float _lerpDuration = 1.0f;
    float _approachWeight = 0.5f;
    float _approachRotationWeight = 0.75f;
    float _rotationSpeed = 500.0f;
    float _riseDistanceTreashold = 1.2f;
    
    public ApproachState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        _elapsedTime = 0.0f;
    }
    public override void ExitState() {}

    public override void UpdateState()
    {
        Quaternion expectedGroundRotation = Quaternion.LookRotation(-Vector3.up, Context.RootTransform.forward);

        Context.CurrentIkTargetTransform.rotation =
            Quaternion.RotateTowards(Context.CurrentIkTargetTransform.rotation, expectedGroundRotation, _rotationSpeed * Time.deltaTime);
        
        _elapsedTime += Time.deltaTime;
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(Context.CurrentMultiRotationConstraint.weight, _approachRotationWeight, _elapsedTime / _lerpDuration);
        Context.CurrentIkConstraint.weight = Mathf.Lerp(Context.CurrentIkConstraint.weight, _approachWeight, _elapsedTime / _lerpDuration);
    }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if ( CheckShouldReset())
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }
        
        bool isWithinArmsReach = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.CurrentShoulderTransform.position) < _riseDistanceTreashold;
        bool isClosesetPointOnColliderReal = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;
        
        if (isClosesetPointOnColliderReal && isWithinArmsReach)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Rise;
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