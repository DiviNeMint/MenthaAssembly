using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum ProcessRights : uint
    {
        AllAccess = Standard_Rights_Required | Synchronize | 0xFFF,

        Terminate = 0x0001,
        Create_Thread = 0x0002,
        SET_SessionId = 0x0004,
        VM_Operation = 0x0008,
        VM_Read = 0x0010,
        VM_Write = 0x0020,
        DUP_Handle = 0x0040,
        Create_Process = 0x0080,
        Set_Quota = 0x0100,
        Set_Information = 0x0200,
        Query_Information = 0x0400,
        Suspend_Resume = 0x0800,
        Query_Limited_Information = 0x1000,

        Delete = 0x00010000,
        Read_Control = 0x00020000,
        Write_DAC = 0x00040000,
        Write_Owner = 0x00080000,

        Standard_Rights_Required = 0x000F0000,
        Synchronize = 0x00100000

    }
}
