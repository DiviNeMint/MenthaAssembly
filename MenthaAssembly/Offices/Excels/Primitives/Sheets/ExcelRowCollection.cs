using System;
using System.Collections;
using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    public class ExcelRowCollection : IEnumerable<ExcelRow>
    {
        private readonly IExcelSheet Parent;
        private readonly Dictionary<int, ExcelRow> Rows = new Dictionary<int, ExcelRow>();

        public ExcelRow this[int Index]
        {
            get
            {
                if (MaxIndex < Index)
                    throw new IndexOutOfRangeException();

                return Rows.TryGetValue(Index, out ExcelRow Row) ? Row : new ExcelRow(Parent, Index, false, -1);
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
            Rows[Row.Index] = Row;
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
            => Rows.Count == Length ? Rows.Values.GetEnumerator() :
                                      EnumRows().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private IEnumerable<ExcelRow> EnumRows()
        {
            int i = 0;
            foreach (KeyValuePair<int, ExcelRow> Data in Rows)
            {
                for (; i < Data.Key; i++)
                    yield return new ExcelRow(Parent, i, false, -1);

                yield return Data.Value;
                i = Data.Key + 1;
            }

            for (; i <= MaxIndex; i++)
                yield return new ExcelRow(Parent, i, false, -1);
        }

        public override string ToString()
            => $"Length : {Length}";

    }
}