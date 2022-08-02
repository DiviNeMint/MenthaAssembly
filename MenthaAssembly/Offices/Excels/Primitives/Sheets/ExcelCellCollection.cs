using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Offices
{
    internal class ExcelCellCollection
    {
        private readonly IExcelSheet Parent;
        private readonly List<ExcelRowCells> RowCellsCollection = new List<ExcelRowCells>();

        public ExcelCellCollection(IExcelSheet Parent)
        {
            this.Parent = Parent;
        }

        public ExcelRowCells Create(int RowIndex)
        {
            ExcelRowCells Cells = new ExcelRowCells(Parent, RowIndex);

            int Index = RowCellsCollection.FindIndex(i => i.RowIndex > Cells.RowIndex);
            RowCellsCollection.Insert(Index < 0 ? RowCellsCollection.Count : Index, Cells);

            return Cells;
        }

        public bool TryGetCellRows(int RowIndex, out ExcelRowCells Cells)
        {
            if (RowCellsCollection.FirstOrDefault(i => i.RowIndex == RowIndex) is not ExcelRowCells TempCells)
            {
                Cells = null;
                return false;
            }

            Cells = TempCells;
            return true;
        }

        public void Clear()
        {
            foreach (ExcelRowCells RowCells in RowCellsCollection)
                RowCells.Clear();

            RowCellsCollection.Clear();
        }

    }
}