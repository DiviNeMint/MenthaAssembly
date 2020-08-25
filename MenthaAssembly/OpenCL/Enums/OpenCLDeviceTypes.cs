using System;

namespace MenthaAssembly.OpenCL
{
    /// <summary>
    /// The types of devices.
    /// </summary>
    [Flags]
    public enum OpenCLDeviceTypes : long
    {
        Default = 1,
        CPU = 2,
        GPU = 4,
        Accelerator = 8,
        All = 0xFFFFFFFF
    }
}
