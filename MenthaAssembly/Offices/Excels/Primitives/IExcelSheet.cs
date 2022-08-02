using System;
using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    public interface IExcelSheet : IDisposable
    {
        public string Name { get; }

        public string CodeName { get; }

        public bool Visible { get; }

        public double DefaultRowHeight { get; }

        public double DefaultColumnWidth { get; }

        public ExcelHeaderFooter HeaderFooter { get; }

        public ExcelRowCollection Rows { get; }

        public ExcelColumnCollection Columns { get; }

        public ExcelCell this[int Column, int Row] { get; }

        public ExcelCellRange[] MergeCells { get; }

        public IExcelWorkbook Workbook { get; }

        public IEnumerable<ExcelRowCells> EnumRows();

        public IEnumerable<ExcelCell> EnumCells();

    }
}
