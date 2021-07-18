using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/zh-tw/windows/win32/api/wingdi/ns-wingdi-textmetrica">TextMetric</see>
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct TextMetric
    {
        public int Height;
        public int Ascent;
        public int Descent;
        public int InternalLeading;
        public int ExternalLeading;
        public int AveCharWidth;
        public int MaxCharWidth;
        public int Weight;
        public int Overhang;
        public int DigitizedAspectX;
        public int DigitizedAspectY;
        public byte FirstChar;    // this assumes the ANSI charset; for the UNICODE charset the type is char (or short)
        public byte LastChar;     // this assumes the ANSI charset; for the UNICODE charset the type is char (or short)
        public byte DefaultChar;  // this assumes the ANSI charset; for the UNICODE charset the type is char (or short)
        public byte BreakChar;    // this assumes the ANSI charset; for the UNICODE charset the type is char (or short)
        public byte Italic;
        public byte Underlined;
        public byte StruckOut;
        public FontPitchAndFamily PitchAndFamily;
        public FontCharSet CharSet;
    }
}
