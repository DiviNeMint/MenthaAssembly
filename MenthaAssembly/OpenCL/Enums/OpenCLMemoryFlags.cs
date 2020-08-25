using System;

namespace MenthaAssembly.OpenCL.Primitives
{
    [Flags]
    internal enum OpenCLMemoryFlags : long
    {
        None = 0,

        /// <summary> 
        /// The <see cref="OpenCLMemory"/> will be accessible from the <see cref="OpenCLKernel"/> for read and write operations.
        /// </summary>
        ReadWrite = 1,

        /// <summary> 
        /// The <see cref="OpenCLMemory"/> will be accessible from the <see cref="OpenCLKernel"/> for write operations only. 
        /// </summary>
        WriteOnly = 2,

        /// <summary> 
        /// The <see cref="OpenCLMemory"/> will be accessible from the <see cref="OpenCLKernel"/> for read operations only. 
        /// </summary>
        ReadOnly = 4,

        UseHostPointer = 8,
        AllocateHostPointer = 16,
        CopyHostPointer = 32
    }
}
