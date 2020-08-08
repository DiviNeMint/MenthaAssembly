using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NotifyIconIdentifier
    {
        public int cbSize;
        public IntPtr Hwnd;
        public uint Uid;
        public Guid Guid;
    }
}
