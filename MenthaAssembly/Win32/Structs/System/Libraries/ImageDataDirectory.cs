using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ImageDataDirectory
    {
        public int VirtualAddress;
        public int Size;
    }
}