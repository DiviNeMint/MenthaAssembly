namespace MenthaAssembly.Win32
{
    internal enum FontClipPrecision : byte
    {
        Default = 0,
        CharActer = 1,
        Stroke = 2,
        Mask = 15,
        LH_Angles = 16,
        TT_Always = 32,
        DFA_Disable = 64,
        Embedded = 128,
    }
}
