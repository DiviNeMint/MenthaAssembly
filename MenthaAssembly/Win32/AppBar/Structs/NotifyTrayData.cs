using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct NotifyTrayData
    {
        public IntPtr Hwnd;
        public uint Uid;
        public int CallbackMessageId;

        private short _States;
        public NotifyIconState State => (NotifyIconState)_States;

        public short UnknownField;

        public int Reserved1;
        public IntPtr hIcon;

#if _WIN32
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
#else
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
#endif
        public int[] Reserved2;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szExePath;

    }
}
