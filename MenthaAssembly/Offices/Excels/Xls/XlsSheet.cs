using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenthaAssembly.Offices
{
    public class XlsSheet : IExcelSheet
    {
        internal readonly XlsWorkbook Parent;

        public ExcelCell this[int Column, int Row] 
        {
            get
            {
                return null;
            }
        }

        public string Name { get; }

        public string CodeName { get; }

        public bool Visible { get; }

        public double DefaultRowHeight { get; }

        public double DefaultColumnWidth { get; }

        public ExcelHeaderFooter HeaderFooter { get; }

        public ExcelRow[] Rows { get; }

        public ExcelColumnCollection Columns { get; }

        public ExcelCellRange[] MergeCells { get; }

        public IExcelWorkbook Workbook { get; }

        private XlsSheet(XlsWorkbook Parent)
        {
            this.Parent = Parent;
            this.DefaultRowHeight = 15d;
            this.Columns = new ExcelColumnCollection(this);
        }

        public IEnumerable<ExcelCell> EnumCells()
        {
            yield break;
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
