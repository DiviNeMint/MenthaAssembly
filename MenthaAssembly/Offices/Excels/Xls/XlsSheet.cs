using MenthaAssembly.Offices.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Offices
{
    internal class XlsSheet : IExcelSheet
    {
        internal readonly XlsWorkbook Parent;
        internal readonly XlsSheetInfo Info;

        public string Name
            => Info.Name;

        public string CodeName { get; }

        public bool Visible
            => Info.Visible;

        public double DefaultRowHeight { get; }

        public double DefaultColumnWidth { get; }

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

        private XlsSheet(XlsWorkbook Parent, XlsSheetInfo Info)
        {
            this.Parent = Parent;
            this.Info = Info;
            DefaultRowHeight = 15d;
            Columns = new ExcelColumnCollection(this);
            Rows = new ExcelRowCollection(this);
            Cells = new ExcelCellCollection(this);
        }
        public XlsSheet(XlsWorkbook Parent, XlsSheetInfo Info, Stream Stream, Encryption Encryption, byte[] SecretKey) : this(Parent, Info)
        {
            Stream.Seek(Info.Position, SeekOrigin.Begin);
            using XlsBiffReader Reader = new XlsBiffReader(Stream, Encryption, SecretKey, true);

            if (!Reader.ReadVariable(out int ID, out _) || (ID != 0x009 && ID != 0x0209 && ID != 0x0409 && ID != 0x0809))
                throw new InvalidDataException();

            int BiffVersion = Reader.Read<ushort>(),
                BiffType = Reader.Read<ushort>();

            if (BiffVersion == 0 || BiffType != 0x0010)
                throw new InvalidDataException();

            ExcelRowCells RowCells = null;
            void UpdateRowCells(int Row)
            {
                if ((RowCells == null || RowCells.RowIndex != Row) &&
                    !Cells.TryGetCellRows(Row, out RowCells))
                    RowCells = Cells.Create(Row);
            }

            while (Reader.ReadVariable(out ID, out int Length) && ID != 0x000A)
            {
                switch (ID)
                {
                    #region Rows
                    case 0x0008:    // ROW_V2
                        {
                            int RowIndex = Reader.Read<ushort>();

                            // Columns
                            Reader.Skip(2);                             // FirstDefinedColumn
                            Columns.SetMaxIndex(Reader.Read<ushort>()); // LastDefinedColumn

                            ushort RowHeightDatas = Reader.Read<ushort>();
                            double RowHeight = (RowHeightDatas & 0x8000) != 0 ? (RowHeightDatas & 0x7FFFF) / 15d : -1;

                            //    UseXFormat = ReadByte(0xA) != 0;
                            //    if (UseXFormat)
                            //        XFormat = ReadUInt16(0x10);

                            Rows.Add(new ExcelRow(this, RowIndex, false, RowHeight));
                            break;
                        }
                    case 0x0208:    // ROW
                        {
                            int RowIndex = Reader.Read<ushort>();

                            // Columns
                            Reader.Skip(2);                             // FirstDefinedColumn
                            Columns.SetMaxIndex(Reader.Read<ushort>()); // LastDefinedColumn

                            ushort RowHeightDatas = Reader.Read<ushort>();
                            double RowHeight = (RowHeightDatas & 0x8000) != 0 ? (RowHeightDatas & 0x7FFF) / 15d : -1;

                            // reserved1 & unused1
                            Reader.Skip(4);

                            byte Flags = Reader.Read<byte>();
                            bool Hidden = (Flags & 0x20) > 0;

                            //    UseXFormat = (flags & RowHeightFlags.GhostDirty) != 0;
                            //    XFormat = (ushort)(ReadUInt16(0xE) & 0xFFF);

                            Rows.Add(new ExcelRow(this, RowIndex, Hidden, RowHeight));
                            break;
                        }
                    #endregion
                    #region Columns
                    case 0x007D:    // COLINFO
                        {
                            ushort colFirst = Reader.Read<ushort>(),
                                   colLast = Reader.Read<ushort>(),
                                   colDx = Reader.Read<ushort>();

                            Reader.Skip(2); // ixfe

                            ushort flags = Reader.Read<ushort>();

                            bool hidden = (flags & 0x01) != 0,
                                 userSet = (flags & 0x02) != 0;

                            Columns.Add(new ExcelColumn(this, colFirst, colLast, hidden, userSet ? colDx / 256d : -1));
                            break;
                        }

                    #endregion
                    #region Cells
                    case 0x0006:    // Formula
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                ixfe = Reader.Read<ushort>();
                            byte[] Datas = Reader.ReadBuffer(8);

                            UpdateRowCells(Row);
                            if (Datas[6] != byte.MaxValue || Datas[7] != byte.MaxValue)
                            {
                                RowCells.Add(new ExcelCell(this, Row, Column, BitConverter.ToDouble(Datas, 0), -1, ixfe, null));
                                break;
                            }

                            switch (Datas[0])
                            {
                                case 0x00:
                                    {
                                        if (Reader.ReadVariable(out ID, out Length))
                                        {
                                            if (ID == 0x04BC && !Reader.ReadVariable(out ID, out Length))
                                                break;

                                            if (ID == 0x0207)
                                            {
                                                int cch = Reader.Read<ushort>();
                                                byte Option = Reader.Read<byte>();
                                                bool fHighByte = (Option & 0x01) > 0;

                                                Datas = Reader.ReadBuffer(fHighByte ? cch << 1 : cch);
                                                string Value = fHighByte ? Encoding.Unicode.GetString(Datas) :
                                                                           Encoding.UTF8.GetString(Datas);

                                                RowCells.Add(new ExcelCell(this, Row, Column, Value, -1, ixfe, null));
                                            }
                                            else
                                            {
                                                RowCells.Add(new ExcelCell(this, Row, Column, null, -1, ixfe, null));
                                            }
                                        }
                                        break;
                                    }
                                case 0x01:
                                    RowCells.Add(new ExcelCell(this, Row, Column, Datas[2] != 0, -1, ixfe, null));
                                    break;
                                case 0x02:
                                    RowCells.Add(new ExcelCell(this, Row, Column, null, -1, ixfe, (ExcelCellError)Datas[2]));
                                    break;
                                default:
                                    RowCells.Add(new ExcelCell(this, Row, Column, null, -1, ixfe, null));
                                    break;
                            }
                            break;
                        }

                    case 0x0202:    // Integer
                        {
                            ushort Row = Reader.Read<ushort>(),
                                   Column = Reader.Read<ushort>(),
                                   ixfe = Reader.Read<ushort>(),
                                   Value = Reader.Read<ushort>();

                            UpdateRowCells(Row);
                            RowCells.Add(new ExcelCell(this, Row, Column, Value, -1, ixfe, null));
                            break;
                        }
                    case 0x0203:    // Number
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                ixfe = Reader.Read<ushort>();
                            double Value = Reader.Read<double>();

                            UpdateRowCells(Row);
                            RowCells.Add(new ExcelCell(this, Row, Column, Value, -1, ixfe, null));
                            break;
                        }
                    case 0x00D6:    // RString
                    case 0x0204:    // Label
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                ixfe = Reader.Read<ushort>();

                            int cch = Reader.Read<ushort>();
                            byte Option = Reader.Read<byte>();
                            bool fHighByte = (Option & 0x01) > 0;

                            byte[] Datas = Reader.ReadBuffer(fHighByte ? cch << 1 : cch);
                            string Value = fHighByte ? Encoding.Unicode.GetString(Datas) :
                                                       Encoding.UTF8.GetString(Datas);

                            UpdateRowCells(Row);
                            RowCells.Add(new ExcelCell(this, Row, Column, Value, -1, ixfe, null));
                            break;
                        }

                    case 0x0205:    // Boolean & Error
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                ixfe = Reader.Read<ushort>();

                            ExcelCellError Error = Reader.Read<ExcelCellError>();
                            bool IsErrorCell = Reader.Read<bool>();

                            UpdateRowCells(Row);
                            if (IsErrorCell)
                                RowCells.Add(new ExcelCell(this, Row, Column, null, -1, ixfe, Error));
                            else
                                RowCells.Add(new ExcelCell(this, Row, Column, Error != ExcelCellError.NULL, -1, ixfe, null));
                            break;
                        }

                    case 0x0201:    // Blank
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                ixfe = Reader.Read<ushort>();

                            UpdateRowCells(Row);
                            RowCells.Add(new ExcelCell(this, Row, Column, null, -1, ixfe, null));
                            break;
                        }
                    case 0x00BE:    // Multi Blank
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                Count = (Length >> 1) - 3;

                            UpdateRowCells(Row);
                            for (int i = 0; i < Count; i++)
                            {
                                int ixfe = Reader.Read<ushort>();
                                RowCells.Add(new ExcelCell(this, Row, Column++, null, -1, ixfe, null));
                            }
                            break;
                        }

                    case 0x027E:    // RK
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                ixfe = Reader.Read<ushort>();
                            object Value = Reader.ReadRkNumber();

                            UpdateRowCells(Row);
                            RowCells.Add(new ExcelCell(this, Row, Column, Value, -1, ixfe, null));
                            break;
                        }
                    case 0x00BD:    // Multi RK
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                Count = Length / 6 - 1;

                            UpdateRowCells(Row);
                            for (int i = 0; i < Count; i++)
                            {
                                int ixfe = Reader.Read<ushort>();
                                object Value = Reader.ReadRkNumber();
                                RowCells.Add(new ExcelCell(this, Row, Column++, Value, -1, ixfe, null));
                            }
                            break;
                        }

                    case 0x00FD:    // LabelSST
                        {
                            int Row = Reader.Read<ushort>(),
                                Column = Reader.Read<ushort>(),
                                ixfe = Reader.Read<ushort>(),
                                SSTIndex = Reader.Read<int>();

                            UpdateRowCells(Row);
                            RowCells.Add(new ExcelCell(this, Row, Column, null, SSTIndex, ixfe, null));
                            break;
                        }
                    #endregion
                    #region MergeCells
                    case 0x00E5:
                        {
                            ushort Count = Reader.Read<ushort>();
                            MergeCells = new ExcelCellRange[Count];

                            for (int i = 0; i < Count; i++)
                            {
                                ushort FromRow = Reader.Read<ushort>(),
                                       ToRow = Reader.Read<ushort>(),
                                       FromCol = Reader.Read<ushort>(),
                                       ToCol = Reader.Read<ushort>();

                                MergeCells[i] = new ExcelCellRange(FromCol, FromRow, ToCol, ToRow);
                            }

                            break;
                        }
                    #endregion
                    #region DefaultRowHeight
                    case 0x0025:    // DefaultRowHeight_V2
                    case 0x0225:    // DefaultRowHeight
                        {
                            if (BiffVersion == 2)
                            {
                                DefaultRowHeight = (Reader.Read<ushort>() & 0x7FFF) / 15d;
                            }
                            else
                            {
                                ushort Flags = Reader.Read<ushort>();
                                DefaultRowHeight = (Flags & 0x02) == 0 ? Reader.Read<ushort>() / 15d : 0d;
                            }
                            break;
                        }
                    #endregion
                    #region Dimensions
                    case 0x0000:
                    case 0x0200:
                        {
                            if (BiffVersion < 8)
                            {
                                // FirstRow
                                Reader.Skip(2);

                                // LastRow
                                Rows.SetMaxIndex(Reader.Read<ushort>());
                            }
                            else
                            {
                                // FirstRow
                                Reader.Skip(4);

                                // LastRow
                                Rows.SetMaxIndex((int)Reader.Read<uint>());
                            }

                            // FirstColumn
                            Reader.Skip(2);

                            // LastColumn
                            Columns.SetMaxIndex(Reader.Read<ushort>());
                        }
                        break;
                    #endregion
                    #region CodeName
                    case 0x01BA:
                        {
                            int cch = Reader.Read<ushort>();
                            byte Option = Reader.Read<byte>();
                            bool fHighByte = (Option & 0x01) > 0;

                            byte[] Datas = Reader.ReadBuffer(fHighByte ? cch << 1 : cch);
                            CodeName = fHighByte ? Encoding.Unicode.GetString(Datas) :
                                                   Encoding.UTF8.GetString(Datas);
                            break;
                        }
                        #endregion
                }
            }
        }

        public IEnumerable<ExcelRowCells> EnumRows()
        {
            for (int i = 0; i < Rows.Length; i++)
            {
                if (!Cells.TryGetCellRows(i, out ExcelRowCells RowCells))
                    RowCells = new ExcelRowCells(this, i);

                yield return RowCells;
            }
        }

        public IEnumerable<ExcelCell> EnumCells()
        {
            foreach (ExcelRowCells Row in EnumRows())
                foreach (ExcelCell Cell in Row)
                    yield return Cell;
        }

        private bool IsDisposed = false;
        public void Dispose()
        {
            if (IsDisposed)
                return;


            IsDisposed = true;
        }

    }
}