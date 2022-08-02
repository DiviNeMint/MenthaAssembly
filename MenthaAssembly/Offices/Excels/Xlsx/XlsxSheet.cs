using MenthaAssembly.Offices.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using static MenthaAssembly.Offices.Primitives.XmlReaderExcelHelper;

namespace MenthaAssembly.Offices
{
    internal unsafe class XlsxSheet : IExcelSheet
    {
        internal readonly XlsxWorkbook Parent;
        internal readonly XlsxSheetInfo Info;

        public string Name
            => Info.Name;

        public string CodeName { get; }

        public bool Visible
            => Info.Visible;

        private readonly double DefaultRowHeight;
        double IExcelSheet.DefaultRowHeight
            => DefaultRowHeight;

        private readonly double DefaultColumnWidth = 8.43d;
        double IExcelSheet.DefaultColumnWidth
            => DefaultColumnWidth;

        public ExcelHeaderFooter HeaderFooter { get; }

        public ExcelRowCollection Rows { get; }

        public ExcelColumnCollection Columns { get; }

        private readonly ExcelCellCollection Cells;

        public ExcelCell this[int Column, int Row]
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(nameof(XlsxSheet));

                if (Column >= Columns.Length)
                    throw new IndexOutOfRangeException($"Column index is out of range.");

                if (Row >= Rows.Length)
                    throw new IndexOutOfRangeException($"Row index is out of range.");

