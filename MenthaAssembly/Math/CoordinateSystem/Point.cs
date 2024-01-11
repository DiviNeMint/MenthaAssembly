using System;
using System.Linq;
#if NET7_0_OR_GREATER
using System.Numerics;
#else
using static MenthaAssembly.OperatorHelper;
#endif

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a coordinate in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct Point<T> : ICoordinateObject<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : unmanaged
#endif
    {
        /// <summary>
        /// Gets the origin of coordinates.
        /// </summary>
        public static Point<T> Origin => new();

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
        public bool IsOrigin
        {
            get
            {
#if NET7_0_OR_GREATER
                return T.IsZero(X) && T.IsZero(Y);
#else
                return IsDefault(X) && IsDefault(Y);
#endif
            }
        }

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

        public void Offset(Vector<T> Vector)
            => Offset(Vector.X, Vector.Y);
        public void Offset(T Dx, T Dy)
        {
#if NET7_0_OR_GREATER
            X = X + Dx;
            Y = Y + Dy;
#else
            X = Add(X, Dx);
            Y = Add(Y, Dy);
#endif
        }

        public void Rotate(double Theta)
            => Rotate(Math.Sin(Theta), Math.Cos(Theta));
        /// <summary>
        /// Rotates this object about the origin.
        /// </summary>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public void Rotate(double Sin, double Cos)
        {
            Rotate(X, Y, Sin, Cos, out T Qx, out T Qy);
            X = Qx;
            Y = Qy;
        }
        public void Rotate(Point<T> Center, double Theta)
            => Rotate(Center.X, Center.Y, Math.Sin(Theta), Math.Cos(Theta));
        /// <summary>
        /// Rotates this object about the specified point.
        /// </summary>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public void Rotate(Point<T> Center, double Sin, double Cos)
            => Rotate(Center.X, Center.Y, Sin, Cos);
        public void Rotate(T Cx, T Cy, double Theta)
            => Rotate(Cx, Cy, Math.Sin(Theta), Math.Cos(Theta));
        /// <summary>
        /// Rotates this object about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public void Rotate(T Cx, T Cy, double Sin, double Cos)
        {
            Rotate(X, Y, Cx, Cy, Sin, Cos, out T Qx, out T Qy);
            X = Qx;
            Y = Qy;
        }

        public void Reflect(Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return;

            Point<T> P1 = Line.Points[0],
                     P2 = Line.Points[1];

            Reflect(P1.X, P1.Y, P2.X, P2.Y);
        }
        public void Reflect(Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        public void Reflect(T Lx1, T Ly1, T Lx2, T Ly2)
        {
#if NET7_0_OR_GREATER
            T v1x = Lx2 - Lx1,
              v1y = Ly2 - Ly1;

            if (T.IsZero(v1x))
            {
                if (T.IsZero(v1y))
                    return;

                // Over Y-Axis
                X = Lx1 * T.CreateChecked(2) - X;
            }
            else if (T.IsZero(v1y))
            {
                // Over X-Axis
                Y = Ly1 * T.CreateChecked(2) - Y;
            }
            else
            {
                double PQx = double.CreateChecked(v1x),
                       PQy = double.CreateChecked(v1y),
                       PAx = double.CreateChecked(X - Lx1),
                       PAy = double.CreateChecked(Y - Ly1),
                       k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                X = Lx1 + T.CreateChecked(PQx * k - PAx);
                Y = Ly1 + T.CreateChecked(PQy * k - PAy);
            }
#else
            T v1x = Subtract(Lx2, Lx1),
              v1y = Subtract(Ly2, Ly1);

            if (IsDefault(v1x))
            {
                if (IsDefault(v1y))
                    return;

                // Over Y-Axis
                X = Subtract(Double(Lx1), X);
            }
            else if (IsDefault(v1y))
            {
                // Over X-Axis
                Y = Subtract(Double(Ly1), Y);
            }
            else
            {
                double PQx = Cast<T, double>(v1x),
                       PQy = Cast<T, double>(v1y),
                       PAx = Cast<T, double>(Subtract(X, Lx1)),
                       PAy = Cast<T, double>(Subtract(Y, Ly1)),
                       k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                X = Add(Lx1, Cast<double, T>(PQx * k - PAx));
                Y = Add(Ly1, Cast<double, T>(PQy * k - PAy));
            }
#endif
        }

        /// <summary>
        /// Creates a new casted point.
        /// </summary>
        /// <returns></returns>
        public Point<U> Cast<U>()
#if NET7_0_OR_GREATER
        where U : INumber<U>
#else
        where U : unmanaged
#endif
        {
#if NET7_0_OR_GREATER
            return new(U.CreateChecked(X), U.CreateChecked(Y));
#else
            return new(Cast<T, U>(X), Cast<T, U>(Y));
#endif
        }
        ICoordinateObject<U> ICoordinateObject<T>.Cast<U>()
            => Cast<U>();

        /// <summary>
        /// Creates a new point that is a copy of the current instance.
        /// </summary>
        public Point<T> Clone()
            => new(X, Y);
        ICoordinateObject<T> ICoordinateObject<T>.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => X.GetHashCode() ^ Y.GetHashCode();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified point
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Point<T> obj)
        {
#if NET7_0_OR_GREATER
            return X == obj.X && Y == obj.Y;
#else
            return OperatorHelper.Equals(X, obj.X) && OperatorHelper.Equals(Y, obj.Y);
#endif
        }

        bool ICoordinateObject<T>.Equals(ICoordinateObject<T> obj)
            => obj is Point<T> Target && Equals(Target);
        public override bool Equals(object obj)
            => obj is Point<T> Target && Equals(Target);

        public override string ToString()
            => $"X : {X}, Y : {Y}";

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
#if NET7_0_OR_GREATER
            T Dx = Qx - Px,
              Dy = Qy - Py;
            return Math.Sqrt(double.CreateChecked(Dx * Dx + Dy * Dy));
#else
            T Dx = Subtract(Qx, Px),
              Dy = Subtract(Qy, Py);
            return Math.Sqrt(Cast<T, double>(Add(Multiply(Dx, Dx), Multiply(Dy, Dy))));
#endif
        }

        /// <summary>
        /// Offsets the specified point by the specified vector.
        /// </summary>
        /// <param name="Point">The point to be offsetted.</param>
        /// <param name="Vector">The vector to be added to this point.</param>
        public static Point<T> Offset(Point<T> Point, Vector<T> Vector)
            => Offset(Point, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified point by the specified amounts.
        /// </summary>
        /// <param name="Point">The point to be offsetted.</param>
        /// <param name="Dx">The amount to offset <see cref="X"/> coordinate.</param>
        /// <param name="Dy">The amount to offset <see cref="Y"/> coordinate.</param>
        public static Point<T> Offset(Point<T> Point, T Dx, T Dy)
        {
#if NET7_0_OR_GREATER
            return new(Point.X + Dx, Point.Y + Dy);
#else
            return new(Add(Point.X, Dx), Add(Point.Y, Dy));
#endif
        }

        /// <summary>
        /// Offsets the specified points by the specified amounts.
        /// </summary>
        /// <param name="Points">The points to be offsetted.</param>
        /// <param name="Dx">The amount to offset <see cref="X"/> coordinate.</param>
        /// <param name="Dy">The amount to offset <see cref="Y"/> coordinate.</param>
        public static Point<T>[] Offset(Point<T>[] Points, T Dx, T Dy)
        {
            int Length = Points.Length;
            Point<T>[] Result = new Point<T>[Length];
            Point<T> Point;
            for (int i = 0; i < Length; i++)
            {
                Point = Points[i];
#if NET7_0_OR_GREATER
                Result[i] = new Point<T>(Point.X + Dx, Point.Y + Dy);
#else
                Result[i] = new Point<T>(Add(Point.X, Dx), Add(Point.Y, Dy));
#endif
            }
            return Result;
        }
        /// <summary>
        /// Offsets the specified points by the specified amounts.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be offsetted.</param>
        /// <param name="Length">The length of the points to be offsetted.</param>
        /// <param name="Dx">The amount to offset <see cref="X"/> coordinate.</param>
        /// <param name="Dy">The amount to offset <see cref="Y"/> coordinate.</param>
        public static void Offset(Point<T>* pPoints, int Length, T Dx, T Dy)
        {
            for (int i = 0; i < Length; i++)
            {
#if NET7_0_OR_GREATER
                pPoints->X = pPoints->X + Dx;
                pPoints->Y = pPoints->Y + Dy;
#else
                pPoints->X = Add(pPoints->X, Dx);
                pPoints->Y = Add(pPoints->Y, Dy);
#endif
            }
        }

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
#if NET7_0_OR_GREATER
            Rotate(Px - Cx, Py - Cy, Sin, Cos, out Qx, out Qy);

            Qx += Cx;
            Qy += Cy;
#else
            Rotate(Subtract(Px, Cx), Subtract(Py, Cy), Sin, Cos, out Qx, out Qy);

            Qx = Add(Qx, Cx);
            Qy = Add(Qy, Cy);
#endif
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
#if NET7_0_OR_GREATER
            Qx = T.CreateChecked(double.CreateChecked(Px) * Cos - double.CreateChecked(Py) * Sin);
            Qy = T.CreateChecked(double.CreateChecked(Px) * Sin + double.CreateChecked(Py) * Cos);
#else
            Qx = Cast<double, T>(Cast<T, double>(Px) * Cos - Cast<T, double>(Py) * Sin);
            Qy = Cast<double, T>(Cast<T, double>(Px) * Sin + Cast<T, double>(Py) * Cos);
#endif
        }
        /// <summary>
        /// Rotates the specified points about the origin.
        /// </summary>
        /// <param name="Points">The points to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Point<T>[] Rotate(Point<T>[] Points, double Theta)
        {
            int Length = Points.Length;
            Point<T>[] Result = new Point<T>[Length];
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            T Px, Py;
            Point<T> Point;
            for (int i = 0; i < Length; i++)
            {
                Point = Points[i];
                Px = Point.X;
                Py = Point.Y;

#if NET7_0_OR_GREATER
                Result[i] = new Point<T>(T.CreateChecked(double.CreateChecked(Px) * Cos - double.CreateChecked(Py) * Sin),
                                         T.CreateChecked(double.CreateChecked(Px) * Sin + double.CreateChecked(Py) * Cos));
#else
                Result[i] = new Point<T>(Cast<double, T>(Cast<T, double>(Px) * Cos - Cast<T, double>(Py) * Sin),
                                         Cast<double, T>(Cast<T, double>(Px) * Sin + Cast<T, double>(Py) * Cos));
#endif
            }

            return Result;
        }
        /// <summary>
        /// Rotates the specified points about the origin.
        /// </summary>
        /// <param name="Points">The points to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Point<T>[] Rotate(Point<T>[] Points, T Cx, T Cy, double Theta)
        {
            int Length = Points.Length;
            Point<T>[] Result = new Point<T>[Length];
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            T Px, Py;
            Point<T> Point;
            for (int i = 0; i < Length; i++)
            {
                Point = Points[i];

#if NET7_0_OR_GREATER
                Px = Point.X - Cx;
                Py = Point.Y - Cy;
                Result[i] = new Point<T>(T.CreateChecked(double.CreateChecked(Px) * Cos - double.CreateChecked(Py) * Sin) + Cx,
                                         T.CreateChecked(double.CreateChecked(Px) * Sin + double.CreateChecked(Py) * Cos) + Cy);
#else
                Px = Subtract(Point.X, Cx);
                Py = Subtract(Point.Y, Cy);
                Result[i] = new Point<T>(Add(Cast<double, T>(Cast<T, double>(Px) * Cos - Cast<T, double>(Py) * Sin), Cx),
                                         Add(Cast<double, T>(Cast<T, double>(Px) * Sin + Cast<T, double>(Py) * Cos), Cy));
#endif
            }

            return Result;
        }
        /// <summary>
        /// Rotates the specified points about the origin.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be rotated.</param>
        /// <param name="Length">The length of the points to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static void Rotate(Point<T>* pPoints, int Length, double Theta)
        {
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);
            Rotate(pPoints, Length, Sin, Cos);
        }
        /// <summary>
        /// Rotates the specified points about the origin.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be rotated.</param>
        /// <param name="Length">The length of the points to be rotated.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public static void Rotate(Point<T>* pPoints, int Length, double Sin, double Cos)
        {
            T Px, Py;
            for (int i = 0; i < Length; i++)
            {
                Px = pPoints->X;
                Py = pPoints->Y;

#if NET7_0_OR_GREATER
                pPoints->X = T.CreateChecked(double.CreateChecked(Px) * Cos - double.CreateChecked(Py) * Sin);
                pPoints->Y = T.CreateChecked(double.CreateChecked(Px) * Sin + double.CreateChecked(Py) * Cos);
#else
                pPoints->X = Cast<double, T>(Cast<T, double>(Px) * Cos - Cast<T, double>(Py) * Sin);
                pPoints->Y = Cast<double, T>(Cast<T, double>(Px) * Sin + Cast<T, double>(Py) * Cos);
#endif
            }
        }
        /// <summary>
        /// Rotates the specified points about the specified point.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be rotated.</param>
        /// <param name="Length">The length of the points to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static void Rotate(Point<T>* pPoints, int Length, T Cx, T Cy, double Theta)
        {
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Rotate(pPoints, Length, Cx, Cy, Sin, Cos);
        }
        /// <summary>
        /// Rotates the specified points about the specified point.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be rotated.</param>
        /// <param name="Length">The length of the points to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Sin">The sine of the specified angle to rotate.</param>
        /// <param name="Cos">The cosine of the specified angle to rotate.</param>
        public static void Rotate(Point<T>* pPoints, int Length, T Cx, T Cy, double Sin, double Cos)
        {
            T Px, Py;
            for (int i = 0; i < Length; i++)
            {
#if NET7_0_OR_GREATER
                Px = pPoints->X - Cx;
                Py = pPoints->Y - Cy;

                pPoints->X = T.CreateChecked(double.CreateChecked(Px) * Cos - double.CreateChecked(Py) * Sin) + Cx;
                pPoints->Y = T.CreateChecked(double.CreateChecked(Px) * Sin + double.CreateChecked(Py) * Cos) + Cy;
#else
                Px = Subtract(pPoints->X, Cx);
                Py = Subtract(pPoints->Y, Cy);

                pPoints->X = Add(Cast<double, T>(Cast<T, double>(Px) * Cos - Cast<T, double>(Py) * Sin), Cx);
                pPoints->Y = Add(Cast<double, T>(Cast<T, double>(Px) * Sin + Cast<T, double>(Py) * Cos), Cy);
#endif
            }
        }

        /// <summary>
        /// Reflects the specified point over the specified line.
        /// </summary>
        /// <param name="Point">The point to be reflect.</param>
        /// <param name="Line">The projection line.</param>
        public static Point<T> Reflect(Point<T> Point, Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return Point.Clone();

            Point<T> P1 = Line.Points[0],
                     P2 = Line.Points[1];

            return Reflect(Point.X, Point.Y, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified point over the specified line.
        /// </summary>
        /// <param name="Point">The point to be reflect.</param>
        /// <param name="LinePoint1">The point on the projection line.</param>
        /// <param name="LinePoint2">The another point on the projection line.</param>
        public static Point<T> Reflect(Point<T> Point, Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(Point.X, Point.Y, LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        /// <summary>
        /// Reflects the specified point over the specified line.
        /// </summary>
        /// <param name="Point">The point to be reflect.</param>
        /// <param name="Lx1">The x-coordinate of a point on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the projection line.</param>
        public static Point<T> Reflect(Point<T> Point, T Lx1, T Ly1, T Lx2, T Ly2)
            => Reflect(Point.X, Point.Y, Lx1, Ly1, Lx2, Ly2);
        /// <summary>
        /// Reflects the specified point over the specified line.
        /// </summary>
        /// <param name="Px">The x-coordinate of point to be reflect.</param>
        /// <param name="Py">The y-coordinate of point to be reflect.</param>
        /// <param name="Lx1">The x-coordinate of a point on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the projection line.</param>
        public static Point<T> Reflect(T Px, T Py, T Lx1, T Ly1, T Lx2, T Ly2)
        {
#if NET7_0_OR_GREATER
            T v1x = Lx2 - Lx1,
              v1y = Ly2 - Ly1;

            if (T.IsZero(v1x))
            {
                if (T.IsZero(v1y))
                    return new Point<T>(Px, Py);

                // Over Y-Axis
                return new Point<T>(Lx1 * T.CreateChecked(2) - Px, Py);
            }
            else if (T.IsZero(v1y))
            {
                // Over X-Axis
                return new Point<T>(Px, Ly1 * T.CreateChecked(2) * Py);
            }
            else
            {
                double PQx = double.CreateChecked(v1x),
                       PQy = double.CreateChecked(v1y),
                       PAx = double.CreateChecked(Px - Lx1),
                       PAy = double.CreateChecked(Py - Ly1),
                       k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                return new Point<T>(Lx1 + T.CreateChecked(PQx * k - PAx),
                                    Ly1 + T.CreateChecked(PQy * k - PAy));
#else
            T v1x = Subtract(Lx2, Lx1),
              v1y = Subtract(Ly2, Ly1);

            if (IsDefault(v1x))
            {
                if (IsDefault(v1y))
                    return new Point<T>(Px, Py);

                // Over Y-Axis
                return new Point<T>(Subtract(Double(Lx1), Px), Py);
            }
            else if (IsDefault(v1y))
            {
                // Over X-Axis
                return new Point<T>(Px, Subtract(Double(Ly1), Py));
            }
            else
            {
                double PQx = Cast<T, double>(v1x),
                       PQy = Cast<T, double>(v1y),
                       PAx = Cast<T, double>(Subtract(Px, Lx1)),
                       PAy = Cast<T, double>(Subtract(Py, Ly1)),
                       k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                return new Point<T>(Add(Lx1, Cast<double, T>(PQx * k - PAx)),
                                    Add(Ly1, Cast<double, T>(PQy * k - PAy)));
#endif
            }
        }
        /// <summary>
        /// Reflects the specified points over the specified line.
        /// </summary>
        /// <param name="Points">The points to be reflect.</param>
        /// <param name="Lx1">The x-coordinate of a point on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the projection line.</param>
        public static Point<T>[] Reflect(Point<T>[] Points, T Lx1, T Ly1, T Lx2, T Ly2)
        {
#if NET7_0_OR_GREATER
            int Length = Points.Length;
            Point<T>[] Result = new Point<T>[Length];
            T v1x = Lx2 - Lx1,
              v1y = Ly2 - Ly1;

            if (T.IsZero(v1x))
            {
                if (T.IsZero(v1y))
                {
                    for (int i = 0; i < Length; i++)
                        Result[i] = Points[i];

                    return Result;
                }

                // Over Y-Axis
                Point<T> Point;
                T Two = T.CreateChecked(2);
                for (int i = 0; i < Length; i++)
                {
                    Point = Points[i];
                    Result[i] = new Point<T>(Lx1 * Two, Point.X - Point.Y);
                }

                return Result;
            }
            else if (T.IsZero(v1y))
            {
                // Over X-Axis
                Point<T> Point;
                T Two = T.CreateChecked(2);
                for (int i = 0; i < Length; i++)
                {
                    Point = Points[i];
                    Result[i] = new Point<T>(Point.X, Ly1 * Two - Point.Y);
                }

                return Result;
            }
            else
            {
                Point<T> Point;
                double PQx = double.CreateChecked(v1x),
                       PQy = double.CreateChecked(v1y);

                for (int i = 0; i < Length; i++)
                {
                    Point = Points[i];
                    double PAx = double.CreateChecked(Point.X - Lx1),
                           PAy = double.CreateChecked(Point.Y - Ly1),
                           k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                    Result[i] = new Point<T>(Lx1 + T.CreateChecked(PQx * k - PAx),
                                             Ly1 + T.CreateChecked(PQy * k - PAy));
                }
            }

            return Result;
#else
            int Length = Points.Length;
            Point<T>[] Result = new Point<T>[Length];
            T v1x = Subtract(Lx2, Lx1),
              v1y = Subtract(Ly2, Ly1);

            if (IsDefault(v1x))
            {
                if (IsDefault(v1y))
                {
                    for (int i = 0; i < Length; i++)
                        Result[i] = Points[i];

                    return Result;
                }

                // Over Y-Axis
                Point<T> Point;
                for (int i = 0; i < Length; i++)
                {
                    Point = Points[i];
                    Result[i] = new Point<T>(Subtract(Double(Lx1), Point.X), Point.Y);
                }

                return Result;
            }
            else if (IsDefault(v1y))
            {
                // Over X-Axis
                Point<T> Point;
                for (int i = 0; i < Length; i++)
                {
                    Point = Points[i];
                    Result[i] = new Point<T>(Point.X, Subtract(Double(Ly1), Point.Y));
                }

                return Result;
            }
            else
            {
                Point<T> Point;
                double PQx = Cast<T, double>(v1x),
                       PQy = Cast<T, double>(v1y);

                for (int i = 0; i < Length; i++)
                {
                    Point = Points[i];
                    double PAx = Cast<T, double>(Subtract(Point.X, Lx1)),
                           PAy = Cast<T, double>(Subtract(Point.Y, Ly1)),
                           k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                    Result[i] = new Point<T>(Add(Lx1, Cast<double, T>(PQx * k - PAx)), 
                                             Add(Ly1, Cast<double, T>(PQy * k - PAy)));
                }
            }

            return Result;
#endif
        }
        /// <summary>
        /// Reflects the specified points over the specified line.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be reflect.</param>
        /// <param name="Length">The length of the points to be reflect.</param>
        /// <param name="Lx1">The x-coordinate of a point on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the projection line.</param>
        public static void Reflect(Point<T>* pPoints, int Length, T Lx1, T Ly1, T Lx2, T Ly2)
        {
#if NET7_0_OR_GREATER
            T v1x = Lx2 - Lx1,
              v1y = Ly2 - Ly1;

            if (T.IsZero(v1x))
            {
                if (T.IsZero(v1y))
                    return;

                // Over Y-Axis
                T Two = T.CreateChecked(2);
                for (int i = 0; i < Length; i++)
                {
                    pPoints->X = Lx1 * Two - pPoints->X;
                    pPoints++;
                }

                return;
            }
            else if (T.IsZero(v1y))
            {
                // Over X-Axis
                T Two = T.CreateChecked(2);
                for (int i = 0; i < Length; i++)
                {
                    pPoints->Y = Ly1 * Two - pPoints->Y;
                    pPoints++;
                }

                return;
            }
            else
            {
                double PQx = double.CreateChecked(v1x),
                       PQy = double.CreateChecked(v1y);

                for (int i = 0; i < Length; i++)
                {
                    double PAx = double.CreateChecked(pPoints->X - Lx1),
                           PAy = double.CreateChecked(pPoints->Y - Ly1),
                           k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                    pPoints->X = Lx1 + T.CreateChecked(PQx * k - PAx);
                    pPoints->Y = Ly1 + T.CreateChecked(PQy * k - PAy);
                    pPoints++;
                }
            }
#else
            T v1x = Subtract(Lx2, Lx1),
              v1y = Subtract(Ly2, Ly1);

            if (IsDefault(v1x))
            {
                if (IsDefault(v1y))
                    return;

                // Over Y-Axis
                for (int i = 0; i < Length; i++)
                {
                    pPoints->X = Subtract(Double(Lx1), pPoints->X);
                    pPoints++;
                }

                return;
            }
            else if (IsDefault(v1y))
            {
                // Over X-Axis
                for (int i = 0; i < Length; i++)
                {
                    pPoints->Y = Subtract(Double(Ly1), pPoints->Y);
                    pPoints++;
                }

                return;
            }
            else
            {
                double PQx = Cast<T, double>(v1x),
                       PQy = Cast<T, double>(v1y);

                for (int i = 0; i < Length; i++)
                {
                    double PAx = Cast<T, double>(Subtract(pPoints->X, Lx1)),
                           PAy = Cast<T, double>(Subtract(pPoints->Y, Ly1)),
                           k = Vector<double>.Dot(PQx, PQy, PAx, PAy) / (PQx * PQx + PQy * PQy) * 2d;

                    pPoints->X = Add(Lx1, Cast<double, T>(PQx * k - PAx));
                    pPoints->Y = Add(Ly1, Cast<double, T>(PQy * k - PAy));
                    pPoints++;
                }
            }
#endif
        }

#if NET7_0_OR_GREATER
#else
#endif
        /// <summary>
        /// Sorts the specified points around the specified point.
        /// </summary>
        /// <param name="Points">The points to be sorted.</param>
        public static Point<T>[] Sort(params Point<T>[] Points)
        {
            Point<T> p = Points[0];
            T Cx = p.X,
              Cy = p.Y;

#if NET7_0_OR_GREATER
            int Length = Points.Length;
            for (int i = 1; i < Length; i++)
            {
                p = Points[i];
                Cx += p.X;
                Cy += p.Y;
            }

            return Sort(Points, T.CreateChecked(double.CreateChecked(Cx) / Length), T.CreateChecked(double.CreateChecked(Cy) / Length));
#else
            int Length = Points.Length;
            for (int i = 1; i < Length; i++)
            {
                p = Points[i];
                Cx = Add(Cx, p.X);
                Cy = Add(Cy, p.Y);
            }

            return Sort(Points, Cast<double, T>(Cast<T, double>(Cx) / Length), Cast<double, T>(Cast<T, double>(Cy) / Length));
#endif
        }
        /// <summary>
        /// Sorts the specified points around the specified point.
        /// </summary>
        /// <param name="Points">The points to be sorted.</param>
        /// <param name="Cx">The x-coordinate of the center of the points.</param>
        /// <param name="Cy">The y-coordinate of the center of the points.</param>
        public static Point<T>[] Sort(Point<T>[] Points, T Cx, T Cy)
        {
#if NET7_0_OR_GREATER
            bool PointCmp(Point<T> P1, Point<T> P2)
            {
                T Px1 = P1.X,
                  Px2 = P2.X;

                if (T.Zero <= Px1 && Px2 < T.Zero)
                    return true;

                T Py1 = P1.Y,
                  Py2 = P2.Y;
                if (T.IsZero(Px1) && T.IsZero(Px2))
                    return Py1 > Py2;

                T v1x = Px1 - Cx,
                  v1y = Py1 - Cy,
                  v2x = Px2 - Cx,
                  v2y = Py2 - Cy,
                  D = Vector<T>.Cross(v1x, v1y, v2x, v2y);

                return T.IsZero(D) ? Vector<T>.Dot(v1x, v1y, v1x, v1y) > Vector<T>.Dot(v2x, v2y, v2x, v2y) :
                                     D < T.Zero;
            }
#else
            T Zero = default;
            bool PointCmp(Point<T> P1, Point<T> P2)
            {
                T Px1 = P1.X,
                  Px2 = P2.X;

                if (!GreaterThan(Zero, Px1) && GreaterThan(Zero, Px2))
                    return true;

                T Py1 = P1.Y,
                  Py2 = P2.Y;
                if (IsDefault(Px1) && IsDefault(Px2))
                    return GreaterThan(Py1, Py2);

                T v1x = Subtract(Px1, Cx),
                  v1y = Subtract(Py1, Cy),
                  v2x = Subtract(Px2, Cx),
                  v2y = Subtract(Py2, Cy),
                  D = Vector<T>.Cross(v1x, v1y, v2x, v2y);

                return IsDefault(D) ? GreaterThan(Vector<T>.Dot(v1x, v1y, v1x, v1y), Vector<T>.Dot(v2x, v2y, v2x, v2y)) :
                                      GreaterThan(Zero, D);
            }
#endif

            int Length = Points.Length,
                Ti;
            Point<T>[] Sorted = Points.ToArray();
            Point<T> p;
            for (int i = 0; i < Length - 1; i++)
            {
                for (int j = 0; j < Length - i - 1; j++)
                {
                    Ti = j + 1;
                    if (PointCmp(Sorted[j], Sorted[Ti]))
                    {
                        p = Sorted[j];
                        Sorted[j] = Sorted[j + 1];
                        Sorted[j + 1] = p;
                    }
                }
            }

            return Sorted;
        }
        /// <summary>
        /// Sorts the specified points around the specified point.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be sorted.</param>
        /// <param name="Length">The length of the points to be sorted.</param>
        public static void Sort(Point<T>* pPoints, int Length)
        {
            Point<T>* pTemp = pPoints;
            Point<T> p = *pTemp++;
            T Cx = p.X,
              Cy = p.Y;

#if NET7_0_OR_GREATER
            for (int i = 1; i < Length; i++)
            {
                p = *pTemp++;
                Cx += p.X;
                Cy += p.Y;
            }

            Sort(pPoints, Length, T.CreateChecked(double.CreateChecked(Cx) / Length), T.CreateChecked(double.CreateChecked(Cy) / Length));
#else
            for (int i = 1; i < Length; i++)
            {
                p = *pTemp++;
                Cx = Add(Cx, p.X);
                Cy = Add(Cy, p.Y);
            }

            Sort(pPoints, Length, Cast<double, T>(Cast<T, double>(Cx) / Length), Cast<double, T>(Cast<T, double>(Cy) / Length));
#endif
        }
        /// <summary>
        /// Sorts the specified points around the specified point.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be sorted.</param>
        /// <param name="Length">The length of the points to be sorted.</param>
        /// <param name="Cx">The x-coordinate of the center of the points.</param>
        /// <param name="Cy">The y-coordinate of the center of the points.</param>
        public static void Sort(Point<T>* pPoints, int Length, T Cx, T Cy)
        {
#if NET7_0_OR_GREATER
            bool PointCmp(Point<T>* P1, Point<T>* P2)
            {
                T Px1 = P1->X,
                  Px2 = P2->X;
                if (T.Zero <= Px1 && Px2 < T.Zero)
                    return true;

                T Py1 = P1->Y,
                  Py2 = P2->Y;
                if (T.IsZero(Px1) && T.IsZero(Px2))
                    return Py1 > Py2;

                T v1x = Px1 - Cx,
                  v1y = Py1 - Cy,
                  v2x = Px2 - Cx,
                  v2y = Py2 - Cy,
                  D = Vector<T>.Cross(v1x, v1y, v2x, v2y);

                return T.IsZero(D) ? Vector<T>.Dot(v1x, v1y, v1x, v1y) > Vector<T>.Dot(v2x, v2y, v2x, v2y) :
                                     D < T.Zero;
            }
#else
            T Zero = default;
            bool PointCmp(Point<T>* P1, Point<T>* P2)
            {
                T Px1 = P1->X,
                  Px2 = P2->X;

                if (!GreaterThan(Zero, Px1) && GreaterThan(Zero, Px2))
                    return true;

                T Py1 = P1->Y,
                  Py2 = P2->Y;
                if (IsDefault(Px1) && IsDefault(Px2))
                    return GreaterThan(Py1, Py2);

                T v1x = Subtract(Px1, Cx),
                  v1y = Subtract(Py1, Cy),
                  v2x = Subtract(Px2, Cx),
                  v2y = Subtract(Py2, Cy),
                  D = Vector<T>.Cross(v1x, v1y, v2x, v2y);

                return IsDefault(D) ? GreaterThan(Vector<T>.Dot(v1x, v1y, v1x, v1y), Vector<T>.Dot(v2x, v2y, v2x, v2y)) :
                                      GreaterThan(Zero, D);
            }
#endif

            Point<T>* pNextPoint = pPoints + 1;
            Point<T> p;
            for (int i = 0; i < Length - 1; i++)
            {
                for (int j = 0; j < Length - i - 1; j++, pPoints++, pNextPoint++)
                {
                    if (PointCmp(pPoints, pNextPoint))
                    {
                        p = *pPoints;
                        *pPoints = *pNextPoint;
                        *pNextPoint = p;
                    }
                }
            }
        }

        /// <summary>
        /// Negates this Point. The Point has the same magnitude as before, but its quadrant is now opposite.
        /// </summary>
        /// <param name="Point">The Point to negates.</param>
        public static Point<T> Negates(Point<T> Point)
        {
#if NET7_0_OR_GREATER
            return new(-Point.X, -Point.Y);
#else
            return new(Negate(Point.X), Negate(Point.Y));
#endif
        }

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
            => new(Point2, Point1);

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