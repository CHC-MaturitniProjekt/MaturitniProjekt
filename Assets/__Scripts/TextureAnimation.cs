using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureAnimation : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private float XOffsetPX;
    [SerializeField] private float YOffsetPX;
    [SerializeField] private float minBlinkTime;
    [SerializeField] private float maxBlinkTime;

    private Material faceBaseTexture;
    
    void Start()
    {
        faceBaseTexture = skinnedMeshRenderer.material;
    }

    void Update()
    {
        
    }
}
