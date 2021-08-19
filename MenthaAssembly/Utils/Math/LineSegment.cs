using System;
using System.Linq;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a line segment in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct LineSegment<T> : IShape<T>
        where T : unmanaged
    {
        /// <summary>
        /// Gets a special value that represents a line with no position.
        /// </summary>
        public static LineSegment<T> Empty => new();

        internal Point<T>[] Points;

        /// <summary>
        /// The start point of this line segment.
        /// </summary>
        public Point<T> Start
        {
            set
            {
                if (Points is null)
                    Points = new Point<T>[2];

                Points[0] = value;
            }
            get => Points?[0] ?? default;
        }

        /// <summary>
        /// The end point of this line segment.
        /// </summary>
        public Point<T> End
        {
            set
            {
                if (Points is null)
                    Points = new Point<T>[2];

                Points[1] = value;
            }
            get => Points?[1] ?? default;
        }

        /// <summary>
        /// The center point of this line segment.
        /// </summary>
        public Point<T> Center
        {
            get
            {
                if (this.IsEmpty)
                    return new Point<T>();

                Point<T> p0 = Points[0],
                         p1 = Points[1];

                return new Point<T>(Div2(Add(p0.X, p1.X)), Div2(Add(p0.Y, p1.Y)));
            }
        }

        /// <summary>
        /// The length of this line segment.
        /// </summary>
        public double Length
            => this.IsEmpty ? 0d : Point<T>.Distance(Points[0], Points[1]);

        double IShape<T>.Area
            => 0d;

        /// <summary>
        /// The normal vector of this line segment.
        /// </summary>
        public Vector<T> NormalVector
        {
            get
            {
                if (this.IsEmpty)
                    return Vector<T>.Zero;

                Point<T> p0 = Points[0],
                         p1 = Points[1];

                return new Vector<T>(p1.Y, p0.X, p0.Y, p1.X);
            }
        }

        /// <summary>
        /// The directional vector of this line segment.
        /// </summary>
        public Vector<T> DirectionalVector
            => this.IsEmpty ? Vector<T>.Zero : new Vector<T>(Points[0], Points[1]);

        /// <summary>
        /// Gets a value that indicates whether the line segment is the empty line segment.
        /// </summary>
        public bool IsEmpty
            => Points is null || Points.Length < 2 || Points[0].Equals(Points[1]);

        /// <summary>
        /// The gradient of the line segment.
        /// </summary>
        public double Gradient
        {
            get
            {
                if (this.IsEmpty)
                    return double.NaN;

                Point<T> p0 = Points[0],
                         p1 = Points[1];

                return ToDouble(Sub(p1.Y, p0.Y)) / ToDouble(Sub(p1.X, p0.X));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment{T}"/> structure.
        /// </summary>
        /// <param name="Start">The start point that the new line segment.</param>
        /// <param name="End">The end point that the new line segment.</param>
        public LineSegment(Point<T> Start, Point<T> End)
        {
            if (Start.Equals(End))
            {
                this = Empty;
                return;
            }

            Points = new[] { Start, End };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment{T}"/> structure.
        /// </summary>
        /// <param name="Point">The start point that the new line segment must contain.</param>
        /// <param name="Vector">The vector of the start point to end point.</param>
        public LineSegment(Point<T> Point, Vector<T> Vector)
        {
            if (Vector.IsZero)
            {
                this = Empty;
                return;
            }

            Points = new[] { Point, Point<T>.Offset(Point, Vector) };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment{T}"/> structure.
        /// </summary>        
        /// <param name="Sx">The x-coordinate of the start point that the new line segment.</param>
        /// <param name="Sy">The y-coordinate of the start point that the new line segment.</param>
        /// <param name="Ex">The x-coordinate of the end point that the new line segment.</param>
        /// <param name="Ey">The y-coordinate of the end point that the new line segment.</param>
        public LineSegment(T Sx, T Sy, T Ex, T Ey)
        {
            if (Sx.Equals(Ex) && Sy.Equals(Ey))
            {
                this = Empty;
                return;
            }

            Points = new[] { new Point<T>(Sx, Sy), new Point<T>(Ex, Ey) };
        }

        public bool Contain(Point<T> Point)
            => this.Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
            => Contain(this, Px, Py);

        public void Offset(Vector<T> Vector)
            => this.Offset(Vector.X, Vector.Y);
        public void Offset(T Dx, T Dy)
        {
            if (this.IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                Point<T>.Offset(pPoints, Points.Length, Dx, Dy);
            }
        }

        public void Scale(T Scale)
            => this.Scale(this.Center, Scale);
        public void Scale(T ScaleX, T ScaleY)
            => this.Scale(this.Center, ScaleX, ScaleY);
        public void Scale(Point<T> Center, T Scale)
            => this.Scale(Center.X, Center.Y, Scale, Scale);
        public void Scale(Point<T> Center, T ScaleX, T ScaleY)
            => this.Scale(Center.X, Center.Y, ScaleX, ScaleY);
        public void Scale(T Cx, T Cy, T Scale)
            => this.Scale(Cx, Cy, Scale, Scale);
        public void Scale(T Cx, T Cy, T ScaleX, T ScaleY)
        {
            if (this.IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                Scale(pPoints, Points.Length, Cx, Cy, ScaleX, ScaleY);
            }
        }

        public void Rotate(double Theta)
        {
            if (this.IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                Point<T>.Rotate(pPoints, Points.Length, Theta);
            }
        }
        public void Rotate(Point<T> Center, double Theta)
            => this.Rotate(Center.X, Center.Y, Theta);
        public void Rotate(T Cx, T Cy, double Theta)
        {
            if (this.IsEmpty)
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

            this.Reflect(P1.X, P1.Y, P2.X, P2.Y);
        }
        public void Reflect(Point<T> LinePoint1, Point<T> LinePoint2)
            => this.Reflect(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        public void Reflect(T Lx1, T Ly1, T Lx2, T Ly2)
        {
            if (this.IsEmpty)
                return;

            fixed (Point<T>* pPoints = &Points[0])
            {
                Point<T>.Reflect(pPoints, Points.Length, Lx1, Ly1, Lx2, Ly2);
            }
        }

        /// <summary>
        /// Creates a new casted line segment.
        /// </summary>
        public LineSegment<U> Cast<U>() where U : unmanaged
            => this.IsEmpty ? LineSegment<U>.Empty : new LineSegment<U>(Points[0].Cast<U>(), Points[1].Cast<U>());
        IShape<U> IShape<T>.Cast<U>()
            => this.Cast<U>();
        IMathObject<U> IMathObject<T>.Cast<U>()
            => this.Cast<U>();

        /// <summary>
        /// Creates a new line segment that is a copy of the current instance.
        /// </summary>
        public LineSegment<T> Clone()
            => this.IsEmpty ? Empty : new LineSegment<T>(Points[0], Points[1]);
        IShape<T> IShape<T>.Clone()
            => this.Clone();
        IMathObject<T> IMathObject<T>.Clone()
            => this.Clone();
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => (Points is null || Points.Length < 2) ? base.GetHashCode() : Points[0].GetHashCode() ^ Points[1].GetHashCode();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="LineSegment{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(LineSegment<T> obj)
        {
            if (this.IsEmpty)
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

            return Counter == 2;
        }
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is LineSegment<T> Target && this.Equals(Target);
        bool IMathObject<T>.Equals(IMathObject<T> obj)
            => obj is LineSegment<T> Target && this.Equals(Target);
        public override bool Equals(object obj)
            => obj is LineSegment<T> Target && this.Equals(Target);

        public override string ToString()
        {
            if (this.IsEmpty)
                return $"{nameof(LineSegment<T>)}<{typeof(T).Name}>.Empty";

            Point<T> S = Points[0],
                     E = Points[1];

            return $"Sx : {S.X}, Sy : {S.Y}, Ex : {E.X}, Ey : {E.Y}";
        }

        private static readonly Func<T, T, T> Add, Sub, Mul, Div;
        private static readonly Func<T, T> Abs, Div2;
        private static readonly Func<T, T, bool> GreaterThan;
        private static readonly Predicate<T> IsDefault;
        private static readonly Func<T, double> ToDouble;
        private static readonly Func<double, T> ToGeneric;
        static LineSegment()
        {
            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();
            Div = ExpressionHelper<T>.CreateDiv();

            Abs = ExpressionHelper<T>.CreateAbs();
            Div2 = ExpressionHelper<T>.CreateDiv(2);

            GreaterThan = ExpressionHelper<T>.CreateGreaterThan();
            IsDefault = ExpressionHelper<T>.CreateIsDefault();

            ToDouble = ExpressionHelper<T>.CreateCast<double>();
            ToGeneric = ExpressionHelper<double>.CreateCast<T>();
        }

        /// <summary>
        /// Indicates whether the line segment contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Segment">The target line segment.</param>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        public static bool Contain(LineSegment<T> Segment, T Px, T Py)
        {
            if (Segment.IsEmpty)
                return false;

            Point<T> p0 = Segment.Points[0],
                     p1 = Segment.Points[1];

            return Contain(p0.X, p0.Y, p1.X, p1.Y, Px, Py);
        }
        /// <summary>
        /// Indicates whether the line segment contains the specified point.
        /// </summary>
        /// <param name="Segment">The target line segment.</param>
        /// <param name="Point">The point to check.</param>
        public static bool Contain(LineSegment<T> Segment, Point<T> Point)
        {
            if (Segment.IsEmpty)
                return false;

            Point<T> p0 = Segment.Points[0],
                     p1 = Segment.Points[1];

            return Contain(p0.X, p0.Y, p1.X, p1.Y, Point.X, Point.Y);
        }
        /// <summary>
        /// Indicates whether the line segment contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Point1">The point on the target line segment.</param>
        /// <param name="Point2">The another point on the target line segment.</param>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        public static bool Contain(Point<T> Point1, Point<T> Point2, T Px, T Py)
            => Contain(Point1.X, Point1.Y, Point2.X, Point2.Y, Px, Py);
        /// <summary>
        /// Indicates whether the line segment contains the specified point.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line segment.</param>
        /// <param name="LinePoint2">The another point on the target line segment.</param>
        /// <param name="Point">The point to check.</param>
        public static bool Contain(Point<T> LinePoint1, Point<T> LinePoint2, Point<T> Point)
            => Contain(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y, Point.X, Point.Y);
        /// <summary>
        /// Indicates whether the line segment contains the specified x-coordinate and y-coordinate.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the target line segment.</param>
        /// <param name="Ly1">The y-coordinate of a point on the target line segment.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the target line segment.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the target line segment.</param>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        public static bool Contain(T Lx1, T Ly1, T Lx2, T Ly2, T Px, T Py)
            => Line<T>.IsCollinear(Lx1, Ly1, Lx2, Ly2, Px, Py) && OnSegment(Lx1, Ly1, Lx2, Ly2, Px, Py);
        /// <summary>
        /// Indicates whether the line segment contains the specified x-coordinate and y-coordinate that is collinear.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the target line segment.</param>
        /// <param name="Ly1">The y-coordinate of a point on the target line segment.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the target line segment.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the target line segment.</param>
        /// <param name="Px">The x-coordinate of the point to check.</param>
        /// <param name="Py">The y-coordinate of the point to check.</param>
        internal static bool OnSegment(T Lx1, T Ly1, T Lx2, T Ly2, T Px, T Py)
        {
            T MaxX, MaxY,
              MinX, MinY;

            if (GreaterThan(Lx2, Lx1))
            {
                MaxX = Lx2;
                MinX = Lx1;
            }
            else
            {
                MaxX = Lx1;
                MinX = Lx2;
            }

            if (GreaterThan(Ly2, Ly1))
            {
                MaxY = Ly2;
                MinY = Ly1;
            }
            else
            {
                MaxY = Ly1;
                MinY = Ly2;
            }

            return !(GreaterThan(MinX, Px) || GreaterThan(Px, MaxX) ||
                     GreaterThan(MinY, Py) || GreaterThan(Py, MaxY));
        }

        /// <summary>
        /// Calculate the cross points between two line segments.
        /// </summary>
        /// <param name="Segment1">The first target line segment.</param>
        /// <param name="Segment2">the second target line segment.</param>
        public static CrossPoints<T> CrossPoint(LineSegment<T> Segment1, LineSegment<T> Segment2)
        {
            if (Segment1.Points is null || Segment1.Points.Length < 2 ||
                Segment2.Points is null || Segment2.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> p1 = Segment1.Points[0],
                     p2 = Segment1.Points[1],
                     p3 = Segment2.Points[0],
                     p4 = Segment2.Points[1];

            return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }
        /// <summary>
        /// Calculate the cross points between two line segments.
        /// </summary>
        /// <param name="Segment">The first target line segment.</param>
        /// <param name="Lx1">The x-coordinate of a point on the second target line segment.</param>
        /// <param name="Ly1">The y-coordinate of a point on the second target line segment.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the second target line segment.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the second target line segment.</param>
        public static CrossPoints<T> CrossPoint(LineSegment<T> Segment, T Lx1, T Ly1, T Lx2, T Ly2)
        {
            if (Segment.Points is null || Segment.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> p1 = Segment.Points[0],
                     p2 = Segment.Points[1];

            return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, Lx1, Ly1, Lx2, Ly2);
        }
        /// <summary>
        /// Calculate the cross points between two line segments.
        /// </summary>
        /// <param name="L1P1">The point on the first target line segment.</param>
        /// <param name="L1P2">The another point on the first target line segment.</param>
        /// <param name="L2P1">The point on the second target line segment.</param>
        /// <param name="L2P2">The another point on the second target line segment.</param>
        public static CrossPoints<T> CrossPoint(Point<T> L1P1, Point<T> L1P2, Point<T> L2P1, Point<T> L2P2)
            => CrossPoint(L1P1.X, L1P1.Y, L1P2.X, L1P2.Y, L2P1.X, L2P1.Y, L2P2.X, L2P2.Y);
        /// <summary>
        /// Calculate the cross points between two line segments.
        /// </summary>
        /// <param name="L1x1">The x-coordinate of a point on the first target line segment.</param>
        /// <param name="L1y1">The y-coordinate of a point on the first target line segment.</param>
        /// <param name="L1x2">The x-coordinate of a another point on the first target line segment.</param>
        /// <param name="L1y2">The y-coordinate of a another point on the first target line segment.</param>
        /// <param name="L2x1">The x-coordinate of a point on the second target line segment.</param>
        /// <param name="L2y1">The y-coordinate of a point on the second target line segment.</param>
        /// <param name="L2x2">The x-coordinate of a another point on the second target line segment.</param>
        /// <param name="L2y2">The y-coordinate of a another point on the second target line segment.</param>
        public static CrossPoints<T> CrossPoint(T L1x1, T L1y1, T L1x2, T L1y2, T L2x1, T L2y1, T L2x2, T L2y2)
        {
            T v1x = Sub(L1x2, L1x1),
              v1y = Sub(L1y2, L1y1);

            if (IsDefault(v1x) && IsDefault(v1y))
                return Contain(L1x1, L1y1, L2x1, L2y1, L2x2, L2y2) ? new CrossPoints<T>(new Point<T>(L1x1, L1y1)) : new CrossPoints<T>(false);

            T v2x = Sub(L2x2, L2x1),
              v2y = Sub(L2y2, L2y1);

            if (IsDefault(v2x) && IsDefault(v2y))
                return Contain(L1x1, L1y1, L1x2, L1y2, L2x1, L2y1) ? new CrossPoints<T>(new Point<T>(L2x1, L2y1)) : new CrossPoints<T>(false);

            T v3x = Sub(L2x1, L1x1),
              v3y = Sub(L2y1, L1y1),
              C1 = Vector<T>.Cross(v1x, v1y, v2x, v2y),
              C2 = Vector<T>.Cross(v3x, v3y, v2x, v2y);

            if (IsDefault(C1))
            {
                if (IsDefault(C2))
                    return new CrossPoints<T>(OnSegment(L1x1, L1y1, L1x2, L1y2, L2x1, L2y1) || OnSegment(L1x1, L1y1, L1x2, L1y2, L2x2, L2y2));

                return new CrossPoints<T>(false);
            }

            double t = ToDouble(C2) / ToDouble(C1);
            if (t < 0)
                t = -t;

            T X = Add(L1x1, ToGeneric(ToDouble(v1x) * t)),
              Y = Add(L1y1, ToGeneric(ToDouble(v1y) * t));

            return OnSegment(L1x1, L1y1, L1x2, L1y2, X, Y) && OnSegment(L2x1, L2y1, L2x2, L2y2, X, Y) ? new CrossPoints<T>(new Point<T>(X, Y)) : new CrossPoints<T>(false);
        }
        /// <summary>
        /// Calculate the cross points between the specified line segment and the specified line.
        /// </summary>
        /// <param name="Segment">The target line segment.</param>
        /// <param name="Line">the target line.</param>
        public static CrossPoints<T> CrossPoint(LineSegment<T> Segment, Line<T> Line)
        {
            if (Segment.Points is null || Segment.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> p1 = Segment.Points[0],
                     p2 = Segment.Points[1];

            return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, Line);
        }
        /// <summary>
        /// Calculate the cross points between the specified line segment and the specified line.
        /// </summary>
        /// <param name="SegmentPoint1">The point on the target line segment.</param>
        /// <param name="SegmentPoint2">The another point on the target line segment.</param>
        /// <param name="Line">the target line.</param>
        public static CrossPoints<T> CrossPoint(Point<T> SegmentPoint1, Point<T> SegmentPoint2, Line<T> Line)
            => CrossPoint(SegmentPoint1.X, SegmentPoint1.Y, SegmentPoint2.X, SegmentPoint2.Y, Line);
        /// <summary>
        /// Calculate the cross points between the specified line segment and the specified line.
        /// </summary>
        /// <param name="Sx1">The x-coordinate of a point on the target line segment.</param>
        /// <param name="Sy1">The y-coordinate of a point on the target line segment.</param>
        /// <param name="Sx2">The x-coordinate of a another point on the target line segment.</param>
        /// <param name="Sy2">The y-coordinate of a another point on the target line segment.</param>
        /// <param name="Line">the target line.</param>
        public static CrossPoints<T> CrossPoint(T Sx1, T Sy1, T Sx2, T Sy2, Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> Lp1 = Line.Points[0],
                     Lp2 = Line.Points[1];

            T Lx1 = Lp1.X,
              Ly1 = Lp1.Y,
              Lx2 = Lp2.X,
              Ly2 = Lp2.Y,
              v1x = Sub(Sx2, Sx1),
              v1y = Sub(Sy2, Sy1);

            if (IsDefault(v1x) && IsDefault(v1y))
                return Line<T>.IsCollinear(Sx1, Sy1, Lx1, Ly1, Lx2, Ly2) ? new CrossPoints<T>(new Point<T>(Sx1, Sy1)) : new CrossPoints<T>(false);

            T v2x = Sub(Lx2, Lx1),
              v2y = Sub(Ly2, Ly1);

            if (IsDefault(v2x) && IsDefault(v2y))
                return Line<T>.IsCollinear(Sx1, Sy1, Sx2, Sy2, Lx1, Ly1) ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : new CrossPoints<T>(false);

            T v3x = Sub(Lx1, Sx1),
              v3y = Sub(Ly1, Sy1),
              C1 = Vector<T>.Cross(v1x, v1y, v2x, v2y),
              C2 = Vector<T>.Cross(v3x, v3y, v2x, v2y);

            if (IsDefault(C1))
                return new CrossPoints<T>(IsDefault(C2));

            double t = ToDouble(C2) / ToDouble(C1);
            if (t < 0)
                t = -t;

            T X = Add(Sx1, ToGeneric(ToDouble(v1x) * t)),
              Y = Add(Sy1, ToGeneric(ToDouble(v1y) * t));

            return OnSegment(Sx1, Sy1, Sx2, Sy2, X, Y) ? new CrossPoints<T>(new Point<T>(X, Y)) : new CrossPoints<T>(false);
        }

        /// <summary>
        /// Scales the specified line segment around the origin.
        /// </summary>
        /// <param name="Segment">The line segment to be scaled.</param>
        /// <param name="Scale">The scale factor.</param>
        public static LineSegment<T> Scale(LineSegment<T> Segment, T Scale)
            => LineSegment<T>.Scale(Segment, Segment.Center, Scale);
        /// <summary>
        /// Scales the specified line segment around the origin.
        /// </summary>
        /// <param name="Segment">The line segment to be scaled.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static LineSegment<T> Scale(LineSegment<T> Segment, T ScaleX, T ScaleY)
            => Scale(Segment, Segment.Center, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified line segment around the specified point.
        /// </summary>
        /// <param name="Segment">The line segment to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static LineSegment<T> Scale(LineSegment<T> Segment, Point<T> Center, T Scale)
            => LineSegment<T>.Scale(Segment, Center.X, Center.Y, Scale, Scale);
        /// <summary>
        /// Scales the specified line segment around the specified point.
        /// </summary>
        /// <param name="Segment">The line segment to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static LineSegment<T> Scale(LineSegment<T> Segment, Point<T> Center, T ScaleX, T ScaleY)
            => Scale(Segment, Center.X, Center.Y, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified line segment around the specified point.
        /// </summary>
        /// <param name="Segment">The line segment to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static LineSegment<T> Scale(LineSegment<T> Segment, T Cx, T Cy, T Scale)
            => LineSegment<T>.Scale(Segment, Cx, Cy, Scale, Scale);
        /// <summary>
        /// Scales the specified line segment around the specified point.
        /// </summary>
        /// <param name="Segment">The line segment to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static LineSegment<T> Scale(LineSegment<T> Segment, T Cx, T Cy, T ScaleX, T ScaleY)
            => Segment.IsEmpty ? Empty : new LineSegment<T> { Points = Scale(Segment.Points, Cx, Cy, ScaleX, ScaleY) };
        /// <summary>
        /// Scales the specified points around the specified point.
        /// </summary>
        /// <param name="Points">The points to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Point<T>[] Scale(Point<T>[] Points, T Cx, T Cy, T ScaleX, T ScaleY)
        {
            int Length = Points.Length;
            Point<T>[] Result = new Point<T>[Length];

            T Dx, Dy;
            Point<T> p;
            for (int i = 0; i < Length; i++)
            {
                p = Points[i];
                Dx = Sub(p.X, Cx);
                Dy = Sub(p.Y, Cy);

                Result[i] = new Point<T>(Add(Cx, Mul(Dx, ScaleX)), Add(Cy, Mul(Dy, ScaleY)));
            }

            return Result;
        }
        /// <summary>
        /// Scales the specified points around the specified point.
        /// </summary>
        /// <param name="pPoints">The pointer of the points to be scaled.</param>
        /// <param name="Length">The length of the points to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static void Scale(Point<T>* pPoints, int Length, T Cx, T Cy, T ScaleX, T ScaleY)
        {
            T Dx, Dy;
            for (int i = 0; i < Length; i++)
            {
                Dx = Sub(pPoints->X, Cx);
                Dy = Sub(pPoints->Y, Cy);

                pPoints->X = Add(Cx, Mul(Dx, ScaleX));
                pPoints->Y = Add(Cy, Mul(Dy, ScaleY));

                pPoints++;
            }
        }

        /// <summary>
        /// Offsets the specified line segment by the specified vector.
        /// </summary>
        /// <param name="Segment">The line to be offsetted.</param>
        /// <param name="Vector">The vector to be added to this LineSegment.</param>
        public static LineSegment<T> Offset(LineSegment<T> Segment, Vector<T> Vector)
            => Offset(Segment, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified line segment by the specified amounts.
        /// </summary>
        /// <param name="Segment">The line to be offsetted.</param>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public static LineSegment<T> Offset(LineSegment<T> Segment, T Dx, T Dy)
            => Segment.IsEmpty ? Empty : new LineSegment<T> { Points = Point<T>.Offset(Segment.Points, Dx, Dy) };

        /// <summary>
        /// Rotates the specified line segment about the origin.
        /// </summary>
        /// <param name="Segment">The line to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static LineSegment<T> Rotate(LineSegment<T> Segment, double Theta)
            => Segment.IsEmpty ? Empty : new LineSegment<T> { Points = Point<T>.Rotate(Segment.Points, Theta) };
        /// <summary>
        /// Rotates the specified line segment about the specified point.
        /// </summary>
        /// <param name="Segment">The line to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static LineSegment<T> Rotate(LineSegment<T> Segment, Point<T> Center, double Theta)
            => Rotate(Segment, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified line segment about the specified point(<paramref name="Cx"/>, <paramref name="Cy"/>).
        /// </summary>
        /// <param name="Segment">The line to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static LineSegment<T> Rotate(LineSegment<T> Segment, T Cx, T Cy, double Theta)
            => Segment.IsEmpty ? Empty : new LineSegment<T> { Points = Point<T>.Rotate(Segment.Points, Cx, Cy, Theta) };

        /// <summary>
        /// Reflects the specified line segment over the specified line.
        /// </summary>
        /// <param name="Segment">The line to be reflects.</param>
        /// <param name="ProjectionLine">The projection line.</param>
        public static LineSegment<T> Reflect(LineSegment<T> Segment, Line<T> ProjectionLine)
        {
            if (ProjectionLine.Points is null || ProjectionLine.Points.Length < 2)
                return Segment.Clone();

            Point<T> P1 = ProjectionLine.Points[0],
                     P2 = ProjectionLine.Points[1];

            return Reflect(Segment, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified line segment over the specified line.
        /// </summary>
        /// <param name="Segment">The line to be reflects.</param>
        /// <param name="LinePoint1">The line on the projection line.</param>
        /// <param name="LinePoint2">The another line on the projection line.</param>
        public static LineSegment<T> Reflect(LineSegment<T> Segment, Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(Segment, LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        /// <summary>
        /// Reflects the specified line segment over the specified line.
        /// </summary>
        /// <param name="Segment">The line to be reflects.</param>
        /// <param name="Lx1">The x-coordinate of a line on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a line on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another line on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another line on the projection line.</param>
        public static LineSegment<T> Reflect(LineSegment<T> Segment, T Lx1, T Ly1, T Lx2, T Ly2)
            => Segment.IsEmpty ? Empty : new LineSegment<T> { Points = Point<T>.Reflect(Segment.Points, Lx1, Ly1, Lx2, Ly2) };

        /// <summary>
        /// Adds the specified vector to the specified line segment.
        /// </summary>
        /// <param name="Segment">The line structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static LineSegment<T> operator +(LineSegment<T> Segment, Vector<T> Vector)
            => Offset(Segment, Vector);
        /// <summary>
        /// Subtracts the specified vector from the specified line segment.
        /// </summary>
        /// <param name="Segment">The line from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from line.</param>
        public static LineSegment<T> operator -(LineSegment<T> Segment, Vector<T> Vector)
            => Offset(Segment, -Vector);

        /// <summary>
        /// Multiplies the specified line segment by the specified scalar.
        /// </summary>
        /// <param name="Segment">The line segment to multiply.</param>
        /// <param name="Scale">The scale factor.</param>
        public static LineSegment<T> operator *(LineSegment<T> Segment, T Scale)
            => LineSegment<T>.Scale(Segment, Scale, Scale);
        /// <summary>
        /// Divides the specified line Segment by the specified scalar.
        /// </summary>
        /// <param name="Segment">The line segment to divide.</param>
        /// <param name="Scale">The scalar to divide.</param>
        /// <returns></returns>
        public static LineSegment<T> operator /(LineSegment<T> Segment, T Scale)
        {
            if (Segment.IsEmpty)
                return Empty;

            Point<T> p0 = Segment.Points[0],
                     p1 = Segment.Points[1];
            T X0 = p0.X,
              Y0 = p0.Y,
              Dx = Sub(p1.X, X0),
              Dy = Sub(p1.Y, Y0);

            p1.X = Add(X0, Div(Dx, Scale));
            p1.Y = Add(Y0, Div(Dy, Scale));

            return new LineSegment<T>(p0, p1);
        }

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="Segment1">The first line to compare.</param>
        /// <param name="Segment2">The second line to compare.</param>
        public static bool operator ==(LineSegment<T> Segment1, LineSegment<T> Segment2)
            => Segment1.Equals(Segment2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="Segment1">The first line to compare.</param>
        /// <param name="Segment2">The second line to compare.</param>
        public static bool operator !=(LineSegment<T> Segment1, LineSegment<T> Segment2)
            => !Segment1.Equals(Segment2);

    }
}