using System;
using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    public class ExcelRow : IEquatable<ExcelRow>
    {
        public IExcelSheet Parent { get; }

        public int Index { get; }

        public bool Hidden { get; }

        private readonly double _Height;
        public double Height
            => _Height == -1 ? Parent.DefaultRowHeight : _Height;

        internal ExcelRow(IExcelSheet Parent, int Index, bool Hidden, double Height)
        {
            this.Index = Index;
            this.Hidden = Hidden;
            this._Height = Height;
            this.Parent = Parent;
        }

        public override bool Equals(object obj)
            => obj is ExcelRow c && Equals(c);
        public bool Equals(ExcelRow other)
            => other.Parent == Parent &&
               other.Index == Index;

        public override int GetHashCode()
        {
            int hashCode = 1130656240;
            hashCode = hashCode * -1521134295 + EqualityComparer<IExcelSheet>.Default.GetHashCode(Parent);
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            return hashCode;
        }

        public override string ToString()
            => $"Index : {Index}, Height : {Height}, Hidden : {Hidden}";

    }
}
