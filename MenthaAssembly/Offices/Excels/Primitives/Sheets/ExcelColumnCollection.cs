using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Offices
{
    public class ExcelColumnCollection : IEnumerable<ExcelColumn>
    {
        private readonly IExcelSheet Parent;
        private readonly Dictionary<int, ExcelColumn> Columns = new Dictionary<int, ExcelColumn>();

        public ExcelColumn this[int Index]
        {
            get
            {
                if (MaxIndex < Index)
                    throw new IndexOutOfRangeException();

                if (Columns.TryGetValue(Index, out ExcelColumn c))
                    return c;

                return Columns.Values.FirstOrDefault(i => i.MinIndex <= Index && Index <= i.MaxIndex) is ExcelColumn Column ? Column : new ExcelColumn(Parent, Index, Index, false, -1);
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
            Columns[Column.MinIndex] = Column;
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
            => Columns.Count == Length ? Columns.Values.GetEnumerator() :
                                         EnumColumns().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private IEnumerable<ExcelColumn> EnumColumns()
        {
            int i = 0;
            foreach (KeyValuePair<int, ExcelColumn> Datas in Columns)
            {
                for (; i < Datas.Key; i++)
                    yield return new ExcelColumn(Parent, i, i, false, -1);

                yield return Datas.Value;

                i = Datas.Value.MaxIndex + 1;
            }

            for (; i <= MaxIndex; i++)
                yield return new ExcelColumn(Parent, i, i, false, -1);
        }

        public override string ToString()
            => $"Length : {Length}";

    }
}