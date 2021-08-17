namespace MenthaAssembly
{
    public interface IPolygonShape<T> : IShape<T>
        where T : unmanaged
    {
        /// <summary>
        /// The points of the shape.
        /// </summary>
        public Point<T>[] Points { set; get; }

    }
}