                return EnumCells().First(i => i.ColumnIndex == Column && i.RowIndex == Row);
            }
        }

        public ExcelCellRange[] MergeCells { get; }

        public IExcelWorkbook Workbook
            => Parent;

        private Stream CompressedStream;
        private readonly Func<Stream, Stream> GetDecompressor;
        private XmlReaderSettings XmlSettings;

        private XlsxSheet(XlsxWorkbook Parent, XlsxSheetInfo Info)
        {
            this.Parent = Parent;
            this.Info = Info;
            DefaultRowHeight = 15d;
            Columns = new ExcelColumnCollection(this);
            Rows = new ExcelRowCollection(this);
            Cells = new ExcelCellCollection(this);
        }
        public XlsxSheet(XlsxWorkbook Parent, XlsxSheetInfo Info, Stream Stream, Func<Stream, Stream> GetDecompressor) : this(Parent, Info)
        {
            bool Reread = GetDecompressor != null;
            if (Reread)
            {
                CompressedStream = Stream;
                this.GetDecompressor = GetDecompressor;
                Stream = GetDecompressor(Stream);
            }

            int RowIndex = 0;
            ExcelRowCells CurrentRowCells = null;
            List<ExcelCellRange> MergeCellList = new List<ExcelCellRange>();

            // https://interoperability.blob.core.windows.net/files/MS-XLSB/%5bMS-XLSB%5d.pdf
            using XlsxBiffReader Reader = new XlsxBiffReader(Stream);
            while (Reader.ReadVariable(out int Id, out _))
            {
                switch (Id)
                {
                    #region Cells
                    case 0x01:  // Empty
                        {
                            if (Reread)
                                break;

                            int column = Reader.Read<int>(),
                                xfIndex = Reader.Read<int>() & 0xFFFFFF;

                            Columns.SetMaxIndex(column);
                            CurrentRowCells.Add(new ExcelCell(this, RowIndex, column, null, -1, xfIndex, null));
                            break;
                        }
                    case 0x02:  // Number
                        {
                            if (Reread)
                                break;

                            int column = Reader.Read<int>(),
                                xfIndex = Reader.Read<int>() & 0xFFFFFF;
                            object RkNumber = Reader.ReadRkNumber();

                            Columns.SetMaxIndex(column);
                            CurrentRowCells.Add(new ExcelCell(this, RowIndex, column, RkNumber, -1, xfIndex, null));
                            break;
                        }
                    case 0x03:  // Error
                    case 0x0B:  // Formula Error
                        {
                            if (Reread)
                                break;

                            int column = Reader.Read<int>(),
                                xfIndex = Reader.Read<int>() & 0xFFFFFF;
                            ExcelCellError Error = Reader.Read<ExcelCellError>();

                            Columns.SetMaxIndex(column);
                            CurrentRowCells.Add(new ExcelCell(this, RowIndex, column, null, -1, xfIndex, Error));
                            break;
                        }
                    case 0x04:  // Boolean
                    case 0x0A:  // Formula Boolean
                        {
                            if (Reread)
                                break;

                            int column = Reader.Read<int>(),
                                xfIndex = Reader.Read<int>() & 0xFFFFFF;
                            bool Bool = Reader.Read<bool>();

                            Columns.SetMaxIndex(column);
                            CurrentRowCells.Add(new ExcelCell(this, RowIndex, column, Bool, -1, xfIndex, null));
                            break;
                        }
                    case 0x05:  // Float
                    case 0x09:  // Formula Float
                        {
                            if (Reread)
                                break;

                            int column = Reader.Read<int>(),
                                xfIndex = Reader.Read<int>() & 0xFFFFFF;
                            double Float = Reader.Read<double>();

                            Columns.SetMaxIndex(column);
                            CurrentRowCells.Add(new ExcelCell(this, RowIndex, column, Float, -1, xfIndex, null));
                            break;
                        }
                    case 0x06:  // String
                    case 0x08:  // Formula String
                        {
                            if (Reread)
                                break;

                            int column = Reader.Read<int>(),
                                xfIndex = Reader.Read<int>() & 0xFFFFFF;
                            string Content = Reader.ReadString();

                            Columns.SetMaxIndex(column);
                            CurrentRowCells.Add(new ExcelCell(this, RowIndex, column, Content, -1, xfIndex, null));
                            break;
                        }
                    case 0x07:  // SST
                        {
                            if (Reread)
                                break;

                            int column = Reader.Read<int>(),
                                xfIndex = Reader.Read<int>() & 0xFFFFFF,
                                SSTIndex = Reader.Read<int>();

                            Columns.SetMaxIndex(column);
                            CurrentRowCells.Add(new ExcelCell(this, RowIndex, column, null, SSTIndex, xfIndex, null));
                            break;
                        }
                    #endregion
                    #region Column
                    case 0x3C:
                        {
                            int Min = Reader.Read<int>(),
                                Max = Reader.Read<int>(),
                                coldx = Reader.Read<int>();

                            Reader.Skip(4); // Skip ixfe.

                            byte Flags = Reader.Read<byte>();
                            bool Hidden = (Flags & 0b01) > 0,
                                 CustomWidth = (Flags & 0b10) > 0;

                            double Width = CustomWidth ? Math.Floor(coldx / 256d) : -1;
                            Columns.Add(new ExcelColumn(this, Min, Max, Hidden, Width));
                            break;
                        }
                    #endregion
                    #region Row
                    case 0x00:
                        {
                            RowIndex = Reader.Read<int>();

                            Reader.Skip(4); // Skip ixfe.

                            ushort miyRw = Reader.Read<ushort>();

                            Reader.Skip(1); // Skip fExtraAsc(1 bit)、fExtraDsc(1 bit)、reserved(6 bits).

                            byte flags = Reader.Read<byte>();

                            bool Hidden = (flags & 0b10000) != 0,
                                 CustomHeight = (flags & 0b100000) != 0;

                            double Height = CustomHeight ? miyRw / 15d : -1;
                            Rows.Add(new ExcelRow(this, RowIndex, Hidden, Height));

                            // Check every time or one time ?
                            if (Columns.Length == 0)
                            {
                                Reader.Skip(1); // Skip fPhShow(1 bit)、reserved(7 bits)

                                uint ccolspan = Reader.Read<uint>();

                                Reader.Skip(((int)((ccolspan - 1) << 3) + 4));    // Skip to last column index

                                int LastColumnIndex = Reader.Read<int>();
                                Columns.SetMaxIndex(LastColumnIndex);
                            }

                            if (!Reread)
                                CurrentRowCells = Cells.Create(RowIndex);

                            break;
                        }
                    #endregion
                    #region MergeCell
                    case 0xB0:
                        {
                            int FromRow = Reader.Read<int>(),
                                ToRow = Reader.Read<int>(),
                                FromColumn = Reader.Read<int>(),
                                ToColumn = Reader.Read<int>();

                            MergeCellList.Add(new ExcelCellRange(FromColumn, FromRow, ToColumn, ToRow));
                            break;
                        }
                    #endregion
                    #region HeaderFooter
                    case 0x1DF:
                        {
                            byte Flags = Reader.Read<byte>();
                            bool DifferentOddEven = (Flags & 0b01) != 0,
                                 DifferentFirst = (Flags & 0b10) != 0;

                            Reader.Skip(1);

                            string Header = Reader.ReadString(),
                                   Footer = Reader.ReadString(),
                                   HeaderEven = Reader.ReadString(),
                                   FooterEven = Reader.ReadString(),
                                   HeaderFirst = Reader.ReadString(),
                                   FooterFirst = Reader.ReadString();

                            HeaderFooter = new ExcelHeaderFooter()
                            {
                                _OddHeader = Header,
                                _OddFooter = Footer,
                                _EvenHeader = HeaderEven,
                                _EvenFooter = FooterEven,
                                _FirstHeader = HeaderFirst,
                                _FirstFooter = FooterFirst,
                                _HasDifferentFirst = DifferentFirst,
                                _HasDifferentOddEven = DifferentOddEven
                            };
                            break;
                        }
                    #endregion
                    #region Properties
                    case 0x93:
                        {
                            Reader.Skip(19);
                            CodeName = Reader.ReadString();
                            break;
                        }
                    #endregion
                    #region Format Properties
                    case 0x1E5:
                        {
                            uint dxGCol = Reader.Read<uint>();
                            ushort cchDefColWidth = Reader.Read<ushort>(),
                                   miyDefRwHeight = Reader.Read<ushort>();

                            DefaultColumnWidth = dxGCol == uint.MaxValue ? cchDefColWidth : dxGCol * 256;
                            DefaultRowHeight = miyDefRwHeight / 15d; // Tips to Pixels.
                            break;
                        }
                        #endregion
                }
            }

            MergeCells = MergeCellList.ToArray();
        }
        public XlsxSheet(XlsxWorkbook Parent, XlsxSheetInfo Info, Stream Stream, Func<Stream, Stream> GetDecompressor, XmlReaderSettings XmlSettings) : this(Parent, Info)
        {
            bool Reread = GetDecompressor != null;
            if (Reread)
            {
                CompressedStream = Stream;
                this.GetDecompressor = GetDecompressor;
                Stream = GetDecompressor(Stream);
            }

            this.XmlSettings = XmlSettings;

            using XmlReader Reader = XmlReader.Create(Stream, XmlSettings);

            if (!Reader.IsStartElement(NWorksheet, NsSpreadsheetMl))
                return;

            if (!Reader.ReadFirstContent())
                return;

            while (!Reader.EOF)
            {
                #region Rows & Cells
                if (Reader.IsStartElement(NSheetData, NsSpreadsheetMl))
                {
                    if (!Reader.ReadFirstContent())
                        continue;

                    int RowIndex = 0;
                    while (!Reader.EOF)
                    {
                        if (Reader.IsStartElement(NRow, NsSpreadsheetMl))
                        {
                            if (int.TryParse(Reader.GetAttribute(AR), out int arValue))
                                RowIndex = arValue - 1;    // The row attribute is 1-based

                            bool CustomHeight = Reader.GetAttribute(ACustomHeight) == "1",
                                 Hidden = Reader.GetAttribute(AHidden) == "1";

                            double Height = CustomHeight && double.TryParse(Reader.GetAttribute(AHt), NumberStyles.Any, CultureInfo.InvariantCulture, out double Value) ? Value : -1;

                            Rows.Add(new ExcelRow(this, RowIndex, Hidden, Height));

                            if (Reread)
                            {
                                Reader.Skip();
                                continue;
                            }

                            if (!Reader.ReadFirstContent())
                                continue;

                            int ColumnIndex = 0;

                            ExcelRowCells RowCells = Cells.Create(RowIndex);
                            while (!Reader.EOF)
                            {
                                if (Reader.IsStartElement(NC, NsSpreadsheetMl))
                                {
                                    if (Reader.GetAttribute(AR).TryParseReference(out int ReferenceColumn, out _))
                                        ColumnIndex = ReferenceColumn - 1;  // ParseReference is 1-based

                                    string aS = Reader.GetAttribute(AS);
                                    int xfIndex = !string.IsNullOrEmpty(aS) &&
                                                  int.TryParse(aS, NumberStyles.Any, CultureInfo.InvariantCulture, out int StyleIndex) ? StyleIndex : -1;

                                    string aT = Reader.GetAttribute(AT);
                                    if (!Reader.ReadFirstContent())
                                    {
                                        Columns.SetMaxIndex(ColumnIndex);
                                        RowCells.Add(new ExcelCell(this, RowIndex, ColumnIndex++, null, -1, xfIndex, null));
                                        continue;
                                    }

                                    object CellValue = null;
                                    int SSTIndex = -1;
                                    ExcelCellError? Error = null;

                                    while (!Reader.EOF)
                                    {
                                        if (Reader.IsStartElement(NV, NsSpreadsheetMl))
                                        {
                                            string RawValue = Reader.ReadElementContentAsString();

                                            if (!string.IsNullOrEmpty(RawValue))
                                                CellValue = ConvertCellValue(RawValue, aT, out SSTIndex, out Error);
                                        }
                                        else if (Reader.IsStartElement(NIs, NsSpreadsheetMl))
                                        {
                                            string RawValue = Reader.ReadStringItem();

                                            if (!string.IsNullOrEmpty(RawValue))
                                                CellValue = ConvertCellValue(RawValue, aT, out SSTIndex, out Error);
                                        }
                                        else if (!Reader.SkipContent())
                                        {
                                            break;
                                        }
                                    }

                                    Columns.SetMaxIndex(ColumnIndex);
                                    RowCells.Add(new ExcelCell(this, RowIndex, ColumnIndex++, CellValue, SSTIndex, xfIndex, Error));
                                }
                                else if (!Reader.SkipContent())
                                {
                                    break;
                                }
                            }
                        }
                        else if (!Reader.SkipContent())
                        {
                            break;
                        }
                    }
                }
                #endregion
                #region Columns
                else if (Reader.IsStartElement(NCols, NsSpreadsheetMl))
                {
                    if (!Reader.ReadFirstContent())
                        continue;

                    while (!Reader.EOF)
                    {
                        if (Reader.IsStartElement(NCol, NsSpreadsheetMl))
                        {
                            int Min = int.Parse(Reader.GetAttribute(AMin)) - 1,
                                Max = int.Parse(Reader.GetAttribute(AMax)) - 1;
                            bool CustomWidth = Reader.GetAttribute(ACustomWidth) == "1",
                                 Hidden = Reader.GetAttribute(AHidden) == "1";

                            double Width = CustomWidth && double.TryParse(Reader.GetAttribute(AWidth), NumberStyles.Any, CultureInfo.InvariantCulture, out double Value) ? Value : -1;

                            Columns.Add(new ExcelColumn(this, Min, Max, Hidden, Width));

                            Reader.Skip();
                        }
                        else if (!Reader.SkipContent())
                        {
                            break;
                        }
                    }
                }
                #endregion
                #region Merge Cells
                else if (Reader.IsStartElement(NMergeCells, NsSpreadsheetMl))
                {
                    if (!Reader.ReadFirstContent())
                        continue;

                    List<ExcelCellRange> MergeCellList = new List<ExcelCellRange>();
                    while (!Reader.EOF)
                    {
                        if (Reader.IsStartElement(NMergeCell, NsSpreadsheetMl))
                        {
                            string CellReferences = Reader.GetAttribute(ARef);
                            MergeCellList.Add(new ExcelCellRange(CellReferences));

                            Reader.Skip();
                        }
                        else if (!Reader.SkipContent())
                        {
                            break;
                        }
                    }
                    MergeCells = MergeCellList.ToArray();
                }
                #endregion
                #region HeaderFooter
                else if (Reader.IsStartElement(NHeaderFooter, NsSpreadsheetMl))
                {
                    HeaderFooter = Reader.ReadHeaderFooter();
                }
                #endregion
                #region Properties
                else if (Reader.IsStartElement(NSheetProperties, NsSpreadsheetMl))
                {
                    CodeName = Reader.GetAttribute("codeName");

                    Reader.Skip();
                }

                #endregion
                #region Format Properties
                else if (Reader.IsStartElement(NSheetFormatProperties, NsSpreadsheetMl))
                {
                    if (double.TryParse(Reader.GetAttribute(ADefaultRowHeight), NumberStyles.Any, CultureInfo.InvariantCulture, out double defaultRowHeight))
                        DefaultRowHeight = defaultRowHeight;

                    Reader.Skip();
                }
                #endregion

                else if (!Reader.SkipContent())
                {
                    break;
                }
            }
        }

        public IEnumerable<ExcelRowCells> EnumRows()
        {
            if (GetDecompressor is null)
            {
                for (int i = 0; i < Rows.Length; i++)
                {
                    if (!Cells.TryGetCellRows(i, out ExcelRowCells RowCells))
                        RowCells = new ExcelRowCells(this, i);

                    yield return RowCells;
                }

                yield break;
            }

            using Stream Stream = GetDecompressor(CompressedStream);

            // Biff
            if (XmlSettings is null)
            {
                using XlsxBiffReader Reader = new XlsxBiffReader(Stream);

                int RowIndex = 0;
                ExcelRowCells RowCells = null;
                bool HasCache = false;
                while (Reader.ReadVariable(out int Id, out _))
                {
                    switch (Id)
                    {
                        #region Row
                        case 0x00:
                            {
                                if (RowCells != null)
                                    yield return RowCells;

                                RowIndex = Reader.Read<int>();
                                HasCache = Cells.TryGetCellRows(RowIndex, out RowCells);
                                if (HasCache)
                                {
                                    yield return RowCells;
                                    RowCells = null;
                                }
                                else
                                {
                                    RowCells = Cells.Create(RowIndex);
                                }
                                break;
                            }
                        #endregion
                        #region Cells
                        case 0x01:  // Empty
                            {
                                if (HasCache)
                                    break;

                                int column = Reader.Read<int>(),
                                    xfIndex = Reader.Read<int>() & 0xFFFFFF;

                                Columns.SetMaxIndex(column);
                                RowCells.Add(new ExcelCell(this, RowIndex, column, null, -1, xfIndex, null));
                                break;
                            }
                        case 0x02:  // Number
                            {
                                if (HasCache)
                                    break;

                                int column = Reader.Read<int>(),
                                    xfIndex = Reader.Read<int>() & 0xFFFFFF;
                                object RkNumber = Reader.ReadRkNumber();

                                Columns.SetMaxIndex(column);
                                RowCells.Add(new ExcelCell(this, RowIndex, column, RkNumber, -1, xfIndex, null));
                                break;
                            }
                        case 0x03:  // Error
                        case 0x0B:  // Formula Error
                            {
                                if (HasCache)
                                    break;

                                int column = Reader.Read<int>(),
                                    xfIndex = Reader.Read<int>() & 0xFFFFFF;
                                ExcelCellError Error = Reader.Read<ExcelCellError>();

                                Columns.SetMaxIndex(column);
                                RowCells.Add(new ExcelCell(this, RowIndex, column, null, -1, xfIndex, Error));
                                break;
                            }
                        case 0x04:  // Boolean
                        case 0x0A:  // Formula Boolean
                            {
                                if (HasCache)
                                    break;

                                int column = Reader.Read<int>(),
                                    xfIndex = Reader.Read<int>() & 0xFFFFFF;
                                bool Bool = Reader.Read<bool>();

                                Columns.SetMaxIndex(column);
                                RowCells.Add(new ExcelCell(this, RowIndex, column, Bool, -1, xfIndex, null));
                                break;
                            }
                        case 0x05:  // Float
                        case 0x09:  // Formula Float
                            {
                                if (HasCache)
                                    break;

                                int column = Reader.Read<int>(),
                                    xfIndex = Reader.Read<int>() & 0xFFFFFF;
                                double Float = Reader.Read<double>();

                                Columns.SetMaxIndex(column);
                                RowCells.Add(new ExcelCell(this, RowIndex, column, Float, -1, xfIndex, null));
                                break;
                            }
                        case 0x06:  // String
                        case 0x08:  // Formula String
                            {
                                if (HasCache)
                                    break;

                                int column = Reader.Read<int>(),
                                    xfIndex = Reader.Read<int>() & 0xFFFFFF;
                                string Content = Reader.ReadString();

                                Columns.SetMaxIndex(column);
                                RowCells.Add(new ExcelCell(this, RowIndex, column, Content, -1, xfIndex, null));
                                break;
                            }
                        case 0x07:  // SST
                            {
                                if (HasCache)
                                    break;

                                int column = Reader.Read<int>(),
                                    xfIndex = Reader.Read<int>() & 0xFFFFFF,
                                    SSTIndex = Reader.Read<int>();

                                Columns.SetMaxIndex(column);
                                RowCells.Add(new ExcelCell(this, RowIndex, column, null, SSTIndex, xfIndex, null));
                                break;
                            }
                            #endregion
                    }
                }

                if (RowCells != null)
                    yield return RowCells;
            }

            // Xml
            else
            {
                using XmlReader Reader = XmlReader.Create(Stream, XmlSettings);

                if (!Reader.IsStartElement(NWorksheet, NsSpreadsheetMl))
                    yield break;

                if (!Reader.ReadFirstContent())
                    yield break;

                while (!Reader.EOF)
                {
                    if (Reader.IsStartElement(NSheetData, NsSpreadsheetMl))
                    {
                        if (!Reader.ReadFirstContent())
                            continue;

                        int RowIndex = 0;
                        while (!Reader.EOF)
                        {
                            if (Reader.IsStartElement(NRow, NsSpreadsheetMl))
                            {
                                if (int.TryParse(Reader.GetAttribute(AR), out int arValue))
                                    RowIndex = arValue - 1;    // The row attribute is 1-based

                                if (Cells.TryGetCellRows(RowIndex, out ExcelRowCells RowCells))
                                {
                                    yield return RowCells;

                                    Reader.Skip();
                                    continue;
                                }

                                if (!Reader.ReadFirstContent())
                                    continue;

                                int ColumnIndex = 0;
                                RowCells = Cells.Create(RowIndex);
                                while (!Reader.EOF)
                                {
                                    if (Reader.IsStartElement(NC, NsSpreadsheetMl))
                                    {
                                        if (Reader.GetAttribute(AR).TryParseReference(out int ReferenceColumn, out _))
                                            ColumnIndex = ReferenceColumn - 1;  // ParseReference is 1-based

                                        string aS = Reader.GetAttribute(AS);
                                        int xfIndex = !string.IsNullOrEmpty(aS) &&
                                                      int.TryParse(aS, NumberStyles.Any, CultureInfo.InvariantCulture, out int StyleIndex) ? StyleIndex : -1;

                                        string aT = Reader.GetAttribute(AT);
                                        if (!Reader.ReadFirstContent())
                                        {
                                            Columns.SetMaxIndex(ColumnIndex);
                                            RowCells.Add(new ExcelCell(this, RowIndex, ColumnIndex++, null, -1, xfIndex, null));
                                            continue;
                                        }

                                        object CellValue = null;
                                        int SSTIndex = -1;
                                        ExcelCellError? Error = null;

                                        while (!Reader.EOF)
                                        {
                                            if (Reader.IsStartElement(NV, NsSpreadsheetMl))
                                            {
                                                string RawValue = Reader.ReadElementContentAsString();

                                                if (!string.IsNullOrEmpty(RawValue))
                                                    CellValue = ConvertCellValue(RawValue, aT, out SSTIndex, out Error);
                                            }
                                            else if (Reader.IsStartElement(NIs, NsSpreadsheetMl))
                                            {
                                                string RawValue = Reader.ReadStringItem();

                                                if (!string.IsNullOrEmpty(RawValue))
                                                    CellValue = ConvertCellValue(RawValue, aT, out SSTIndex, out Error);
                                            }
                                            else if (!Reader.SkipContent())
                                            {
                                                break;
                                            }
                                        }

                                        Columns.SetMaxIndex(ColumnIndex);
                                        RowCells.Add(new ExcelCell(this, RowIndex, ColumnIndex++, CellValue, SSTIndex, xfIndex, Error));
                                    }
                                    else if (!Reader.SkipContent())
                                    {
                                        break;
                                    }
                                }

                                yield return RowCells;
                            }
                            else if (!Reader.SkipContent())
                            {
                                break;
                            }
                        }
                    }
                    else if (!Reader.SkipContent())
                    {
                        break;
                    }
                }
            }

        }

        public IEnumerable<ExcelCell> EnumCells()
        {
            foreach (ExcelRowCells Row in EnumRows())
                foreach (ExcelCell Cell in Row)
                    yield return Cell;
        }

        private object ConvertCellValue(string RawValue, string aT, out int SSTIndex, out ExcelCellError? Error)
        {
            switch (aT)
            {
                #region String
                case AS:
                    {
                        Error = null;

                        if (int.TryParse(RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int sstIndex))
                        {
                            SSTIndex = sstIndex;
                            return null;
                        }

                        SSTIndex = -1;
                        return RawValue;
                    }
                #endregion
                #region  String Inline / Cached Formula String
                case "inlineStr":
                case "str":
                    {
                        Error = null;
                        SSTIndex = -1;
                        return ExcelSheetHelper.ConvertEscapeChars(RawValue);
                    }
                #endregion
                #region Boolean
                case "b":
                    {
                        Error = null;
                        SSTIndex = -1;
                        return RawValue == "1";
                    }
                #endregion
                #region Date (ISO 8601)
                case "d":
                    {
                        Error = null;
                        SSTIndex = -1;

                        if (DateTime.TryParseExact(RawValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite, out DateTime Date))
                            return Date;

                        return RawValue;
                    }
                #endregion
                #region Error
                case "e":
                    {
                        Error = RawValue switch
                        {
                            "#NULL!" => ExcelCellError.NULL,
                            "#DIV/0!" => ExcelCellError.DIV0,
                            "#VALUE!" => ExcelCellError.VALUE,
                            "#REF!" => ExcelCellError.REF,
                            "#NAME?" => ExcelCellError.NAME,
                            "#NUM!" => ExcelCellError.NUM,
                            "#N/A" => ExcelCellError.NA,
                            "#GETTING_DATA" => ExcelCellError.GETTING_DATA,
                            _ => null
                        };
                        SSTIndex = -1;
                        return null;
                    }
                #endregion
                default:
                    {
                        Error = null;
                        SSTIndex = -1;

                        if (double.TryParse(RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                        {
                            //var format = Workbook.GetNumberFormatString(numberFormatIndex);
                            //if (format != null)
                            //{
                            //    if (format.IsDateTimeFormat)
                            //        return Helpers.ConvertFromOATime(number, Workbook.IsDate1904);
                            //    if (format.IsTimeSpanFormat)
                            //        return TimeSpan.FromDays(number);
                            //}

                            return number;
                        }

                        return RawValue;
                    }
            }
        }

        private bool IsDisposed = false;
        public void Dispose()
        {
            if (IsDisposed)
                return;

            CompressedStream.Dispose();
            CompressedStream = null;

            XmlSettings = null;

            Rows.Clear();
            Columns.Clear();
            Cells.Clear();

            IsDisposed = true;
        }

    }
}