using System;
#if NET7_0_OR_GREATER
using System.Numerics;
#else
using static MenthaAssembly.OperatorHelper;
#endif

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a circle in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct Circle<T> : IShape<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : unmanaged
#endif
    {
        /// <summary>
        /// Gets a special value that represents a circle with no position or area.
        /// </summary>
        public static Circle<T> Empty => new();

        public bool IsEmpty
            => Radius is 0d || double.IsNaN(Radius) || double.IsInfinity(Radius);

        public Point<T> Center { set; get; }

        public double Radius { set; get; }

        public double Area
            => Radius * Radius * Math.PI;

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new circle must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new circle must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new circle must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new circle must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new circle must contain.</param>
        public Circle(Point<T> Point1, T Px2, T Py2, T Px3, T Py3) : this(Point1.X, Point1.Y, Px2, Py2, Px3, Py3)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Circle{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new circle must contain.</param>
        /// <param name="Point2">The second point that the new circle must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new circle must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new circle must contain.</param>
        public Circle(Point<T> Point1, Point<T> Point2, T Px3, T Py3) : this(Point1.X, Point1.Y, Point2.X, Point2.Y, Px3, Py3)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Circle{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new circle must contain.</param>
        /// <param name="Point2">The second point that the new circle must contain.</param>
        /// <param name="Point3">The third point that the new circle must contain.</param>
        public Circle(Point<T> Point1, Point<T> Point2, Point<T> Point3) : this(Point1.X, Point1.Y, Point2.X, Point2.Y, Point3.X, Point3.Y)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Circle{T}"/> structure.
        /// </summary>
        /// <param name="Px1">The x-coordinate of the first point that the new circle must contain.</param>
        /// <param name="Py1">The y-coordinate of the first point that the new circle must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new circle must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new circle must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new circle must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new circle must contain.</param>
        public Circle(T Px1, T Py1, T Px2, T Py2, T Px3, T Py3)
        {
            if (Line<T>.IsCollinear(Px1, Py1, Px2, Py2, Px3, Py3))
            {
                this = Empty;
                return;
            }

            CalculateCenter(Px1, Py1, Px2, Py2, Px3, Py3, out T Cx, out T Cy);

            Center = new Point<T>(Cx, Cy);
            Radius = Point<T>.Distance(Px1, Py1, Cx, Cy);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Circle{T}"/> structure.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center point of the new circle..</param>
        /// <param name="Cy">The y-coordinate of the center point of the new circle..</param>
        /// <param name="Radius">The radius of the new circle.</param>
        public Circle(T Cx, T Cy, double Radius)
        {
            Center = new Point<T>(Cx, Cy);
            this.Radius = Radius;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Circle{T}"/> structure.
        /// </summary>
        /// <param name="Center">The center point of the new circle.</param>
        /// <param name="Radius">The radius of the new circle.</param>
        public Circle(Point<T> Center, double Radius)
        {
            this.Center = Center;
            this.Radius = Radius;
        }

        public bool Contain(Point<T> Point)
            => Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
        {
            if (IsEmpty)
                return false;

            Point<T> C = Center;
#if NET7_0_OR_GREATER
            T Dx = C.X - Px,
              Dy = C.Y - Py;

            return double.CreateChecked(Dx * Dx + Dy * Dy) <= Radius * Radius;
#else
            T Dx = Subtract(C.X, Px),
              Dy = Subtract(C.Y, Py);

            return Cast<T, double>(Add(Multiply(Dx, Dx), Multiply(Dy, Dy))) <= Radius * Radius;
#endif
        }

        public void Offset(Vector<T> Vector)
            => Offset(Vector.X, Vector.Y);
        public void Offset(T Dx, T Dy)
        {
            if (IsEmpty)
                return;

            Center.Offset(Dx, Dy);
        }

        public void Scale(T Scale)
        {
#if NET7_0_OR_GREATER
            Radius *= double.CreateChecked(Scale);
#else
            Radius *= Cast<T, double>(Scale);
#endif
        }
        void IShape<T>.Scale(T ScaleX, T ScaleY)
            => throw new NotSupportedException();
        void IShape<T>.Scale(Point<T> Center, T Scale)
            => throw new NotSupportedException();
        void IShape<T>.Scale(Point<T> Center, T ScaleX, T ScaleY)
            => throw new NotSupportedException();
        void IShape<T>.Scale(T Cx, T Cy, T Scale)
            => throw new NotSupportedException();
        void IShape<T>.Scale(T Cx, T Cy, T ScaleX, T ScaleY)
            => throw new NotSupportedException();

        public void Rotate(double Theta)
        {
            if (IsEmpty)
                return;

            Center.Rotate(Theta);
        }
        public void Rotate(Point<T> Center, double Theta)
            => Rotate(Center.X, Center.Y, Theta);
        public void Rotate(T Cx, T Cy, double Theta)
        {
            if (IsEmpty)
                return;

            Center.Rotate(Cx, Cy, Theta);
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
            if (IsEmpty)
                return;

            Center.Reflect(Lx1, Ly1, Lx2, Ly2);
        }

        /// <summary>
        /// Creates a new casted <see cref="Circle{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Circle<U> Cast<U>()
#if NET7_0_OR_GREATER
        where U : INumber<U>
#else
        where U : unmanaged
#endif
        {
            return IsEmpty ? Circle<U>.Empty : new Circle<U>(Center.Cast<U>(), Radius);
        }
        IShape<U> IShape<T>.Cast<U>()
            => Cast<U>();
        ICoordinateObject<U> ICoordinateObject<T>.Cast<U>()
            => Cast<U>();

        /// <summary>
        /// Creates a new <see cref="Circle{T}"/> that is a copy of the current instance.
        /// </summary>
        public Circle<T> Clone()
            => IsEmpty ? Empty : new Circle<T>(Center, Radius);
        IShape<T> IShape<T>.Clone()
            => Clone();
        ICoordinateObject<T> ICoordinateObject<T>.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => Center.GetHashCode() ^ Radius.GetHashCode();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Circle{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Circle<T> obj)
        {
            if (IsEmpty)
                return obj.IsEmpty;

            if (obj.IsEmpty)
                return false;

            return Center.Equals(obj.Center) && Radius.Equals(obj.Radius);
        }
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is Circle<T> Tri && Equals(Tri);
        bool ICoordinateObject<T>.Equals(ICoordinateObject<T> obj)
            => obj is Circle<T> Tri && Equals(Tri);
        public override bool Equals(object obj)
            => obj is Circle<T> Tri && Equals(Tri);

        public override string ToString()
            => IsEmpty ? $"{nameof(Circle<T>)}<{typeof(T).Name}>.Empty" :
                              $"Center : {Center}, Radius : {Radius}";

        public static void CalculateCenter(T Px1, T Py1, T Px2, T Py2, T Px3, T Py3, out T Cx, out T Cy)
        {
#if NET7_0_OR_GREATER
            T Lp = Px1 * Px1 + Py1 * Py1,
              Lq = Px2 * Px2 + Py2 * Py2,
              Lr = Px3 * Px3 + Py3 * Py3,
              Xrq = Px2 - Px3,
              Yrq = Py2 - Py3,
              Xqp = Px1 - Px2,
              Yqp = Py1 - Py2,
              Xpr = Px3 - Px1,
              Ypr = Py3 - Py1;

            Cx = T.CreateChecked(double.CreateChecked(Lp * Yrq + Lq * Ypr + Lr * Yqp) / (2d * double.CreateChecked(Px1 * Yrq + Px2 * Ypr + Px3 * Yqp)));
            Cy = T.CreateChecked(double.CreateChecked(Lp * Xrq + Lq * Xpr + Lr * Xqp) / (2d * double.CreateChecked(Py1 * Xrq + Py2 * Xpr + Py3 * Xqp)));
#else
            T Lp = Add(Multiply(Px1, Px1), Multiply(Py1, Py1)),
              Lq = Add(Multiply(Px2, Px2), Multiply(Py2, Py2)),
              Lr = Add(Multiply(Px3, Px3), Multiply(Py3, Py3)),
              Xrq = Subtract(Px2, Px3),
              Yrq = Subtract(Py2, Py3),
              Xqp = Subtract(Px1, Px2),
              Yqp = Subtract(Py1, Py2),
              Xpr = Subtract(Px3, Px1),
              Ypr = Subtract(Py3, Py1);

            Cx = Cast<double, T>(Cast<T, double>(Add(Add(Multiply(Lp, Yrq), Multiply(Lq, Ypr)), Multiply(Lr, Yqp))) / (2d * Cast<T, double>(Add(Add(Multiply(Px1, Yrq), Multiply(Px2, Ypr)), Multiply(Px3, Yqp)))));
            Cy = Cast<double, T>(Cast<T, double>(Add(Add(Multiply(Lp, Xrq), Multiply(Lq, Xpr)), Multiply(Lr, Xqp))) / (2d * Cast<T, double>(Add(Add(Multiply(Py1, Xrq), Multiply(Py2, Xpr)), Multiply(Py3, Xqp)))));
#endif
        }

        /// <summary>
        /// Offsets the specified circle's coordinates by the specified vector.
        /// </summary>
        /// <param name="Circle">The circle to be offsetted.</param>
        /// <param name="Vector">The vector to be added to the specified circle.</param>
        public static Circle<T> Offset(Circle<T> Circle, Vector<T> Vector)
            => Offset(Circle, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified circle's coordinates by the specified amounts.
        /// </summary>
        /// <param name="Circle">The circle to be offsetted.</param>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public static Circle<T> Offset(Circle<T> Circle, T Dx, T Dy)
            => Circle.IsEmpty ? Empty : new Circle<T>(Point<T>.Offset(Circle.Center, Dx, Dy), Circle.Radius);

        /// <summary>
        /// Scales the specified circle around the origin.
        /// </summary>
        /// <param name="Circle">The circle to be scaled.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Circle<T> Scale(Circle<T> Circle, T Scale)
        {
#if NET7_0_OR_GREATER
            return Circle.IsEmpty ? Empty : new Circle<T>(Circle.Center, Circle.Radius * double.CreateChecked(Scale));
#else
            return Circle.IsEmpty ? Empty : new Circle<T>(Circle.Center, Circle.Radius * Cast<T, double>(Scale));
#endif
        }

        /// <summary>
        /// Rotates the specified circle about the origin.
        /// </summary>
        /// <param name="Circle">The circle to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Circle<T> Rotate(Circle<T> Circle, double Theta)
        {
            if (Circle.IsEmpty)
                return Empty;

            Point<T> C = Circle.Center;
            Point<T>.Rotate(C.X, C.Y, Theta, out T Qx, out T Qy);
            return Circle.IsEmpty ? Empty : new Circle<T>(Qx, Qy, Circle.Radius);
        }

        /// <summary>
        /// Rotates the specified circle about the specified point.
        /// </summary>
        /// <param name="Circle">The circle to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Circle<T> Rotate(Circle<T> Circle, Point<T> Center, double Theta)
            => Rotate(Circle, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified circle about the specified point.
        /// </summary>
        /// <param name="Circle">The circle to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Circle<T> Rotate(Circle<T> Circle, T Cx, T Cy, double Theta)
            => Circle.IsEmpty ? Empty : new Circle<T>(Point<T>.Rotate(Circle.Center, Cx, Cy, Theta), Circle.Radius);

        /// <summary>
        /// Reflects the specified circle over the specified line.
        /// </summary>
        /// <param name="Circle">The circle to be reflects.</param>
        /// <param name="Line">The projection line.</param>
        public static Circle<T> Reflect(Circle<T> Circle, Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return Circle.Clone();

            Point<T> P1 = Line.Points[0],
                     P2 = Line.Points[1];

            return Reflect(Circle, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified circle over the specified line.
        /// </summary>
        /// <param name="Circle">The circle to be reflects.</param>
        /// <param name="LinePoint1">The circle on the projection line.</param>
        /// <param name="LinePoint2">The another circle on the projection line.</param>
        public static Circle<T> Reflect(Circle<T> Circle, Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(Circle, LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        /// <summary>
        /// Reflects the specified circle over the specified line.
        /// </summary>
        /// <param name="Circle">The circle to be reflects.</param>
        /// <param name="Lx1">The x-coordinate of a circle on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a circle on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another circle on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another circle on the projection line.</param>
        public static Circle<T> Reflect(Circle<T> Circle, T Lx1, T Ly1, T Lx2, T Ly2)
            => Circle.IsEmpty ? Empty : new Circle<T>(Point<T>.Reflect(Circle.Center, Lx1, Ly1, Lx2, Ly2), Circle.Radius);

        /// <summary>
        /// Adds the specified vector to the specified circle.
        /// </summary>
        /// <param name="Circle">The circle structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static Circle<T> operator +(Circle<T> Circle, Vector<T> Vector)
            => Offset(Circle, Vector);
        /// <summary>
        /// Subtracts the specified vector from the specified circle.
        /// </summary>
        /// <param name="Circle">The circle from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from circle.</param>
        public static Circle<T> operator -(Circle<T> Circle, Vector<T> Vector)
            => Offset(Circle, -Vector);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="Circle1">The first circle to compare.</param>
        /// <param name="Circle2">The second circle to compare.</param>
        public static bool operator ==(Circle<T> Circle1, Circle<T> Circle2)
            => Circle1.Equals(Circle2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="Circle1">The first circle to compare.</param>
        /// <param name="Circle2">The second circle to compare.</param>
        public static bool operator !=(Circle<T> Circle1, Circle<T> Circle2)
            => !Circle1.Equals(Circle2);

    }
}