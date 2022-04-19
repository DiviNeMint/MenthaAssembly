using System;

namespace MenthaAssembly.Media.Imaging
{
    [Flags]
    public enum ImageChannel : byte
    {
        R = 1,
        G = 2,
        B = 4,
        All = R | G | B
    }
}
