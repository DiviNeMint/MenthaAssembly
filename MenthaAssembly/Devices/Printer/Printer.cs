using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Devices
{
    public static unsafe partial class Printer
    {
        #region Windows APIs
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int OpenPrinter(string PrinterName, out IntPtr hPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        internal static extern int ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetPrinter(IntPtr hPrinter, int dwLevel, IntPtr pPrinter, int dwBuf, out int dwNeeded);

        [DllImport("winspool.Drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool StartDocPrinter(IntPtr hPrinter, int dwLevel, DocInfo Doc);

        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [DllImport("winspool.Drv", SetLastError = true)]
        internal static extern bool WritePrinter(IntPtr hPrinter, byte* pBytes, int dwCount, out int dwWritten);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool EnumPrinters(PrinterFlags Flags, string Name, uint Level, IntPtr pPrinterEnum, uint cbBuf, out uint pcbNeeded, out uint pcReturned);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetDefaultPrinter(StringBuilder pszBuffer, out int pcchBuffer);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int DeviceCapabilities(string Device, string Port, DeviceCapabilitiesFlags Capability, IntPtr pOutputBuffer, IntPtr pDeviceMode);

        #endregion

        public static IEnumerable<PrinterInfo> Printers
            => GetPrinters(PrinterFlags.Local | PrinterFlags.Connections);

        public static IEnumerable<string> PrinterNames
            => GetInternalPrinterInfos(PrinterFlags.Local | PrinterFlags.Connections).Select(i => i.PrinterName);

        public static string DefaultPrinterName
        {
            get
            {
                if (GetDefaultPrinter(null, out int Length))
                    return null;

                StringBuilder Buffer = new StringBuilder(Length);
                try
                {
                    if (GetDefaultPrinter(Buffer, out Length))
                        return Buffer.ToString();

                    return null;
                }
                finally
                {
                    Buffer.Clear();
                }
            }
        }

        public static PrinterInfo GetPrinter(string Name)
        {
            if (OpenPrinter(Name, out IntPtr hPrinter, IntPtr.Zero) > 0)
            {
                try
                {
                    GetPrinter(hPrinter, 2, IntPtr.Zero, 0, out int Size);
                    if (Size > 0)
                    {
                        IntPtr pPrinter = Marshal.AllocHGlobal(Size);

                        try
                        {
                            if (GetPrinter(hPrinter, 2, pPrinter, Size, out _))
                                return new PrinterInfo(Marshal.PtrToStructure<PRINTER_INFO_2>(pPrinter));
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(pPrinter);
                        }
                    }
                }
                finally
                {
                    ClosePrinter(hPrinter);
                }
            }

            return null;
        }

        public static IEnumerable<PrinterInfo> GetPrinters(PrinterFlags Flags)
            => GetInternalPrinterInfos(Flags).Select(i => new PrinterInfo(i));

        internal static IEnumerable<PRINTER_INFO_2> GetInternalPrinterInfos(PrinterFlags Flags)
        {
            if (EnumPrinters(Flags, null, 2, IntPtr.Zero, 0, out uint cbNeeded, out _))
                yield break;

            IntPtr pAddr = Marshal.AllocHGlobal((int)cbNeeded);
            try
            {
                if (EnumPrinters(Flags, null, 2, pAddr, cbNeeded, out _, out uint cReturned))
                {
                    IntPtr pInfo = pAddr;
                    int InfoSize = Marshal.SizeOf<PRINTER_INFO_2>();
                    for (int i = 0; i < cReturned; i++)
                    {
                        yield return Marshal.PtrToStructure<PRINTER_INFO_2>(pInfo);
                        pInfo += InfoSize;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pAddr);
            }
        }

        internal static PrinterPaper[] GetSupportPapers(PrinterModeFlags ModeFlag, string PrinterName, string PortName)
        {
            if ((ModeFlag & PrinterModeFlags.PaperSize) > 0)
            {
                int Count = DeviceCapabilities(PrinterName, PortName, DeviceCapabilitiesFlags.PaperNames, IntPtr.Zero, IntPtr.Zero);
                if (Count > 0)
                {
                    PrinterPaper[] Papers = new PrinterPaper[Count];
                    int NameLength = Marshal.SystemDefaultCharSize << 6;
                    IntPtr NamesBuffer = Marshal.AllocCoTaskMem(NameLength * Count),
                           KindsBuffer = Marshal.AllocCoTaskMem(Count << 1),
                           SizesBuffer = Marshal.AllocCoTaskMem(Count << 3);

                    try
                    {
                        DeviceCapabilities(PrinterName, PortName, DeviceCapabilitiesFlags.PaperNames, NamesBuffer, IntPtr.Zero);
                        DeviceCapabilities(PrinterName, PortName, DeviceCapabilitiesFlags.Papers, KindsBuffer, IntPtr.Zero);
                        DeviceCapabilities(PrinterName, PortName, DeviceCapabilitiesFlags.PaperSize, SizesBuffer, IntPtr.Zero);

                        IntPtr pNames = NamesBuffer;
                        PaperTypes* pKinds = (PaperTypes*)KindsBuffer;
                        int* pSizes = (int*)SizesBuffer;

                        for (int i = 0; i < Count; i++, pNames += NameLength)
                            Papers[i] = new PrinterPaper(Marshal.PtrToStringAuto(pNames, 64).TrimEnd('\0'), *pKinds++, *pSizes++ / 10d, *pSizes++ / 10d, LengthUnit.mm);

                        return Papers;
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(NamesBuffer);
                        Marshal.FreeCoTaskMem(KindsBuffer);
                        Marshal.FreeCoTaskMem(SizesBuffer);
                    }
                }
            }

            return new PrinterPaper[0];
        }

    }
}