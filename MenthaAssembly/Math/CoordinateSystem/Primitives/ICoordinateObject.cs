using System;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a object in 2-D space.
    /// </summary>
    public interface ICoordinateObject<T> : ICloneable
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : unmanaged
#endif
    {
        /// <summary>
        /// Offsets the object's coordinates by the specified vector.
        /// </summary>
        /// <param name="Vector">The vector to be added to the object.</param>
        public void Offset(Vector<T> Vector);
        /// <summary>
        /// Offsets the object's coordinates by the specified amounts.
        /// </summary>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public void Offset(T Dx, T Dy);

        /// <summary>
        /// Rotates the object about the origin.
        /// </summary>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(double Theta);
        /// <summary>
        /// Rotates the object about the specified point.
        /// </summary>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(Point<T> Center, double Theta);
        /// <summary>
        /// Rotates the object about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(T Cx, T Cy, double Theta);

        /// <summary>
        /// Reflects the object over the specified line.
        /// </summary>
        /// <param name="Line">The projection line.</param>
        public void Reflect(Line<T> Line);
        /// <summary>
        /// Reflects the object over the specified line.
        /// </summary>
        /// <param name="LinePoint1">The point on the projection line.</param>
        /// <param name="LinePoint2">The another point on the projection line.</param>
        public void Reflect(Point<T> LinePoint1, Point<T> LinePoint2);
        /// <summary>
        /// Reflects the object over the specified line.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the projection line.</param>
        public void Reflect(T Lx1, T Ly1, T Lx2, T Ly2);

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="ICoordinateObject{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(ICoordinateObject<T> obj);

        /// <summary>
        /// Creates a new casted <see cref="ICoordinateObject{T}"/>.
        /// </summary>
        public ICoordinateObject<U> Cast<U>()
#if NET7_0_OR_GREATER
        where U : INumber<U>;
#else
        where U : unmanaged;
#endif

        /// <summary>
        /// Creates a new <see cref="ICoordinateObject{T}"/> that is a copy of the current instance.
        /// </summary>
        public new ICoordinateObject<T> Clone();

    }
}
