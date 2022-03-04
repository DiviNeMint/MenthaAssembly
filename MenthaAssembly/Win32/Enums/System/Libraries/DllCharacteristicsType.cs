namespace MenthaAssembly.Win32
{
    internal enum DllCharacteristicsType : ushort
    {
        RES_0 = 0x0001,
        RES_1 = 0x0002,
        RES_2 = 0x0004,
        RES_3 = 0x0008,
        Dynamic_Base = 0x0040,
        Force_Integrity = 0x0080,
        NX_Compat = 0x0100,
        NO_Isolation = 0x0200,
        NO_SEH = 0x0400,
        NO_Bind = 0x0800,
        RES_4 = 0x1000,
        WDM_Driver = 0x2000,
        Terminal_Server_Aware = 0x8000
    }
}