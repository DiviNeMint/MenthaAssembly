using System;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a coordinate in 2-D space.
    /// </summary>
    [Serializable]
    public struct Point<T> : ICloneable
        where T : struct
    {
        /// <summary>
        /// Gets the origin of coordinates.
        /// </summary>
        public static Point<T> Origin => new Point<T>();

        /// <summary>
        /// The x-coordinate of this Point.
        /// </summary>
        public T X { set; get; }

        /// <summary>
        /// The y-coordinate of this Point.
        /// </summary>
        public T Y { set; get; }

        /// <summary>
        ///  Gets a value indicating whether the point is the origin of coordinates.
        /// </summary>
        public bool IsOrigin => IsDefault(X) && IsDefault(Y);

        /// <summary>
        /// Constructor which accepts the X and Y values
        /// </summary>
        /// <param name="X">The value for the X coordinate of the new point.</param>
        /// <param name="Y">The value for the Y coordinate of the new point.</param>
        public Point(T X, T Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Offsets <see cref="X"/> and <see cref="Y"/> coordinates by the specified vector.
        /// </summary>
        /// <param name="Vector">The vector to be added to this point.</param>
        public void Offset(Vector<T> Vector)
        {
            X = Add(X, Vector.X);
            Y = Add(Y, Vector.Y);
        }
        /// <summary>
        /// Offsets <see cref="X"/> and <see cref="Y"/> coordinates by the specified amounts.
        /// </summary>
        /// <param name="Dx">The amount to offset <see cref="X"/> coordinate.</param>
        /// <param name="Dy">The amount to offset <see cref="Y"/> coordinate.</param>
        public void Offset(T Dx, T Dy)
        {
            X = Add(X, Dx);
            Y = Add(Y, Dy);
        }

        /// <summary>
        /// Rotates this point about the origin.
        /// </summary>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(double Theta)
            => Rotate(Math.Sin(Theta), Math.Cos(Theta));
        /// <summary>
        /// Rotates this point about the origin.
        /// </summary>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public void Rotate(double Sin, double Cos)
        {
            Rotate(X, Y, Sin, Cos, out T Qx, out T Qy);
            this.X = Qx;
            this.Y = Qy;
        }
        /// <summary>
        /// Rotates this point about the specified point.
        /// </summary>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(Point<T> Center, double Theta)
            => Rotate(Center.X, Center.Y, Math.Sin(Theta), Math.Cos(Theta));
        /// <summary>
        /// Rotates this point about the specified point.
        /// </summary>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public void Rotate(Point<T> Center, double Sin, double Cos)
            => Rotate(Center.X, Center.Y, Sin, Cos);
        /// <summary>
        /// Rotates this point about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(T Cx, T Cy, double Theta)
            => Rotate(Cx, Cy, Math.Sin(Theta), Math.Cos(Theta));
        /// <summary>
        /// Rotates this point about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public void Rotate(T Cx, T Cy, double Sin, double Cos)
        {
            Rotate(X, Y, Cx, Cy, Sin, Cos, out T Qx, out T Qy);
            this.X = Qx;
            this.Y = Qy;
        }

        /// <summary>
        /// Creates a new casted point.
        /// </summary>
        /// <returns></returns>
        public Point<U> Cast<U>()
            where U : struct
        {
            Func<T, U> CastHandler = ExpressionHelper<T>.CreateCast<U>();
            return new Point<U>(CastHandler(this.X), CastHandler(this.Y));
        }

        /// <summary>
        /// Creates a new point that is a copy of the current instance.
        /// </summary>
        public Point<T> Clone()
            => new Point<T>(X, Y);
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => X.GetHashCode() ^ Y.GetHashCode();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified point
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Point<T> obj)
            => Equal(this.X, obj.X) && Equal(this.Y, obj.Y);
        public override bool Equals(object obj)
        {
            if (obj is Point<T> Target)
                return Equals(Target);

            return false;
        }

        public override string ToString()
            => $"X : {this.X}, Y : {this.Y}";

        internal static readonly Func<T, T> Neg;
        internal static readonly Func<T, T, T> Add, Sub, Mul, Div;
        internal static readonly Predicate<T> IsDefault;
        internal static readonly Func<T, T, bool> Equal;
        internal static readonly Func<T, double> ToDouble;
        internal static readonly Func<double, T> ToGeneric;
        static Point()
        {
            Neg = ExpressionHelper<T>.CreateNeg();

            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();
            Div = ExpressionHelper<T>.CreateDiv();

            IsDefault = ExpressionHelper<T>.CreateIsDefault();
            Equal = ExpressionHelper<T>.CreateEqual();

            ToDouble = ExpressionHelper<T>.CreateCast<double>();
            ToGeneric = ExpressionHelper<double>.CreateCast<T>();
        }

        /// <summary>
        /// Calculate the distance between two points.
        /// </summary>
        /// <param name="Point1">The first target point.</param>
        /// <param name="Point2">the second target point.</param>
        public static double Distance(Point<T> Point1, Point<T> Point2)
            => Distance(Point1.X, Point1.Y, Point2.X, Point2.Y);
        /// <summary>
        /// Calculate the distance between two points.
        /// </summary>
        /// <param name="Point">The first target point.</param>
        /// <param name="Qx">The x-coordinate of the second target point.</param>
        /// <param name="Qy">The y-coordinate of the second target point.</param>
        public static double Distance(Point<T> Point, T Qx, T Qy)
            => Distance(Point.X, Point.Y, Qx, Qy);
        /// <summary>
        /// Calculate the distance between two points.
        /// </summary>
        /// <param name="Px">The x-coordinate of the first target point.</param>
        /// <param name="Py">The y-coordinate of the first target point.</param>
        /// <param name="Qx">The x-coordinate of the second target point.</param>
        /// <param name="Qy">The y-coordinate of the second target point.</param>
        public static double Distance(T Px, T Py, T Qx, T Qy)
        {
            T Dx = Sub(Qx, Px),
              Dy = Sub(Qy, Py);
            return Math.Sqrt(ToDouble(Add(Mul(Dx, Dx), Mul(Dy, Dy))));
        }

        /// <summary>
        /// Offsets the specified point by the specified vector.
        /// </summary>
        /// <param name="Point">The point to be offsetted.</param>
        /// <param name="Vector">The vector to be added to this point.</param>
        public static Point<T> Offset(Point<T> Point, Vector<T> Vector)
            => new Point<T>(Add(Point.X, Vector.X), Add(Point.Y, Vector.Y));
        /// <summary>
        /// Offsets the specified point by the specified amounts.
        /// </summary>
        /// <param name="Point">The point to be offsetted.</param>
        /// <param name="Dx">The amount to offset <see cref="X"/> coordinate.</param>
        /// <param name="Dy">The amount to offset <see cref="Y"/> coordinate.</param>
        public static Point<T> Offset(Point<T> Point, T Dx, T Dy)
            => new Point<T>(Add(Point.X, Dx), Add(Point.Y, Dy));

        /// <summary>
        /// Rotates the specified point about the specified point.
        /// </summary>
        /// <param name="Point">The point to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Point<T> Rotate(Point<T> Point, Point<T> Center, double Theta)
            => Rotate(Point, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified point about the specified point.
        /// </summary>
        /// <param name="Point">The point to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Point<T> Rotate(Point<T> Point, T Cx, T Cy, double Theta)
        {
            Rotate(Point.X, Point.Y, Cx, Cy, Theta, out T Qx, out T Qy);
            return new Point<T>(Qx, Qy);
        }
        /// <summary>
        /// Rotates the specified point about the specified point.
        /// </summary>
        /// <param name="Px">The x-coordinate of point to be rotated.</param>
        /// <param name="Py">The y-coordinate of point to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        /// <param name="Qx">The x-coordinate of the rotated point.</param>
        /// <param name="Qy">The y-coordinate of the rotated point.</param>
        public static void Rotate(T Px, T Py, T Cx, T Cy, double Theta, out T Qx, out T Qy)
            => Rotate(Px, Py, Cx, Cy, Math.Sin(Theta), Math.Cos(Theta), out Qx, out Qy);
        /// <summary>
        /// Rotates the specified point about the specified point.
        /// </summary>
        /// <param name="Px">The x-coordinate of point to be rotated.</param>
        /// <param name="Py">The y-coordinate of point to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        /// <param name="Qx">The x-coordinate of the rotated point.</param>
        /// <param name="Qy">The y-coordinate of the rotated point.</param>
        public static void Rotate(T Px, T Py, T Cx, T Cy, double Sin, double Cos, out T Qx, out T Qy)
        {
            Rotate(Sub(Px, Cx), Sub(Py, Cy), Sin, Cos, out Qx, out Qy);

            Qx = Add(Qx, Cx);
            Qy = Add(Qy, Cy);
        }
        /// <summary>
        /// Rotates the specified point about the origin.
        /// </summary>
        /// <param name="Px">The x-coordinate of point to be rotated.</param>
        /// <param name="Py">The y-coordinate of point to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        /// <param name="Qx">The x-coordinate of the rotated point.</param>
        /// <param name="Qy">The y-coordinate of the rotated point.</param>
        public static void Rotate(T Px, T Py, double Theta, out T Qx, out T Qy)
            => Rotate(Px, Py, Math.Sin(Theta), Math.Cos(Theta), out Qx, out Qy);
        /// <summary>
        /// Rotates the specified point about the origin.
        /// </summary>
        /// <param name="Px">The x-coordinate of point to be rotated.</param>
        /// <param name="Py">The y-coordinate of point to be rotated.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        /// <param name="Qx">The x-coordinate of the rotated point.</param>
        /// <param name="Qy">The y-coordinate of the rotated point.</param>
        public static void Rotate(T Px, T Py, double Sin, double Cos, out T Qx, out T Qy)
        {
            Qx = ToGeneric(ToDouble(Px) * Cos - ToDouble(Py) * Sin);
            Qy = ToGeneric(ToDouble(Px) * Sin + ToDouble(Py) * Cos);
        }

        /// <summary>
        /// Negates this Point. The Point has the same magnitude as before, but its quadrant is now opposite.
        /// </summary>
        /// <param name="Point">The Point to negates.</param>
        public static Point<T> Negates(Point<T> Point)
            => new Point<T>(Neg(Point.X), Neg(Point.Y));

        /// <summary>
        /// Negates this Point. The Point has the same magnitude as before, but its quadrant is now opposite.
        /// </summary>
        /// <param name="Point">The Point to negates.</param>
        public static Point<T> operator -(Point<T> Point)
            => Negates(Point);

        /// <summary>
        /// Subtracts the specified vector from the specified point.
        /// </summary>
        /// <param name="Point">The point from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from point.</param>
        public static Point<T> operator -(Point<T> Point, Vector<T> Vector)
            => Offset(Point, -Vector);
        /// <summary>
        /// Adds the specified vector to the specified point.
        /// </summary>
        /// <param name="Point">The point structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static Point<T> operator +(Point<T> Point, Vector<T> Vector)
            => Offset(Point, Vector);

        /// <summary>
        /// Subtracts the specified point from another specified point.
        /// </summary>
        /// <param name="Point1">The point from which point2 is subtracted.</param>
        /// <param name="Point2">The point to subtract from point1.</param>
        /// <returns></returns>
        public static Vector<T> operator -(Point<T> Point1, Point<T> Point2)
            => new Vector<T>(Point2, Point1);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="Point1">The first point to compare.</param>
        /// <param name="Point2">The second point to compare.</param>
        public static bool operator ==(Point<T> Point1, Point<T> Point2)
            => Point1.Equals(Point2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="Point1">The first point to compare.</param>
        /// <param name="Point2">The second point to compare.</param>
        public static bool operator !=(Point<T> Point1, Point<T> Point2)
            => !Point1.Equals(Point2);

    }
}
