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
    private static UltraWeb _instance;
    private static bool _disposed = false;

    public bool IsDisposed => _disposed;


    public static UltraWeb Instance => _instance ?? throw new InvalidOperationException("UltraWeb not initialized.");

    private static bool _isInitialized = false; // Add initialization flag

    public static void Initialize(int width, int height)
    {
        if (_isInitialized) return;

        if (InitializeUltralight() != 1)
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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        _instance = null;
        _isInitialized = false;

        // Force native cleanup if needed
#if UNITY_EDITOR
        ShutdownUltralight();
#endif
    }
}