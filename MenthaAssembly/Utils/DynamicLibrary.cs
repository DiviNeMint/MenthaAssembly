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

        private readonly LibraryIntPtr pLibrary;
        private DynamicLibrary(string LibraryPath, LibraryIntPtr pLibrary)
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

            if (pLibrary.IsInvalid)
                return null;

            IntPtr pProc = GetProcAddress(pLibrary.DangerousGetHandle(), FunctionName);

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

        //~DynamicLibrary()
        //{
        //    FreeLibrary(pLibrary);
        //}

        public static DynamicLibrary Load(string Path)
        {
            if (!File.Exists(Path))
                throw new FileNotFoundException("", Path);

            IntPtr pLibrary = LoadLibrary(Path);
            if (pLibrary == IntPtr.Zero)
                throw new ApplicationException($"There was an error during dll loading : {Path}, ErrorCode : {Marshal.GetLastWin32Error()}");

            return new DynamicLibrary(Path, new LibraryIntPtr(pLibrary));
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

            DynamicLibrary = new DynamicLibrary(Path, new LibraryIntPtr(pLibrary));
            return true;
        }

        private class LibraryIntPtr : SafeHandle
        {
            private bool _IsInvalid = false;
            public override bool IsInvalid => _IsInvalid;

            public LibraryIntPtr(IntPtr Handle) : base(Handle, true)
            {

            }

            protected override bool ReleaseHandle()
            {
                FreeLibrary(handle);
                _IsInvalid = true;
                return true;
            }
        }

    }
}
