using System;

namespace MenthaAssembly.IO
{
    [Flags]
    public enum StreamAccess
    {
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write
    }
}
