using System;
using System.Linq;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a polygon in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct Polygon<T> : IPolygonShape<T>
        where T : unmanaged
    {
        /// <summary>
        /// Gets a special value that represents a polygon with no position or area.
        /// </summary>
        public static Polygon<T> Empty => new();

        public Point<T>[] Points { set; get; }

        public bool IsEmpty
            => this.Points is null || Line<T>.IsCollinear(this.Points);

        public Point<T> Center
        {
            get
            {
                if (this.IsEmpty)
                    return new Point<T>();

                Point<T> p = this.Points[0];
                T Cx = p.X,
                  Cy = p.Y;

                int Length = this.Points.Length;
                for (int i = 1; i < Length; i++)
                {
                    p = this.Points[i];
                    Cx = Add(Cx, p.X);
                    Cy = Add(Cy, p.Y);
                }

                return new Point<T>(ToGeneric(ToDouble(Cx) / Length), ToGeneric(ToDouble(Cy) / Length));
            }
        }

        public double Area
        {
            get
            {
                if (this.IsEmpty)
                    return default;

                Point<T> p = this.Points[0];
                T LPx = p.X,
                  LPy = p.Y,
                  Sum = default;

                for (int i = 1; i < this.Points.Length; i++)
                {
                    p = this.Points[i];
                    T CPx = p.X,
                      CPy = p.Y;

                    Sum = Add(Sum, Vector<T>.Cross(LPx, LPy, CPx, CPy));
                    LPx = CPx;
                    LPy = CPy;
                }

                p = this.Points[0];
                Sum = Add(Sum, Vector<T>.Cross(LPx, LPy, p.X, p.Y));

                return ToDouble(Abs(Sum)) / 2d;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon{T}"/> structure.
        /// </summary>
        /// <param name="PointDatas">The target points in x and y pairs that the new polygon must contain, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        public Polygon(params T[] PointDatas) : this(false, PointDatas)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon{T}"/> structure.
        /// </summary>
        /// <param name="Points">The points that the new polygon must contain.</param>
        public Polygon(params Point<T>[] Points) : this(false, Points)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon{T}"/> structure.
        /// </summary>
        /// <param name="Sort">Decides whether the sort the points.</param>
        /// <param name="PointDatas">The target points in x and y pairs that the new polygon must contain, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        public Polygon(bool Sort, params T[] PointDatas)
        {
            int Length = PointDatas.Length >> 1;
            if (Length < 3 || Line<T>.IsCollinear(PointDatas))
            {
                this = Empty;
                return;
            }

            this.Points = new Point<T>[Length];

            Length <<= 1;
            int j = 0;
            for (int i = 0; i < Length; i++)
            {
                this.Points[j] = new Point<T>(PointDatas[i++], PointDatas[i]);
                j++;
            }

            if (Sort)
            {
                fixed (Point<T>* pPoints = &this.Points[0])
                {
                    Point<T>.Sort(pPoints, Length);
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon{T}"/> structure.
        /// </summary>
        /// <param name="Sort">Decides whether the sort the points.</param>
        /// <param name="Points">The points that the new polygon must contain.</param>
        public Polygon(bool Sort, params Point<T>[] Points)
        {
            int Length = Points.Length;
            if (Length < 3 || Line<T>.IsCollinear(Points))
            {
                this = Empty;
                return;
            }

            this.Points = Points;

            if (Sort)
            {
                fixed (Point<T>* pPoints = &this.Points[0])
                {
                    Point<T>.Sort(pPoints, Length);
                }
            }
        }

        public bool Contain(Point<T> Point)
            => this.Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
        {
            if (this.IsEmpty)
                return false;

            bool IsBetweenPoints = false;
            int Length = this.Points.Length;
            Point<T> p = this.Points[0];
            T LPx = p.X,
              LPy = p.Y,
              CPx, CPy,
              Max, Min;

            bool HasCrossPoint()
            {
                // X
                if (GreaterThan(CPx, LPx))
                {
                    Min = LPx;
                    Max = CPx;
                }
                else
                {
                    Min = CPx;
                    Max = LPx;
                }

                // Px < Min
                if (GreaterThan(Min, Px))
                {
                    IsBetweenPoints = true;
                    return false;
                }

                // Px < Max
                if (GreaterThan(Max, Px))
                    IsBetweenPoints = true;

                // Y
                if (GreaterThan(CPy, LPy))
                {
                    Min = LPy;
                    Max = CPy;
                }
                else
                {
                    Min = CPy;
                    Max = LPy;
                }

                // Min <= Py && Py <= Max
                return !(GreaterThan(Min, Py) || GreaterThan(Py, Max));
            }

            int Counter = 0;
            CrossPoints<T> Cross;
            for (int i = 1; i < Length; i++)
            {
                p = this.Points[i];
                CPx = p.X;
                CPy = p.Y;

                if (HasCrossPoint())
                {
                    Cross = LineSegment<T>.CrossPoint(LPx, LPy, CPx, CPy, Min, Py, Px, Py);
                    if (Cross.IsInfinity)
                    {
                        if (LineSegment<T>.OnSegment(LPx, LPy, CPx, CPy, Px, Py))
                            return true;
                    }
                    else if (Cross.Count > 0)
                    {
                        p = Cross.FirstOrDefault();
                        if (Equal(p.X, Px) && Equal(p.Y, Py))
                            return true;

                        if (!(Equal(p.X, CPx) && Equal(p.Y, CPy)))
                            Counter++;
                    }
                }

                LPx = CPx;
                LPy = CPy;
            }

            p = this.Points[0];
            CPx = p.X;
            CPy = p.Y;

            if (HasCrossPoint())
            {
                Cross = LineSegment<T>.CrossPoint(LPx, LPy, CPx, CPy, Min, Py, Px, Py);
                if (Cross.IsInfinity)
                {
                    if (LineSegment<T>.OnSegment(LPx, LPy, CPx, CPy, Px, Py))
                        return true;
                }
                else if (Cross.Count > 0)
                {
                    p = Cross.FirstOrDefault();
                    if (Equal(p.X, Px) && Equal(p.Y, Py))
                        return true;

                    if (!(Equal(p.X, CPx) && Equal(p.Y, CPy)))
                        Counter++;
                }
            }

            return IsBetweenPoints && (Counter & 1) > 0;
        }

        public void Offset(Vector<T> Vector)
            => this.Offset(Vector.X, Vector.Y);
        public void Offset(T Dx, T Dy)
        {
            if (this.IsEmpty)
                return;

            fixed (Point<T>* pPoints = &this.Points[0])
            {
                Point<T>.Offset(pPoints, this.Points.Length, Dx, Dy);
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

            fixed (Point<T>* pPoints = &this.Points[0])
            {
                LineSegment<T>.Scale(pPoints, this.Points.Length, Cx, Cy, ScaleX, ScaleY);
            }
        }

        public void Rotate(double Theta)
        {
            if (this.IsEmpty)
                return;

            fixed (Point<T>* pPoints = &this.Points[0])
            {
                Point<T>.Rotate(pPoints, this.Points.Length, Theta);
            }
        }
        public void Rotate(Point<T> Center, double Theta)
            => this.Rotate(Center.X, Center.Y, Theta);
        public void Rotate(T Cx, T Cy, double Theta)
        {
            if (this.IsEmpty)
                return;

            fixed (Point<T>* pPoints = &this.Points[0])
            {
                Point<T>.Rotate(pPoints, this.Points.Length, Cx, Cy, Theta);
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

            fixed (Point<T>* pPoints = &this.Points[0])
            {
                Point<T>.Reflect(pPoints, this.Points.Length, Lx1, Ly1, Lx2, Ly2);
            }
        }

        /// <summary>
        /// Creates a new casted <see cref="Polygon{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Polygon<U> Cast<U>() where U : unmanaged
            => this.IsEmpty ? Polygon<U>.Empty : new Polygon<U> { Points = this.Points.Select(i => i.Cast<U>()).ToArray() };
        IShape<U> IShape<T>.Cast<U>()
            => this.Cast<U>();
        IMathObject<U> IMathObject<T>.Cast<U>()
            => this.Cast<U>();

        /// <summary>
        /// Creates a new <see cref="Polygon{T}"/> that is a copy of the current instance.
        /// </summary>
        public Polygon<T> Clone()
            => this.IsEmpty ? Empty : new Polygon<T> { Points = new[] { this.Points[0], this.Points[1], this.Points[2], this.Points[3] } };
        IShape<T> IShape<T>.Clone()
            => this.Clone();
        IMathObject<T> IMathObject<T>.Clone()
            => this.Clone();
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
        {
            if (this.Points is null)
                return base.GetHashCode();

            int Code = this.Points[0].GetHashCode();
            for (int i = 1; i < this.Points.Length; i++)
                Code ^= this.Points[i].GetHashCode();

            return Code;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Polygon{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Polygon<T> obj)
        {
            if (this.IsEmpty)
                return obj.IsEmpty;

            if (obj.IsEmpty)
                return false;

            int Index = -1,
                Length = Points.Length;
            Point<T> p = this.Points[0];
            for (int i = 0; i < Length; i++)
            {
                if (p.Equals(obj.Points[i]))
                {
                    Index = i;
                    break;
                }
            }

            if (Index == -1)
                return false;

            int j = 1;
            bool Reverse = false;
            for (int i = Index + 1; j < Length; i++, j++)
            {
                if (i >= Length)
                    i %= Length;

                if (!this.Points[j].Equals(obj.Points[i]))
                {
                    Reverse = true;
                    break;
                }
            }

            if (Reverse)
            {
                for (int i = Index - 1; i >= 0; i--, j++)
                    if (!this.Points[j].Equals(obj.Points[i]))
                        return false;

                for (int i = Length - 1; i > Index; i--, j++)
                    if (!this.Points[j].Equals(obj.Points[i]))
                        return false;
            }

            return true;
        }
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is Polygon<T> Poly && this.Equals(Poly);
        bool IMathObject<T>.Equals(IMathObject<T> obj)
            => obj is Polygon<T> Poly && this.Equals(Poly);
        public override bool Equals(object obj)
            => obj is Polygon<T> Poly && this.Equals(Poly);

        public override string ToString()
            => this.IsEmpty ? $"{nameof(Polygon<T>)}<{typeof(T).Name}>.Empty" :
                              string.Join(", ", this.Points.Select(i => $"{{{i}}}"));

        private static readonly Func<T, T> Abs;
        private static readonly Func<T, T, T> Add, Sub, Mul;
        private static readonly Func<T, T, bool> Equal, GreaterThan;
        private static readonly Func<T, double> ToDouble;
        private static readonly Func<double, T> ToGeneric;
        static Polygon()
        {
            Abs = ExpressionHelper<T>.CreateAbs();

            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();

            Equal = ExpressionHelper<T>.CreateEqual();
            GreaterThan = ExpressionHelper<T>.CreateGreaterThan();

            ToDouble = ExpressionHelper<T>.CreateCast<double>();
            ToGeneric = ExpressionHelper<double>.CreateCast<T>();
        }

        /// <summary>
        /// Offsets the specified polygon's coordinates by the specified vector.
        /// </summary>
        /// <param name="Polygon">The polygon to be offsetted.</param>
        /// <param name="Vector">The vector to be added to the specified polygon.</param>
        public static Polygon<T> Offset(Polygon<T> Polygon, Vector<T> Vector)
            => Offset(Polygon, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified polygon's coordinates by the specified amounts.
        /// </summary>
        /// <param name="Polygon">The polygon to be offsetted.</param>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public static Polygon<T> Offset(Polygon<T> Polygon, T Dx, T Dy)
            => Polygon.IsEmpty ? Empty : new Polygon<T> { Points = Point<T>.Offset(Polygon.Points, Dx, Dy) };

        /// <summary>
        /// Scales the specified polygon around the origin.
        /// </summary>
        /// <param name="Polygon">The polygon to be scaled.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Polygon<T> Scale(Polygon<T> Polygon, T Scale)
            => Polygon<T>.Scale(Polygon, Polygon.Center, Scale);
        /// <summary>
        /// Scales the specified polygon around the origin.
        /// </summary>
        /// <param name="Polygon">The polygon to be scaled.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Polygon<T> Scale(Polygon<T> Polygon, T ScaleX, T ScaleY)
            => Scale(Polygon, Polygon.Center, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified polygon around the specified point.
        /// </summary>
        /// <param name="Polygon">The polygon to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Polygon<T> Scale(Polygon<T> Polygon, Point<T> Center, T Scale)
            => Polygon<T>.Scale(Polygon, Center.X, Center.Y, Scale, Scale);
        /// <summary>
        /// Scales the specified polygon around the specified point.
        /// </summary>
        /// <param name="Polygon">The polygon to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Polygon<T> Scale(Polygon<T> Polygon, Point<T> Center, T ScaleX, T ScaleY)
            => Scale(Polygon, Center.X, Center.Y, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified polygon around the specified point.
        /// </summary>
        /// <param name="Polygon">The polygon to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Polygon<T> Scale(Polygon<T> Polygon, T Cx, T Cy, T Scale)
            => Polygon<T>.Scale(Polygon, Cx, Cy, Scale, Scale);
        /// <summary>
        /// Scales the specified polygon around the specified point.
        /// </summary>
        /// <param name="Polygon">The polygon to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Polygon<T> Scale(Polygon<T> Polygon, T Cx, T Cy, T ScaleX, T ScaleY)
            => Polygon.IsEmpty ? Empty : new Polygon<T> { Points = LineSegment<T>.Scale(Polygon.Points, Cx, Cy, ScaleX, ScaleY) };

        /// <summary>
        /// Rotates the specified polygon about the origin.
        /// </summary>
        /// <param name="Polygon">The polygon to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Polygon<T> Rotate(Polygon<T> Polygon, double Theta)
            => Polygon.IsEmpty ? Empty : new Polygon<T> { Points = Point<T>.Rotate(Polygon.Points, Theta) };
        /// <summary>
        /// Rotates the specified polygon about the specified point.
        /// </summary>
        /// <param name="Polygon">The polygon to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Polygon<T> Rotate(Polygon<T> Polygon, Point<T> Center, double Theta)
            => Rotate(Polygon, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified polygon about the specified point.
        /// </summary>
        /// <param name="Polygon">The polygon to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Polygon<T> Rotate(Polygon<T> Polygon, T Cx, T Cy, double Theta)
            => Polygon.IsEmpty ? Empty : new Polygon<T> { Points = Point<T>.Rotate(Polygon.Points, Cx, Cy, Theta) };

        /// <summary>
        /// Reflects the specified polygon over the specified line.
        /// </summary>
        /// <param name="Polygon">The polygon to be reflects.</param>
        /// <param name="Line">The projection line.</param>
        public static Polygon<T> Reflect(Polygon<T> Polygon, Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return Polygon.Clone();

            Point<T> P1 = Line.Points[0],
                     P2 = Line.Points[1];

            return Reflect(Polygon, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified polygon over the specified line.
        /// </summary>
        /// <param name="Polygon">The polygon to be reflects.</param>
        /// <param name="LinePoint1">The polygon on the projection line.</param>
        /// <param name="LinePoint2">The another polygon on the projection line.</param>
        public static Polygon<T> Reflect(Polygon<T> Polygon, Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(Polygon, LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        /// <summary>
        /// Reflects the specified polygon over the specified line.
        /// </summary>
        /// <param name="Polygon">The polygon to be reflects.</param>
        /// <param name="Lx1">The x-coordinate of a polygon on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a polygon on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another polygon on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another polygon on the projection line.</param>
        public static Polygon<T> Reflect(Polygon<T> Polygon, T Lx1, T Ly1, T Lx2, T Ly2)
            => Polygon.IsEmpty ? Empty : new Polygon<T> { Points = Point<T>.Reflect(Polygon.Points, Lx1, Ly1, Lx2, Ly2) };

        /// <summary>
        /// Adds the specified vector to the specified polygon.
        /// </summary>
        /// <param name="Polygon">The polygon structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static Polygon<T> operator +(Polygon<T> Polygon, Vector<T> Vector)
            => Offset(Polygon, Vector);
        /// <summary>
        /// Subtracts the specified vector from the specified polygon.
        /// </summary>
        /// <param name="Polygon">The polygon from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from polygon.</param>
        public static Polygon<T> operator -(Polygon<T> Polygon, Vector<T> Vector)
            => Offset(Polygon, -Vector);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="Polygon1">The first polygon to compare.</param>
        /// <param name="Polygon2">The second polygon to compare.</param>
        public static bool operator ==(Polygon<T> Polygon1, Polygon<T> Polygon2)
            => Polygon1.Equals(Polygon2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="Polygon1">The first polygon to compare.</param>
        /// <param name="Polygon2">The second polygon to compare.</param>
        public static bool operator !=(Polygon<T> Polygon1, Polygon<T> Polygon2)
            => !Polygon1.Equals(Polygon2);

    }
}