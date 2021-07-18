using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-glyphmetrics">GlyphMetrics</see>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphMetrics
    {
        public int BlackBoxX;
        public int BlackBoxY;
        public int GlyphOriginX;
        public int GlyphOriginY;
        public short CellIncX;
        public short CellIncY;
    }
}
