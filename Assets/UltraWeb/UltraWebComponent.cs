using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UltraWebComponent : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int width = 1024;
    [SerializeField] private int height = 768;
    [SerializeField] private string url = "https://example.com";

    [Header("References")]
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private GameObject cantUseInEditor;

    private Coroutine _updateCoroutine;
    public bool canInteract = false;

    private void Start()
    {
        if (_rawImage == null)
            _rawImage = gameObject.AddComponent<RawImage>();
        _rawImage.rectTransform.sizeDelta = new Vector2(width, height);

#if UNITY_EDITOR
        cantUseInEditor.SetActive(true);
        return; 
#endif

        try
        {
            UltraWeb.Initialize(width, height);
            UltraWeb.Instance.LoadUrl(url);
            _updateCoroutine = StartCoroutine(UpdateTextureRoutine());

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Initialization failed: {e.Message}");
            enabled = false;
        }
    }
    private void Update()
    {
        if (!canInteract || !enabled)
            return;

        Vector2 localPoint;
        if (!GetLocalPointerPosition(out localPoint))
            return;

        int x = Mathf.Clamp((int)localPoint.x, 0, width - 1);
        int y = Mathf.Clamp((int)localPoint.y, 0, height - 1);

        if (IsPointerOverRawImage())
        {
            HandleMouseInput(x, y);
        }
        HandleKeyboardInput();

        float scrollDelta = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            UltraWeb.Instance.SendMouseScroll((int)(scrollDelta * 120));
        }
    }

    private bool GetLocalPointerPosition(out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (_rawImage == null || _rawImage.rectTransform == null)
            return false;

        Vector2 screenPoint = Input.mousePosition;

        // Převod obrazovkové pozice na lokální pozici uvnitř RawImage
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rawImage.rectTransform, screenPoint, null, out localPoint))
            return false;

        // Získání přesné velikosti RawImage v pixelech
        Rect pixelRect = RectTransformUtility.PixelAdjustRect(_rawImage.rectTransform, _rawImage.canvas);

        // Převedení localPoint (který je relativní ke středu) na [0, width/height]
        float adjustedX = localPoint.x + pixelRect.width / 2f;
        float adjustedY = pixelRect.height / 2f - localPoint.y;

        // Mapa na UltraWeb texture resolution
        float scaleX = (float)width / pixelRect.width;
        float scaleY = (float)height / pixelRect.height;

        localPoint = new Vector2(adjustedX * scaleX, adjustedY * scaleY);

        return true;
    }

    private bool IsPointerOverRawImage()
    {
        if (_rawImage == null || _rawImage.rectTransform == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(_rawImage.rectTransform, Input.mousePosition, null);
    }

    private void HandleMouseInput(int x, int y)
    {
        if (Input.GetMouseButtonDown(0))
            UltraWeb.Instance.SendMouseEvent(x, y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Left);
        if (Input.GetMouseButtonUp(0))
            UltraWeb.Instance.SendMouseEvent(x, y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Left);

        if (Input.GetMouseButtonDown(1))
            UltraWeb.Instance.SendMouseEvent(x, y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Right);
        if (Input.GetMouseButtonUp(1))
            UltraWeb.Instance.SendMouseEvent(x, y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Right);

        if (Input.GetMouseButtonDown(2))
            UltraWeb.Instance.SendMouseEvent(x, y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Middle);
        if (Input.GetMouseButtonUp(2))
            UltraWeb.Instance.SendMouseEvent(x, y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Middle);

        // Pohyb myši pouze jednou
        UltraWeb.Instance.SendMouseEvent(x, y, UltraWeb.MouseEventType.Move, UltraWeb.MouseButton.None);
    }

    private void HandleKeyboardInput()
    {
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
                UltraWeb.Instance.SendKeyPress(key);

            if (Input.GetKeyUp(key))
                UltraWeb.Instance.SendKeyUp(key);
        }
    }

    private IEnumerator UpdateTextureRoutine()
    {
        while (!UltraWeb.Instance.IsDisposed)
        {
            yield return new WaitForEndOfFrame();

            // Získání textury z UltraWeb
            var texture = UltraWeb.Instance.getTexture();

            if (texture != null && _rawImage != null)
            {
                // Aktualizace RawImage
                _rawImage.texture = texture;

                // Optimalizace: Přeskočit 1 snímek pro snížení vytížení CPU
                yield return null;
            }
            else
            {
                Debug.LogWarning("Texture or RawImage is null");
            }
        }
    }

    private void OnDestroy()
    {
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);

        if (UltraWeb.Instance != null && !UltraWeb.Instance.IsDisposed)
        {
            UltraWeb.Instance.Dispose();
        }
    }

}