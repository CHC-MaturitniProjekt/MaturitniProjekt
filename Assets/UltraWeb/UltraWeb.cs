using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class UltraWeb : IDisposable
{
    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int InitializeUltralight(string sourcePath);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int CreateRenderer();

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int CreateView(int width, int height);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int LoadURL(string url);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetBitmapData(out int width, out int height, out int stride, out IntPtr pixels);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ShutdownUltralight();

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int MouseInput(int x, int y, int type, int button);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int KeyInput(int key, int type, string text);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ScrollInput(int deltaX, int deltaY);

    public enum KeyEventType
    {
        RawKeyDown,
        KeyUp,
        Char
    };
    public enum MouseEventType
    {
        Move,
        Down,
        Up
    };
    public enum MouseButton
    {
        None,
        Left,
        Middle,
        Right
    };

    public int width;
    public int height;
    private Texture2D _texture;
    private static UltraWeb _instance;
    private static bool _disposed = false;
    public bool IsDisposed => _disposed;


#if UNITY_EDITOR
    static string pluginPath = Path.Combine(Application.dataPath, "UltraWeb/Plugins/x86_64");
#else
    static string pluginPath = Path.Combine(Application.dataPath, "Plugins/x86_64");
#endif
    public static UltraWeb Instance => _instance ?? throw new InvalidOperationException("UltraWeb not initialized.");

    private static bool _isInitialized = false; // Add initialization flag

    public static void Initialize(int width, int height)
    {
        if (_isInitialized) return;

        pluginPath = Path.GetFullPath(pluginPath).Replace('\\', '/');

        Debug.Log("Using pluginPath: " + pluginPath);

        if (InitializeUltralight(pluginPath) != 1)
            throw new Exception("Ultralight initialization failed.");

        _instance = new UltraWeb(width, height);


    }

    private UltraWeb(int width, int height)
    {
        if (CreateRenderer() != 1 || CreateView(width, height) != 1)
            throw new Exception("Failed to create renderer/view");
    }

    public void LoadUrl(string url)
    {
        LoadURL(url);
    }


    public Texture2D getTexture()
    {
        int width, height, stride;
        IntPtr pixels;
        if (GetBitmapData(out width, out height, out stride, out pixels) != 1)
            return null;

        if (_texture == null || _texture.width != width || _texture.height != height)
        {
            _texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
            _texture.filterMode = FilterMode.Point;
        }

        UpdateTextureData(pixels, width, height, stride, _texture);

        return _texture;
    }

    private static void UpdateTextureData(IntPtr pixels, int width, int height, int stride, Texture2D texture)
    {
        byte[] pixelData = new byte[width * height * 4];
        byte[] sourceData = new byte[height * stride];

        Marshal.Copy(pixels, sourceData, 0, sourceData.Length);

        for (int y = 0; y < height; y++)
        {
            int sourceY = height - 1 - y;
            int sourceOffset = sourceY * stride;
            int destOffset = y * width * 4;

            Buffer.BlockCopy(
                sourceData, sourceOffset,
                pixelData, destOffset,
                Math.Min(stride, width * 4)
            );
        }

        //texture.LoadRawTextureData(pixels, stride * height); musim zkusit
        texture.LoadRawTextureData(pixelData);
        texture.Apply(false);
    }

    public void SendKeyPress(KeyCode keyCode)
    {
        int virtualKeyCode = ConvertKeyCodeToVirtualKeyCode(keyCode);
        if (virtualKeyCode == -1) return;
        string text = "";
        if (IsPrintableCharacter(virtualKeyCode)) text = keyCode.ToString().ToLower();
        KeyInput(virtualKeyCode, (int)KeyEventType.RawKeyDown, null);
        KeyInput(virtualKeyCode, (int)KeyEventType.Char, text);
    }

    public void SendKeyUp(KeyCode keyCode)
    {
        int virtualKeyCode = ConvertKeyCodeToVirtualKeyCode(keyCode);
        if (virtualKeyCode == -1 || !IsPrintableCharacter(virtualKeyCode)) return;
        KeyInput(virtualKeyCode, (int)KeyEventType.KeyUp, null);
    }

    private bool IsPrintableCharacter(int vkCode)
    {
        return (vkCode >= 0x20 && vkCode <= 0x7E);
    }

    public void SendMouseEvent(int x, int y, MouseEventType type, MouseButton button)
    {
        MouseInput(x, y, (int)type, (int)button);
    }

    public void SendMouseScroll(int deltaY)
    {
        ScrollInput(0, deltaY);
    }

    public static void ResetStaticState()
    {
        _instance = null;
    }

    public void Dispose()
    {
        if (!_isInitialized) return;

        ShutdownUltralight();
        _isInitialized = false;
        _instance = null;

        // Add explicit GC cleanup
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    public static int ConvertKeyCodeToVirtualKeyCode(KeyCode key)
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
}