using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public static class Native
{
    private static readonly Lazy<IntPtr> LazyLoadedLibUltralightCore;

    private static readonly Lazy<IntPtr> LazyLoadedLibUltralight;

    private static readonly Lazy<IntPtr> LazyLoadedLibAppCore;

    private static readonly Lazy<IntPtr> LazyLoadedLibWebCore;

    private static readonly Lazy<IntPtr> LazyLoadedIcudata;

    private static readonly Lazy<IntPtr> LazyLoadedIcuuc;

    private static readonly Lazy<IntPtr> LazyLoadedIcui18n;

    private static readonly Lazy<IntPtr> LazyLoadedULWrapper;

    public static IntPtr LibUltralightCore => LazyLoadedLibUltralightCore.Value;

    public static IntPtr LibUltralight => LazyLoadedLibUltralight.Value;

    public static IntPtr LibAppCore => LazyLoadedLibAppCore.Value;

    public static IntPtr LibWebCore => LazyLoadedLibWebCore.Value;
    public static IntPtr LibULWRapper => LazyLoadedULWrapper.Value;

    private static IntPtr LibIcudata => LazyLoadedIcudata.Value;

    private static IntPtr LibIcuuc => LazyLoadedIcuuc.Value;

    private static IntPtr LibIcui18n => LazyLoadedIcui18n.Value;

    private unsafe static IntPtr LoadLib(string libName)
    {
        string localCodeBaseDirectory = Application.dataPath; ;
        int num = sizeof(void*) * 8;
        IntPtr lib = default(IntPtr);
        string text;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            text = Path.Combine(localCodeBaseDirectory, libName + ".dll");
            if (!TryLoad(text, out lib))
            {
                text = Path.Combine(localCodeBaseDirectory, "UltraWeb","Plugins", (num == 32) ? "win-x86" : "win-x64", libName + ".dll");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            text = Path.Combine(localCodeBaseDirectory, "lib" + libName + ".dylib");
            if (!TryLoad(text, out lib))
            {
                text = Path.Combine(localCodeBaseDirectory, "runtimes", "osx-x64", "native", "lib" + libName + ".dylib");
            }
        }
        else
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new PlatformNotSupportedException();
            }

            text = Path.Combine(localCodeBaseDirectory, "lib" + libName + ".so");
            if (!TryLoad(text, out lib))
            {
                text = Path.Combine(localCodeBaseDirectory, "runtimes", (IsMusl() ? "linux-musl-" : "linux-") + GetProcArchString(), "native", "lib" + libName + ".so");
            }
        }

        if (lib == (IntPtr)0)
        {
            lib = NativeLibrary.Load(text);
            if (lib == (IntPtr)0)
            {
                throw new DllNotFoundException(text);
            }
        }

        return lib;
    }

    private static bool TryLoad(string libPath, out IntPtr lib)
    {
        try
        {
            lib = NativeLibrary.Load(libPath);
        }
        catch (Exception arg)
        {
            Console.Error.WriteLine($"Library loading error: {libPath}\n{arg}");
            lib = default(IntPtr);
            return false;
        }

        return true;
    }

    static Native()
    {
        LazyLoadedLibUltralightCore = new Lazy<IntPtr>(() => LoadLib("UltralightCore"), LazyThreadSafetyMode.ExecutionAndPublication);
        LazyLoadedLibUltralight = new Lazy<IntPtr>(() => LoadLib("Ultralight"), LazyThreadSafetyMode.ExecutionAndPublication);
        LazyLoadedLibAppCore = new Lazy<IntPtr>(() => LoadLib("AppCore"), LazyThreadSafetyMode.ExecutionAndPublication);
        LazyLoadedLibWebCore = new Lazy<IntPtr>(() => LoadLib("WebCore"), LazyThreadSafetyMode.ExecutionAndPublication);
        LazyLoadedIcudata = new Lazy<IntPtr>(() => LoadLib("icudata"), LazyThreadSafetyMode.ExecutionAndPublication);
        LazyLoadedIcuuc = new Lazy<IntPtr>(() => LoadLib("icuuc"), LazyThreadSafetyMode.ExecutionAndPublication);
        LazyLoadedIcui18n = new Lazy<IntPtr>(() => LoadLib("icui18n"), LazyThreadSafetyMode.ExecutionAndPublication);
        LazyLoadedULWrapper = new Lazy<IntPtr>(() => LoadLib("ULWrapper"), LazyThreadSafetyMode.ExecutionAndPublication);
        NativeLibrary.SetDllImportResolver(typeof(Native).Assembly, (string name, Assembly assembly, DllImportSearchPath? path) => name switch
        {
            "UltralightCore" => LibUltralightCore,
            "Ultralight" => LibUltralight,
            "AppCore" => LibAppCore,
            "WebCoreCore" => LibWebCore,
            "icudata" => LibIcudata,
            "icuuc" => LibIcuuc,
            "icui18n" => LibIcui18n,
            "ULWrapper" => LibULWRapper,
            _ => (IntPtr)0,
        });
    }

    public static void Init()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (LibUltralightCore == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload LibUltralightCore");
            }

            if (LibIcudata == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload LibIcudata");
            }

            if (LibIcuuc == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload LibIcuuc");
            }

            if (LibIcui18n == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload LibIcui18n");
            }

            if (LibWebCore == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload LibWebCore");
            }

            if (LibUltralight == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload LibUltralight");
            }

            if (LibAppCore == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload LibAppCore");
            }

            if (LibULWRapper == (IntPtr)0)
            {
                throw new PlatformNotSupportedException("Can't preload ULWrapper");
            }
        }
    }

    private static bool IsMusl()
    {
        using Process process = Process.GetCurrentProcess();
        foreach (ProcessModule module in process.Modules)
        {
            if (module == null)
            {
                continue;
            }

            string fileName = module.FileName;
            if (fileName.Contains("libc"))
            {
                if (fileName.Contains("musl"))
                {
                    return true;
                }

                break;
            }
        }

        return false;
    }

    private static string GetProcArchString()
    {
        Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;
        return processArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException(processArchitecture.ToString()),
        };
    }
}