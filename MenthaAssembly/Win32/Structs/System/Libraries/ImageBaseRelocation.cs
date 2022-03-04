using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ImageBaseRelocation
    {
        public int VirtualAddress;
        public int SizeOfBlock;
    }
}