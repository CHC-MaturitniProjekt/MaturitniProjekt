using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Animator))]
public class NPCAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetLayerWeight(animator.GetLayerIndex("Holding"),0);

    }

    public void SetMovementSpeed(float speed) => animator.SetFloat("X", speed);
    public void TriggerAnimation(string triggerName) => animator.SetTrigger(triggerName);
    public void Sit(string animName)
    {
        if (string.IsNullOrEmpty(animName)) return;
        animator.SetBool(animName, true);
    }

    public bool CurrentAnimationCompleted()
    {

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 1f;
    }
    public void ResetSit(string animName)
    {
        if (string.IsNullOrEmpty(animName)) return;
        animator.SetBool(animName, false);
    }
    
}
