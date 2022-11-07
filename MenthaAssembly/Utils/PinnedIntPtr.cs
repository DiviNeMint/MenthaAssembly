using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Utils
{
    public class PinnedIntPtr : SafeHandle
    {
        public override bool IsInvalid
            => !Handle.IsAllocated;

        private readonly GCHandle Handle;
        public PinnedIntPtr(object Value) : base(IntPtr.Zero, true)
        {
            Handle = GCHandle.Alloc(Value, GCHandleType.Pinned);
            SetHandle(Handle.AddrOfPinnedObject());
        }

        protected override bool ReleaseHandle()
        {
            Handle.Free();
            return true;
        }

    }
}