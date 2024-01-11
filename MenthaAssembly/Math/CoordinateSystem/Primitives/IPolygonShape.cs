#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a polygon shape in 2-D space.
    /// </summary>
    public interface IPolygonShape<T> : IShape<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : unmanaged
#endif
    {
        /// <summary>
        /// The points of the shape.
        /// </summary>
        public Point<T>[] Points { set; get; }

    }
}
