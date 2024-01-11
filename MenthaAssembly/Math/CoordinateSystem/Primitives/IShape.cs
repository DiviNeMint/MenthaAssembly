#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a shape in 2-D space.
    /// </summary>
    public interface IShape<T> : ICoordinateObject<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : unmanaged
#endif
    {
        /// <summary>
        /// The center of the shape.
        /// </summary>
        public Point<T> Center { get; }

        /// <summary>
        /// The area of the shape.
        /// </summary>
        public double Area { get; }

        /// <summary>
        /// Gets a value that indicates whether the shape is the empty shape.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Indicates whether the shape contains the specified point.
        /// </summary>
        /// <param name="Point">The point to check.</param>
        public bool Contain(Point<T> Point);
        /// <summary>
        /// Indicates whether the shape contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        public bool Contain(T Px, T Py);

        /// <summary>
        /// Scales this shape around the origin.
        /// </summary>
        /// <param name="Scale">The scale factor.</param>
        public void Scale(T Scale);
        /// <summary>
        /// Scales this shape around the origin.
        /// </summary>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public void Scale(T ScaleX, T ScaleY);
        /// <summary>
        /// Scales this shape around the specified point.
        /// </summary>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public void Scale(Point<T> Center, T Scale);
        /// <summary>
        /// Scales this shape around the specified point.
        /// </summary>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public void Scale(Point<T> Center, T ScaleX, T ScaleY);
        /// <summary>
        /// Scales this shape around the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public void Scale(T Cx, T Cy, T Scale);
        /// <summary>
        /// Scales this shape around the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public void Scale(T Cx, T Cy, T ScaleX, T ScaleY);

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="IShape{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(IShape<T> obj);

        /// <summary>
        /// Creates a new casted <see cref="IShape{T}"/>.
        /// </summary>
        public new IShape<U> Cast<U>()
#if NET7_0_OR_GREATER
        where U : INumber<U>;
#else
        where U : unmanaged;
#endif

        /// <summary>
        /// Creates a new <see cref="IShape{T}"/> that is a copy of the current instance.
        /// </summary>
        public new IShape<T> Clone();

    }
}
