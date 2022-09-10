using System;
using System.Linq;
using System.Runtime.InteropServices;
using static MenthaAssembly.Devices.Printer;

namespace MenthaAssembly.Devices
{
    public unsafe class PrinterInfo
    {
        private readonly string PortName;
        private PrinterMode Mode;

        public string Name { get; }

        private PrinterStatus _Status;
        public PrinterStatus Status
        {
            get
            {
                UpdatePrinterMode();
                return _Status;
            }
        }

        private PrinterPaper[] _SupportPapers;
        public PrinterPaper[] SupportPapers
        {
            get
            {
                if (_SupportPapers is null)
                    _SupportPapers = GetSupportPapers(Mode.dmFields, Name, PortName);

                return _SupportPapers;
            }
        }

        public PrinterPaper Paper
        {
            get
            {
                UpdatePrinterMode();
                return SupportPapers.FirstOrDefault(i => i.Type.Equals(Mode.dmPaperSize)) ?? new PrinterPaper("Custom", Mode.dmPaperWidth / 10d, Mode.dmPaperLength / 10d, LengthUnit.mm);
            }
        }

        internal PrinterInfo(PRINTER_INFO_2 Info)
        {
            Name = Info.PrinterName;
            PortName = Info.PortName;
            Mode = Marshal.PtrToStructure<PrinterMode>(Info.pDevMode);
        }

        public bool Print(byte[] RawDatas)
        {
            if (OpenPrinter(Name, out IntPtr hPrinter, IntPtr.Zero) > 0)
            {
                try
                {
                    DocInfo Doc = new DocInfo
                    {
                        Name = "Document",
                        DataType = "Raw"
                    };

                    if (StartDocPrinter(hPrinter, 1, Doc))
                    {
                        try
                        {
                            // Start a page.
                            if (StartPagePrinter(hPrinter))
                            {
                                try
                                {
                                    // Send the data to the printer.
                                    return WritePrinter(hPrinter, RawDatas.ToPointer(), RawDatas.Length, out int Written) && RawDatas.Length == Written;
                                }
                                finally
                                {
                                    EndPagePrinter(hPrinter);
                                }
                            }
                        }
                        finally
                        {
                            // Inform the spooler that the document is ending. 
                            EndDocPrinter(hPrinter);
                        }
                    }
                }
                finally
                {
                    ClosePrinter(hPrinter);
                }
            }

            return false;
        }

        private void UpdatePrinterMode()
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
                            {
                                PRINTER_INFO_2 Info = Marshal.PtrToStructure<PRINTER_INFO_2>(pPrinter);
                                _Status = Info.Status;

                                Mode = Marshal.PtrToStructure<PrinterMode>(Info.pDevMode);
                            }
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
        }

        public override string ToString()
            => $"{Name}";

    }
}