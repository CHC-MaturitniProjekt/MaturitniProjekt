using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimation : MonoBehaviour
{
    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    public void SetMovementSpeed(float speed) => _animator.SetFloat("X", speed);
    public void TriggerAnimation(string triggerName) => _animator.SetTrigger(triggerName);
    public void Sit(string animName)
    {
        if (string.IsNullOrEmpty(animName)) return;
        _animator.SetBool(animName, true);
    }
    
    public void ResetSit(string animName)
    {
        if (string.IsNullOrEmpty(animName)) return;
        _animator.SetBool(animName, false);
    }
    
}
