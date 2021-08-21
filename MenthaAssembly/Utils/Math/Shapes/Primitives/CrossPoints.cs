using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents the collection of coordinate in 2-D space.
    /// </summary>
    [Serializable]
    public sealed class CrossPoints<T> : IEnumerable<Point<T>>
        where T : unmanaged
    {
        private readonly IEnumerable<Point<T>> Points;

        /// <summary>
        /// The count of cross points.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets a value that indicates whether the count of cross points is infinity.
        /// </summary>
        public bool IsInfinity { get; }

        internal CrossPoints(bool IsInfinity)
        {
            Points = new Point<T>[0];
            this.IsInfinity = IsInfinity;
            this.Count = IsInfinity ? int.MaxValue : 0;
        }
        internal CrossPoints(Point<T> Point)
        {
            Points = new[] { Point };
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
            => this.IsInfinity ? "Infinity" : string.Join(", ", Points.Select(i => $"{{{i}}}"));

        public IEnumerator<Point<T>> GetEnumerator()
            => Points.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => Points.GetEnumerator();

    }
}
