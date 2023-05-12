using System;
using System.Linq.Expressions;
using static MenthaAssembly.OperatorHelper;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a displacement in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct Vector<T> : ICoordinateObject<T>
        where T : unmanaged
    {
        /// <summary>
        /// Gets a zero vector.
        /// </summary>
        public static Vector<T> Zero => new();

        /// <summary>
        /// The delta on x-coordinate.
        /// </summary>
        public T X { set; get; }

        /// <summary>
        /// The delta on y-coordinate.
        /// </summary>
        public T Y { set; get; }

        /// <summary>
        /// The squared length of this Vector.
        /// </summary>
        public T LengthSquare
            => Add<T>(Multiply<T>(X, X), Multiply<T>(Y, Y));

        /// <summary>
        /// The length of this Vector.
        /// </summary>
        public double Length
            => Math.Sqrt(Cast<T, double>(LengthSquare));

        /// <summary>
        ///  Gets a value indicating whether the <see cref="Vector{T}"/> is zero vector.
        /// </summary>
        public bool IsZero
            => IsDefault(X) && IsDefault(Y);

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Dx">The delta on x-coordinate.</param>
        /// <param name="Dy">The delta on y-coordinate.</param>
        public Vector(T Dx, T Dy)
        {
            X = Dx;
            Y = Dy;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start point.</param>
        /// <param name="Sy">The y-coordinate of the start point.</param>
        /// <param name="End">The end point.</param>
        public Vector(T Sx, T Sy, Point<T> End)
        {
            X = Subtract<T>(End.X, Sx);
            Y = Subtract<T>(End.Y, Sy);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Start">The start point.</param>
        /// <param name="Ex">The x-coordinate of the end point.</param>
        /// <param name="Ey">The y-coordinate of the end point.</param>
        public Vector(Point<T> Start, T Ex, T Ey)
        {
            X = Subtract<T>(Ex, Start.X);
            Y = Subtract<T>(Ey, Start.Y);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Start">The start point.</param>
        /// <param name="End">The end point.</param>
        public Vector(Point<T> Start, Point<T> End)
        {
            X = Subtract<T>(End.X, Start.X);
            Y = Subtract<T>(End.Y, Start.Y);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start point.</param>
        /// <param name="Sy">The y-coordinate of the start point.</param>
        /// <param name="Ex">The x-coordinate of the end point.</param>
        /// <param name="Ey">The y-coordinate of the end point.</param>
        public Vector(T Sx, T Sy, T Ex, T Ey)
        {
            X = Subtract<T>(Ex, Sx);
            Y = Subtract<T>(Ey, Sy);
        }

        void ICoordinateObject<T>.Offset(Vector<T> Vector)
            => throw new NotSupportedException();
        void ICoordinateObject<T>.Offset(T Dx, T Dy)
            => throw new NotSupportedException();

        void ICoordinateObject<T>.Rotate(double Theta)
            => throw new NotSupportedException();
        void ICoordinateObject<T>.Rotate(Point<T> Center, double Theta)
            => throw new NotSupportedException();
        void ICoordinateObject<T>.Rotate(T Cx, T Cy, double Theta)
            => throw new NotSupportedException();

        void ICoordinateObject<T>.Reflect(Line<T> Line)
            => throw new NotSupportedException();
        void ICoordinateObject<T>.Reflect(Point<T> LinePoint1, Point<T> LinePoint2)
            => throw new NotSupportedException();
        void ICoordinateObject<T>.Reflect(T Lx1, T Ly1, T Lx2, T Ly2)
            => throw new NotSupportedException();

        /// <summary>
        /// Creates a new casted <see cref="Vector{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Vector<U> Cast<U>()
            where U : unmanaged
            => new(Cast<T, U>(X), Cast<T, U>(Y));
        ICoordinateObject<U> ICoordinateObject<T>.Cast<U>()
            => Cast<U>();

        /// <summary>
        /// Creates a new <see cref="Vector{T}"/> that is a copy of the current instance.
        /// </summary>
        public Vector<T> Clone()
            => new(X, Y);
        ICoordinateObject<T> ICoordinateObject<T>.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => X.GetHashCode() ^ Y.GetHashCode();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Vector{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Vector<T> obj)
            => OperatorHelper.Equals(X, obj.X) && OperatorHelper.Equals(Y, obj.Y);
        bool ICoordinateObject<T>.Equals(ICoordinateObject<T> obj)
            => obj is Vector<T> Target && Equals(Target);
        public override bool Equals(object obj)
            => obj is Vector<T> Target && Equals(Target);

        public override string ToString()
            => $"X : {X}, Y : {Y}";

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="Vector1">The first vector to evaluate.</param>
        /// <param name="Vector2">The second vector to evaluate.</param>
        public static T Dot(Vector<T> Vector1, Vector<T> Vector2)
            => Add<T>(Multiply<T>(Vector1.X, Vector2.X), Multiply<T>(Vector1.Y, Vector2.Y));
        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="Dx1">The delta on x-coordinate of first vector to evaluate.</param>
        /// <param name="Dy1">The delta on y-coordinate of first vector to evaluate.</param>
        /// <param name="Dx2">The delta on x-coordinate of second vector to evaluate.</param>
        /// <param name="Dy2">The delta on y-coordinate of second vector to evaluate.</param>
        /// <returns></returns>
        public static T Dot(T Dx1, T Dy1, T Dx2, T Dy2)
            => Add<T>(Multiply<T>(Dx1, Dx2), Multiply<T>(Dy1, Dy2));

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="Vector1">The first vector to evaluate.</param>
        /// <param name="Vector2">The second vector to evaluate.</param>
        public static T Cross(Vector<T> Vector1, Vector<T> Vector2)
            => Subtract<T>(Multiply<T>(Vector1.X, Vector2.Y), Multiply<T>(Vector1.Y, Vector2.X));
        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="Dx1">The delta on x-coordinate of first vector to evaluate.</param>
        /// <param name="Dy1">The delta on y-coordinate of first vector to evaluate.</param>
        /// <param name="Dx2">The delta on x-coordinate of second vector to evaluate.</param>
        /// <param name="Dy2">The delta on y-coordinate of second vector to evaluate.</param>
        /// <returns></returns>
        public static T Cross(T Dx1, T Dy1, T Dx2, T Dy2)
            => Subtract<T>(Multiply<T>(Dx1, Dy2), Multiply<T>(Dy1, Dx2));

        /// <summary>
        /// Retrieves the angle, expressed in degrees, between the two specified vectors.
        /// </summary>
        /// <param name="Vector1">The first vector to evaluate.</param>
        /// <param name="Vector2">The second vector to evaluate.</param>
        /// <returns></returns>
        public static double AngleBetween(Vector<T> Vector1, Vector<T> Vector2)
        {
            T Dot = Vector<T>.Dot(Vector1, Vector2);
            return Math.Sqrt(Cast<T, double>(Multiply<T>(Dot, Dot)) / Cast<T, double>(Multiply<T>(Vector1.LengthSquare, Vector2.LengthSquare)));
        }
        /// <summary>
        /// Retrieves the angle, expressed in degrees, between the two specified vectors.
        /// </summary>
        /// <param name="Dx1">The delta on x-coordinate of first vector to evaluate.</param>
        /// <param name="Dy1">The delta on y-coordinate of first vector to evaluate.</param>
        /// <param name="Dx2">The delta on x-coordinate of second vector to evaluate.</param>
        /// <param name="Dy2">The delta on y-coordinate of second vector to evaluate.</param>
        public static double AngleBetween(T Dx1, T Dy1, T Dx2, T Dy2)
        {
            T Dot = Vector<T>.Dot(Dx1, Dy1, Dx2, Dy2),
              LengthSquare1 = Add<T>(Multiply<T>(Dx1, Dx1), Multiply<T>(Dy1, Dy1)),
              LengthSquare2 = Add<T>(Multiply<T>(Dx2, Dx2), Multiply<T>(Dy2, Dy2));
            return Math.Sqrt(Cast<T, double>(Multiply<T>(Dot, Dot)) / Cast<T, double>(Multiply<T>(LengthSquare1, LengthSquare2)));
        }

        /// <summary>
        /// Adds two vectors and returns the result as a <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Vector1">The first vector to add.</param>
        /// <param name="Vector2">The second vector to add.</param>
        /// <returns></returns>
        public static Vector<T> Add(Vector<T> Vector1, Vector<T> Vector2)
            => new(Add<T>(Vector1.X, Vector2.X), Add<T>(Vector1.Y, Vector2.Y));
        /// <summary>
        /// Adds two vectors and returns the result as a <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Vector">The first vector to add.</param>
        /// <param name="Dx">The delta on x-coordinate of second vector to add.</param>
        /// <param name="Dy">The delta on y-coordinate of second vector to add.</param>
        /// <returns></returns>
        public static Vector<T> Add(Vector<T> Vector, T Dx, T Dy)
            => new(Add<T>(Vector.X, Dx), Add<T>(Vector.Y, Dy));

        /// <summary>
        /// Subtracts the specified vector from another specified vector.
        /// </summary>
        /// <param name="Vector1">The vector from which Vector2 is subtracted.</param>
        /// <param name="Vector2">The vector to subtract from Vector1.</param>
        /// <returns></returns>
        public static Vector<T> Subtract(Vector<T> Vector1, Vector<T> Vector2)
            => new(Subtract<T>(Vector1.X, Vector2.X), Subtract<T>(Vector1.Y, Vector2.Y));
        /// <summary>
        /// Subtracts the specified vector from another specified vector.
        /// </summary>
        /// <param name="Vector">The vector from which Vector2 is subtracted.</param>
        /// <param name="Dx">The delta on x-coordinate of the vector to subtract from Vector.</param>
        /// <param name="Dy">The delta on y-coordinate of the vector to subtract from Vector.</param>
        /// <returns></returns>
        public static Vector<T> Subtract(Vector<T> Vector, T Dx, T Dy)
            => new(Subtract<T>(Vector.X, Dx), Subtract<T>(Vector.Y, Dy));

        /// <summary>
        /// Multiplies the specified vector by the specified scalar.
        /// </summary>
        /// <param name="Vector">The vector to multiply.</param>
        /// <param name="Scalar">The scalar to multiply.</param>
        /// <returns></returns>
        public static Vector<T> Multiply(Vector<T> Vector, T Scalar)
            => Multiply(Vector, Scalar, Scalar);
        /// <summary>
        /// Multiplies the specified vector by the specified scalar.
        /// </summary>
        /// <param name="Vector">The vector to multiply.</param>
        /// <param name="ScalarX">The scalar to multiply on x-coordinate.</param>
        /// <param name="ScalarY">The scalar to multiply on y-coordinate.</param>
        /// <returns></returns>
        public static Vector<T> Multiply(Vector<T> Vector, T ScalarX, T ScalarY)
            => new(Multiply<T>(Vector.X, ScalarX), Multiply<T>(Vector.Y, ScalarY));

        /// <summary>
        /// Divides the specified vector by the specified scalar.
        /// </summary>
        /// <param name="Vector">The vector to divide.</param>
        /// <param name="Scalar">The scalar to divide.</param>
        /// <returns></returns>
        public static Vector<T> Divide(Vector<T> Vector, T Scalar)
            => Divide(Vector, Scalar, Scalar);
        /// <summary>
        /// Divides the specified vector by the specified scalar.
        /// </summary>
        /// <param name="Vector">The vector to divide.</param>
        /// <param name="ScalarX">The scalar to divide on x-coordinate.</param>
        /// <param name="ScalarY">The scalar to divide on y-coordinate.</param>
        /// <returns></returns>
        public static Vector<T> Divide(Vector<T> Vector, T ScalarX, T ScalarY)
            => new(Divide<T>(Vector.X, ScalarX), Divide<T>(Vector.Y, ScalarY));

        /// <summary>
        /// Negates this vector. The vector has the same magnitude as before, but its direction is now opposite.
        /// </summary>
        /// <param name="Vector">The vector to reverse.</param>
        public static Vector<T> Reverse(Vector<T> Vector)
            => new(Negate(Vector.X), Negate(Vector.Y));

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="Vector1">The first vector to evaluate.</param>
        /// <param name="Vector2">The second vector to evaluate.</param>
        public static T operator *(Vector<T> Vector1, Vector<T> Vector2)
            => Dot(Vector1, Vector2);

        /// <summary>
        /// Negates the specified vector.
        /// </summary>
        /// <param name="Vector">The vector to negate.</param>
        /// <returns></returns>
        public static Vector<T> operator -(Vector<T> Vector)
            => Reverse(Vector);
        /// <summary>
        /// Subtracts the specified vector from another specified vector.
        /// </summary>
        /// <param name="Vector1">The vector from which Vector2 is subtracted.</param>
        /// <param name="Vector2">The vector to subtract from Vector1.</param>
        /// <returns></returns>
        public static Vector<T> operator -(Vector<T> Vector1, Vector<T> Vector2)
            => Subtract(Vector1, Vector2);
        /// <summary>
        /// Adds two vectors and returns the result as a <see cref="Vector{T}"/> structure.
        /// </summary>
        /// <param name="Vector1">The first vector to add.</param>
        /// <param name="Vector2">The second vector to add.</param>
        /// <returns></returns>
        public static Vector<T> operator +(Vector<T> Vector1, Vector<T> Vector2)
            => Add(Vector1, Vector2);

        /// <summary>
        /// Multiplies the specified vector by the specified scalar.
        /// </summary>
        /// <param name="Vector">The vector to multiply.</param>
        /// <param name="Scalar">The scalar to multiply.</param>
        /// <returns></returns>
        public static Vector<T> operator *(Vector<T> Vector, T Scalar)
            => Multiply(Vector, Scalar, Scalar);
        /// <summary>
        /// Divides the specified vector by the specified scalar.
        /// </summary>
        /// <param name="Vector">The vector to divide.</param>
        /// <param name="Scalar">The scalar to divide.</param>
        /// <returns></returns>
        public static Vector<T> operator /(Vector<T> Vector, T Scalar)
            => Divide(Vector, Scalar, Scalar);

        /// <summary>
        /// Compares two vectors for equality.
        /// </summary>
        /// <param name="Vector1">The first vector to compare.</param>
        /// <param name="Vector2">The second vector to compare.</param>
        public static bool operator ==(Vector<T> Vector1, Vector<T> Vector2)
            => Vector1.Equals(Vector2);
        /// <summary>
        /// Compares two vectors for inequality.
        /// </summary>
        /// <param name="Vector1">The first vector to compare.</param>
        /// <param name="Vector2">The second vector to compare.</param>
        public static bool operator !=(Vector<T> Vector1, Vector<T> Vector2)
            => !Vector1.Equals(Vector2);

    }
}
