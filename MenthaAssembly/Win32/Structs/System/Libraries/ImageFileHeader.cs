using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ImageFileHeader
    {
        public ImageFileMachineType Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ImageFileCharFlags Characteristics;
    }
}