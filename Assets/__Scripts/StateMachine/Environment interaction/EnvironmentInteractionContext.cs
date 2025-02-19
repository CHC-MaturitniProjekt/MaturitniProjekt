using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionContext
{
    private TwoBoneIKConstraint _leftIkConstraint;
    private TwoBoneIKConstraint _rightIkConstraint;
    private MultiRotationConstraint _leftHandRotationConstraint;
    private MultiRotationConstraint _rightHandRotationConstraint;
    private Rigidbody _rigidbody;
    private CapsuleCollider _rootCollider;
    
    public EnvironmentInteractionContext(TwoBoneIKConstraint leftIkConstraint, TwoBoneIKConstraint rightIkConstraint, MultiRotationConstraint leftHandRotationConstraint, MultiRotationConstraint rightHandRotationConstraint, Rigidbody rigidbody, CapsuleCollider rootCollider)
    {
        _leftIkConstraint = leftIkConstraint;
        _rightIkConstraint = rightIkConstraint;
        _leftHandRotationConstraint = leftHandRotationConstraint;
        _rightHandRotationConstraint = rightHandRotationConstraint;
        _rigidbody = rigidbody;
        _rootCollider = rootCollider;
    }
    
}
