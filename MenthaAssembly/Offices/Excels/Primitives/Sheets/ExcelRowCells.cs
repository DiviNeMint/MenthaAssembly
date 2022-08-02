using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Offices
{
    public class ExcelRowCells : IEnumerable<ExcelCell>
    {
        private readonly IExcelSheet Parent;
        private readonly List<ExcelCell> Cells = new List<ExcelCell>();

        public ExcelCell this[int ColumnIndex]
        {
            get
            {
                if (Length <= ColumnIndex)
                    throw new IndexOutOfRangeException();

                return Cells.FirstOrDefault(i => i.ColumnIndex == ColumnIndex) is ExcelCell Cell ? Cell : new ExcelCell(Parent, RowIndex, ColumnIndex, null, -1, -1, null);
            }
        }

        public int RowIndex { get; }

        public int Length
            => Parent.Columns.Length;

        internal ExcelRowCells(IExcelSheet Parent, int RowIndex)
        {
            this.Parent = Parent;
            this.RowIndex = RowIndex;
        }

        internal void Add(ExcelCell Cell)
        {
            int Index = Cells.FindIndex(i => i.ColumnIndex > Cell.ColumnIndex);
            Cells.Insert(Index < 0 ? Cells.Count : Index, Cell);
        }

        //internal bool TryGetCell(int ColumnIndex, out ExcelCell Cell)
        //{
        //    if (Length <= ColumnIndex ||
        //        Cells.FirstOrDefault(i => i.ColumnIndex == ColumnIndex) is not ExcelCell TempCell)
        //    {
        //        Cell = null;
        //        return false;
        //    }

        //    Cell = TempCell;
        //    return true;
        //}

        internal void Clear()
            => Cells.Clear();

        public IEnumerator<ExcelCell> GetEnumerator()
            => Cells.Count == Length ? Cells.GetEnumerator() :
                                       EnumCells().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private IEnumerable<ExcelCell> EnumCells()
        {
            int CellsIndex = 0;
            ExcelCell Cell = null;
            for (int i = 0; i < Length; i++)
            {
                CellsIndex = Cells.FindIndex(CellsIndex, c => i <= c.ColumnIndex);
                if (CellsIndex == -1)
                {
                    for (; i < Length; i++)
                        yield return new ExcelCell(Parent, RowIndex, i, null, -1, -1, null);

                    yield break;
                }

                Cell = Cells[CellsIndex];
                int TempIndex = Cell.ColumnIndex;
                for (; i < TempIndex; i++)
                    yield return new ExcelCell(Parent, RowIndex, i, null, -1, -1, null);

                CellsIndex++;
                yield return Cell;
            }
        }

        public override string ToString()
            => $"Length : {Length}";

    }
}