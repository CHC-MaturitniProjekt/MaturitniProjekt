using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCTextureAnimation : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    [SerializeField] private bool isSpeaking = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (!isSpeaking)
        {
            StartCoroutine(OffsetTexture(-0.27f));
        }
    }

    private float currentOffset = 0f;

    private IEnumerator OffsetTexture(float offsetSwitch)
    {
        isSpeaking = true;
        currentOffset = (currentOffset == 0f) ? offsetSwitch : 0f;
        meshRenderer.material.mainTextureOffset = new Vector2(currentOffset, 0);
        yield return new WaitForSeconds(1f);
        isSpeaking = false;
    }
}