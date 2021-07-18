using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-mat2">Mat2</see>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Mat2
    {
        public Fixed eM11;
        public Fixed eM12;
        public Fixed eM21;
        public Fixed eM22;
    }
}
