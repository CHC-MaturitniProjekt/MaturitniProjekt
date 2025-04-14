using Language.Lua;
using System;
using System.Collections;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Security.Cryptography;

public class UltraWebComponent : MonoBehaviour
{
    [Header("Settings")]
    public int width = 1024;
    public int height = 768;
    public string url = "https://example.com";
    public bool canInteract = false;

    [Header("References")]
    [SerializeField] private RawImage _rawImage;

    private Texture2D _texture;
    private byte[] _pixelBuffer;
    private Coroutine _updateCoroutine;
    private readonly WaitForSeconds _frameDelay = new(1f / 30);
    private readonly string _sharedMemName = "UltraWebSharedMemory";
    private bool _ready = false;
    private byte[] _lastRawBuffer;
    private byte[] _newBuffer;
    private Rect _uvRect = new Rect(0, 1, 1, -1);

    private MemoryMappedFile _mmf;
    private MemoryMappedViewAccessor _accessor;

    private void InitSharedMemory()
    {
        _mmf = MemoryMappedFile.OpenExisting(_sharedMemName, MemoryMappedFileRights.Read);
        _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
    }

    private async void Start()
    {
        InitSharedMemory();

        if (_rawImage == null)
            _rawImage = gameObject.AddComponent<RawImage>();
       // _rawImage.uvRect = _uvRect;

        while (UltraWebProcessManager.Instance == null)
            await Task.Yield();

        await UltraWebProcessManager.Instance.CreateWindow(width, height);
        await UltraWebProcessManager.Instance.LoadURL(url);

        _texture = new Texture2D(width, height, TextureFormat.BGRA32, false)
        {
            filterMode = FilterMode.Point
        };

        _rawImage.texture = _texture;
        _rawImage.rectTransform.sizeDelta = new Vector2(width, height);
        _ready = true;
        _updateCoroutine = StartCoroutine(UpdateTextureRoutine());
    }

    private void Update()
    {
        if (!canInteract || !_ready) return;

        Vector3 mousePos = Input.mousePosition;
        int mouseX = (int)mousePos.x;
        int mouseY = Mathf.Abs(height - (int)mousePos.y); // převrácení Y souřadnice

        // Mouse move
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            UltraWebProcessManager.Instance.SendMouseEvent(mouseX, mouseY, UltraWebProcessManager.MouseEventType.Move, UltraWebProcessManager.MouseButton.None);
        }

        // Mouse buttons
        CheckMouseButton(0, UltraWebProcessManager.MouseButton.Left, mouseX, mouseY);
        CheckMouseButton(1, UltraWebProcessManager.MouseButton.Right, mouseX, mouseY);
        CheckMouseButton(2, UltraWebProcessManager.MouseButton.Middle, mouseX, mouseY);

        // Scroll
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            UltraWebProcessManager.Instance.SendMouseScroll((int)(scroll * 120)); // Ultralight expects scroll in 120 units
        }

        // Keyboard
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                UltraWebProcessManager.Instance.SendKeyDown(key);
            }

            if (Input.GetKeyUp(key))
            {
                UltraWebProcessManager.Instance.SendKeyUp(key);
            }

            if (Input.inputString.Length > 0 && Input.GetKey(key))
            {
                foreach (char c in Input.inputString)
                {
                    UltraWebProcessManager.Instance.SendCharPress(c);
                }
            }
        }
    }

    private void CheckMouseButton(int buttonIndex, UltraWebProcessManager.MouseButton button, int x, int y)
    {
        if (Input.GetMouseButtonDown(buttonIndex))
        {
            UltraWebProcessManager.Instance.SendMouseEvent(x, y, UltraWebProcessManager.MouseEventType.Down, button);
        }

        if (Input.GetMouseButtonUp(buttonIndex))
        {
            UltraWebProcessManager.Instance.SendMouseEvent(x, y, UltraWebProcessManager.MouseEventType.Up, button);
        }
    }

    private IEnumerator UpdateTextureRoutine()
    {
        var endOfFrameWait = new WaitForEndOfFrame();
        var maxFPSWait = new WaitForSeconds(1f / 60);

        while (true)
        {
            yield return endOfFrameWait;

            if (!_ready || UltraWebProcessManager.Instance == null) continue;

            // Nejdříve načti nová data
            var getTextureTask = GetTextureAsync();
            yield return new WaitUntil(() => getTextureTask.IsCompleted);

            yield return maxFPSWait;
        }
    }
    private byte[] _headerBuffer = new byte[12];

    private async Task GetTextureAsync()
    {
        await UltraWebProcessManager.Instance.RequestBitmapAsync();

        const int headerSize = 12;
        _accessor.ReadArray(0, _headerBuffer, 0, headerSize);

        int w = BitConverter.ToInt32(_headerBuffer, 0);
        int h = BitConverter.ToInt32(_headerBuffer, 4);
        int stride = BitConverter.ToInt32(_headerBuffer, 8);
        int rawSize = stride * h;

        if (_newBuffer == null || _newBuffer.Length != rawSize)
            _newBuffer = new byte[rawSize];

        _accessor.ReadArray(headerSize, _newBuffer, 0, rawSize);

        if (_texture == null || _texture.width != w || _texture.height != h)
        {
            _texture = new Texture2D(w, h, TextureFormat.BGRA32, false)
            {
                filterMode = FilterMode.Point
            };
            _rawImage.texture = _texture;
        }

        UpdateTextureData(w, h, stride);
    }

    private void UpdateTextureData(int width, int height, int stride)
    {
        if (_newBuffer == null) return;

        try
        {
            // Create temp buffer for stride mismatch case
            if (stride != width * 4)
            {
                byte[] tempBuffer = new byte[width * height * 4];

                unsafe
                {
                    fixed (byte* src = _newBuffer)
                    fixed (byte* dst = tempBuffer)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            Buffer.MemoryCopy(
                                src + y * stride,
                                dst + y * width * 4,
                                width * 4,
                                width * 4
                            );
                        }
                    }
                }

                _texture.LoadRawTextureData(tempBuffer);
            }
            else
            {
                _texture.LoadRawTextureData(_newBuffer);
            }

            _texture.Apply(false);
            _lastRawBuffer = _newBuffer.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Texture update failed: {e}");
        }
    }

    private bool HasBufferChanged()
    {
        if (_lastRawBuffer == null || _lastRawBuffer.Length != _newBuffer.Length)
            return true;

        var crc = new CRC32();
        crc.Append(_lastRawBuffer);
        uint lastHash = crc.GetCurrentHash();

        crc = new CRC32();
        crc.Append(_newBuffer);
        uint newHash = crc.GetCurrentHash();

        return lastHash != newHash;
    }

    private static int ConvertKeyCodeToVirtualKeyCode(KeyCode key)
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

public class CRC32
{
    private static readonly uint[] Table = new uint[256];
    private uint _hash = 0xFFFFFFFF;

    static CRC32()
    {
        for (uint i = 0; i < 256; i++)
        {
            var entry = i;
            for (var j = 0; j < 8; j++)
                entry = (entry & 1) == 1 ? (entry >> 1) ^ 0xEDB88320 : entry >> 1;
            Table[i] = entry;
        }
    }

    public void Append(byte[] data)
    {
        foreach (var b in data)
            _hash = Table[(_hash ^ b) & 0xFF] ^ (_hash >> 8);
    }

    public uint GetCurrentHash()
    {
        return ~_hash;
    }
}