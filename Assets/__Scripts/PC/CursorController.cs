using UnityEngine;
using UnityEngine.UI;

public class FollowMouseUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    public RectTransform cursorImageTransform;
    [SerializeField]
    public RectTransform canvasRect;
    [SerializeField]
    public Camera pcCamera;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("Base movement speed")]
    private float baseSpeed = 1000f;

    [SerializeField]
    [Tooltip("Smoothing factor (lower = smoother)")]
    [Range(0.01f, 1f)]
    private float smoothingFactor = 0.1f;

    [SerializeField]
    [Tooltip("Enable dynamic speed based on distance")]
    private bool useDynamicSpeed = true;

    [SerializeField]
    [Tooltip("Max movement speed")]
    private float maxSpeed = 2000f;

    private Vector2 targetPosition;
    private Vector2 currentVelocity;

    private void Update()
    {
        if (Pc.Instance.isInteracting)
            MoveCursor();
    }

    private void MoveCursor()
    {
        // Get target mouse position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            pcCamera,
            out targetPosition
        );

        // Calculate movement
        if (useDynamicSpeed)
        {
            // Dynamická rychlost podle vzdálenosti
            float distance = Vector2.Distance(cursorImageTransform.anchoredPosition, targetPosition);
            float dynamicSpeed = Mathf.Clamp(distance * baseSpeed, 0, maxSpeed);

            cursorImageTransform.anchoredPosition = Vector2.SmoothDamp(
                cursorImageTransform.anchoredPosition,
                targetPosition,
                ref currentVelocity,
                smoothingFactor,
                dynamicSpeed
            );
        }
        else
        {
            // Lineární interpolace s plynulým dojezdem
            cursorImageTransform.anchoredPosition = Vector2.Lerp(
                cursorImageTransform.anchoredPosition,
                targetPosition,
                Time.deltaTime * baseSpeed
            );
        }
    }
}