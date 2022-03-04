using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct ImageSectionHeader
    {
        [FieldOffset(0)]
        public fixed byte Name[8];

        [FieldOffset(8)]
        public uint VirtualSize;

        [FieldOffset(8)]
        public IntPtr PhysicalAddress;

        [FieldOffset(12)]
        public uint VirtualAddress;

        [FieldOffset(16)]
        public uint SizeOfRawData;

        [FieldOffset(20)]
        public uint PointerToRawData;

        [FieldOffset(24)]
        public uint PointerToRelocations;

        [FieldOffset(28)]
        public uint PointerToLinenumbers;

        [FieldOffset(32)]
        public ushort NumberOfRelocations;

        [FieldOffset(34)]
        public ushort NumberOfLinenumbers;

        [FieldOffset(36)]
        public ImageDataSectionFlags Characteristics;

    }
}