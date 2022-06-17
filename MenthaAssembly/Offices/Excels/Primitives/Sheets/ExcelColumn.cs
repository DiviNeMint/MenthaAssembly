namespace MenthaAssembly.Offices
{
    public class ExcelColumn
    {
        public int MinIndex { get; }

        public int MaxIndex { get; }

        public bool Hidden { get; }

        private readonly double _Width;
        public double Width
            => _Width == -1 ? Parent.DefaultColumnWidth : _Width;

        public IExcelSheet Parent { get; }

        public ExcelColumn(IExcelSheet Parent, int MinIndex, int MaxIndex, bool Hidden, double Width)
        {
            this.Parent = Parent;
            this.MinIndex = MinIndex;
            this.MaxIndex = MaxIndex;
            this.Hidden = Hidden;
            this._Width = Width;
        }

        public override string ToString()
            => $"MinIndex : {MinIndex}, MaxIndex : {MaxIndex}, Width : {Width}, Hidden : {Hidden}";

    }
}
