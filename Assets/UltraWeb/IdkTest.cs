using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class IdkTest : MonoBehaviour
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
        try
        {
            // Inicializace UltraWeb
            UltraWeb.Initialize(width, height);

            // Načtení URL
            UltraWeb.Instance.LoadUrl(url);

            // Spuštění coroutine pro aktualizaci textury
            _updateCoroutine = StartCoroutine(UpdateTextureRoutine());

            // Nastavení počáteční velikosti RawImage
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
                int vkCode = ConvertKeyCodeToVirtualKeyCode(key);
                if (vkCode == -1) continue;

                if (Input.GetKeyDown(key))
                {
                    if (IsPrintableCharacter(vkCode))
                    {
                        UltraWeb.Instance.SendKeyPress(vkCode, key.ToString().ToLower());
                    }
                    else
                    {
                        UltraWeb.Instance.SendKeyPress(vkCode, "");
                    }
                }

                if (Input.GetKeyUp(key))
                {
                    UltraWeb.Instance.SendKeyUp(vkCode);
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

    private bool IsPrintableCharacter(int vkCode)
    {
        return (vkCode >= 0x20 && vkCode <= 0x7E);
    }

    private int ConvertKeyCodeToVirtualKeyCode(KeyCode key)
    {
        // A-Z (velká písmena odpovídají ASCII)
        if (key >= KeyCode.A && key <= KeyCode.Z)
        {
            return key - KeyCode.A + 0x41; // 0x41 = 'A' v VK kódech
        }

        // Čísla 0-9 (horní řada, ne NumPad)
        if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
        {
            return key - KeyCode.Alpha0 + 0x30; // 0x30 = '0' v ASCII
        }

        // Čísla 0-9 (NumPad)
        if (key >= KeyCode.Keypad0 && key <= KeyCode.Keypad9)
        {
            return key - KeyCode.Keypad0 + 0x60; // 0x60 = VK_NUMPAD0
        }

        // Funkční klávesy F1-F12
        if (key >= KeyCode.F1 && key <= KeyCode.F12)
        {
            return key - KeyCode.F1 + 0x70; // 0x70 = VK_F1
        }

        // Speciální klávesy
        switch (key)
        {
            case KeyCode.Space: return 0x20;
            case KeyCode.Return: return 0x0D;
            case KeyCode.Backspace: return 0x08;
            case KeyCode.Tab: return 0x09;
            case KeyCode.Escape: return 0x1B;
            case KeyCode.LeftArrow: return 0x25;
            case KeyCode.UpArrow: return 0x26;
            case KeyCode.RightArrow: return 0x27;
            case KeyCode.DownArrow: return 0x28;
            case KeyCode.LeftShift:
            case KeyCode.RightShift: return 0xA0; // VK_LSHIFT
            case KeyCode.LeftControl:
            case KeyCode.RightControl: return 0xA2; // VK_LCONTROL
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt: return 0xA4; // VK_LMENU (Alt)
            case KeyCode.CapsLock: return 0x14;
            case KeyCode.Insert: return 0x2D;
            case KeyCode.Delete: return 0x2E;
            case KeyCode.Home: return 0x24;
            case KeyCode.End: return 0x23;
            case KeyCode.PageUp: return 0x21;
            case KeyCode.PageDown: return 0x22;
            case KeyCode.Numlock: return 0x90;
            case KeyCode.ScrollLock: return 0x91;
            case KeyCode.Print: return 0x2C;

            // Zvláštní znaky (anglické rozložení klávesnice)
            case KeyCode.Minus: return 0xBD; // '-'
            case KeyCode.Equals: return 0xBB; // '='
            case KeyCode.LeftBracket: return 0xDB; // '['
            case KeyCode.RightBracket: return 0xDD; // ']'
            case KeyCode.Semicolon: return 0xBA; // ';'
            case KeyCode.Quote: return 0xDE; // '''
            case KeyCode.Comma: return 0xBC; // ','
            case KeyCode.Period: return 0xBE; // '.'
            case KeyCode.Slash: return 0xBF; // '/'
            case KeyCode.Backslash: return 0xDC; // '\'

            default: return -1; // Neznámá klávesa
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