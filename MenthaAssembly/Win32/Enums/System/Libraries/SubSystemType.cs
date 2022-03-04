namespace MenthaAssembly.Win32
{
    public enum SubSystemType : ushort
    {
        Unknown = 0,
        Native = 1,
        Windows_GUI = 2,
        Windows_CUI = 3,
        POSIX_CUI = 7,
        Windows_CE_GUI = 9,
        EFI_Application = 10,
        EFI_Boot_Service_Driver = 11,
        EFI_Runtime_Driver = 12,
        EFI_ROM = 13,
        XBOX = 14
    }
}