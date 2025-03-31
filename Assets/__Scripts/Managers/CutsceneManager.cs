using System.Collections;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private Animator[] cutsceneAnimators;
    
    [Header("Debug")]
    [SerializeField] private bool startOnAwake = false;

    private void Awake()
    {
        foreach (var animator in cutsceneAnimators)
        {
            animator.enabled = false;
        }
    }

    private void Start()
    {
        if (startOnAwake)
        {
            TriggerCutscene();
        }
    }
    
    public void TriggerCutscene()
    {
        StartCoroutine(PlayCutsceneSequence());
    }
    
    private IEnumerator PlayCutsceneSequence()
    {
        for (int i = 0; i < cutsceneAnimators.Length; i++)
        {
            Animator animator = cutsceneAnimators[i];
            animator.enabled = true;
            animator.Play(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

            animator.enabled = false;
        }
    }
}