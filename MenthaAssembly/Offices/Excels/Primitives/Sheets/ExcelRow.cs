namespace MenthaAssembly.Offices
{
    public class ExcelRow
    {
        public int Index { get; }

        public bool Hidden { get; }

        private readonly double _Height;
        public double Height
            => _Height == -1 ? Parent.DefaultRowHeight : _Height;

        public IExcelSheet Parent { get; }

        internal ExcelRow(IExcelSheet Parent, int Index, bool Hidden, double Height)
        {
            this.Index = Index;
            this.Hidden = Hidden;
            this._Height = Height;
            this.Parent = Parent;
        }

        public override string ToString()
            => $"Index : {Index}, Height : {Height}, Hidden : {Hidden}";

    }
}
