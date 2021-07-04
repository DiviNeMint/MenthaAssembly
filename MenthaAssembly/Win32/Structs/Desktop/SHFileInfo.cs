using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct SHFileInfo
    {
        /// <summary>Maximal Length of unmanaged Windows-Path-strings</summary>
        private const int Max_Path = 260;
        /// <summary>Maximal Length of unmanaged Typename</summary>
        private const int Max_Ttpe = 80;

        public IntPtr hIcon;

        public int IconIndex;

        public uint Attributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Max_Path)]
        public string DisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Max_Ttpe)]
        public string TypeName;

    };
}
