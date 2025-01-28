using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class UltraWeb
{
    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Test();

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int InitializeUltralight();

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int CreateRenderer();

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int CreateView(int width, int height);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int LoadURL(string url);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetBitmapData(out int width, out int height, out int stride);

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void UpdateLogic();

    [DllImport("ULWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ShutdownUltralight();


    public int width;
    public int height;

    public UltraWeb(int height, int width)
    {
        this.height = height;
        this.width = width;


        Debug.Log(InitializeUltralight());
        Debug.Log(CreateRenderer());
        Debug.Log(CreateView(width, height));
    }

    public void getTexture()
    {
        UpdateLogic();
        int width, height, stride;
        int status = GetBitmapData(out width, out height, out stride);
        Debug.Log(status);
        if(status == 1)
        {
           //Debug.Log(width);
            //Debug.Log(height);
            //Debug.Log(stride);
        }
    }

    ~UltraWeb() 
    {
      //  ShutdownUltralight();
    }
}
