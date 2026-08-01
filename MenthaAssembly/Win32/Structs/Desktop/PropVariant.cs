using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PropVariant : IDisposable
    {
        private const ushort EmptyValueType = 0;
        private const ushort BooleanValueType = 11;
        private const ushort UnicodeStringValueType = 31;

        private ushort ValueType;

        private ushort Reserved1;

        private ushort Reserved2;

        private ushort Reserved3;

        private IntPtr Value;

        public PropVariant(bool Value)
        {
            this = default;
            ValueType = BooleanValueType;
            this.Value = Value ? new IntPtr(-1) : IntPtr.Zero;
        }

        public PropVariant(string Value)
        {
            this = default;
            ValueType = UnicodeStringValueType;
            this.Value = Marshal.StringToCoTaskMemUni(Value);
        }

        public void Dispose()
        {
            if (ValueType == UnicodeStringValueType && Value != IntPtr.Zero)
                Marshal.FreeCoTaskMem(Value);

            Value = IntPtr.Zero;
            ValueType = EmptyValueType;
        }

    }
}
