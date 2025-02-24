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
    private Transform _rootTransform;
    
    private Vector3 _leftOriginalTargetPosition;
    private Vector3 _rightOriginalTargetPosition;

    public enum EBodySide
    {
        RIGHT,
        LEFT
    }
    
    public float CharacterShoulderHeight;
    
    public EnvironmentInteractionContext(TwoBoneIKConstraint leftIkConstraint, TwoBoneIKConstraint rightIkConstraint,
        MultiRotationConstraint leftHandRotationConstraint, MultiRotationConstraint rightHandRotationConstraint,
        Rigidbody rigidbody, CapsuleCollider rootCollider, Transform rootTransform)
    {
        _leftIkConstraint = leftIkConstraint;
        _rightIkConstraint = rightIkConstraint;
        _leftHandRotationConstraint = leftHandRotationConstraint;
        _rightHandRotationConstraint = rightHandRotationConstraint;
        _rigidbody = rigidbody;
        _rootCollider = rootCollider;
        _rootTransform = rootTransform;
        _leftOriginalTargetPosition = _leftIkConstraint.data.target.transform.localPosition;
        _rightOriginalTargetPosition = _rightIkConstraint.data.target.transform.localPosition;
        OriginalTargetRotation = _leftIkConstraint.data.target.transform.rotation;
        
        CharacterShoulderHeight = leftIkConstraint.data.root.transform.position.y;
        SetCurrentSide(Vector3.positiveInfinity);
    }
    
    public TwoBoneIKConstraint LeftIkConstraint => _leftIkConstraint;
    public TwoBoneIKConstraint RightIkConstraint => _rightIkConstraint;
    public MultiRotationConstraint LeftHandRotationConstraint => _leftHandRotationConstraint;
    public MultiRotationConstraint RightHandRotationConstraint => _rightHandRotationConstraint;
    public Rigidbody Rb => _rigidbody;
    public CapsuleCollider RootCollider => _rootCollider;
    public Transform RootTransform => _rootTransform;
    
    public Collider CurrentIntersectingCollider { get; set; }
    public TwoBoneIKConstraint CurrentIkConstraint { get; private set; }
    public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
    public Transform CurrentIkTargetTransform { get; private set; }
    public Transform CurrentShoulderTransform { get; private set; }
    public EBodySide CurrentBodySide { get; private set; }
    public Vector3 ClosestPointOnColliderFromShoulder { get; set; } = Vector3.positiveInfinity;
    public float InteractionPointYOffset { get; set; } = 0;
    public float ColliderCenterY { get; set; }
    public Vector3 CurrentOriginalTargetPosition { get; private set; }
    public Quaternion OriginalTargetRotation { get; private set; }
    public float LowestDistance { get; set; } = Mathf.Infinity;
    
    public void SetCurrentSide(Vector3 posToCheck)
    {
        Vector3 leftShoulder = _leftIkConstraint.data.root.transform.position;
        Vector3 rightShoulder = _rightIkConstraint.data.root.transform.position;
        
        bool isLeftCloser = Vector3.Distance(posToCheck, leftShoulder) < Vector3.Distance(posToCheck, rightShoulder);
        
        SwitchSides(isLeftCloser);
    }

    public void SwitchSides(bool isLeftCloser)
    {
        if (isLeftCloser)
        {
            CurrentBodySide = EBodySide.LEFT;
            CurrentIkConstraint = _leftIkConstraint;
            CurrentMultiRotationConstraint = _leftHandRotationConstraint; 
            CurrentOriginalTargetPosition = _leftOriginalTargetPosition;
            ResetHand(EBodySide.RIGHT);
        }
        else
        {
            CurrentBodySide = EBodySide.RIGHT;
            CurrentIkConstraint = _rightIkConstraint;
            CurrentMultiRotationConstraint = _rightHandRotationConstraint;
            CurrentOriginalTargetPosition = _rightOriginalTargetPosition;
            ResetHand(EBodySide.LEFT);
        }
        
        CurrentShoulderTransform = CurrentIkConstraint.data.root.transform;
        CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
    }
    
    protected void ResetHand(EBodySide side)
    {
        if (side == EBodySide.LEFT)
        {
            _leftIkConstraint.weight = Mathf.Lerp(_leftIkConstraint.weight, 0, 0.3f);
            _leftHandRotationConstraint.weight = Mathf.Lerp(_leftIkConstraint.weight, 0, 0.3f);
        }
        else
        {
            _rightIkConstraint.weight = Mathf.Lerp(_rightIkConstraint.weight, 0, 0.3f);
            _rightHandRotationConstraint.weight = Mathf.Lerp(_rightIkConstraint.weight, 0, 0.3f);
        }
    }
    
    
}
