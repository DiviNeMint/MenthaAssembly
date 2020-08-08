using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32.Primitives
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TBButtonInfo
    {
        public int cbSize;
        public TBButtonFlag dwMask;
        public int idCommand;
        public int iImage;
        public TBButtonState fsState;
        private readonly byte fsStyle;
        public TBButtonStyle Style
            => (TBButtonStyle)this.fsStyle;
        public short cx;
        public IntPtr lParam;
        public IntPtr pszText;
        public int cchText;
    }
}
