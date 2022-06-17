using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    public class ExcelCell
    {
        public int RowIndex { get; }

        public int ColumnIndex { get; }

        private object _Value; 
        public object Value
        {
            get
            {
                if (SharedStringIndex != -1)
                {
                    List<string> Table = Sheet.Workbook.SharedStrings;
                    if (SharedStringIndex < Table.Count)
                        return Table[SharedStringIndex];
                }

                return _Value;
            } 
        }

        public ExcelCellError? Error { get; }

        public IExcelSheet Sheet { get; }

        internal readonly int SharedStringIndex;
        internal readonly int StyleIndex;

        internal ExcelCell(IExcelSheet Sheet, int RowIndex, int ColumnIndex, object Value, int SharedStringIndex, int StyleIndex, ExcelCellError? Error)
        {
            this.Sheet = Sheet;
            this.RowIndex = RowIndex;
            this.ColumnIndex = ColumnIndex;
            this._Value = Value;
            this.SharedStringIndex = SharedStringIndex;
            this.StyleIndex = StyleIndex;
            this.Error = Error;
        }

        public override string ToString()
            => $"Row : {RowIndex}, Column : {ColumnIndex}, Value : {Value}";

    }
}