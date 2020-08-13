using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum MemAllocType : uint
    {
        Commit = 0x1000,
        Reserve = 0x2000,

        Reset = 0x80000,
        Reset_Undo = 0x1000000,

        Top_Down = 0x100000,
        Physical = 0x400000,
        Large_Pages = 0x20000000,
    }

}
