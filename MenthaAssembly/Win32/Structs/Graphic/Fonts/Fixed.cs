using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-fixed">Fixed</see>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Fixed
    {
        public short Fract;
        public short Value;
    }
}
