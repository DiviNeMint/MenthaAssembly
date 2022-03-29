using System;
using System.Linq;
using System.Linq.Expressions;

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
        /// The circle center point of this arc segment.
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
        IMathObject<U> IMathObject<T>.Cast<U>()
            => Cast<U>();

        /// <summary>
        /// Creates a new ArcSegment that is a copy of the current instance.
        /// </summary>
        public ArcSegment<T> Clone()
            => IsEmpty ? Empty : new ArcSegment<T>(Points[0], Points[1], Points[2]);
        IShape<T> IShape<T>.Clone()
            => Clone();
        IMathObject<T> IMathObject<T>.Clone()
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
        bool IMathObject<T>.Equals(IMathObject<T> obj)
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

        private static readonly Func<T, T, T> Add, Sub, Mul;
        private static readonly Func<T, T> Abs, Div2;
        private static readonly Func<T, T, bool> GreaterThan;
        private static readonly Predicate<T> IsDefault;
        private static readonly Func<T, double> ToDouble;
        private static readonly Func<double, T> ToGeneric;
        static ArcSegment()
        {
            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();

            Abs = ExpressionHelper<T>.CreateAbs();
            Div2 = ExpressionHelper<T>.CreateDiv(2);

            GreaterThan = ExpressionHelper<T>.CreateGreaterThan();
            IsDefault = ExpressionHelper<T>.CreateIsDefault();

            ToDouble = ExpressionHelper<T>.CreateCast<double>();
            ToGeneric = ExpressionHelper<double>.CreateCast<T>();
        }

        private static void SortPoints(ref T Px1, ref T Py1, ref T Px2, ref T Py2, ref T Px3, ref T Py3)
        {
            Circle<T>.CalculateCenter(Px1, Py1, Px2, Py2, Px3, Py3, out T Cx, out T Cy);

            double Theta1 = MathHelper.Atan(ToDouble(Sub(Cx, Px1)), ToDouble(Sub(Cy, Py1))),
                   Theta2 = MathHelper.Atan(ToDouble(Sub(Cx, Px2)), ToDouble(Sub(Cy, Py2))),
                   Theta3 = MathHelper.Atan(ToDouble(Sub(Cx, Px3)), ToDouble(Sub(Cy, Py3)));

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

            double Theta1 = MathHelper.Atan(ToDouble(Sub(Cx, Px1)), ToDouble(Sub(Cy, Py1))),
                   Theta2 = MathHelper.Atan(ToDouble(Sub(Cx, Px2)), ToDouble(Sub(Cy, Py2))),
                   Theta3 = MathHelper.Atan(ToDouble(Sub(Cx, Px3)), ToDouble(Sub(Cy, Py3)));

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

            T Dx1 = Sub(Cx, Lx1),
              Dy1 = Sub(Cy, Ly1),
              TDx = Sub(Cx, Px),
              TDy = Sub(Cy, Py);

            double Radius = Math.Sqrt(ToDouble(Add(Mul(Dx1, Dx1), Mul(Dy1, Dy1)))),
                   D = Math.Sqrt(ToDouble(Add(Mul(TDx, TDx), Mul(TDy, TDy))));

            if (Radius != D)
                return false;

            T Dx3 = Sub(Cx, Lx3),
              Dy3 = Sub(Cy, Ly3);

            double Theta1 = MathHelper.Atan(ToDouble(Sub(Cx, Dx1)), ToDouble(Sub(Cy, Dy1))),
                   Theta3 = MathHelper.Atan(ToDouble(Sub(Cx, Dx3)), ToDouble(Sub(Cy, Dy3))),
                   Alpha = MathHelper.Atan(ToDouble(Sub(Cx, TDx)), ToDouble(Sub(Cy, TDy)));

            return Theta1 <= Alpha && Alpha <= Theta3;
        }

        ///// <summary>
        ///// Calculate the cross points between the specified Arc Segment and the specified Line.
        ///// </summary>
        ///// <param name="ArcSegment">The target ArcSegment.</param>
        ///// <param name="Line">The target Line.</param>
        //public static CrossPoints<T> CrossPoint(ArcSegment<T> ArcSegment, Line<T> Line)
        //{
        //    if (ArcSegment.Points is null || ArcSegment.Points.Length < 2 ||
        //        Line.Points is null || Line.Points.Length < 2)
        //        return CrossPoints<T>.None;

        //    Point<T> p1 = ArcSegment.Points[0],
        //             p2 = ArcSegment.Points[1],
        //             p3 = ArcSegment.Points[2],
        //             p4 = Line.Points[0],
        //             p5 = Line.Points[1];

        //    return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y, p5.X, p5.Y);
        //}
        ///// <summary>
        ///// Calculate the cross points between the specified Arc Segment and the specified Line.
        ///// </summary>
        ///// <param name="ArcSegment">The target ArcSegment.</param>
        ///// <param name="Lx1">The x-coordinate of a point on the second target Line.</param>
        ///// <param name="Ly1">The y-coordinate of a point on the second target Line.</param>
        ///// <param name="Lx2">The x-coordinate of a another point on the second target Line.</param>
        ///// <param name="Ly2">The y-coordinate of a another point on the second target Line.</param>
        //public static CrossPoints<T> CrossPoint(ArcSegment<T> ArcSegment, T Lx1, T Ly1, T Lx2, T Ly2)
        //{
        //    if (ArcSegment.Points is null || ArcSegment.Points.Length < 2)
        //        return CrossPoints<T>.None;

        //    Point<T> p1 = ArcSegment.Points[0],
        //             p2 = ArcSegment.Points[1],
        //             p3 = ArcSegment.Points[2];

        //    return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, Lx1, Ly1, Lx2, Ly2);
        //}
        ///// <summary>
        ///// Calculate the cross points between the specified Arc Segment and the specified Line.
        ///// </summary>
        ///// <param name="ArcP1">The first point on the first target ArcSegment.</param>
        ///// <param name="ArcP2">The second point on the first target ArcSegment.</param>
        ///// <param name="ArcP3">The third point on the first target ArcSegment.</param>
        ///// <param name="LineP1">The point on the second target Line.</param>
        ///// <param name="LineP2">The another point on the second target Line.</param>
        //public static CrossPoints<T> CrossPoint(Point<T> ArcP1, Point<T> ArcP2, Point<T> ArcP3, Point<T> LineP1, Point<T> LineP2)
        //    => CrossPoint(ArcP1.X, ArcP1.Y, ArcP2.X, ArcP2.Y, ArcP3.X, ArcP3.Y, LineP1.X, LineP1.Y, LineP2.X, LineP2.Y);
        ///// <summary>
        ///// Calculate the cross points between the specified Arc Segment and the specified Line.
        ///// </summary>
        ///// <param name="Ax1">The x-coordinate of a first point on the first target ArcSegment.</param>
        ///// <param name="Ay1">The y-coordinate of a first point on the first target ArcSegment.</param>
        ///// <param name="Ax2">The x-coordinate of a second point on the first target ArcSegment.</param>
        ///// <param name="Ay2">The y-coordinate of a second point on the first target ArcSegment.</param>
        ///// <param name="Ax3">The x-coordinate of a third point on the first target ArcSegment.</param>
        ///// <param name="Ay3">The y-coordinate of a third point on the first target ArcSegment.</param>
        ///// <param name="Lx1">The x-coordinate of a point on the second target Line.</param>
        ///// <param name="Ly1">The y-coordinate of a point on the second target Line.</param>
        ///// <param name="Lx2">The x-coordinate of a another point on the second target Line.</param>
        ///// <param name="Ly2">The y-coordinate of a another point on the second target Line.</param>
        //public static CrossPoints<T> CrossPoint(T Ax1, T Ay1, T Ax2, T Ay2, T Ax3, T Ay3, T Lx1, T Ly1, T Lx2, T Ly2)
        //{
        //    T v1x = Sub(Ax2, Ax1),
        //      v1y = Sub(Ay2, Ay1);

        //    if (IsDefault(v1x) && IsDefault(v1y))
        //        return IsColArcSegmentar(Ax1, Ay1, Lx1, Ly1, Lx2, Ly2) ? new CrossPoints<T>(new Point<T>(Ax1, Ay1)) : CrossPoints<T>.None;

        //    T v2x = Sub(Lx2, Lx1),
        //      v2y = Sub(Ly2, Ly1),
        //      v3x = Sub(Lx1, Ax1),
        //      v3y = Sub(Ly1, Ay1);

        //    if (IsDefault(v2x) && IsDefault(v2y))
        //        return (IsDefault(v3x) && IsDefault(v3x)) || IsDefault(Vector<T>.Cross(v1x, v1y, v3x, v3y)) ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : CrossPoints<T>.None;

        //    T C1 = Vector<T>.Cross(v1x, v1y, v2x, v2y),
        //      C2 = Vector<T>.Cross(v3x, v3y, v2x, v2y);

        //    if (IsDefault(C1))
        //        return IsDefault(C2) ? CrossPoints<T>.Infinity : CrossPoints<T>.None;

        //    double t = ToDouble(C2) / ToDouble(C1);
        //    if (t < 0)
        //        t = -t;

        //    return new CrossPoints<T>(new Point<T>(Add(Ax1, ToGeneric(ToDouble(v1x) * t)), Add(Ay1, ToGeneric(ToDouble(v1y) * t))));
        //}
        ///// <summary>
        ///// Calculate the cross points between the specified ArcSegment and the specified ArcSegment segment.
        ///// </summary>
        ///// <param name="ArcSegment">the target ArcSegment.</param>
        ///// <param name="Segment">The target ArcSegment segment.</param>
        //public static CrossPoints<T> CrossPoint(ArcSegment<T> ArcSegment, ArcSegment<T> Segment)
        //{
        //    if (ArcSegment.Points is null || ArcSegment.Points.Length < 2)
        //        return CrossPoints<T>.None;

        //    Point<T> p1 = ArcSegment.Points[0],
        //             p2 = ArcSegment.Points[1];

        //    return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, Segment);
        //}
        ///// <summary>
        ///// Calculate the cross points between the specified ArcSegment and the specified ArcSegment segment.
        ///// </summary>
        ///// <param name="ArcSegmentPoint1">The point on the target ArcSegment.</param>
        ///// <param name="ArcSegmentPoint2">The another point on the target ArcSegment.</param>
        ///// <param name="Segment">the target ArcSegment segment.</param>
        //public static CrossPoints<T> CrossPoint(Point<T> ArcSegmentPoint1, Point<T> ArcSegmentPoint2, ArcSegment<T> Segment)
        //    => CrossPoint(ArcSegmentPoint1.X, ArcSegmentPoint1.Y, ArcSegmentPoint2.X, ArcSegmentPoint2.Y, Segment);
        ///// <summary>
        ///// Calculate the cross points between the specified ArcSegment and the specified ArcSegment segment.
        ///// </summary>
        ///// <param name="Lx1">The x-coordinate of a point on the target ArcSegment.</param>
        ///// <param name="Ly1">The y-coordinate of a point on the target ArcSegment.</param>
        ///// <param name="Lx2">The x-coordinate of a another point on the target ArcSegment.</param>
        ///// <param name="Ly2">The y-coordinate of a another point on the target ArcSegment.</param>
        ///// <param name="Segment">the target ArcSegment segment.</param>
        //public static CrossPoints<T> CrossPoint(T Lx1, T Ly1, T Lx2, T Ly2, ArcSegment<T> Segment)
        //{
        //    if (Segment.Points is null || Segment.Points.Length < 2)
        //        return CrossPoints<T>.None;

        //    Point<T> Sp1 = Segment.Points[0],
        //             Sp2 = Segment.Points[1];

        //    T Sx1 = Sp1.X,
        //      Sy1 = Sp1.Y,
        //      Sx2 = Sp2.X,
        //      Sy2 = Sp2.Y,
        //      v1x = Sub(Lx2, Lx1),
        //      v1y = Sub(Ly2, Ly1);

        //    if (IsDefault(v1x) && IsDefault(v1y))
        //        return ArcSegmentSegment<T>.Contain(Sx1, Sy1, Sx2, Sy2, Lx1, Ly1) ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : CrossPoints<T>.None;

        //    T v2x = Sub(Sx2, Sx1),
        //      v2y = Sub(Sy2, Sy1);

        //    if (IsDefault(v2x) && IsDefault(v2y))
        //        return IsColArcSegmentar(Lx1, Ly1, Lx2, Ly2, Sx1, Sy1) ? new CrossPoints<T>(new Point<T>(Sx1, Sy1)) : CrossPoints<T>.None;

        //    T v3x = Sub(Sx1, Lx1),
        //      v3y = Sub(Sy1, Ly1),
        //      C1 = Vector<T>.Cross(v1x, v1y, v2x, v2y),
        //      C2 = Vector<T>.Cross(v3x, v3y, v2x, v2y);

        //    if (IsDefault(C1))
        //        return IsDefault(C2) ? CrossPoints<T>.Infinity : CrossPoints<T>.None;

        //    double t = ToDouble(C2) / ToDouble(C1);
        //    if (t < 0)
        //        t = -t;

        //    T X = Add(Lx1, ToGeneric(ToDouble(v1x) * t)),
        //      Y = Add(Ly1, ToGeneric(ToDouble(v1y) * t));

        //    return ArcSegmentSegment<T>.OnSegment(Sx1, Sy1, Sx2, Sy2, X, Y) ? new CrossPoints<T>(new Point<T>(X, Y)) : CrossPoints<T>.None;
        //}
        ///// <summary>
        ///// Calculate the cross points between the specified ArcSegment and the specified circle.
        ///// </summary>
        ///// <param name="ArcSegment">the target ArcSegment.</param>
        ///// <param name="Circle">The target circle.</param>
        //public static CrossPoints<T> CrossPoint(ArcSegment<T> ArcSegment, Circle<T> Circle)
        //{
        //    if (ArcSegment.Points is null || ArcSegment.Points.Length < 2)
        //        return CrossPoints<T>.None;

        //    Point<T> p1 = ArcSegment.Points[0],
        //             p2 = ArcSegment.Points[1];

        //    return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, Circle);
        //}
        ///// <summary>
        ///// Calculate the cross points between the specified ArcSegment and the specified circle.
        ///// </summary>
        ///// <param name="ArcSegmentPoint1">The point on the target ArcSegment.</param>
        ///// <param name="ArcSegmentPoint2">The another point on the target ArcSegment.</param>
        ///// <param name="Circle">the target circle.</param>
        //public static CrossPoints<T> CrossPoint(Point<T> ArcSegmentPoint1, Point<T> ArcSegmentPoint2, Circle<T> Circle)
        //    => CrossPoint(ArcSegmentPoint1.X, ArcSegmentPoint1.Y, ArcSegmentPoint2.X, ArcSegmentPoint2.Y, Circle);
        ///// <summary>
        ///// Calculate the cross points between the specified ArcSegment and the specified circle.
        ///// </summary>
        ///// <param name="Lx1">The x-coordinate of a point on the target ArcSegment.</param>
        ///// <param name="Ly1">The y-coordinate of a point on the target ArcSegment.</param>
        ///// <param name="Lx2">The x-coordinate of a another point on the target ArcSegment.</param>
        ///// <param name="Ly2">The y-coordinate of a another point on the target ArcSegment.</param>
        ///// <param name="Circle">the target circle.</param>
        //public static CrossPoints<T> CrossPoint(T Lx1, T Ly1, T Lx2, T Ly2, Circle<T> Circle)
        //{
        //    if (Circle.IsEmpty)
        //        return CrossPoints<T>.None;

        //    double Dx = ToDouble(Sub(Lx2, Lx1)),
        //           Dy = ToDouble(Sub(Ly2, Ly1));

        //    if (Dx is 0d && Dy is 0d)
        //        return CrossPoints<T>.None;

        //    // Lx' = Lx1 + Dx * t
        //    // Ly' = Ly1 + Dy * t

        //    // Circle Equation
        //    // X ^ 2 + Y ^ 2 = R ^ 2
        //    double R = Circle.Radius,
        //           DLx = ToDouble(Lx1),
        //           DLy = ToDouble(Ly1),
        //           a = Dx * Dx + Dy * Dy,
        //           b = DLx * Dx + DLy * Dy,
        //           c = DLx * DLx + DLy * DLy - R * R,
        //           D = b * b - a * c;

        //    if (D < 0)
        //        return CrossPoints<T>.None;

        //    double t;
        //    if (D == 0)
        //    {
        //        t = -b / a;
        //        return new CrossPoints<T>(new Point<T>(ToGeneric(DLx + Dx * t), ToGeneric(DLy + Dy * t)));
        //    }

        //    Point<T>[] Crosses = new Point<T>[2];

        //    double SqrD = Math.Sqrt(D);

        //    t = (-b + SqrD) / a;
        //    Crosses[0] = new Point<T>(ToGeneric(DLx + Dx * t), ToGeneric(DLy + Dy * t));

        //    t = (-b - SqrD) / a;
        //    Crosses[1] = new Point<T>(ToGeneric(DLx + Dx * t), ToGeneric(DLy + Dy * t));

        //    return new CrossPoints<T>(Crosses);
        //}

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