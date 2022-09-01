using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct DocInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;

        [MarshalAs(UnmanagedType.LPStr)]
        public string OutputFile;

        [MarshalAs(UnmanagedType.LPStr)]
        public string DataType;

    }
}