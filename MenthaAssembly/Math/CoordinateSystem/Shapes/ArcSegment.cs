using System;
using System.Collections.Generic;
using System.Linq;
using static MenthaAssembly.OperatorHelper;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a ArcSegment in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct ArcSegment<T> : IShape<T>
        where T : unmanaged
    {
        /// <summary>
        /// Gets a special value that represents a ArcSegment with no position.
        /// </summary>
        public static ArcSegment<T> Empty => new();

        internal Point<T>[] Points;

        /// <summary>
        /// The start point of this arc segment.
        /// </summary>
        public Point<T> Start
        {
            set
            {
                if (Points is null)
                    Points = new Point<T>[3];

                Points[0] = value;
                SortPoints(ref Points[0], ref Points[1], ref Points[2]);
            }
            get => Points?[0] ?? default;
        }

        /// <summary>
        /// The control point of this arc segment.
        /// </summary>
        public Point<T> Control
        {
            set
            {
                if (Points is null)
                    Points = new Point<T>[3];

                Points[1] = value;
                SortPoints(ref Points[0], ref Points[1], ref Points[2]);
            }
            get => Points?[1] ?? default;
        }

        /// <summary>
        /// The end point of this arc segment.
        /// </summary>
        public Point<T> End
        {
            set
            {
                if (Points is null)
                    Points = new Point<T>[3];

                Points[2] = value;
                SortPoints(ref Points[0], ref Points[1], ref Points[2]);
            }
            get => Points?[2] ?? default;
        }

        /// <summary>
        /// The equivalent circle center point of this arc segment.
        /// </summary>
        public Point<T> Center
        {
            get
            {
                Point<T> P1 = Points[0],
                         P2 = Points[1],
                         P3 = Points[2];

                Circle<T>.CalculateCenter(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, out T Cx, out T Cy);

                return new Point<T>(Cx, Cy);
            }
        }

        double IShape<T>.Area
            => 0d;

        /// <summary>
        /// Gets a value that indicates whether the shape is the empty ArcSegment.
        /// </summary>
        public bool IsEmpty
            => Points is null || Points.Length < 3 || Points[0].Equals(Points[1]);

        /// <summary>
        /// Initializes a new instance of the <see cref="ArcSegment{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new ArcSegment must contain.</param>
        /// <param name="Point2">The second point that the new ArcSegment must contain.</param>
        public ArcSegment(Point<T> Point1, Point<T> Point2, Point<T> Point3)
        {
            if (Line<T>.IsCollinear(Point1, Point2, Point3))
            {
                this = Empty;
                return;
            }

            SortPoints(ref Point1, ref Point2, ref Point3);
            Points = new[] { Point1, Point2, Point3 };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ArcSegment{T}"/> structure.
        /// </summary>        
        /// <param name="Px1">The x-coordinate of the first point that the new ArcSegment must contain.</param>
        /// <param name="Py1">The y-coordinate of the first point that the new ArcSegment must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new ArcSegment must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new ArcSegment must contain.</param>
        public ArcSegment(T Px1, T Py1, T Px2, T Py2, T Px3, T Py3)
        {
            if (Line<T>.IsCollinear(Px1, Py1, Px2, Py2, Px3, Py3))
            {
                this = Empty;
                return;
            }

            SortPoints(ref Px1, ref Py1, ref Px2, ref Py2, ref Px3, ref Py3);
            Points = new[] { new Point<T>(Px1, Py1), new Point<T>(Px2, Py2), new Point<T>(Px3, Py3) };
        }

        public bool Contain(Point<T> Point)
            => Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
            => Contain(this, Px, Py);

        public void Offset(Vector<T> Vector)
            => Offset(Vector.X, Vector.Y);
        public void Offset(T Dx, T Dy)
        {
            if (IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                Point<T>.Offset(pPoints, Points.Length, Dx, Dy);
            }
        }

        public void Rotate(double Theta)
        {
            if (IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                Point<T>.Rotate(pPoints, Points.Length, Theta);
            }
        }
        public void Rotate(Point<T> Center, double Theta)
            => Rotate(Center.X, Center.Y, Theta);
        public void Rotate(T Cx, T Cy, double Theta)
        {
            if (IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                Point<T>.Rotate(pPoints, Points.Length, Cx, Cy, Theta);
            }
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

            fixed (Point<T>* pPoints = &Points[0])
            {
                Point<T>.Reflect(pPoints, Points.Length, Lx1, Ly1, Lx2, Ly2);
            }
        }

        public void Scale(T Scale)
            => this.Scale(Center, Scale);
        public void Scale(T ScaleX, T ScaleY)
            => Scale(Center, ScaleX, ScaleY);
        public void Scale(Point<T> Center, T Scale)
            => this.Scale(Center.X, Center.Y, Scale, Scale);
        public void Scale(Point<T> Center, T ScaleX, T ScaleY)
            => Scale(Center.X, Center.Y, ScaleX, ScaleY);
        public void Scale(T Cx, T Cy, T Scale)
            => this.Scale(Cx, Cy, Scale, Scale);
        public void Scale(T Cx, T Cy, T ScaleX, T ScaleY)
        {
            if (IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                LineSegment<T>.Scale(pPoints, Points.Length, Cx, Cy, ScaleX, ScaleY);
            }
        }

        /// <summary>
        /// Creates a new casted ArcSegment.
        /// </summary>
        public ArcSegment<U> Cast<U>() where U : unmanaged
            => IsEmpty ? ArcSegment<U>.Empty : new ArcSegment<U>(Points[0].Cast<U>(), Points[1].Cast<U>(), Points[2].Cast<U>());
        IShape<U> IShape<T>.Cast<U>()
            => Cast<U>();
        ICoordinateObject<U> ICoordinateObject<T>.Cast<U>()
            => Cast<U>();

        /// <summary>
        /// Creates a new ArcSegment that is a copy of the current instance.
        /// </summary>
        public ArcSegment<T> Clone()
            => IsEmpty ? Empty : new ArcSegment<T>(Points[0], Points[1], Points[2]);
        IShape<T> IShape<T>.Clone()
            => Clone();
        ICoordinateObject<T> ICoordinateObject<T>.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => (Points is null || Points.Length < 3) ? base.GetHashCode() : Points[0].GetHashCode() ^ Points[1].GetHashCode() ^ Points[2].GetHashCode();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="ArcSegment{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(ArcSegment<T> obj)
        {
            if (IsEmpty)
                return obj.IsEmpty;

            if (obj.IsEmpty)
                return false;

            int Counter = 0;
            for (int i = 0; i < Points.Length; i++)
            {
                if (obj.Points.Contains(Points[i]))
                {
                    Counter++;
                    continue;
                }
            }

            return Counter == 3;
        }
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is ArcSegment<T> Target && Equals(Target);
        bool ICoordinateObject<T>.Equals(ICoordinateObject<T> obj)
            => obj is ArcSegment<T> Target && Equals(Target);
        public override bool Equals(object obj)
            => obj is ArcSegment<T> Target && Equals(Target);

        public override string ToString()
        {
            if (IsEmpty)
                return $"{nameof(ArcSegment<T>)}<{typeof(T).Name}>.Empty";

            Point<T> S = Points[0],
                     P = Points[1],
                     E = Points[2];

            return $"Sx : {S.X}, Sy : {S.Y}, Px : {P.X}, Py : {P.Y}, Ex : {E.X}, Ey : {E.Y}";
        }

        private static void SortPoints(ref T Px1, ref T Py1, ref T Px2, ref T Py2, ref T Px3, ref T Py3)
        {
            Circle<T>.CalculateCenter(Px1, Py1, Px2, Py2, Px3, Py3, out T Cx, out T Cy);

            double Theta1 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px1)), Cast<T, double>(Subtract(Cy, Py1))),
                   Theta2 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px2)), Cast<T, double>(Subtract(Cy, Py2))),
                   Theta3 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px3)), Cast<T, double>(Subtract(Cy, Py3)));

            if (Theta2 < Theta1)
            {
                MathHelper.Swap(ref Px1, ref Px2);
                MathHelper.Swap(ref Py1, ref Py2);
                MathHelper.Swap(ref Theta1, ref Theta2);
            }

            if (Theta3 < Theta1)
            {
                MathHelper.Swap(ref Px1, ref Px3);
                MathHelper.Swap(ref Py1, ref Py3);
                MathHelper.Swap(ref Theta1, ref Theta3);
            }

            if (Theta3 < Theta2)
            {
                MathHelper.Swap(ref Px2, ref Px3);
                MathHelper.Swap(ref Py2, ref Py3);
                MathHelper.Swap(ref Theta2, ref Theta3);
            }
        }
        private static void SortPoints(ref T Px1, ref T Py1, ref T Px2, ref T Py2, ref T Px3, ref T Py3, out T Cx, out T Cy, out double Theta1, out double Theta2, out double Theta3)
        {
            Circle<T>.CalculateCenter(Px1, Py1, Px2, Py2, Px3, Py3, out Cx, out Cy);

            Theta1 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px1)), Cast<T, double>(Subtract(Cy, Py1)));
            Theta2 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px2)), Cast<T, double>(Subtract(Cy, Py2)));
            Theta3 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px3)), Cast<T, double>(Subtract(Cy, Py3)));

            if (Theta2 < Theta1)
            {
                MathHelper.Swap(ref Px1, ref Px2);
                MathHelper.Swap(ref Py1, ref Py2);
                MathHelper.Swap(ref Theta1, ref Theta2);
            }

            if (Theta3 < Theta1)
            {
                MathHelper.Swap(ref Px1, ref Px3);
                MathHelper.Swap(ref Py1, ref Py3);
                MathHelper.Swap(ref Theta1, ref Theta3);
            }

            if (Theta3 < Theta2)
            {
                MathHelper.Swap(ref Px2, ref Px3);
                MathHelper.Swap(ref Py2, ref Py3);
                MathHelper.Swap(ref Theta2, ref Theta3);
            }
        }
        private static void SortPoints(ref Point<T> Point1, ref Point<T> Point2, ref Point<T> Point3)
        {
            T Px1 = Point1.X,
              Py1 = Point1.Y,
              Px2 = Point2.X,
              Py2 = Point2.Y,
              Px3 = Point3.X,
              Py3 = Point3.Y;

            Circle<T>.CalculateCenter(Px1, Py1, Px2, Py2, Px3, Py3, out T Cx, out T Cy);

            double Theta1 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px1)), Cast<T, double>(Subtract(Cy, Py1))),
                   Theta2 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px2)), Cast<T, double>(Subtract(Cy, Py2))),
                   Theta3 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Px3)), Cast<T, double>(Subtract(Cy, Py3)));

            if (Theta2 < Theta1)
            {
                MathHelper.Swap(ref Point1, ref Point2);
                MathHelper.Swap(ref Theta1, ref Theta2);
            }

            if (Theta3 < Theta1)
            {
                MathHelper.Swap(ref Point1, ref Point3);
                MathHelper.Swap(ref Theta1, ref Theta3);
            }

            if (Theta3 < Theta2)
            {
                MathHelper.Swap(ref Point2, ref Point3);
                MathHelper.Swap(ref Theta2, ref Theta3);
            }
        }

        /// <summary>
        /// Indicates whether the arc segment contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Segment">The target arc segment.</param>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        public static bool Contain(ArcSegment<T> Segment, T Px, T Py)
        {
            if (Segment.IsEmpty)
                return false;

            Point<T> p0 = Segment.Points[0],
                     p1 = Segment.Points[1],
                     p2 = Segment.Points[2];

            return Contain(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y, Px, Py);
        }
        /// <summary>
        /// Indicates whether the arc segment contains the specified point.
        /// </summary>
        /// <param name="Segment">The target arc segment.</param>
        /// <param name="Point">The point to check.</param>
        public static bool Contain(ArcSegment<T> Segment, Point<T> Point)
        {
            if (Segment.IsEmpty)
                return false;

            Point<T> p0 = Segment.Points[0],
                     p1 = Segment.Points[1],
                     p2 = Segment.Points[2];

            return Contain(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y, Point.X, Point.Y);
        }
        /// <summary>
        /// Indicates whether the arc segment contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Point1">The first point on the target arc segment.</param>
        /// <param name="Point2">The second point on the target arc segment.</param>
        /// <param name="Point3">The third point on the target arc segment.</param>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        public static bool Contain(Point<T> Point1, Point<T> Point2, Point<T> Point3, T Px, T Py)
            => Contain(Point1.X, Point1.Y, Point2.X, Point2.Y, Point3.X, Point3.Y, Px, Py);
        /// <summary>
        /// Indicates whether the arc segment contains the specified point.
        /// </summary>
        /// <param name="ArcPoint1">The first point on the target arc segment.</param>
        /// <param name="ArcPoint2">The second point on the target arc segment.</param>
        /// <param name="ArcPoint3">The third point on the target arc segment.</param>
        /// <param name="Point">The point to check.</param>
        public static bool Contain(Point<T> ArcPoint1, Point<T> ArcPoint2, Point<T> ArcPoint3, Point<T> Point)
            => Contain(ArcPoint1.X, ArcPoint1.Y, ArcPoint2.X, ArcPoint2.Y, ArcPoint3.X, ArcPoint3.Y, Point.X, Point.Y);
        /// <summary>
        /// Indicates whether the arc segment contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of first point on the target arc segment.</param>
        /// <param name="Ly1">The y-coordinate of first point on the target arc segment.</param>
        /// <param name="Lx2">The x-coordinate of second point on the target arc segment.</param>
        /// <param name="Ly2">The y-coordinate of second point on the target arc segment.</param>
        /// <param name="Lx3">The x-coordinate of third point on the target arc segment.</param>
        /// <param name="Ly3">The y-coordinate of third point on the target arc segment.</param>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        public static bool Contain(T Lx1, T Ly1, T Lx2, T Ly2, T Lx3, T Ly3, T Px, T Py)
        {
            Circle<T>.CalculateCenter(Lx1, Ly1, Lx2, Ly2, Lx3, Ly3, out T Cx, out T Cy);

            T Dx1 = Subtract(Cx, Lx1),
              Dy1 = Subtract(Cy, Ly1),
              TDx = Subtract(Cx, Px),
              TDy = Subtract(Cy, Py);

            double Radius = Math.Sqrt(Cast<T, double>(Add(Multiply(Dx1, Dx1), Multiply(Dy1, Dy1)))),
                   D = Math.Sqrt(Cast<T, double>(Add(Multiply(TDx, TDx), Multiply(TDy, TDy))));

            if (Radius != D)
                return false;

            T Dx3 = Subtract(Cx, Lx3),
              Dy3 = Subtract(Cy, Ly3);

            double Theta1 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Dx1)), Cast<T, double>(Subtract(Cy, Dy1))),
                   Theta3 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Dx3)), Cast<T, double>(Subtract(Cy, Dy3))),
                   Alpha = MathHelper.Atan(Cast<T, double>(Subtract(Cx, TDx)), Cast<T, double>(Subtract(Cy, TDy)));

            return Theta1 <= Alpha && Alpha <= Theta3;
        }

        /// <summary>
        /// Calculate the cross points between the specified Arc Segment and the specified Line.
        /// </summary>
        /// <param name="ArcSegment">The target ArcSegment.</param>
        /// <param name="Line">The target Line.</param>
        public static CrossPoints<T> CrossPoint(ArcSegment<T> ArcSegment, Line<T> Line)
        {
            if (ArcSegment.Points is null || ArcSegment.Points.Length < 2 ||
                Line.Points is null || Line.Points.Length < 2)
                return CrossPoints<T>.None;

            Point<T> Ap1 = ArcSegment.Points[0],
                     Ap2 = ArcSegment.Points[1],
                     Ap3 = ArcSegment.Points[2],
                     Lp1 = Line.Points[0],
                     Lp2 = Line.Points[1];

            T Ax1 = Ap1.X,
              Ay1 = Ap1.Y,
              Ax2 = Ap2.X,
              Ay2 = Ap2.Y,
              Ax3 = Ap3.X,
              Ay3 = Ap3.Y,
              Lx1 = Lp1.X,
              Ly1 = Lp1.Y,
              Lx2 = Lp2.X,
              Ly2 = Lp2.Y;

            Circle<T>.CalculateCenter(Ax1, Ay1, Ax2, Ay2, Ax3, Ay3, out T Cx, out T Cy);

            double Theta1 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Ax1)), Cast<T, double>(Subtract(Cy, Ay1))),
                   Theta3 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Ax3)), Cast<T, double>(Subtract(Cy, Ay3))),
                   Radius = Point<T>.Distance(Cx, Cy, Ax1, Ay1);

            T Dx = Subtract(Lx2, Lx1),
              Dy = Subtract(Ly2, Ly1);

            if (IsDefault(Dx) && IsDefault(Dy))
            {
                double Tr = Point<T>.Distance(Cx, Cy, Lx1, Ly1);

                if (Radius != Tr)
                    return CrossPoints<T>.None;

                T TDx = Subtract(Cx, Lx1),
                  TDy = Subtract(Cy, Ly1);

                double Alpha = MathHelper.Atan(Cast<T, double>(TDx), Cast<T, double>(TDy));

                return Theta1 <= Alpha && Alpha <= Theta3 ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : CrossPoints<T>.None;
            }

            T Kx = Subtract(Lx1, Cx),
              Ky = Subtract(Ly1, Cy);
            double a = Cast<T, double>(Add(Multiply(Dx, Dx), Multiply(Dy, Dy))),
                   b = Cast<T, double>(Add(Multiply(Kx, Dx), Multiply(Ky, Dy))),
                   c = Cast<T, double>(Add(Multiply(Kx, Kx), Multiply(Ky, Ky))) - Radius * Radius,
                   D = b * b - a * c,
                   DDx = Cast<T, double>(Dx),
                   DDy = Cast<T, double>(Dy);

            double t;
            if (D == 0)
            {
                t = -b / a;
                return new CrossPoints<T>(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t))));
            }

            Point<T>[] Crosses = new Point<T>[2];
            double SqrD = Math.Sqrt(D);

            t = (-b + SqrD) / a;
            Crosses[0] = new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t)));
            t = (-b - SqrD) / a;
            Crosses[1] = new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t)));

            return new CrossPoints<T>(Crosses);
        }
        /// <summary>
        /// Calculate the cross points between the specified Arc Segment and the specified Line.
        /// </summary>
        /// <param name="ArcP1">The first point on the target ArcSegment.</param>
        /// <param name="ArcP2">The second point on the target ArcSegment.</param>
        /// <param name="ArcP3">The third point on the target ArcSegment.</param>
        /// <param name="Line">The target Line.</param>
        public static CrossPoints<T> CrossPoint(Point<T> ArcP1, Point<T> ArcP2, Point<T> ArcP3, Line<T> Line)
            => CrossPoint(ArcP1.X, ArcP1.Y, ArcP2.X, ArcP2.Y, ArcP3.X, ArcP3.Y, Line);
        /// <summary>
        /// Calculate the cross points between the specified Arc Segment and the specified Line.
        /// </summary>
        /// <param name="Ax1">The x-coordinate of a first point on the target ArcSegment.</param>
        /// <param name="Ay1">The y-coordinate of a first point on the target ArcSegment.</param>
        /// <param name="Ax2">The x-coordinate of a second point on the target ArcSegment.</param>
        /// <param name="Ay2">The y-coordinate of a second point on the target ArcSegment.</param>
        /// <param name="Ax3">The x-coordinate of a third point on the target ArcSegment.</param>
        /// <param name="Ay3">The y-coordinate of a third point on the target ArcSegment.</param>
        /// <param name="Line">The target Line.</param>
        public static CrossPoints<T> CrossPoint(T Ax1, T Ay1, T Ax2, T Ay2, T Ax3, T Ay3, Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return CrossPoints<T>.None;

            Point<T> Lp1 = Line.Points[0],
                     Lp2 = Line.Points[1];

            T Lx1 = Lp1.X,
              Ly1 = Lp1.Y,
              Lx2 = Lp2.X,
              Ly2 = Lp2.Y;

            SortPoints(ref Ax1, ref Ay1, ref Ax2, ref Ay2, ref Ax3, ref Ay3, out T Cx, out T Cy, out double Theta1, out double Theta2, out double Theta3);
            double Radius = Point<T>.Distance(Cx, Cy, Ax1, Ay1);

            T Dx = Subtract(Lx2, Lx1),
              Dy = Subtract(Ly2, Ly1);

            if (IsDefault(Dx) && IsDefault(Dy))
            {
                double Tr = Point<T>.Distance(Cx, Cy, Lx1, Ly1);

                if (Radius != Tr)
                    return CrossPoints<T>.None;

                T TDx = Subtract(Cx, Lx1),
                  TDy = Subtract(Cy, Ly1);

                double Alpha = MathHelper.Atan(Cast<T, double>(TDx), Cast<T, double>(TDy));

                return Theta1 <= Alpha && Alpha <= Theta3 ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : CrossPoints<T>.None;
            }

            T Kx = Subtract(Lx1, Cx),
              Ky = Subtract(Ly1, Cy);
            double a = Cast<T, double>(Add(Multiply(Dx, Dx), Multiply(Dy, Dy))),
                   b = Cast<T, double>(Add(Multiply(Kx, Dx), Multiply(Ky, Dy))),
                   c = Cast<T, double>(Add(Multiply(Kx, Kx), Multiply(Ky, Ky))) - Radius * Radius,
                   D = b * b - a * c,
                   DDx = Cast<T, double>(Dx),
                   DDy = Cast<T, double>(Dy);

            double t;
            if (D == 0)
            {
                t = -b / a;
                return new CrossPoints<T>(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t))));
            }

            Point<T>[] Crosses = new Point<T>[2];
            double SqrD = Math.Sqrt(D);

            t = (-b + SqrD) / a;
            Crosses[0] = new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t)));
            t = (-b - SqrD) / a;
            Crosses[1] = new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t)));

            return new CrossPoints<T>(Crosses);
        }
        /// <summary>
        /// Calculate the cross points between the specified Arc Segment and the specified Line Segment.
        /// </summary>
        /// <param name="ArcSegment">The target ArcSegment.</param>
        /// <param name="LineSegment">The target LineSegment.</param>
        public static CrossPoints<T> CrossPoint(ArcSegment<T> ArcSegment, LineSegment<T> LineSegment)
        {
            if (ArcSegment.Points is null || ArcSegment.Points.Length < 2 ||
                LineSegment.Points is null || LineSegment.Points.Length < 2)
                return CrossPoints<T>.None;

            Point<T> Ap1 = ArcSegment.Points[0],
                     Ap2 = ArcSegment.Points[1],
                     Ap3 = ArcSegment.Points[2],
                     Lp1 = LineSegment.Points[0],
                     Lp2 = LineSegment.Points[1];

            T Ax1 = Ap1.X,
              Ay1 = Ap1.Y,
              Ax2 = Ap2.X,
              Ay2 = Ap2.Y,
              Ax3 = Ap3.X,
              Ay3 = Ap3.Y,
              Lx1 = Lp1.X,
              Ly1 = Lp1.Y,
              Lx2 = Lp2.X,
              Ly2 = Lp2.Y;

            Circle<T>.CalculateCenter(Ax1, Ay1, Ax2, Ay2, Ax3, Ay3, out T Cx, out T Cy);

            double Theta1 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Ax1)), Cast<T, double>(Subtract(Cy, Ay1))),
                   Theta3 = MathHelper.Atan(Cast<T, double>(Subtract(Cx, Ax3)), Cast<T, double>(Subtract(Cy, Ay3))),
                   Radius = Point<T>.Distance(Cx, Cy, Ax1, Ay1);

            T Dx = Subtract(Lx2, Lx1),
              Dy = Subtract(Ly2, Ly1);

            if (IsDefault(Dx) && IsDefault(Dy))
            {
                double Tr = Point<T>.Distance(Cx, Cy, Lx1, Ly1);

                if (Radius != Tr)
                    return CrossPoints<T>.None;

                T TDx = Subtract(Cx, Lx1),
                  TDy = Subtract(Cy, Ly1);

                double Alpha = MathHelper.Atan(Cast<T, double>(TDx), Cast<T, double>(TDy));

                return Theta1 <= Alpha && Alpha <= Theta3 ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : CrossPoints<T>.None;
            }

            T Kx = Subtract(Lx1, Cx),
              Ky = Subtract(Ly1, Cy);
            double a = Cast<T, double>(Add(Multiply(Dx, Dx), Multiply(Dy, Dy))),
                   b = Cast<T, double>(Add(Multiply(Kx, Dx), Multiply(Ky, Dy))),
                   c = Cast<T, double>(Add(Multiply(Kx, Kx), Multiply(Ky, Ky))) - Radius * Radius,
                   D = b * b - a * c,
                   DDx = Cast<T, double>(Dx),
                   DDy = Cast<T, double>(Dy);

            double t;
            if (D == 0)
            {
                t = -b / a;
                return 0d <= t && t <= 1d ? new CrossPoints<T>(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t)))) :
                                          CrossPoints<T>.None;
            }

            List<Point<T>> Crosses = new List<Point<T>>();
            double SqrD = Math.Sqrt(D);

            t = (-b + SqrD) / a;
            if (0d <= t && t <= 1d)
                Crosses.Add(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t))));

            t = (-b - SqrD) / a;
            if (0d <= t && t <= 1d)
                Crosses.Add(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t))));

            return new CrossPoints<T>(Crosses);
        }
        /// <summary>
        /// Calculate the cross points between the specified Arc Segment and the specified Line Segment.
        /// </summary>
        /// <param name="ArcP1">The first point on the target ArcSegment.</param>
        /// <param name="ArcP2">The second point on the target ArcSegment.</param>
        /// <param name="ArcP3">The third point on the target ArcSegment.</param>
        /// <param name="LineSegment">The target LineSegment.</param>
        public static CrossPoints<T> CrossPoint(Point<T> ArcP1, Point<T> ArcP2, Point<T> ArcP3, LineSegment<T> LineSegment)
            => CrossPoint(ArcP1.X, ArcP1.Y, ArcP2.X, ArcP2.Y, ArcP3.X, ArcP3.Y, LineSegment);
        /// <summary>
        /// Calculate the cross points between the specified Arc Segment and the specified Line Segment.
        /// </summary>
        /// <param name="Ax1">The x-coordinate of a first point on the target ArcSegment.</param>
        /// <param name="Ay1">The y-coordinate of a first point on the target ArcSegment.</param>
        /// <param name="Ax2">The x-coordinate of a second point on the target ArcSegment.</param>
        /// <param name="Ay2">The y-coordinate of a second point on the target ArcSegment.</param>
        /// <param name="Ax3">The x-coordinate of a third point on the target ArcSegment.</param>
        /// <param name="Ay3">The y-coordinate of a third point on the target ArcSegment.</param>
        /// <param name="LineSegment">The target LineSegment.</param>
        public static CrossPoints<T> CrossPoint(T Ax1, T Ay1, T Ax2, T Ay2, T Ax3, T Ay3, LineSegment<T> LineSegment)
        {
            if (LineSegment.Points is null || LineSegment.Points.Length < 2)
                return CrossPoints<T>.None;

            Point<T> Lp1 = LineSegment.Points[0],
                     Lp2 = LineSegment.Points[1];

            T Lx1 = Lp1.X,
              Ly1 = Lp1.Y,
              Lx2 = Lp2.X,
              Ly2 = Lp2.Y;

            SortPoints(ref Ax1, ref Ay1, ref Ax2, ref Ay2, ref Ax3, ref Ay3, out T Cx, out T Cy, out double Theta1, out double Theta2, out double Theta3);
            double Radius = Point<T>.Distance(Cx, Cy, Ax1, Ay1);

            T Dx = Subtract(Lx2, Lx1),
              Dy = Subtract(Ly2, Ly1);

            if (IsDefault(Dx) && IsDefault(Dy))
            {
                double Tr = Point<T>.Distance(Cx, Cy, Lx1, Ly1);

                if (Radius != Tr)
                    return CrossPoints<T>.None;

                T TDx = Subtract(Cx, Lx1),
                  TDy = Subtract(Cy, Ly1);

                double Alpha = MathHelper.Atan(Cast<T, double>(TDx), Cast<T, double>(TDy));

                return Theta1 <= Alpha && Alpha <= Theta3 ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : CrossPoints<T>.None;
            }

            T Kx = Subtract(Lx1, Cx),
              Ky = Subtract(Ly1, Cy);
            double a = Cast<T, double>(Add(Multiply(Dx, Dx), Multiply(Dy, Dy))),
                   b = Cast<T, double>(Add(Multiply(Kx, Dx), Multiply(Ky, Dy))),
                   c = Cast<T, double>(Add(Multiply(Kx, Kx), Multiply(Ky, Ky))) - Radius * Radius,
                   D = b * b - a * c,
                   DDx = Cast<T, double>(Dx),
                   DDy = Cast<T, double>(Dy);

            double t;
            if (D == 0)
            {
                t = -b / a;
                return 0d <= t && t <= 1d ? new CrossPoints<T>(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t)))) :
                                          CrossPoints<T>.None;
            }

            List<Point<T>> Crosses = new List<Point<T>>();
            double SqrD = Math.Sqrt(D);

            t = (-b + SqrD) / a;
            if (0d <= t && t <= 1d)
                Crosses.Add(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t))));

            t = (-b - SqrD) / a;
            if (0d <= t && t <= 1d)
                Crosses.Add(new Point<T>(Add(Lx1, Cast<double, T>(DDx * t)), Add(Ly1, Cast<double, T>(DDy * t))));

            return new CrossPoints<T>(Crosses);
        }

        /// <summary>
        /// Offsets the specified ArcSegment by the specified vector.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be offsetted.</param>
        /// <param name="Vector">The vector to be added to this ArcSegment.</param>
        public static ArcSegment<T> Offset(ArcSegment<T> ArcSegment, Vector<T> Vector)
            => Offset(ArcSegment, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified ArcSegment by the specified amounts.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be offsetted.</param>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public static ArcSegment<T> Offset(ArcSegment<T> ArcSegment, T Dx, T Dy)
            => ArcSegment.IsEmpty ? Empty : new ArcSegment<T> { Points = Point<T>.Offset(ArcSegment.Points, Dx, Dy) };

        /// <summary>
        /// Rotates the specified ArcSegment about the origin.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static ArcSegment<T> Rotate(ArcSegment<T> ArcSegment, double Theta)
            => ArcSegment.IsEmpty ? Empty : new ArcSegment<T> { Points = Point<T>.Rotate(ArcSegment.Points, Theta) };
        /// <summary>
        /// Rotates the specified ArcSegment about the specified point.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static ArcSegment<T> Rotate(ArcSegment<T> ArcSegment, Point<T> Center, double Theta)
            => Rotate(ArcSegment, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified ArcSegment about the specified point(<paramref name="Cx"/>, <paramref name="Cy"/>).
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static ArcSegment<T> Rotate(ArcSegment<T> ArcSegment, T Cx, T Cy, double Theta)
            => ArcSegment.IsEmpty ? Empty : new ArcSegment<T> { Points = Point<T>.Rotate(ArcSegment.Points, Cx, Cy, Theta) };

        /// <summary>
        /// Reflects the specified ArcSegment over the specified ArcSegment.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be reflects.</param>
        /// <param name="ProjectionArcSegment">The projection ArcSegment.</param>
        public static ArcSegment<T> Reflect(ArcSegment<T> ArcSegment, ArcSegment<T> ProjectionArcSegment)
        {
            if (ProjectionArcSegment.Points is null || ProjectionArcSegment.Points.Length < 2)
                return ArcSegment.Clone();

            Point<T> P1 = ProjectionArcSegment.Points[0],
                     P2 = ProjectionArcSegment.Points[1];

            return Reflect(ArcSegment, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified ArcSegment over the specified ArcSegment.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be reflects.</param>
        /// <param name="ArcSegmentPoint1">The ArcSegment on the projection ArcSegment.</param>
        /// <param name="ArcSegmentPoint2">The another ArcSegment on the projection ArcSegment.</param>
        public static ArcSegment<T> Reflect(ArcSegment<T> ArcSegment, Point<T> ArcSegmentPoint1, Point<T> ArcSegmentPoint2)
            => Reflect(ArcSegment, ArcSegmentPoint1.X, ArcSegmentPoint1.Y, ArcSegmentPoint2.X, ArcSegmentPoint2.Y);
        /// <summary>
        /// Reflects the specified ArcSegment over the specified ArcSegment.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment to be reflects.</param>
        /// <param name="Lx1">The x-coordinate of a ArcSegment on the projection ArcSegment.</param>
        /// <param name="Ly1">The y-coordinate of a ArcSegment on the projection ArcSegment.</param>
        /// <param name="Lx2">The x-coordinate of a another ArcSegment on the projection ArcSegment.</param>
        /// <param name="Ly2">The y-coordinate of a another ArcSegment on the projection ArcSegment.</param>
        public static ArcSegment<T> Reflect(ArcSegment<T> ArcSegment, T Lx1, T Ly1, T Lx2, T Ly2)
            => ArcSegment.IsEmpty ? Empty : new ArcSegment<T> { Points = Point<T>.Reflect(ArcSegment.Points, Lx1, Ly1, Lx2, Ly2) };

        /// <summary>
        /// Adds the specified vector to the specified ArcSegment.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static ArcSegment<T> operator +(ArcSegment<T> ArcSegment, Vector<T> Vector)
            => Offset(ArcSegment, Vector);
        /// <summary>
        /// Subtracts the specified vector from the specified ArcSegment.
        /// </summary>
        /// <param name="ArcSegment">The ArcSegment from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from ArcSegment.</param>
        public static ArcSegment<T> operator -(ArcSegment<T> ArcSegment, Vector<T> Vector)
            => Offset(ArcSegment, -Vector);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="ArcSegment1">The first ArcSegment to compare.</param>
        /// <param name="ArcSegment2">The second ArcSegment to compare.</param>
        public static bool operator ==(ArcSegment<T> ArcSegment1, ArcSegment<T> ArcSegment2)
            => ArcSegment1.Equals(ArcSegment2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="ArcSegment1">The first ArcSegment to compare.</param>
        /// <param name="ArcSegment2">The second ArcSegment to compare.</param>
        public static bool operator !=(ArcSegment<T> ArcSegment1, ArcSegment<T> ArcSegment2)
            => !ArcSegment1.Equals(ArcSegment2);

    }
}