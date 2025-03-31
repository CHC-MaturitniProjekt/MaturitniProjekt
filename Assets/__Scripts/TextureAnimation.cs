using System.Collections;
using UnityEngine;

public class TextureAnimation : MonoBehaviour
{
    public enum AnimationType
    {
        Blinking,
        Speaking
    }

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private float XOffsetPX;
    [SerializeField] private float YOffsetPX;
    [SerializeField] private float minBlinkTime;
    [SerializeField] private float maxBlinkTime;
    [SerializeField] private int framesPerAnimation;
    [SerializeField] private int numberOfAnimations;

    private Material faceBaseTexture;
    private Vector2 textureOffset;
    private int currentFrame;
    private AnimationType currentAnimation;

    void Start()
    {
        faceBaseTexture = skinnedMeshRenderer.material;
        textureOffset = faceBaseTexture.mainTextureOffset;
        
        currentAnimation = AnimationType.Blinking;
        
        faceBaseTexture.mainTextureOffset = new Vector2(0, 0);
        StartCoroutine(AnimateTexture());
    }

    private IEnumerator AnimateTexture()
    {
        while (true)
        {
            textureOffset.x = ((framesPerAnimation - 1 - currentFrame) % framesPerAnimation) * XOffsetPX;
            textureOffset.y = (int)currentAnimation * YOffsetPX;
            faceBaseTexture.mainTextureOffset = textureOffset;

            currentFrame = (currentFrame + 1) % framesPerAnimation;

            if (currentAnimation == AnimationType.Blinking && currentFrame == 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
            else if (currentAnimation == AnimationType.Speaking)
            {
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(minBlinkTime, maxBlinkTime));
            }
        }
    }

    public void SetAnimation(AnimationType animation)
    {
        currentAnimation = animation;
        currentFrame = 0;
    }
}