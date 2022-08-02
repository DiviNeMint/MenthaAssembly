using System;
using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    public class ExcelCell : IEquatable<ExcelCell>
    {
        public IExcelSheet Sheet { get; }

        public int RowIndex { get; }

        public int ColumnIndex { get; }

        private readonly object _Value;
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

        public override bool Equals(object obj)
            => obj is ExcelCell c && Equals(c);
        public bool Equals(ExcelCell other)
            => other.Sheet == Sheet &&
               other.RowIndex == RowIndex &&
               other.ColumnIndex == ColumnIndex &&
               other._Value == _Value &&
               other.SharedStringIndex == SharedStringIndex;

        public override int GetHashCode()
        {
            int hashCode = -1506529040;
            hashCode = hashCode * -1521134295 + EqualityComparer<IExcelSheet>.Default.GetHashCode(Sheet);
            hashCode = hashCode * -1521134295 + RowIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + ColumnIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(_Value);
            hashCode = hashCode * -1521134295 + SharedStringIndex.GetHashCode();
            return hashCode;
        }

        public override string ToString()
            => $"Row : {RowIndex}, Column : {ColumnIndex}, Value : {Value}";

    }
}