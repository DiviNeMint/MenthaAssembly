using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Offices
{
    internal class CsvSheet : IExcelSheet
    {
        internal readonly CsvWorkbook Parent;

        public string Name { get; }

        public string CodeName { get; }

        public bool Visible
            => true;

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
                    throw new ObjectDisposedException(nameof(CsvSheet));

                if (Column >= Columns.Length)
                    throw new IndexOutOfRangeException($"Column index is out of range.");

                if (Row >= Rows.Length)
                    throw new IndexOutOfRangeException($"Row index is out of range.");

                if (Cells.TryGetCellRows(Row, out ExcelRowCells RowCells))
                    return RowCells[Column];

                return EnumRows().First(i => i.RowIndex == Row)[Column];
            }
        }

        public ExcelCellRange[] MergeCells { get; }

        public IExcelWorkbook Workbook
            => Parent;

        private CsvSheet(CsvWorkbook Parent)
        {
            this.Parent = Parent;
            DefaultRowHeight = 15d;
            DefaultColumnWidth = 109.59d;   // 8.43 * 13;
            Columns = new ExcelColumnCollection(this);
            Rows = new ExcelRowCollection(this);
            Cells = new ExcelCellCollection(this);
        }
        public CsvSheet(CsvWorkbook Parent, Stream Stream, Encoding Encoding) : this(Parent)
        {
            StreamReader r = new StreamReader(Stream, Encoding, false, 1024);
            StringBuilder Builder = new StringBuilder();

            int RowIndex = 0,
                ColumnIndex = 0,
                ColumnCount = 0,
                Length;

            string Line;
            char Separator = Excel.CsvSeparator,
                 c;

            ExcelRowCells RowCells;
            while (!r.EndOfStream)
            {
                RowCells = Cells.Create(RowIndex);

                Line = r.ReadLine();
                Length = Line.Length;

                for (int i = 0; i < Length;)
                {
                    c = Line[i++];
                    if (c.Equals('"'))
                    {
                        for (; i < Length;)
                        {
                            c = Line[i++];
                            if (c.Equals('"'))
                                break;

                            Builder.Append(c);
                        }
                        if (Builder.Length > 0)
                        {
                            RowCells.Add(new ExcelCell(this, RowIndex, ColumnIndex, Builder.ToString(), -1, -1, null));
                            Builder.Clear();
                        }

                        ColumnCount = Math.Max(ColumnCount, ++ColumnIndex);
                        continue;
                    }
                    else if (Separator.Equals(c))
                    {
                        if (Builder.Length > 0)
                        {
                            RowCells.Add(new ExcelCell(this, RowIndex, ColumnIndex, Builder.ToString(), -1, -1, null));
                            Builder.Clear();
                        }

                        ColumnCount = Math.Max(ColumnCount, ++ColumnIndex);
                        continue;
                    }

                    Builder.Append(c);
                }

                if (Builder.Length > 0)
                {
                    RowCells.Add(new ExcelCell(this, RowIndex, ColumnIndex++, Builder.ToString(), -1, -1, null));
                    Builder.Clear();
                    ColumnCount = Math.Max(ColumnCount, ColumnIndex);
                }

                Rows.Add(new ExcelRow(this, RowIndex++, false, -1d));
                ColumnIndex = 0;
            }

            for (int i = 0; i < ColumnCount; i++)
                Columns.Add(new ExcelColumn(this, i, i, false, -1d));
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
