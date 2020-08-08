using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AppBarData
    {
        public int cbSize;
        public IntPtr Hwnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public Int32Bound Bound;
        public int lParam;
    }
}
