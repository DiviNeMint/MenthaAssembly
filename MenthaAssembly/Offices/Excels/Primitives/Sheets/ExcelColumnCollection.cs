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

                return Columns.FirstOrDefault(i => i.MinIndex <= Index && Index <= i.MaxIndex) is ExcelColumn Column ? Column : new ExcelColumn(Parent, Index, Index, false, -1);
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
            int Index = Columns.FindIndex(i => i.MinIndex > Column.MinIndex);
            Columns.Insert(Index < 0 ? Columns.Count : Index, Column);
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
            => GetEnumerator();

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
                        yield return new ExcelColumn(Parent, i, i, false, -1);

                    yield break;
                }

                Column = Columns[ColumnsIndex];
                int TempIndex = Column.MinIndex;
                for (; i < TempIndex; i++)
                    yield return new ExcelColumn(Parent, i, i, false, -1);

                ColumnsIndex++;
                yield return Column;
            }
        }

        public override string ToString()
            => $"Length : {Length}";

    }
}