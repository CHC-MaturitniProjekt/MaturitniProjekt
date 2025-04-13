using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;

public class HideTextOnTriggerHandler : MonoBehaviour
{
    [SerializeField] private TextMeshPro textToHide;
    [SerializeField] private bool fadeOut;
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (fadeOut)
            {
                StartCoroutine(DisableTextWithFadeOut(textToHide));
            }
            else
            {
                DisableText(textToHide);
            }
        }
    }

    private void DisableText(TextMeshPro text)
    {
        text.gameObject.SetActive(false);
    }

    private IEnumerator DisableTextWithFadeOut(TextMeshPro text)
    {
        Color color = text.color;
        while (color.a > 0)
        {
            color.a -= Time.deltaTime;
            text.color = color;
            yield return null;
        }
        text.gameObject.SetActive(false);
    }    
    
}
