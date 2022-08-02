using System;
using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    public class ExcelColumn : IEquatable<ExcelColumn>
    {
        public IExcelSheet Parent { get; }

        public int MinIndex { get; }

        public int MaxIndex { get; }

        public bool Hidden { get; }

        private readonly double _Width;
        public double Width
            => _Width == -1 ? Parent.DefaultColumnWidth : _Width;

        public ExcelColumn(IExcelSheet Parent, int MinIndex, int MaxIndex, bool Hidden, double Width)
        {
            this.Parent = Parent;
            this.MinIndex = MinIndex;
            this.MaxIndex = MaxIndex;
            this.Hidden = Hidden;
            this._Width = Width;
        }

        public override bool Equals(object obj)
            => obj is ExcelColumn c && Equals(c);
        public bool Equals(ExcelColumn other)
            => other.Parent == Parent &&
               other.MinIndex == MinIndex &&
               other.MaxIndex == MaxIndex;

        public override int GetHashCode()
        {
            int hashCode = 1844283282;
            hashCode = hashCode * -1521134295 + MinIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IExcelSheet>.Default.GetHashCode(Parent);
            return hashCode;
        }

        public override string ToString()
            => $"MinIndex : {MinIndex}, MaxIndex : {MaxIndex}, Width : {Width}, Hidden : {Hidden}";

    }
}
