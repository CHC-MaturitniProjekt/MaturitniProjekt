using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

public class PickUpInteractionStateMachine : StateManager<PickUpInteractionStateMachine.EPickUpInteractionState> 
{
    public enum EPickUpInteractionState
    {
        PickedUp,
        Down
    }
    
    private PickUpInteractionContext _context;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (_context != null && _context.ClosestPointOnColliderFromShoulder != null)
        {
            Gizmos.DrawSphere(_context.ClosestPointOnColliderFromShoulder, 0.1f);
        }
    }
    
    [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
    [SerializeField] private MultiRotationConstraint _rightHandRotationConstraint;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private CapsuleCollider _rootCollider;
    
    private void Awake()
    {
        _context = new PickUpInteractionContext(_rightIkConstraint, _rightHandRotationConstraint, _rigidbody, _rootCollider, transform.parent);
        InitializeStates();
        
    }

    private void InitializeStates()
    {
        States.Add(EPickUpInteractionState.PickedUp, new PickedUpState(_context, EPickUpInteractionState.PickedUp));
        States.Add(EPickUpInteractionState.Down, new DownState(_context, EPickUpInteractionState.Down));
        CurrentState = States[EPickUpInteractionState.Down];
    }
}
