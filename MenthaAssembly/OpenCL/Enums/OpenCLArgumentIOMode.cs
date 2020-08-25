using System;

namespace MenthaAssembly.OpenCL
{
    [Flags]
    public enum OpenCLArgumentIOMode
    {
        Unknown = 0,
        In = 1,
        Out = 2,
        InOut = In | Out
    }
}
