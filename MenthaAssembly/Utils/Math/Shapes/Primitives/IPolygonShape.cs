namespace MenthaAssembly
{
    public interface IPolygonShape<T> : IShape<T>
        where T : struct
    {
        /// <summary>
        /// The points of the shape.
        /// </summary>
        public Point<T>[] Points { set; get; }

    }
}
