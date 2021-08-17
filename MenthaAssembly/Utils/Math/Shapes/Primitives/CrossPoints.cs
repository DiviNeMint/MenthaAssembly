using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenthaAssembly
{
    public sealed class CrossPoints<T> : IEnumerable<Point<T>>
        where T : unmanaged
    {
        private readonly IEnumerable<Point<T>> Points;

        public int Count { get; }

        public bool IsInfinity { get; }

        internal CrossPoints(bool IsInfinity)
        {
            this.Points = new Point<T>[0];
            this.IsInfinity = IsInfinity;
            this.Count = IsInfinity ? int.MaxValue : 0;
        }
        internal CrossPoints(Point<T> Point)
        {
            this.Points = new[] { Point };
            this.Count = 1;
        }
        internal CrossPoints(params Point<T>[] Points)
        {
            this.Points = Points;
            this.Count = Points.Length;
        }
        internal CrossPoints(IEnumerable<Point<T>> Points)
        {
            this.Points = Points;
            this.Count = Points.Count();
        }

        public override string ToString()
            => IsInfinity ? "Infinity" : string.Join(", ", Points.Select(i => $"{{{i}}}"));

        public IEnumerator<Point<T>> GetEnumerator()
            => this.Points.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => this.Points.GetEnumerator();

    }
}
