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

    private Material faceBaseTexture;
    private Vector2 textureOffset;
    private Coroutine animationCoroutine;

    void Start()
    {
        faceBaseTexture = skinnedMeshRenderer.material;
        textureOffset = faceBaseTexture.mainTextureOffset;

        PlayAnimation(AnimationType.Blinking);
    }

    public void PlayAnimation(AnimationType animation)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateTexture(animation));
    }

    private IEnumerator AnimateTexture(AnimationType animationType)
    {
        int currentFrame = 0;

        while (true)
        {
            if (framesPerAnimation <= 0)
            {
                Debug.LogWarning("framesPerAnimation is 0 or less. Skipping animation.");
                yield break;
            }

            int frameIndex = animationType == AnimationType.Blinking
                ? (framesPerAnimation - 1 - currentFrame)
                : currentFrame;

            textureOffset.x = frameIndex * XOffsetPX;
            textureOffset.y = ((animationType == AnimationType.Blinking) ? 0 : 1) * YOffsetPX;

            faceBaseTexture.mainTextureOffset = textureOffset;

            currentFrame = (currentFrame + 1) % framesPerAnimation;

            float waitTime = animationType switch
            {
                AnimationType.Blinking when currentFrame == 1 => 0.2f,
                AnimationType.Speaking => 0.5f,
                _ => Random.Range(minBlinkTime, maxBlinkTime)
            };

            yield return new WaitForSeconds(waitTime);
        }
    }
}
