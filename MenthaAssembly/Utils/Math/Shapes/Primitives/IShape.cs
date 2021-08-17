using System;

namespace MenthaAssembly
{
    public interface IShape<T> : ICloneable
        where T : unmanaged
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
        /// Indicates whether the rectangle contains the specified point.
        /// </summary>
        /// <param name="Point">The point to check.</param>
        /// <returns></returns>
        public bool Contain(Point<T> Point);
        /// <summary>
        /// Indicates whether the rectangle contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        /// <returns></returns>
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
        /// Offsets this shape's coordinates by the specified vector.
        /// </summary>
        /// <param name="Vector">The vector to be added to this shape.</param>
        public void Offset(Vector<T> Vector);
        /// <summary>
        /// Offsets this shape's coordinates by the specified amounts.
        /// </summary>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public void Offset(T Dx, T Dy);

        /// <summary>
        /// Rotates this shape about the origin.
        /// </summary>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(double Theta);
        /// <summary>
        /// Rotates this shape about the specified point.
        /// </summary>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(Point<T> Center, double Theta);
        /// <summary>
        /// Rotates this shape about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(T Cx, T Cy, double Theta);

        /// <summary>
        /// Reflects the shape over the specified line.
        /// </summary>
        /// <param name="Line">The projection line.</param>
        public void Reflect(Line<T> Line);
        /// <summary>
        /// Reflects the shape over the specified line.
        /// </summary>
        /// <param name="LinePoint1">The point on the projection line.</param>
        /// <param name="LinePoint2">The another point on the projection line.</param>
        public void Reflect(Point<T> LinePoint1, Point<T> LinePoint2);
        /// <summary>
        /// Reflects the shape over the specified line.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the projection line.</param>
        public void Reflect(T Lx1, T Ly1, T Lx2, T Ly2);

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="IShape{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(IShape<T> obj);

        /// <summary>
        /// Creates a new casted <see cref="IShape{T}"/>.
        /// </summary>
        /// <returns></returns>
        public IShape<U> Cast<U>() where U : unmanaged;

        /// <summary>
        /// Creates a new <see cref="IShape{T}"/> that is a copy of the current instance.
        /// </summary>
        public new IShape<T> Clone();

    }

}
