using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UltraWebComponent : MonoBehaviour
{
    [Header("Settings")]
    public int width = 1024;
    public int height = 768;
    public string url = "https://example.com";

    [Header("References")]
    [SerializeField] private RawImage _rawImage;

    private Coroutine _updateCoroutine;

    public bool canInteract = false;

    private void Start()
    {
        if (_rawImage == null)
            _rawImage = gameObject.AddComponent<RawImage>();

        try
        {
            UltraWeb.Initialize(width, height);
            UltraWeb.Instance.LoadUrl(url);
            _updateCoroutine = StartCoroutine(UpdateTextureRoutine());
            _rawImage.rectTransform.sizeDelta = new Vector2(width, height);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Initialization failed: {e.Message}");
            enabled = false;
        }
    }

    private void Update()
    {
        if (canInteract)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.y = Mathf.Abs(height - (int)mousePos.y);
            if (Input.GetMouseButtonDown(0))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Left);
            }

            if (Input.GetMouseButtonUp(0))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Left);
            }

            if (Input.GetMouseButton(0)) 
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Move, UltraWeb.MouseButton.Left);
            }

            if (Input.GetMouseButtonDown(1))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Right);
            }

            if (Input.GetMouseButtonUp(1))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Right);
            }

            if (Input.GetMouseButton(1))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Move, UltraWeb.MouseButton.Right);
            }

            if (Input.GetMouseButtonDown(2))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Down, UltraWeb.MouseButton.Middle);
            }

            if (Input.GetMouseButtonUp(2))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Up, UltraWeb.MouseButton.Middle);
            }

            if (Input.GetMouseButton(2))
            {
                UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Move, UltraWeb.MouseButton.Middle);
            }

            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    UltraWeb.Instance.SendKeyPress(key);
                }

                if (Input.GetKeyUp(key))
                {
                    UltraWeb.Instance.SendKeyUp(key);
                }
            }

            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta != 0)
            {
                UltraWeb.Instance.SendMouseScroll((int)scrollDelta * 120);
            }

           // UltraWeb.Instance.SendMouseEvent((int)mousePos.x, (int)mousePos.y, UltraWeb.MouseEventType.Move, UltraWeb.MouseButton.None);
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
        // Zastavení coroutine
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);

        // Uvolnění prostředků
        if (UltraWeb.Instance != null && !UltraWeb.Instance.IsDisposed)
        {
            UltraWeb.Instance.Dispose();
        }

        UltraWeb.ResetStaticState();
    }

    private void OnApplicationQuit()
    {
        // Nastavíme globální flag pro všechny instance

        if (UltraWeb.Instance != null && !UltraWeb.Instance.IsDisposed)
        {
            UltraWeb.Instance.Dispose();
        }

        UltraWeb.ResetStaticState();
    }

}