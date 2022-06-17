using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Offices
{
    public class ExcelColumnCollection : IEnumerable<ExcelColumn>
    {
        private readonly IExcelSheet Parent;
        private readonly List<ExcelColumn> Columns = new List<ExcelColumn>();

        public ExcelColumn this[int Index]
        {
            get
            {
                if (MaxIndex < Index)
                    throw new IndexOutOfRangeException();

                if (Columns.FirstOrDefault(i => i.MinIndex <= Index && Index <= i.MaxIndex) is ExcelColumn Column)
                    return Column;

                int ColumnIndex = Math.Max(Columns.FindIndex(i => Index < i.MinIndex), 0);

                Column = new ExcelColumn(Parent, Index, Index, false, -1);
                Columns.Insert(ColumnIndex, Column);

                return Column;
            }
        }

        private int MaxIndex = -1;
        public int Length
            => MaxIndex + 1;

        internal ExcelColumnCollection(IExcelSheet Parent)
        {
            this.Parent = Parent;
        }

        internal void Add(ExcelColumn Column)
        {
            int Index = Math.Max(Columns.FindIndex(i => i.MinIndex > Column.MinIndex), 0);
            Columns.Insert(Index, Column);
            SetMaxIndex(Column.MaxIndex);
        }

        internal void Clear()
        {
            MaxIndex = -1;
            Columns.Clear();
        }

        internal void SetMaxIndex(int Index)
        {
            if (MaxIndex < Index)
                MaxIndex = Index;
        }

        public IEnumerator<ExcelColumn> GetEnumerator()
            => Columns.Count == Length ? Columns.GetEnumerator() :
                                         EnumColumns().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        private IEnumerable<ExcelColumn> EnumColumns()
        {
            int ColumnsIndex = 0;
            ExcelColumn Column = null;
            for (int i = 0; i <= MaxIndex; i++)
            {
                ColumnsIndex = Columns.FindIndex(ColumnsIndex, c => i <= c.MinIndex);
                if (ColumnsIndex == -1)
                {
                    for (; i <= MaxIndex; i++)
                    {
                        Column = new ExcelColumn(Parent, i, i, false, -1);
                        Columns.Add(Column);
                        yield return Column;
                    }

                    yield break;
                }

                Column = Columns[ColumnsIndex];
                int TempIndex = Column.MinIndex;
                for (; i < TempIndex; i++)
                {
                    ExcelColumn NewColumn = new ExcelColumn(Parent, i, i, false, -1);
                    Columns.Insert(ColumnsIndex++, NewColumn);
                    yield return NewColumn;
                }

                ColumnsIndex++;
                yield return Column;
            }
        }

        public override string ToString()
            => $"Length : {Length}";

    }
}
