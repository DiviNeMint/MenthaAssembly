using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct PRINTER_INFO_2
    {
        [MarshalAs(UnmanagedType.LPTStr)]
        public string ServerName;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string PrinterName;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string ShareName;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string PortName;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string DriverName;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string Comment;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string Location;

        public IntPtr pDevMode;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string SepFile;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string PrintProcessor;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string Datatype;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string Parameters;

        public IntPtr pSecurityDescriptor;

        public PrinterAttributes Attributes;

        public uint Priority;

        public uint DefaultPriority;

        public uint StartTime;

        public uint UntilTime;

        public PrinterStatus Status;

        public uint cJobs;

        public uint AveragePPM;

    }

}