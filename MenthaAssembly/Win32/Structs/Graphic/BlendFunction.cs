using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BlendFunction
    {
        public const int AC_SRC_OVER = 0x00;
        public const int AC_SRC_ALPHA = 0x01;

        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }
}