using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Offices
{
    public class ExcelRowCollection : IEnumerable<ExcelRow>
    {
        private readonly IExcelSheet Parent;
        private readonly List<ExcelRow> Rows = new List<ExcelRow>();

        public ExcelRow this[int Index]
        {
            get
            {
                if (MaxIndex < Index)
                    throw new IndexOutOfRangeException();

                return Rows.FirstOrDefault(i => i.Index == Index) is ExcelRow Row ? Row : new ExcelRow(Parent, Index, false, -1);
            }
        }

        private int MaxIndex = -1;
        public int Length
            => MaxIndex + 1;

        internal ExcelRowCollection(IExcelSheet Parent)
        {
            this.Parent = Parent;
        }

        internal void Add(ExcelRow Row)
        {
            int Index = Rows.FindIndex(i => i.Index > Row.Index);
            Rows.Insert(Index < 0 ? Rows.Count : Index, Row);
            SetMaxIndex(Row.Index);
        }

        internal void Clear()
        {
            MaxIndex = -1;
            Rows.Clear();
        }

        internal void SetMaxIndex(int Index)
        {
            if (MaxIndex < Index)
                MaxIndex = Index;
        }

        public IEnumerator<ExcelRow> GetEnumerator()
            => Rows.Count == Length ? Rows.GetEnumerator() :
                                      EnumRows().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private IEnumerable<ExcelRow> EnumRows()
        {
            int RowsIndex = 0;
            ExcelRow Row = null;
            for (int i = 0; i <= MaxIndex; i++)
            {
                RowsIndex = Rows.FindIndex(RowsIndex, r => i <= r.Index);
                if (RowsIndex == -1)
                {
                    for (; i <= MaxIndex; i++)
                        yield return new ExcelRow(Parent, i, false, -1);

                    yield break;
                }

                Row = Rows[RowsIndex];
                int TempIndex = Row.Index;
                for (; i < TempIndex; i++)
                    yield return new ExcelRow(Parent, i, false, -1);

                RowsIndex++;
                yield return Row;
            }
        }

        public override string ToString()
            => $"Length : {Length}";

    }
}