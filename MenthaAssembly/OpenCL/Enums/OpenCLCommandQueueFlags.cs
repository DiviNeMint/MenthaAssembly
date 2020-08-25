using System;

namespace MenthaAssembly.OpenCL
{
    [Flags]
    public enum OpenCLCommandQueueFlags : long
    {
        None = 0,
        OutOfOrderExecution = 1,
        Profiling = 2
    }
}
