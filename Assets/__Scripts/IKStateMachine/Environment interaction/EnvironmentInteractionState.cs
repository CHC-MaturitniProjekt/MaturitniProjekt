using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    protected EnvironmentInteractionContext Context;
    private float _movingAwayOffset = 0.2f;
    private bool _shouldReset;
    
    public EnvironmentInteractionState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState stateKey) : base(stateKey)
    {
        Context = context;
    }

    protected bool CheckShouldReset()
    {
        if (_shouldReset)
        {
            Context.LowestDistance = Mathf.Infinity;
            _shouldReset = false;
            return true;
        }
        
        bool isMovingAway = CheckIsMovingAway();
        bool isPlayerJumping = Mathf.Round(Context.Rb.velocity.y) >= 1;
        
        if (isMovingAway || isPlayerJumping)
        {
            Context.LowestDistance = Mathf.Infinity;
            return true;
        }

        return false;
    }

    protected bool CheckIsMovingAway()
    {
        float currentDistanceToTarget = Vector3.Distance(Context.RootTransform.position, Context.ClosestPointOnColliderFromShoulder);
        
        bool isSearchingForNewInteraction = Context.CurrentIntersectingCollider == null;
        if (isSearchingForNewInteraction)
        {
            return false;
        }
        
        bool isGettingCloserToTarget = currentDistanceToTarget <= Context.LowestDistance;
        if (isGettingCloserToTarget)
        {
            Context.LowestDistance = currentDistanceToTarget;
            return false;
        }
        
        bool isMovingAwayFromTarget = currentDistanceToTarget > Context.LowestDistance + _movingAwayOffset;
        if (isMovingAwayFromTarget)
        {
            Context.LowestDistance = Mathf.Infinity;
            return true;
        }

        return false;
    }
    
    private Vector3 GetClosestPointOnCollider(Collider collider, Vector3 point)
    {
        return collider.ClosestPoint(point);
    }

    protected void StartIkTargetPositionTracking(Collider intersectingCollider)
    {
        if (intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Wall") && Context.CurrentIntersectingCollider == null)
        {
            Context.CurrentIntersectingCollider = intersectingCollider;
            Vector3 closestPointFromRoot = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
            Context.SetCurrentSide(closestPointFromRoot);
            
            SetIkTargetPosition();
        }
    }
    
    protected void UpdateIkTargetPosition(Collider intersectingCollider)
    {
        if (intersectingCollider == Context.CurrentIntersectingCollider)
        {
            SetIkTargetPosition();
        }
    }
    
    protected void ResetIkTargetPositionTracking(Collider intersectingCollider)
    {
        if (intersectingCollider == Context.CurrentIntersectingCollider)
        {
            Context.CurrentIntersectingCollider = null;
            Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
            _shouldReset = true;
        }
    }
    
    private void SetIkTargetPosition()
    {
        Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.CurrentIntersectingCollider,
            new Vector3(Context.CurrentShoulderTransform.position.x, Context.CharacterShoulderHeight, Context.CurrentShoulderTransform.position.z));
        
        Vector3 rayDirection = Context.CurrentShoulderTransform.position - Context.ClosestPointOnColliderFromShoulder;
        Vector3 normalizedRayDirection = rayDirection.normalized;
        float offsetDistance = .05f;
        Vector3 offset = normalizedRayDirection * offsetDistance;
        
        Vector3 offsetPositon = Context.ClosestPointOnColliderFromShoulder + offset;
        Context.CurrentIkTargetTransform.position = new Vector3(offsetPositon.x, Context.InteractionPointYOffset, offsetPositon.z);
    }
}
