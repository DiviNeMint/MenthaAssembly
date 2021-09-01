using System;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a line in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct Line<T> : IShape<T>
        where T : unmanaged
    {
        /// <summary>
        /// Gets a special value that represents a line with no position.
        /// </summary>
        public static Line<T> Empty => new();

        /// <summary>
        /// Gets a special value that represents X-Axis.
        /// </summary>
        public static Line<T> XAxis => new() { Points = new[] { new Point<T>(), new Point<T>(ToGeneric(1), default) } };

        /// <summary>
        /// Gets a special value that represents Y-Axis.
        /// </summary>
        public static Line<T> YAxis => new() { Points = new[] { new Point<T>(), new Point<T>(default, ToGeneric(1)) } };

        Point<T> IShape<T>.Center
            => this.IsEmpty ? default : Points[0];

        double IShape<T>.Area
            => 0d;

        internal Point<T>[] Points;

        /// <summary>
        /// The normal vector of this line.
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
        /// The directional vector of this line.
        /// </summary>
        public Vector<T> DirectionalVector
            => this.IsEmpty ? Vector<T>.Zero : new Vector<T>(Points[0], Points[1]);

        /// <summary>
        /// Gets a value that indicates whether the shape is the empty line.
        /// </summary>
        public bool IsEmpty
            => Points is null || Points.Length < 2 || Points[0].Equals(Points[1]);

        /// <summary>
        /// The gradient of the line.
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
        /// Initializes a new instance of the <see cref="Line{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new line must contain.</param>
        /// <param name="Point2">The second point that the new line must contain.</param>
        public Line(Point<T> Point1, Point<T> Point2)
        {
            if (Point1.Equals(Point2))
            {
                this = Empty;
                return;
            }

            Points = new[] { Point1, Point2 };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Line{T}"/> structure.
        /// </summary>
        /// <param name="Point">The point that the new line must contain.</param>
        /// <param name="Vector">The directional vector of the new line.</param>
        public Line(Point<T> Point, Vector<T> Vector)
        {
            if (Vector.IsZero)
            {
                this = Empty;
                return;
            }

            Points = new[] { Point, Point<T>.Offset(Point, Vector) };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Line{T}"/> structure.
        /// </summary>        
        /// <param name="Px1">The x-coordinate of the first point that the new line must contain.</param>
        /// <param name="Py1">The y-coordinate of the first point that the new line must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new line must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new line must contain.</param>
        public Line(T Px1, T Py1, T Px2, T Py2)
        {
            if (Px1.Equals(Px2) && Py1.Equals(Py2))
            {
                this = Empty;
                return;
            }

            Points = new[] { new Point<T>(Px1, Py1), new Point<T>(Px2, Py2) };
        }

        public bool Contain(Point<T> Point)
            => this.Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
            => Points is not null && IsCollinear(Points[0], Points[1], Px, Py);

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

        void IShape<T>.Scale(T Scale) { }
        void IShape<T>.Scale(T ScaleX, T ScaleY) { }
        void IShape<T>.Scale(Point<T> Center, T Scale) { }
        void IShape<T>.Scale(Point<T> Center, T ScaleX, T ScaleY) { }
        void IShape<T>.Scale(T Cx, T Cy, T Scale) { }
        void IShape<T>.Scale(T Cx, T Cy, T ScaleX, T ScaleY) { }

        /// <summary>
        /// Creates a new casted line.
        /// </summary>
        public Line<U> Cast<U>() where U : unmanaged
            => this.IsEmpty ? Line<U>.Empty : new Line<U>(Points[0].Cast<U>(), Points[1].Cast<U>());
        IShape<U> IShape<T>.Cast<U>()
            => this.Cast<U>();
        IMathObject<U> IMathObject<T>.Cast<U>()
            => this.Cast<U>();

        /// <summary>
        /// Creates a new line that is a copy of the current instance.
        /// </summary>
        public Line<T> Clone()
            => this.IsEmpty ? Empty : new Line<T>(Points[0], Points[1]);
        IShape<T> IShape<T>.Clone()
            => this.Clone();
        IMathObject<T> IMathObject<T>.Clone()
            => this.Clone();
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => (Points is null || Points.Length < 2) ? base.GetHashCode() : Points[0].GetHashCode() ^ Points[1].GetHashCode();

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Line{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Line<T> obj)
            => this.IsEmpty ? obj.IsEmpty :
                              !obj.IsEmpty && IsCollinear(Points[0], Points[1], obj.Points[0], obj.Points[1]);
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is Line<T> Target && this.Equals(Target);
        bool IMathObject<T>.Equals(IMathObject<T> obj)
            => obj is Line<T> Target && this.Equals(Target);
        public override bool Equals(object obj)
            => obj is Line<T> Target && this.Equals(Target);

        public override string ToString()
        {
            if (this.IsEmpty)
                return $"{nameof(Line<T>)}<{typeof(T).Name}>.Empty";

            Point<T> S = Points[0],
                     E = Points[1];

            return $"Sx : {S.X}, Sy : {S.Y}, Ex : {E.X}, Ey : {E.Y}";
        }

        private static readonly Func<T, T, T> Add, Sub, Mul;
        private static readonly Func<T, T> Abs, Div2;
        private static readonly Func<T, T, bool> GreaterThan;
        private static readonly Predicate<T> IsDefault;
        private static readonly Func<T, double> ToDouble;
        private static readonly Func<double, T> ToGeneric;
        static Line()
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

        /// <summary>
        /// Calculate the perpendicular bisector of two points.
        /// </summary>
        /// <param name="Point">The first target point.</param>
        /// <param name="Px">The x-coordinate of the second target point.</param>
        /// <param name="Py">The y-coordinate of the second target point.</param>
        public static Line<T> PerpendicularBisector(Point<T> Point, T Px, T Py)
            => PerpendicularBisector(Point.X, Point.Y, Px, Py);
        /// <summary>
        /// Calculate the perpendicular bisector of two points.
        /// </summary>
        /// <param name="Point1">The first target point.</param>
        /// <param name="Point2">the second target point.</param>
        public static Line<T> PerpendicularBisector(Point<T> Point1, Point<T> Point2)
            => PerpendicularBisector(Point1.X, Point1.Y, Point2.X, Point2.Y);
        /// <summary>
        /// Calculate the perpendicular bisector of two points.
        /// </summary>
        /// <param name="Px">The x-coordinate of the first target point.</param>
        /// <param name="Py">The y-coordinate of the first target point.</param>
        /// <param name="Qx">The x-coordinate of the second target point.</param>
        /// <param name="Qy">The y-coordinate of the second target point.</param>
        public static Line<T> PerpendicularBisector(T Px, T Py, T Qx, T Qy)
        {
            T Dx = Sub(Qx, Px),
              Dy = Sub(Qy, Py);

            if (IsDefault(Dx) && IsDefault(Dy))
                return Empty;

            T Mx = Div2(Dx),
              My = Div2(Dy);

            return new Line<T>(Mx, My, Sub(Mx, Dy), Add(My, Dx));
        }

        /// <summary>
        /// Calculate the perpendicular from the specified point to the specified line.
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Px">The x-coordinate of the target point.</param>
        /// <param name="Py">The y-coordinate of the target point.</param>
        public static Line<T> Perpendicular(Line<T> Line, T Px, T Py)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return Empty;

            Point<T> p0 = Line.Points[0],
                     p1 = Line.Points[1];

            return Perpendicular(p0.X, p0.Y, p1.X, p1.Y, Px, Py);
        }
        /// <summary>
        /// Calculate the perpendicular from the specified point to the specified line.
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Point"></param>
        public static Line<T> Perpendicular(Line<T> Line, Point<T> Point)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return Empty;

            Point<T> p0 = Line.Points[0],
                     p1 = Line.Points[1];

            return Perpendicular(p0.X, p0.Y, p1.X, p1.Y, Point.X, Point.Y);
        }
        /// <summary>
        /// Calculate the perpendicular from the specified point to the specified line.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line.</param>
        /// <param name="LinePoint2">The another point on the target line.</param>
        /// <param name="Px">The x-coordinate of the target point.</param>
        /// <param name="Py">The y-coordinate of the target point.</param>
        public static Line<T> Perpendicular(Point<T> LinePoint1, Point<T> LinePoint2, T Px, T Py)
            => Perpendicular(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y, Px, Py);
        /// <summary>
        /// Calculate the perpendicular from the specified point to the specified line.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line.</param>
        /// <param name="LinePoint2">The another point on the target line.</param>
        /// <param name="Point">The target point.</param>
        public static Line<T> Perpendicular(Point<T> LinePoint1, Point<T> LinePoint2, Point<T> Point)
            => Perpendicular(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y, Point.X, Point.Y);
        /// <summary>
        /// Calculate the perpendicular from the specified point to the specified line.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the line.</param>
        /// <param name="Px">The x-coordinate of the target point.</param>
        /// <param name="Py">The y-coordinate of the target point.</param>
        public static Line<T> Perpendicular(T Lx1, T Ly1, T Lx2, T Ly2, T Px, T Py)
        {
            T Dx = Sub(Lx2, Lx1),
              Dy = Sub(Ly2, Ly1);

            return IsDefault(Dx) && IsDefault(Dy) ? Empty : new Line<T>(Px, Py, Sub(Px, Dy), Add(Py, Dx));
        }

        /// <summary>
        /// Calculate the distance from a point to a line.
        /// </summary>
        /// <param name="Line">The target line.</param>
        /// <param name="Px">The x-coordinate of the target point.</param>
        /// <param name="Py">The y-coordinate of the target point.</param>
        public static double Distance(Line<T> Line, T Px, T Py)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return double.NaN;

            Point<T> p0 = Line.Points[0],
                     p1 = Line.Points[1];

            return Distance(p0.X, p0.Y, p1.X, p1.Y, Px, Py);
        }
        /// <summary>
        /// Calculate the distance from a point to a line.
        /// </summary>
        /// <param name="Line">The target line.</param>
        /// <param name="Point">The target point.</param>
        public static double Distance(Line<T> Line, Point<T> Point)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return double.NaN;

            Point<T> p0 = Line.Points[0],
                     p1 = Line.Points[1];

            return Distance(p0.X, p0.Y, p1.X, p1.Y, Point.X, Point.Y);
        }
        /// <summary>
        /// Calculate the distance from a point to a line.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the line.</param>
        /// <param name="Px">The x-coordinate of the target point.</param>
        /// <param name="Py">The y-coordinate of the target point.</param>
        public static double Distance(T Lx1, T Ly1, T Lx2, T Ly2, T Px, T Py)
        {
            T v1x = Sub(Lx2, Lx1),
              v1y = Sub(Ly2, Ly1),
              v2x = Sub(Px, Lx1),
              v2y = Sub(Py, Ly1);

            double v2L = Math.Sqrt(ToDouble(Add(Mul(v2x, v2x), Mul(v2y, v2y))));
            return IsDefault(v1x) && IsDefault(v1y) ? v2L : ToDouble(Abs(Vector<T>.Cross(v1x, v1y, v2x, v2y))) / v2L;
        }
        /// <summary>
        /// Calculate the distance between two lines.
        /// </summary>
        /// <param name="Line1">The first target line.</param>
        /// <param name="Line2">the second target line.</param>
        public static double Distance(Line<T> Line1, Line<T> Line2)
        {
            if (Line1.Points is null || Line1.Points.Length < 2 ||
                Line2.Points is null || Line2.Points.Length < 2)
                return double.NaN;

            Point<T> p1 = Line1.Points[0],
                     p2 = Line1.Points[1],
                     p3 = Line2.Points[0],
                     p4 = Line2.Points[1];

            return Distance(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }
        /// <summary>
        /// Calculate the distance between two lines.
        /// </summary>
        /// <param name="Line">The first target line.</param>
        /// <param name="Lx1">The x-coordinate of a point on the second target line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the second target line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the second target line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the second target line.</param>
        public static double Distance(Line<T> Line, T Lx1, T Ly1, T Lx2, T Ly2)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return double.NaN;

            Point<T> p1 = Line.Points[0],
                     p2 = Line.Points[1];

            return Distance(p1.X, p1.Y, p2.X, p2.Y, Lx1, Ly1, Lx2, Ly2);
        }
        /// <summary>
        /// Calculate the distance between two lines.
        /// </summary>
        /// <param name="L1P1">The point on the first target line.</param>
        /// <param name="L1P2">The another point on the first target line.</param>
        /// <param name="L2P1">The point on the second target line.</param>
        /// <param name="L2P2">The another point on the second target line.</param>
        public static double Distance(Point<T> L1P1, Point<T> L1P2, Point<T> L2P1, Point<T> L2P2)
            => Distance(L1P1.X, L1P1.Y, L1P2.X, L1P2.Y, L2P1.X, L2P1.Y, L2P2.X, L2P2.Y);
        /// <summary>
        /// Calculate the distance between two lines.
        /// </summary>
        /// <param name="L1x1">The x-coordinate of a point on the first target line.</param>
        /// <param name="L1y1">The y-coordinate of a point on the first target line.</param>
        /// <param name="L1x2">The x-coordinate of a another point on the first target line.</param>
        /// <param name="L1y2">The y-coordinate of a another point on the first target line.</param>
        /// <param name="L2x1">The x-coordinate of a point on the second target line.</param>
        /// <param name="L2y1">The y-coordinate of a point on the second target line.</param>
        /// <param name="L2x2">The x-coordinate of a another point on the second target line.</param>
        /// <param name="L2y2">The y-coordinate of a another point on the second target line.</param>
        public static double Distance(T L1x1, T L1y1, T L1x2, T L1y2, T L2x1, T L2y1, T L2x2, T L2y2)
        {
            T v1x = Sub(L1x2, L1x1),
              v1y = Sub(L1y2, L1y1);

            if (IsDefault(v1x) && IsDefault(v1y))
                return Distance(L2x1, L2y1, L2x2, L2y2, L1x1, L1y1);

            T v2x = Sub(L2x2, L2x1),
              v2y = Sub(L2y2, L2y1);

            // Check if two lines intersect
            if (!IsDefault(Vector<T>.Cross(v1x, v1y, v2x, v2y)))
                return 0d;

            v2x = Sub(L2x2, L1x1);
            v2y = Sub(L2y2, L1y1);

            return ToDouble(Abs(Vector<T>.Cross(v1x, v1y, v2x, v2y))) / Math.Sqrt(ToDouble(Add(Mul(v2x, v2x), Mul(v2y, v2y))));
        }

        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="Point1">The first target point.</param>
        /// <param name="Px2">The x-coordinate of the second target point.</param>
        /// <param name="Py2">The y-coordinate of the second target point.</param>
        /// <param name="Px3">The x-coordinate of the third target point.</param>
        /// <param name="Py3">The y-coordinate of the third target point.</param>
        public static bool IsCollinear(Point<T> Point1, T Px2, T Py2, T Px3, T Py3)
            => IsCollinear(Point1.X, Point1.Y, Px2, Py2, Px3, Py3);
        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="Point1">The first target point.</param>
        /// <param name="Point2">The second target point.</param>
        /// <param name="Px3">The x-coordinate of the third target point.</param>
        /// <param name="Py3">The y-coordinate of the third target point.</param>
        public static bool IsCollinear(Point<T> Point1, Point<T> Point2, T Px3, T Py3)
            => IsCollinear(Point1.X, Point1.Y, Point2.X, Point2.Y, Px3, Py3);
        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="Point1">The first target point.</param>
        /// <param name="Point2">The second target point.</param>
        /// <param name="Point3">The third target point.</param>
        public static bool IsCollinear(Point<T> Point1, Point<T> Point2, Point<T> Point3)
            => IsCollinear(Point1.X, Point1.Y, Point2.X, Point2.Y, Point3.X, Point3.Y);
        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="Px1">The x-coordinate of the first target point.</param>
        /// <param name="Py1">The y-coordinate of the first target point.</param>
        /// <param name="Px2">The x-coordinate of the second target point.</param>
        /// <param name="Py2">The y-coordinate of the second target point.</param>
        /// <param name="Px3">The x-coordinate of the third target point.</param>
        /// <param name="Py3">The y-coordinate of the third target point.</param>
        public static bool IsCollinear(T Px1, T Py1, T Px2, T Py2, T Px3, T Py3)
        {
            // Build line equation
            // aX + bY + c = 0

            T a = Sub(Py2, Py1),
              b = Sub(Px1, Px2);

            if (IsDefault(a) && IsDefault(b))
                return true;

            T c = Sub(Mul(Px2, Py1), Mul(Py2, Px1));

            return IsDefault(Add(Add(Mul(a, Px3), Mul(b, Py3)), c));

            //T Dx = Sub(Px2, Px1),
            //  Dy = Sub(Py2, Py1);

            //if (IsDefault(Dx) && IsDefault(Dx))
            //    return true;

            //T v2x = Sub(Px3, Px1),
            //  v2y = Sub(Py3, Py1);

            //if (IsDefault(v2x) && IsDefault(v2y))
            //    return true;

            //return IsDefault(Vector<T>.Cross(Dx, Dy, v2x, v2y));
        }
        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="Point1">The first target point.</param>
        /// <param name="Point2">The second target point.</param>
        /// <param name="Point3">The third target point.</param>
        /// <param name="Point4">The fourth target point.</param>
        public static bool IsCollinear(Point<T> Point1, Point<T> Point2, Point<T> Point3, Point<T> Point4)
            => IsCollinear(Point1.X, Point1.Y, Point2.X, Point2.Y, Point3.X, Point3.Y, Point4.X, Point4.Y);
        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="Px1">The x-coordinate of the first target point.</param>
        /// <param name="Py1">The y-coordinate of the first target point.</param>
        /// <param name="Px2">The x-coordinate of the second target point.</param>
        /// <param name="Py2">The y-coordinate of the second target point.</param>
        /// <param name="Px3">The x-coordinate of the third target point.</param>
        /// <param name="Py3">The y-coordinate of the third target point.</param>
        /// <param name="Px4">The x-coordinate of the fourth target point.</param>
        /// <param name="Py4">The y-coordinate of the fourth target point.</param>
        public static bool IsCollinear(T Px1, T Py1, T Px2, T Py2, T Px3, T Py3, T Px4, T Py4)
        {
            // Build line equation
            // aX + bY + c = 0

            T a = Sub(Py2, Py1),
              b = Sub(Px1, Px2);

            if (IsDefault(a) && IsDefault(b))
                return IsCollinear(Px2, Py2, Px3, Py3, Px4, Py4);

            T c = Sub(Mul(Px2, Py1), Mul(Py2, Px1));

            return IsDefault(Add(Add(Mul(a, Px3), Mul(b, Py3)), c)) &&
                   IsDefault(Add(Add(Mul(a, Px4), Mul(b, Py4)), c));

            //T Dx = Sub(Px2, Px1),
            //  Dy = Sub(Py2, Py1);

            //if (IsDefault(Dx) && IsDefault(Dx))
            //    return IsCollinear(Px2, Py2, Px3, Py3, Px4, Py4);

            //T v2x = Sub(Px3, Px1),
            //  v2y = Sub(Py3, Py1);

            //if (IsDefault(v2x) && IsDefault(v2y) ||
            //    IsDefault(Vector<T>.Cross(Dx, Dy, v2x, v2y)))
            //{
            //    v2x = Sub(Px4, Px1);
            //    v2y = Sub(Py4, Py1);

            //    if (IsDefault(v2x) && IsDefault(v2y))
            //        return true;

            //    return IsDefault(Vector<T>.Cross(Dx, Dy, v2x, v2y));
            //}

            //return false;
        }
        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="Points">The target points.</param>
        public static bool IsCollinear(params Point<T>[] Points)
        {
            int Length = Points.Length;
            if (Length < 3)
                return true;

            // Build line equation
            // aX + bY + c = 0

            Point<T> p0 = Points[0], p1;
            T Px1 = p0.X,
              Py1 = p0.Y,
              Px2, Py2, a, b;

            int i = 1;
            for (; i < Length; i++)
            {
                p1 = Points[i];
                Px2 = p1.X;
                Py2 = p1.Y;

                a = Sub(Py2, Py1);
                b = Sub(Px1, Px2);

                if (IsDefault(a) && IsDefault(b))
                    continue;

                T c = Sub(Mul(Px2, Py1), Mul(Py2, Px1));

                // Check on line
                for (; i < Length; i++)
                {
                    p1 = Points[i];

                    if (!IsDefault(Add(Add(Mul(a, p1.X), Mul(b, p1.Y)), c)))
                        return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Returns a value indicating whether the points are collinear.
        /// </summary>
        /// <param name="PointDatas">The target points in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        public static bool IsCollinear(params T[] PointDatas)
        {
            // Valid Point Length
            int Length = PointDatas.Length >> 1;
            if (Length < 3)
                return true;

            // Data Length
            Length <<= 1;

            // Build line equation
            // aX + bY + c = 0
            T Px1 = PointDatas[0],
              Py1 = PointDatas[1],
              Px2, Py2, a, b;

            int i = 2;
            for (; i < Length; i++)
            {
                Px2 = PointDatas[i++];
                Py2 = PointDatas[i];

                a = Sub(Py2, Py1);
                b = Sub(Px1, Px2);

                if (IsDefault(a) && IsDefault(b))
                    continue;

                T c = Sub(Mul(Px2, Py1), Mul(Py2, Px1));

                // Check on line
                for (; i < Length; i++)
                {
                    Px2 = PointDatas[i++];
                    Py2 = PointDatas[i];

                    if (!IsDefault(Add(Add(Mul(a, Px2), Mul(b, Py2)), c)))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="Line">The target line.</param>
        /// <param name="Px1">The x-coordinate of the first target point.</param>
        /// <param name="Py1">The y-coordinate of the first target point.</param>
        /// <param name="Px2">The x-coordinate of the second target point.</param>
        /// <param name="Py2">The y-coordinate of the second target point.</param>
        public static bool IsSameSide(Line<T> Line, T Px1, T Py1, T Px2, T Py2)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return false;

            Point<T> p0 = Line.Points[0],
                     p1 = Line.Points[1];

            return IsSameSide(p0.X, p0.Y, p1.X, p1.Y, Px1, Py1, Px2, Py2);
        }
        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="Line">The target line.</param>
        /// <param name="Points">The target points.</param>
        public static bool IsSameSide(Line<T> Line, params Point<T>[] Points)
            => Line.Points is not null && Line.Points.Length > 1 && IsSameSide(Line.Points[0], Line.Points[1], Points);
        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="Line">The target line.</param>
        /// <param name="PointDatas">The target points in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        public static bool IsSameSide(Line<T> Line, params T[] PointDatas)
            => Line.Points is not null && Line.Points.Length > 1 && IsSameSide(Line.Points[0], Line.Points[1], PointDatas);
        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line.</param>
        /// <param name="LinePoint2">The another point on the target line.</param>
        /// <param name="Px1">The x-coordinate of the first target point.</param>
        /// <param name="Py1">The y-coordinate of the first target point.</param>
        /// <param name="Px2">The x-coordinate of the second target point.</param>
        /// <param name="Py2">The y-coordinate of the second target point.</param>
        public static bool IsSameSide(Point<T> LinePoint1, Point<T> LinePoint2, T Px1, T Py1, T Px2, T Py2)
            => IsSameSide(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y, Px1, Py1, Px2, Py2);
        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line.</param>
        /// <param name="LinePoint2">The another point on the target line.</param>
        /// <param name="Points">The target points.</param>
        public static bool IsSameSide(Point<T> LinePoint1, Point<T> LinePoint2, params Point<T>[] Points)
        {
            // Valid Point Length
            int Length = Points.Length;
            if (Length < 2)
                return true;

            // Build line equation
            // aX + bY + c = 0

            T Lx1 = LinePoint1.X,
              Ly1 = LinePoint1.Y,
              Lx2 = LinePoint2.X,
              Ly2 = LinePoint2.Y;

            T a = Sub(Ly2, Ly1),
              b = Sub(Lx1, Lx2);

            if (IsDefault(a) && IsDefault(b))
                return false;

            Point<T> p = Points[0];
            T c = Sub(Mul(Lx2, Ly1), Mul(Ly2, Lx1)),
              D = Add(Add(Mul(a, p.X), Mul(b, p.Y)), c);

            if (IsDefault(D))
                return false;

            T Zero = default;

            bool Predict = GreaterThan(D, Zero);
            for (int i = 1; i < Length; i++)
            {
                p = Points[i];
                D = Add(Add(Mul(a, p.X), Mul(b, p.Y)), c);

                if (Predict != GreaterThan(D, Zero))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line.</param>
        /// <param name="LinePoint2">The another point on the target line.</param>
        /// <param name="PointDatas">The target points in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        public static bool IsSameSide(Point<T> LinePoint1, Point<T> LinePoint2, params T[] PointDatas)
            => IsSameSide(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y, PointDatas);
        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the target line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the target line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the target line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the target line.</param>
        /// <param name="Px1">The x-coordinate of the first target point.</param>
        /// <param name="Py1">The y-coordinate of the first target point.</param>
        /// <param name="Px2">The x-coordinate of the second target point.</param>
        /// <param name="Py2">The y-coordinate of the second target point.</param>
        public static bool IsSameSide(T Lx1, T Ly1, T Lx2, T Ly2, T Px1, T Py1, T Px2, T Py2)
        {
            // Build line equation
            // aX + bY + c = 0

            T a = Sub(Ly2, Ly1),
              b = Sub(Lx1, Lx2);

            if (IsDefault(a) && IsDefault(b))
                return false;

            T c = Sub(Mul(Lx2, Ly1), Mul(Ly2, Lx1)),
              F0 = Add(Add(Mul(a, Px1), Mul(b, Py1)), c);

            if (IsDefault(F0))
                return false;

            T F1 = Add(Add(Mul(a, Px2), Mul(b, Py2)), c);
            if (IsDefault(F1))
                return false;

            T Zero = default;
            return GreaterThan(F0, Zero) == GreaterThan(F1, Zero);
        }
        /// <summary>
        /// Returns a value indicating whether the points are same side.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the target line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the target line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the target line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the target line.</param>
        /// <param name="PointDatas">The target points in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        public static bool IsSameSide(T Lx1, T Ly1, T Lx2, T Ly2, params T[] PointDatas)
        {
            // Valid Point Length
            int Length = PointDatas.Length >> 1;
            if (Length < 2)
                return true;

            // Data Length
            Length <<= 1;

            // Build line equation
            // aX + bY + c = 0

            T a = Sub(Ly2, Ly1),
              b = Sub(Lx1, Lx2);

            if (IsDefault(a) && IsDefault(b))
                return false;

            T c = Sub(Mul(Lx2, Ly1), Mul(Ly2, Lx1)),
              D = Add(Add(Mul(a, PointDatas[0]), Mul(b, PointDatas[1])), c);

            if (IsDefault(D))
                return false;

            T Zero = default;
            bool Predict = GreaterThan(D, Zero);

            for (int i = 2; i < Length; i++)
            {
                D = Add(Add(Mul(a, PointDatas[i++]), Mul(b, PointDatas[i])), c);

                if (Predict != GreaterThan(D, Zero))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate the cross points between two lines.
        /// </summary>
        /// <param name="Line1">The first target line.</param>
        /// <param name="Line2">the second target line.</param>
        public static CrossPoints<T> CrossPoint(Line<T> Line1, Line<T> Line2)
        {
            if (Line1.Points is null || Line1.Points.Length < 2 ||
                Line2.Points is null || Line2.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> p1 = Line1.Points[0],
                     p2 = Line1.Points[1],
                     p3 = Line2.Points[0],
                     p4 = Line2.Points[1];

            return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }
        /// <summary>
        /// Calculate the cross points between two lines.
        /// </summary>
        /// <param name="Line">The first target line.</param>
        /// <param name="Lx1">The x-coordinate of a point on the second target line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the second target line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the second target line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the second target line.</param>
        public static CrossPoints<T> CrossPoint(Line<T> Line, T Lx1, T Ly1, T Lx2, T Ly2)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> p1 = Line.Points[0],
                     p2 = Line.Points[1];

            return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, Lx1, Ly1, Lx2, Ly2);
        }
        /// <summary>
        /// Calculate the cross points between two lines.
        /// </summary>
        /// <param name="L1P1">The point on the first target line.</param>
        /// <param name="L1P2">The another point on the first target line.</param>
        /// <param name="L2P1">The point on the second target line.</param>
        /// <param name="L2P2">The another point on the second target line.</param>
        public static CrossPoints<T> CrossPoint(Point<T> L1P1, Point<T> L1P2, Point<T> L2P1, Point<T> L2P2)
            => CrossPoint(L1P1.X, L1P1.Y, L1P2.X, L1P2.Y, L2P1.X, L2P1.Y, L2P2.X, L2P2.Y);
        /// <summary>
        /// Calculate the cross points between two lines.
        /// </summary>
        /// <param name="L1x1">The x-coordinate of a point on the first target line.</param>
        /// <param name="L1y1">The y-coordinate of a point on the first target line.</param>
        /// <param name="L1x2">The x-coordinate of a another point on the first target line.</param>
        /// <param name="L1y2">The y-coordinate of a another point on the first target line.</param>
        /// <param name="L2x1">The x-coordinate of a point on the second target line.</param>
        /// <param name="L2y1">The y-coordinate of a point on the second target line.</param>
        /// <param name="L2x2">The x-coordinate of a another point on the second target line.</param>
        /// <param name="L2y2">The y-coordinate of a another point on the second target line.</param>
        public static CrossPoints<T> CrossPoint(T L1x1, T L1y1, T L1x2, T L1y2, T L2x1, T L2y1, T L2x2, T L2y2)
        {
            T v1x = Sub(L1x2, L1x1),
              v1y = Sub(L1y2, L1y1);

            if (IsDefault(v1x) && IsDefault(v1y))
                return IsCollinear(L1x1, L1y1, L2x1, L2y1, L2x2, L2y2) ? new CrossPoints<T>(new Point<T>(L1x1, L1y1)) : new CrossPoints<T>(false);

            T v2x = Sub(L2x2, L2x1),
              v2y = Sub(L2y2, L2y1),
              v3x = Sub(L2x1, L1x1),
              v3y = Sub(L2y1, L1y1);

            if (IsDefault(v2x) && IsDefault(v2y))
                return (IsDefault(v3x) && IsDefault(v3x)) || IsDefault(Vector<T>.Cross(v1x, v1y, v3x, v3y)) ? new CrossPoints<T>(new Point<T>(L2x1, L2y1)) : new CrossPoints<T>(false);

            T C1 = Vector<T>.Cross(v1x, v1y, v2x, v2y),
              C2 = Vector<T>.Cross(v3x, v3y, v2x, v2y);

            if (IsDefault(C1))
                return new CrossPoints<T>(IsDefault(C2));

            double t = ToDouble(C2) / ToDouble(C1);
            if (t < 0)
                t = -t;

            return new CrossPoints<T>(new Point<T>(Add(L1x1, ToGeneric(ToDouble(v1x) * t)), Add(L1y1, ToGeneric(ToDouble(v1y) * t))));
        }
        /// <summary>
        /// Calculate the cross points between the specified line and the specified line segment.
        /// </summary>
        /// <param name="Line">the target line.</param>
        /// <param name="Segment">The target line segment.</param>
        public static CrossPoints<T> CrossPoint(Line<T> Line, LineSegment<T> Segment)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> p1 = Line.Points[0],
                     p2 = Line.Points[1];

            return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, Segment);
        }
        /// <summary>
        /// Calculate the cross points between the specified line and the specified line segment.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line.</param>
        /// <param name="LinePoint2">The another point on the target line.</param>
        /// <param name="Segment">the target line segment.</param>
        public static CrossPoints<T> CrossPoint(Point<T> LinePoint1, Point<T> LinePoint2, LineSegment<T> Segment)
            => CrossPoint(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y, Segment);
        /// <summary>
        /// Calculate the cross points between the specified line and the specified line segment.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the target line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the target line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the target line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the target line.</param>
        /// <param name="Segment">the target line segment.</param>
        public static CrossPoints<T> CrossPoint(T Lx1, T Ly1, T Lx2, T Ly2, LineSegment<T> Segment)
        {
            if (Segment.Points is null || Segment.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> Sp1 = Segment.Points[0],
                     Sp2 = Segment.Points[1];

            T Sx1 = Sp1.X,
              Sy1 = Sp1.Y,
              Sx2 = Sp2.X,
              Sy2 = Sp2.Y,
              v1x = Sub(Lx2, Lx1),
              v1y = Sub(Ly2, Ly1);

            if (IsDefault(v1x) && IsDefault(v1y))
                return LineSegment<T>.Contain(Sx1, Sy1, Sx2, Sy2, Lx1, Ly1) ? new CrossPoints<T>(new Point<T>(Lx1, Ly1)) : new CrossPoints<T>(false);

            T v2x = Sub(Sx2, Sx1),
              v2y = Sub(Sy2, Sy1);

            if (IsDefault(v2x) && IsDefault(v2y))
                return IsCollinear(Lx1, Ly1, Lx2, Ly2, Sx1, Sy1) ? new CrossPoints<T>(new Point<T>(Sx1, Sy1)) : new CrossPoints<T>(false);

            T v3x = Sub(Sx1, Lx1),
              v3y = Sub(Sy1, Ly1),
              C1 = Vector<T>.Cross(v1x, v1y, v2x, v2y),
              C2 = Vector<T>.Cross(v3x, v3y, v2x, v2y);

            if (IsDefault(C1))
                return new CrossPoints<T>(IsDefault(C2));

            double t = ToDouble(C2) / ToDouble(C1);
            if (t < 0)
                t = -t;

            T X = Add(Lx1, ToGeneric(ToDouble(v1x) * t)),
              Y = Add(Ly1, ToGeneric(ToDouble(v1y) * t));

            return LineSegment<T>.OnSegment(Sx1, Sy1, Sx2, Sy2, X, Y) ? new CrossPoints<T>(new Point<T>(X, Y)) : new CrossPoints<T>(false);
        }
        /// <summary>
        /// Calculate the cross points between the specified line and the specified circle.
        /// </summary>
        /// <param name="Line">the target line.</param>
        /// <param name="Circle">The target circle.</param>
        public static CrossPoints<T> CrossPoint(Line<T> Line, Circle<T> Circle)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return new CrossPoints<T>(false);

            Point<T> p1 = Line.Points[0],
                     p2 = Line.Points[1];

            return CrossPoint(p1.X, p1.Y, p2.X, p2.Y, Circle);
        }
        /// <summary>
        /// Calculate the cross points between the specified line and the specified circle.
        /// </summary>
        /// <param name="LinePoint1">The point on the target line.</param>
        /// <param name="LinePoint2">The another point on the target line.</param>
        /// <param name="Circle">the target circle.</param>
        public static CrossPoints<T> CrossPoint(Point<T> LinePoint1, Point<T> LinePoint2, Circle<T> Circle)
            => CrossPoint(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y, Circle);
        /// <summary>
        /// Calculate the cross points between the specified line and the specified circle.
        /// </summary>
        /// <param name="Lx1">The x-coordinate of a point on the target line.</param>
        /// <param name="Ly1">The y-coordinate of a point on the target line.</param>
        /// <param name="Lx2">The x-coordinate of a another point on the target line.</param>
        /// <param name="Ly2">The y-coordinate of a another point on the target line.</param>
        /// <param name="Circle">the target circle.</param>
        public static CrossPoints<T> CrossPoint(T Lx1, T Ly1, T Lx2, T Ly2, Circle<T> Circle)
        {
            if (Circle.IsEmpty)
                return new CrossPoints<T>(false);

            double Dx = ToDouble(Sub(Lx2, Lx1)),
                   Dy = ToDouble(Sub(Ly2, Ly1));

            if (Dx is 0d && Dy is 0d)
                return new CrossPoints<T>(false);

            // Lx' = Lx1 + Dx * t
            // Ly' = Ly1 + Dy * t

            // Circle Equation
            // X ^ 2 + Y ^ 2 = R ^ 2
            double R = Circle.Radius,
                   DLx = ToDouble(Lx1),
                   DLy = ToDouble(Ly1),
                   a = Dx * Dx + Dy * Dy,
                   b = DLx * Dx + DLy * Dy,
                   c = DLx * DLx + DLy * DLy - R * R,
                   D = b * b - a * c;

            if (D < 0)
                return new CrossPoints<T>(false);

            double t;
            if (D == 0)
            {
                t = -b / a;
                return new CrossPoints<T>(new Point<T>(ToGeneric(DLx + Dx * t), ToGeneric(DLy + Dy * t)));
            }

            Point<T>[] Crosses = new Point<T>[2];

            double SqrD = Math.Sqrt(D);

            t = (-b + SqrD) / a;
            Crosses[0] = new Point<T>(ToGeneric(DLx + Dx * t), ToGeneric(DLy + Dy * t));

            t = (-b - SqrD) / a;
            Crosses[1] = new Point<T>(ToGeneric(DLx + Dx * t), ToGeneric(DLy + Dy * t));

            return new CrossPoints<T>(Crosses);
        }

        /// <summary>
        /// Offsets the specified line by the specified vector.
        /// </summary>
        /// <param name="Line">The line to be offsetted.</param>
        /// <param name="Vector">The vector to be added to this Line.</param>
        public static Line<T> Offset(Line<T> Line, Vector<T> Vector)
            => Offset(Line, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified line by the specified amounts.
        /// </summary>
        /// <param name="Line">The line to be offsetted.</param>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public static Line<T> Offset(Line<T> Line, T Dx, T Dy)
            => Line.IsEmpty ? Empty : new Line<T> { Points = Point<T>.Offset(Line.Points, Dx, Dy) };

        /// <summary>
        /// Rotates the specified line about the origin.
        /// </summary>
        /// <param name="Line">The line to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Line<T> Rotate(Line<T> Line, double Theta)
            => Line.IsEmpty ? Empty : new Line<T> { Points = Point<T>.Rotate(Line.Points, Theta) };
        /// <summary>
        /// Rotates the specified line about the specified point.
        /// </summary>
        /// <param name="Line">The line to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Line<T> Rotate(Line<T> Line, Point<T> Center, double Theta)
            => Rotate(Line, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified line about the specified point(<paramref name="Cx"/>, <paramref name="Cy"/>).
        /// </summary>
        /// <param name="Line">The line to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Line<T> Rotate(Line<T> Line, T Cx, T Cy, double Theta)
            => Line.IsEmpty ? Empty : new Line<T> { Points = Point<T>.Rotate(Line.Points, Cx, Cy, Theta) };

        /// <summary>
        /// Reflects the specified line over the specified line.
        /// </summary>
        /// <param name="Line">The line to be reflects.</param>
        /// <param name="ProjectionLine">The projection line.</param>
        public static Line<T> Reflect(Line<T> Line, Line<T> ProjectionLine)
        {
            if (ProjectionLine.Points is null || ProjectionLine.Points.Length < 2)
                return Line.Clone();

            Point<T> P1 = ProjectionLine.Points[0],
                     P2 = ProjectionLine.Points[1];

            return Reflect(Line, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified line over the specified line.
        /// </summary>
        /// <param name="Line">The line to be reflects.</param>
        /// <param name="LinePoint1">The line on the projection line.</param>
        /// <param name="LinePoint2">The another line on the projection line.</param>
        public static Line<T> Reflect(Line<T> Line, Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(Line, LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        /// <summary>
        /// Reflects the specified line over the specified line.
        /// </summary>
        /// <param name="Line">The line to be reflects.</param>
        /// <param name="Lx1">The x-coordinate of a line on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a line on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another line on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another line on the projection line.</param>
        public static Line<T> Reflect(Line<T> Line, T Lx1, T Ly1, T Lx2, T Ly2)
            => Line.IsEmpty ? Empty : new Line<T> { Points = Point<T>.Reflect(Line.Points, Lx1, Ly1, Lx2, Ly2) };

        /// <summary>
        /// Adds the specified vector to the specified line.
        /// </summary>
        /// <param name="Line">The line structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static Line<T> operator +(Line<T> Line, Vector<T> Vector)
            => Offset(Line, Vector);
        /// <summary>
        /// Subtracts the specified vector from the specified line.
        /// </summary>
        /// <param name="Line">The line from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from line.</param>
        public static Line<T> operator -(Line<T> Line, Vector<T> Vector)
            => Offset(Line, -Vector);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="Line1">The first line to compare.</param>
        /// <param name="Line2">The second line to compare.</param>
        public static bool operator ==(Line<T> Line1, Line<T> Line2)
            => Line1.Equals(Line2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="Line1">The first line to compare.</param>
        /// <param name="Line2">The second line to compare.</param>
        public static bool operator !=(Line<T> Line1, Line<T> Line2)
            => !Line1.Equals(Line2);

    }
}