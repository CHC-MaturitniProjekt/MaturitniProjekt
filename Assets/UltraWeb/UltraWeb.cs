using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class UltraWeb : IDisposable
{
    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int InitializeUltralight();

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


    public int width;
    public int height;
    private Texture2D _texture;
    private static bool _isInitialized = false;
    private bool _disposed = false;
    private static bool _appIsQuitting = false; // Pro prevenci volání ShutdownUltralight pøi domain reload v editoru.

    public UltraWeb(int height, int width)
    {
        this.height = height;
        this.width = width;

        if (!_isInitialized)
        {
            if (UltralightInitializer.Initialize() != 1) // Použijeme statickou inicializaci
                throw new Exception("Failed to initialize Ultralight");
            _isInitialized = true;
        }

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

        texture.LoadRawTextureData(pixelData);
        texture.Apply();
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (!_appIsQuitting) // Nechceme volat ShutdownUltralight pri domain reload v editoru
        {
            // ShutdownUltralight se volá globálnì pøi ukonèení aplikace, ne pro každou instanci UltraWeb.
        }

        if (_texture != null)
        {
            UnityEngine.Object.Destroy(_texture);
            _texture = null;
        }
        _disposed = true;
    }

    ~UltraWeb()
    {
        Dispose();
    }

    // Statická tøída pro globální inicializaci a vypnutí Ultralight
    public static class UltralightInitializer
    {
        public static int Initialize()
        {
            if (!_isInitialized)
            {
                Debug.Log("UltralightInitializer: Initialize() called."); // Log inicializácie v C#
                if (InitializeUltralight() != 1)
                    return -1;
                _isInitialized = true;
                Debug.Log("UltralightInitializer: Initialize() finished."); // Log dokonèenia inicializácie v C#
            }
            return 1;
        }

        public static void Shutdown()
        {
            if (_isInitialized)
            {
                Debug.Log("UltralightInitializer: Shutdown() called."); // Log shutdownu v C#
                ShutdownUltralight();
                _isInitialized = false;
                Debug.Log("UltralightInitializer: Shutdown() finished."); // Log dokonèenia shutdownu v C#
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            Initialize();
            Application.quitting += OnApplicationQuitting;

#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload; // Pridanie listenera pre domain reload
#endif
        }

        static void OnApplicationQuitting()
        {
            _appIsQuitting = true;
            Shutdown();
        }

#if UNITY_EDITOR
        static void OnBeforeAssemblyReload()
        {
            Debug.Log("UltralightInitializer: OnBeforeAssemblyReload - Editor Domain Reload detected. Forcing Shutdown."); // Log domain reload
            Shutdown(); // Explicitné volanie shutdown pri domain reload
        }
#endif
    }
}