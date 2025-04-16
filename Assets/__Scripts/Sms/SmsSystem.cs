using NUnit.Framework;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.Articy.Articy_4_0;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Random = Unity.Mathematics.Random;

public class SmsSystem : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject ResponseButton;
    [SerializeField] private GameObject NPCMessage;
    [SerializeField] private GameObject PCMessage;

    [Header("References")]
    [SerializeField] public GameObject ParentaResponse;
    [SerializeField] public GameObject ParentaMessages;


    [HideInInspector] public SmSManager dialogManager;

    private void Awake()
    {
        dialogManager = GetComponent<SmSManager>();
    }

    public void onResponseClick(int index)
    {
        foreach (Transform child in ParentaResponse.transform)
        {
            Destroy(child.gameObject);
        }


        dialogManager.OnResponseSelected(index);
    }

    public void showResponses(List<string> responses)
    {
        for(int i = 0; i < responses.Count; i++)
        {
            int jj = i;
            GameObject temp = Instantiate(ResponseButton, ParentaResponse.transform);
            temp.GetComponentInChildren<UnityEngine.UI.Text>().text = responses[i];
            temp.GetComponent<Button>().onClick.AddListener(()=>onResponseClick(jj));
        }
    }

    public void npcType(List<string> texts, System.Action onFinished = null)
    {
        StartCoroutine(NpcTypingCoroutine(texts, onFinished));
    }

    private IEnumerator<WaitForSeconds> NpcTypingCoroutine(List<string> texts, System.Action onFinished)
    {
        foreach (var realText in texts)
        {
            GameObject temp = Instantiate(NPCMessage, ParentaMessages.transform);
            var textComponent = temp.GetComponentInChildren<UnityEngine.UI.Text>();
            textComponent.text = "...";

            yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 4f));
            textComponent.text = realText;
            yield return new WaitForSeconds(0.3f);
        }

        onFinished?.Invoke();
    }

    public void pcType(string text)
    {
        GameObject temp = Instantiate(PCMessage, ParentaMessages.transform);
        temp.GetComponentInChildren<UnityEngine.UI.Text>().text = text;
    }
}
