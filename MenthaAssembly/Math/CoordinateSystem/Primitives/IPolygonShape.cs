namespace MenthaAssembly
{
    /// <summary>
    /// Represents a polygon shape in 2-D space.
    /// </summary>
    public interface IPolygonShape<T> : IShape<T>
        where T : unmanaged
    {
        /// <summary>
        /// The points of the shape.
        /// </summary>
        public Point<T>[] Points { set; get; }

    }
}
