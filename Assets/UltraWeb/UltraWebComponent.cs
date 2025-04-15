using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
    private int viewId = -1;
    private Texture2D texture;

    private void Start()
    {
        if (_rawImage == null)
            _rawImage = gameObject.AddComponent<RawImage>();
        _rawImage.rectTransform.sizeDelta = new Vector2(width, height);

#if UNITY_EDITOR
        if (cantUseInEditor != null)
            cantUseInEditor.SetActive(true);
        return;
#endif

        try
        {
            texture = new Texture2D(width, height, TextureFormat.BGRA32, false);

            UltraWeb.Initialize();
            viewId = UltraWeb.Instance.CreatenewView(width, height);
            if (viewId < 0)
            {
                Debug.LogError("Failed to create UltraWeb view");
                enabled = false;
                return;
            }

            UltraWeb.Instance.LoadUrl(viewId, url);
            //UltraWeb.Instance.LoadFile(viewId, @"C:\Projects\Tic-Tac-Two\dist\tic-tac-two\browser");

            _updateCoroutine = StartCoroutine(UpdateTextureRoutine());
        }
        catch (Exception e)
        {
            Debug.LogError($"Initialization failed: {e.Message}");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!canInteract || !enabled || viewId < 0)
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

        if (IsPointerOverRawImage())
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                UltraWeb.Instance.SendMouseScroll(viewId, (int)(scrollDelta * 120));
            }
        }
    }

    private bool GetLocalPointerPosition(out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (_rawImage == null || _rawImage.rectTransform == null)
            return false;

        Vector2 screenPoint = Input.mousePosition;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rawImage.rectTransform, screenPoint, null, out localPoint))
            return false;

        Rect pixelRect = RectTransformUtility.PixelAdjustRect(_rawImage.rectTransform, _rawImage.canvas);

        float adjustedX = localPoint.x + pixelRect.width / 2f;
        float adjustedY = pixelRect.height / 2f - localPoint.y;

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
            UltraWeb.Instance.SendMouseEvent(viewId, x, y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Left);
        if (Input.GetMouseButtonUp(0))
            UltraWeb.Instance.SendMouseEvent(viewId, x, y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Left);

        if (Input.GetMouseButtonDown(1))
            UltraWeb.Instance.SendMouseEvent(viewId, x, y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Right);
        if (Input.GetMouseButtonUp(1))
            UltraWeb.Instance.SendMouseEvent(viewId, x, y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Right);

        if (Input.GetMouseButtonDown(2))
            UltraWeb.Instance.SendMouseEvent(viewId, x, y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Middle);
        if (Input.GetMouseButtonUp(2))
            UltraWeb.Instance.SendMouseEvent(viewId, x, y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Middle);

        UltraWeb.Instance.SendMouseEvent(viewId, x, y, UltraWeb.MouseEventType.Move, UltraWeb.MouseButton.None);
    }

    private void HandleKeyboardInput()
    {
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
                UltraWeb.Instance.SendKeyPress(viewId, key);

            if (Input.GetKeyUp(key))
                UltraWeb.Instance.SendKeyUp(viewId, key);
        }
    }

    private IEnumerator UpdateTextureRoutine()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            UltraWeb.Instance.getTexture(viewId, texture);

            if (texture != null && _rawImage != null)
            {
                _rawImage.texture = texture;
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

        if (viewId >= 0)
        {
            UltraWeb.Instance.destroyView(viewId);
        }
    }
}
