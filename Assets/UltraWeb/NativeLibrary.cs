using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal abstract class NativeLibrary
{
    private interface INativeLibraryLoader
    {
        void Init();

        IntPtr Load(string libraryPath);

        IntPtr GetExport(IntPtr handle, string name);
    }

    private static class LibDl
    {
        internal static readonly INativeLibraryLoader Instance;

        static LibDl()
        {
            INativeLibraryLoader nativeLibraryLoader;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                nativeLibraryLoader = new LibDl1();
                nativeLibraryLoader.Init();
            }
            else
            {
                try
                {
                    nativeLibraryLoader = new LibDl2();
                    nativeLibraryLoader.Init();
                }
                catch
                {
                    nativeLibraryLoader = new LibDl1();
                    nativeLibraryLoader.Init();
                }
            }

            Instance = nativeLibraryLoader;
        }
    }

    private sealed class LibDl1 : INativeLibraryLoader
    {
        private const string LibName = "dl";

        [DllImport("dl", EntryPoint = "dlopen")]
        internal static extern IntPtr Load(string fileName, int flags);

        [DllImport("dl", EntryPoint = "dlsym")]
        private static extern IntPtr GetExport(IntPtr handle, string symbol);

        [DllImport("dl", EntryPoint = "dlerror")]
        internal unsafe static extern sbyte* GetLastError();

        unsafe IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            IntPtr intPtr = Load(libraryPath, 2);
            if (intPtr != (IntPtr)0)
            {
                return intPtr;
            }

            sbyte* lastError = GetLastError();
            if (lastError == null)
            {
                return (IntPtr)0;
            }

            throw new InvalidOperationException(new string(lastError));
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
        {
            return GetExport(handle, name);
        }

        public void Init()
        {
        }
    }

    private sealed class LibDl2 : INativeLibraryLoader
    {
        private const string LibName = "dl.so.2";

        [DllImport("dl.so.2", EntryPoint = "dlopen")]
        internal static extern IntPtr Load(string fileName, int flags);

        [DllImport("dl.so.2", EntryPoint = "dlsym")]
        private static extern IntPtr GetExport(IntPtr handle, string symbol);

        [DllImport("dl.so.2", EntryPoint = "dlerror")]
        internal unsafe static extern sbyte* GetLastError();

        unsafe IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            IntPtr intPtr = Load(libraryPath, 2);
            if (intPtr != (IntPtr)0)
            {
                return intPtr;
            }

            sbyte* lastError = GetLastError();
            if (lastError == null)
            {
                return (IntPtr)0;
            }

            throw new InvalidOperationException(new string(lastError));
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
        {
            return GetExport(handle, name);
        }

        public void Init()
        {
        }
    }

    private sealed class Kernel32 : INativeLibraryLoader
    {
        private const string LibName = "kernel32";

        internal static readonly INativeLibraryLoader Instance = new Kernel32();

        private Kernel32()
        {
        }

        [DllImport("kernel32", EntryPoint = "LoadLibrary", SetLastError = true)]
        private static extern IntPtr Load(string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetProcAddress")]
        private static extern IntPtr GetExport(IntPtr handle, string procedureName);

        IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            IntPtr intPtr = Load(libraryPath);
            if (intPtr != (IntPtr)0)
            {
                return intPtr;
            }

            if (Marshal.GetLastWin32Error() == 0)
            {
                return (IntPtr)0;
            }

            throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
        {
            return GetExport(handle, name);
        }

        public void Init()
        {
        }
    }

    internal delegate IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath);

    private static readonly INativeLibraryLoader Loader = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Kernel32.Instance : LibDl.Instance);

    private static readonly ConditionalWeakTable<Assembly, LinkedList<DllImportResolver>> Resolvers = new ConditionalWeakTable<Assembly, LinkedList<DllImportResolver>>();

    public static void SetDllImportResolver(Assembly assembly, DllImportResolver resolver)
    {
        lock (Resolvers)
        {
            Resolvers.GetOrCreateValue(assembly).AddLast(resolver);
        }
    }

    public static IntPtr GetExport(IntPtr handle, string name)
    {
        if (handle == (IntPtr)0)
        {
            throw new ArgumentNullException("handle");
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("name");
        }

        IntPtr export = Loader.GetExport(handle, name);
        if (export == (IntPtr)0)
        {
            throw new TypeLoadException("Entry point not found: " + name);
        }

        return export;
    }

    public static IntPtr Load(string libraryName, Assembly assembly, DllImportSearchPath? searchPath = null)
    {
        if (string.IsNullOrEmpty(libraryName))
        {
            throw new ArgumentNullException("libraryName");
        }

        if (assembly == null)
        {
            throw new ArgumentNullException("assembly");
        }

        lock (Resolvers)
        {
            if (!Resolvers.TryGetValue(assembly, out LinkedList<DllImportResolver> value))
            {
                return Load(libraryName);
            }

            foreach (DllImportResolver item in value)
            {
                IntPtr intPtr = item(libraryName, assembly, searchPath);
                if (intPtr != (IntPtr)0)
                {
                    return intPtr;
                }
            }

            IntPtr intPtr2 = Load(libraryName);
            if (intPtr2 == (IntPtr)0)
            {
                throw new InvalidProgramException(libraryName);
            }

            return intPtr2;
        }
    }

    public static IntPtr Load(string libraryPath)
    {
        if (string.IsNullOrEmpty(libraryPath))
        {
            throw new ArgumentNullException("libraryPath");
        }

        IntPtr intPtr = Loader.Load(libraryPath);
        if (intPtr == (IntPtr)0)
        {
            throw new InvalidProgramException(libraryPath);
        }

        return intPtr;
    }
}