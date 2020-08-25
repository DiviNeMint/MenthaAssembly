using System;

namespace MenthaAssembly.OpenCL
{
    [Flags]
    public enum OpenCLSingleCapabilities : long
    {
        Denorm = 1,
        InfNan = 2,
        RoundToNearest = 4,
        RoundToZero = 8,
        RoundToInf = 16,
        Fma = 32,
        SoftFloat = 64
    }
}
