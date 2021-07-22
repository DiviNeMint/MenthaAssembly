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
        public Bound<int> Bound;
        public int lParam;
    }
}
