using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

public class EnvironmentInteractionStateMachine : StateManager<EnvironmentInteractionStateMachine.EEnvironmentInteractionState> 
{
    public enum EEnvironmentInteractionState
    {
        Search,
        Approach,
        Rise,
        Reset
    }
    
    private EnvironmentInteractionContext _context;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (_context != null && _context.ClosestPointOnColliderFromShoulder != null)
        {
            Gizmos.DrawSphere(_context.ClosestPointOnColliderFromShoulder, 0.1f);
        }
    }
    
    [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
    [SerializeField] private MultiRotationConstraint _leftHandRotationConstraint;
    [SerializeField] private MultiRotationConstraint _rightHandRotationConstraint;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private CapsuleCollider _rootCollider;
    
    private void Awake()
    {
        _context = new EnvironmentInteractionContext(_leftIkConstraint, _rightIkConstraint, _leftHandRotationConstraint, _rightHandRotationConstraint, _rigidbody, _rootCollider, transform.parent);
        InitializeStates();
        
        _context.ColliderCenterY = 1.7f;
    }

    private void InitializeStates()
    {
        States.Add(EEnvironmentInteractionState.Reset, new ResetState(_context, EEnvironmentInteractionState.Reset));
        States.Add(EEnvironmentInteractionState.Approach, new ApproachState(_context, EEnvironmentInteractionState.Approach));
        States.Add(EEnvironmentInteractionState.Rise, new RiseState(_context, EEnvironmentInteractionState.Rise));
        States.Add(EEnvironmentInteractionState.Search, new SearchState(_context, EEnvironmentInteractionState.Search));
        CurrentState = States[EEnvironmentInteractionState.Reset];
    }
}
