namespace MenthaAssembly.Win32
{
    internal enum GetGlyphOutlineFormats : uint
    {
        Metrics = 0,
        Bitmap = 1,
        Native = 2,
        Bezier = 3,
        Gray2_Bitmap = 4,
        Gray4_Bitmap = 5,
        Gray8_Bitmap = 6,
        Glyph_Index = 0x80,
        Unhinted = 0x100,
    }
}
