using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PropertyKey
    {
        public Guid FormatId;

        public uint PropertyId;

        public PropertyKey(Guid FormatId, uint PropertyId)
        {
            this.FormatId = FormatId;
            this.PropertyId = PropertyId;
        }

    }
}
