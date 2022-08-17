using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    internal class ExcelCellCollection
    {
        private readonly IExcelSheet Parent;
        private readonly Dictionary<int, ExcelRowCells> RowCellsCollection = new Dictionary<int, ExcelRowCells>();

        public ExcelCellCollection(IExcelSheet Parent)
        {
            this.Parent = Parent;
        }

        public ExcelRowCells Create(int RowIndex)
        {
            ExcelRowCells Cells = new ExcelRowCells(Parent, RowIndex);
            RowCellsCollection[RowIndex] = Cells;
            return Cells;
        }

        public bool TryGetCellRows(int RowIndex, out ExcelRowCells Cells)
        {
            if (!RowCellsCollection.TryGetValue(RowIndex, out ExcelRowCells TempCells))
            {
                Cells = null;
                return false;
            }

            Cells = TempCells;
            return true;
        }

        public void Clear()
        {
            foreach (ExcelRowCells RowCells in RowCellsCollection.Values)
                RowCells.Clear();

            RowCellsCollection.Clear();
        }

    }
}