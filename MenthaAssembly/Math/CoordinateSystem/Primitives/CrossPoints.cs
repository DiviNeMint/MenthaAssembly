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
        /// <summary>
        /// Gets a special value that represents no cross point.
        /// </summary>
        public static CrossPoints<T> None => new(false);

        /// <summary>
        /// Gets a special value that represents infinity cross point.
        /// </summary>
        public static CrossPoints<T> Infinity => new(true);

        private readonly IEnumerable<Point<T>> Points;

        /// <summary>
        /// The count of cross points.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets a value that indicates whether the count of cross points is infinity.
        /// </summary>
        public bool IsInfinity { get; }

        private CrossPoints(bool IsInfinity)
        {
            Points = new Point<T>[0];
            this.IsInfinity = IsInfinity;
            Count = IsInfinity ? int.MinValue : 0;
        }
        internal CrossPoints(params Point<T>[] Points)
        {
            this.Points = Points;
            Count = Points.Length;
        }
        internal CrossPoints(IEnumerable<Point<T>> Points)
        {
            this.Points = Points;
            Count = Points.Count();
        }

        public override string ToString()
            => IsInfinity ? "Infinity" : string.Join(", ", Points.Select(i => $"{{{i}}}"));

        public IEnumerator<Point<T>> GetEnumerator()
            => Points.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => Points.GetEnumerator();

    }
}