using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Utils
{
    public class HGlobalIntPtr : SafeHandle
    {
        private bool _IsInvalid = false;
        public override bool IsInvalid => _IsInvalid;

        public HGlobalIntPtr(long Length) : base(Marshal.AllocHGlobal(new IntPtr(Length)), true)
        {

        }
        public HGlobalIntPtr(IntPtr AllocHGlobal) : base(AllocHGlobal, true)
        {

        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            _IsInvalid = true;
            return true;
        }
    }
}
