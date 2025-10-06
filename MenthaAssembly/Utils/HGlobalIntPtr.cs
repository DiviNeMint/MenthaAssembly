using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Utils
{
    public class HGlobalIntPtr : SafeHandleZeroOrMinusOneIsInvalid
    {
        public HGlobalIntPtr(long Length) : base(true)
        {
            SetHandle(Marshal.AllocHGlobal(new IntPtr(Length)));
        }
        public HGlobalIntPtr(IntPtr AllocHGlobal) : base(true)
        {
            SetHandle(AllocHGlobal);
        }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
            }
            return true;
        }
    }
}
