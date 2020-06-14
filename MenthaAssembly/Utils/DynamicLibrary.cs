using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;

namespace MenthaAssembly
{
    public class DynamicLibrary
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string Path);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string ProcedureName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        private readonly ConcurrentDictionary<string, Delegate> MethodInfos = new ConcurrentDictionary<string, Delegate>();

        public string Path { get; }

        private readonly IntPtr pLibrary;
        private DynamicLibrary(string LibraryPath, IntPtr pLibrary)
        {
            this.Path = LibraryPath;
            this.pLibrary = pLibrary;
        }

        public TDelegate GetMethod<TDelegate>(string FunctionName)
            where TDelegate : Delegate
        {
            if (MethodInfos.TryGetValue(FunctionName, out Delegate MethodBase) &&
                MethodBase is TDelegate Method)
                return Method;

            IntPtr pProc = GetProcAddress(pLibrary, FunctionName);

            Method = Marshal.GetDelegateForFunctionPointer<TDelegate>(pProc);
            MethodInfos.AddOrUpdate(FunctionName, Method, (k, v) => Method);
            return Method;
        }

        public void Invoke<TDelegate>(string FunctionName, params object[] Args)
            where TDelegate : Delegate
            => GetMethod<TDelegate>(FunctionName).DynamicInvoke(Args);
        public TResult Invoke<TDelegate, TResult>(string FunctionName, params object[] Args)
            where TDelegate : Delegate
            => (TResult)GetMethod<TDelegate>(FunctionName).DynamicInvoke(Args);

        ~DynamicLibrary()
        {
            FreeLibrary(pLibrary);
        }

        public static DynamicLibrary Load(string Path)
        {
            if (!File.Exists(Path))
                throw new FileNotFoundException("", Path);

            IntPtr pLibrary = LoadLibrary(Path);
            if (pLibrary == IntPtr.Zero)
                throw new ApplicationException($"There was an error during dll loading : {Path}, ErrorCode : {Marshal.GetLastWin32Error()}");

            return new DynamicLibrary(Path, pLibrary);
        }
        public static bool TryLoad(string Path, out DynamicLibrary DynamicLibrary)
        {
            if (!File.Exists(Path))
            {
                DynamicLibrary = null;
                return false;
            }

            IntPtr pLibrary = LoadLibrary(Path);
            if (pLibrary == IntPtr.Zero)
            {
                DynamicLibrary = null;
                return false;
            }

            DynamicLibrary = new DynamicLibrary(Path, pLibrary);
            return true;
        }

    }
}
