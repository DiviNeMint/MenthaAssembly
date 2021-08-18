using System;
using System.Linq;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a quadrilateral in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct Quadrilateral<T> : IPolygonShape<T>
        where T : unmanaged
    {
        private const int Vertices = 4;

        /// <summary>
        /// Gets a special value that represents a quadrilateral with no position or area.
        /// </summary>
        public static Quadrilateral<T> Empty => new();

        public Point<T>[] Points { set; get; }

        public bool IsEmpty
            => this.Points is null || this.Points.Length < Vertices || Line<T>.IsCollinear(this.Points);

        public Point<T> Center
        {
            get
            {
                if (this.IsEmpty)
                    return new Point<T>();

                Point<T> p = this.Points[0];
                T Cx = p.X,
                  Cy = p.Y;

                for (int i = 1; i < Vertices; i++)
                {
                    p = this.Points[i];
                    Cx = Add(Cx, p.X);
                    Cy = Add(Cy, p.Y);
                }

                return new Point<T>(DivVertices(Cx), DivVertices(Cy));
            }
        }

        public double Area
        {
            get
            {
                if (this.IsEmpty)
                    return default;

                Point<T> p0 = this.Points[0],
                         p1 = this.Points[1],
                         p2 = this.Points[2],
                         p3 = this.Points[3];

                T p0x = p0.X,
                  p0y = p0.Y,
                  v02x = Sub(p2.X, p0x),
                  v02y = Sub(p2.Y, p0y);

                return ToDouble(Abs(Sub(Vector<T>.Cross(v02x, v02y, Sub(p1.X, p0x), Sub(p1.Y, p0y)), Vector<T>.Cross(v02x, v02y, Sub(p3.X, p0x), Sub(p3.Y, p0y))))) / 2d;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quadrilateral{T}"/> structure.
        /// </summary>
        /// <param name="Px1">The x-coordinate of the first point that the new quadrilateral must contain.</param>
        /// <param name="Py1">The y-coordinate of the first point that the new quadrilateral must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new quadrilateral must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new quadrilateral must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new quadrilateral must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new quadrilateral must contain.</param>
        /// <param name="Px4">The x-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        /// <param name="Py4">The y-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        public Quadrilateral(T Px1, T Py1, T Px2, T Py2, T Px3, T Py3, T Px4, T Py4) : this(new Point<T>(Px1, Py1), new Point<T>(Px2, Py2), new Point<T>(Px3, Py3), new Point<T>(Px4, Py4))
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Quadrilateral{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new quadrilateral must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new quadrilateral must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new quadrilateral must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new quadrilateral must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new quadrilateral must contain.</param>
        /// <param name="Px4">The x-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        /// <param name="Py4">The y-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        public Quadrilateral(Point<T> Point1, T Px2, T Py2, T Px3, T Py3, T Px4, T Py4) : this(Point1, new Point<T>(Px2, Py2), new Point<T>(Px3, Py3), new Point<T>(Px4, Py4))
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Quadrilateral{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new quadrilateral must contain.</param>
        /// <param name="Point2">The second point that the new quadrilateral must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new quadrilateral must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new quadrilateral must contain.</param>
        /// <param name="Px4">The x-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        /// <param name="Py4">The y-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        public Quadrilateral(Point<T> Point1, Point<T> Point2, T Px3, T Py3, T Px4, T Py4) : this(Point1, Point2, new Point<T>(Px3, Py3), new Point<T>(Px4, Py4))
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Quadrilateral{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new quadrilateral must contain.</param>
        /// <param name="Point2">The second point that the new quadrilateral must contain.</param>
        /// <param name="Point3">The third point that the new quadrilateral must contain.</param>
        /// <param name="Px4">The x-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        /// <param name="Py4">The y-coordinate of the fourth  point that the new quadrilateral must contain.</param>
        public Quadrilateral(Point<T> Point1, Point<T> Point2, Point<T> Point3, T Px4, T Py4) : this(Point1, Point2, Point3, new Point<T>(Px4, Py4))
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Quadrilateral{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new quadrilateral must contain.</param>
        /// <param name="Point2">The second point that the new quadrilateral must contain.</param>
        /// <param name="Point3">The third point that the new quadrilateral must contain.</param>
        /// <param name="Point4">The fourth point that the new quadrilateral must contain.</param>
        public Quadrilateral(Point<T> Point1, Point<T> Point2, Point<T> Point3, Point<T> Point4)
        {
            if (Line<T>.IsCollinear(Point1, Point2, Point3, Point4))
            {
                this = Empty;
                return;
            }

            this.Points = Line<T>.IsSameSide(Point1, Point2, Point3, Point4) ? (Line<T>.IsSameSide(Point1, Point3, Point2, Point4) ? new[] { Point1, Point2, Point4, Point3 } :
                                                                                                                                     new[] { Point1, Point2, Point3, Point4 }) :
                                                                               new[] { Point1, Point3, Point2, Point4 };
        }

        public bool Contain(Point<T> Point)
            => this.Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
        {
            if (this.IsEmpty)
                return false;

            Point<T> p0 = this.Points[0],
                     p1 = this.Points[1],
                     p2 = this.Points[3];

            // Compute Vectors
            T p0x = p0.X,
              p0y = p0.Y,
              Vx0 = Sub(p2.X, p0x),
              Vy0 = Sub(p2.Y, p0y),
              Vx1 = Sub(p1.X, p0x),
              Vy1 = Sub(p1.Y, p0y),
              Vx2 = Sub(Px, p0x),
              Vy2 = Sub(Py, p0y);

            // Compute Dot
            T Dot00 = Vector<T>.Dot(Vx0, Vy0, Vx0, Vy0),
              Dot01 = Vector<T>.Dot(Vx0, Vy0, Vx1, Vy1),
              Dot02 = Vector<T>.Dot(Vx0, Vy0, Vx2, Vy2),
              Dot11 = Vector<T>.Dot(Vx1, Vy1, Vx1, Vy1),
              Dot12 = Vector<T>.Dot(Vx1, Vy1, Vx2, Vy2);

            // Compute barycentric coordinates
            double invDenom = 1d / ToDouble(Sub(Mul(Dot00, Dot11), Mul(Dot01, Dot01))),
                   u = ToDouble(Sub(Mul(Dot11, Dot02), Mul(Dot01, Dot12))) * invDenom;

            if (u is < 0d or > 1d)
                return false;

            double v = ToDouble(Sub(Mul(Dot00, Dot12), Mul(Dot01, Dot02))) * invDenom;
            return 0d <= v && v <= 1d && u + v <= 2d;
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
        /// Creates a new casted <see cref="Quadrilateral{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Quadrilateral<U> Cast<U>() where U : unmanaged
            => this.IsEmpty ? Quadrilateral<U>.Empty : new Quadrilateral<U> { Points = this.Points.Select(i => i.Cast<U>()).ToArray() };
        IShape<U> IShape<T>.Cast<U>()
            => this.Cast<U>();
        IMathObject<U> IMathObject<T>.Cast<U>()
            => this.Cast<U>();

        /// <summary>
        /// Creates a new <see cref="Quadrilateral{T}"/> that is a copy of the current instance.
        /// </summary>
        public Quadrilateral<T> Clone()
            => this.IsEmpty ? Empty : new Quadrilateral<T> { Points = new[] { this.Points[0], this.Points[1], this.Points[2], this.Points[3] } };
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
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Quadrilateral{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Quadrilateral<T> obj)
        {
            if (this.IsEmpty)
                return obj.IsEmpty;

            if (obj.IsEmpty)
                return false;

            for (int i = 0; i < this.Points.Length; i++)
                if (!this.Points[i].Equals(obj.Points[i]))
                    return false;

            return true;
        }
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is Quadrilateral<T> Quad && this.Equals(Quad);
        bool IMathObject<T>.Equals(IMathObject<T> obj)
            => obj is Quadrilateral<T> Quad && this.Equals(Quad);
        public override bool Equals(object obj)
            => obj is Quadrilateral<T> Quad && this.Equals(Quad);

        public override string ToString()
            => this.IsEmpty ? $"{nameof(Quadrilateral<T>)}<{typeof(T).Name}>.Empty" :
                              string.Join(", ", this.Points.Select(i => $"{{{i}}}"));

        private static readonly Func<T, T, T> Add, Sub, Mul;
        private static readonly Func<T, T> Abs, DivVertices;
        private static readonly Func<T, double> ToDouble;
        static Quadrilateral()
        {
            Abs = ExpressionHelper<T>.CreateAbs();

            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();

            DivVertices = ExpressionHelper<T>.CreateDiv(Vertices);

            ToDouble = ExpressionHelper<T>.CreateCast<double>();
        }

        /// <summary>
        /// Offsets the specified quadrilateral's coordinates by the specified vector.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be offsetted.</param>
        /// <param name="Vector">The vector to be added to the specified quadrilateral.</param>
        public static Quadrilateral<T> Offset(Quadrilateral<T> Quadrilateral, Vector<T> Vector)
            => Offset(Quadrilateral, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified quadrilateral's coordinates by the specified amounts.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be offsetted.</param>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public static Quadrilateral<T> Offset(Quadrilateral<T> Quadrilateral, T Dx, T Dy)
            => Quadrilateral.IsEmpty ? Empty : new Quadrilateral<T> { Points = Point<T>.Offset(Quadrilateral.Points, Dx, Dy) };

        /// <summary>
        /// Scales the specified quadrilateral around the origin.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be scaled.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Quadrilateral<T> Scale(Quadrilateral<T> Quadrilateral, T Scale)
            => Quadrilateral<T>.Scale(Quadrilateral, Quadrilateral.Center, Scale);
        /// <summary>
        /// Scales the specified quadrilateral around the origin.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be scaled.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Quadrilateral<T> Scale(Quadrilateral<T> Quadrilateral, T ScaleX, T ScaleY)
            => Scale(Quadrilateral, Quadrilateral.Center, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified quadrilateral around the specified point.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Quadrilateral<T> Scale(Quadrilateral<T> Quadrilateral, Point<T> Center, T Scale)
            => Quadrilateral<T>.Scale(Quadrilateral, Center.X, Center.Y, Scale, Scale);
        /// <summary>
        /// Scales the specified quadrilateral around the specified point.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Quadrilateral<T> Scale(Quadrilateral<T> Quadrilateral, Point<T> Center, T ScaleX, T ScaleY)
            => Scale(Quadrilateral, Center.X, Center.Y, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified quadrilateral around the specified point.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Quadrilateral<T> Scale(Quadrilateral<T> Quadrilateral, T Cx, T Cy, T Scale)
            => Quadrilateral<T>.Scale(Quadrilateral, Cx, Cy, Scale, Scale);
        /// <summary>
        /// Scales the specified quadrilateral around the specified point.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Quadrilateral<T> Scale(Quadrilateral<T> Quadrilateral, T Cx, T Cy, T ScaleX, T ScaleY)
            => Quadrilateral.IsEmpty ? Empty : new Quadrilateral<T> { Points = LineSegment<T>.Scale(Quadrilateral.Points, Cx, Cy, ScaleX, ScaleY) };

        /// <summary>
        /// Rotates the specified quadrilateral about the origin.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Quadrilateral<T> Rotate(Quadrilateral<T> Quadrilateral, double Theta)
            => Quadrilateral.IsEmpty ? Empty : new Quadrilateral<T> { Points = Point<T>.Rotate(Quadrilateral.Points, Theta) };
        /// <summary>
        /// Rotates the specified quadrilateral about the specified point.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Quadrilateral<T> Rotate(Quadrilateral<T> Quadrilateral, Point<T> Center, double Theta)
            => Rotate(Quadrilateral, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified quadrilateral about the specified point.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Quadrilateral<T> Rotate(Quadrilateral<T> Quadrilateral, T Cx, T Cy, double Theta)
            => Quadrilateral.IsEmpty ? Empty : new Quadrilateral<T> { Points = Point<T>.Rotate(Quadrilateral.Points, Cx, Cy, Theta) };

        /// <summary>
        /// Reflects the specified quadrilateral over the specified line.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be reflects.</param>
        /// <param name="Line">The projection line.</param>
        public static Quadrilateral<T> Reflect(Quadrilateral<T> Quadrilateral, Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return Quadrilateral.Clone();

            Point<T> P1 = Line.Points[0],
                     P2 = Line.Points[1];

            return Reflect(Quadrilateral, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified quadrilateral over the specified line.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be reflects.</param>
        /// <param name="LinePoint1">The quadrilateral on the projection line.</param>
        /// <param name="LinePoint2">The another quadrilateral on the projection line.</param>
        public static Quadrilateral<T> Reflect(Quadrilateral<T> Quadrilateral, Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(Quadrilateral, LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        /// <summary>
        /// Reflects the specified quadrilateral over the specified line.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral to be reflects.</param>
        /// <param name="Lx1">The x-coordinate of a quadrilateral on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a quadrilateral on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another quadrilateral on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another quadrilateral on the projection line.</param>
        public static Quadrilateral<T> Reflect(Quadrilateral<T> Quadrilateral, T Lx1, T Ly1, T Lx2, T Ly2)
            => Quadrilateral.IsEmpty ? Empty : new Quadrilateral<T> { Points = Point<T>.Reflect(Quadrilateral.Points, Lx1, Ly1, Lx2, Ly2) };

        /// <summary>
        /// Adds the specified vector to the specified quadrilateral.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static Quadrilateral<T> operator +(Quadrilateral<T> Quadrilateral, Vector<T> Vector)
            => Offset(Quadrilateral, Vector);
        /// <summary>
        /// Subtracts the specified vector from the specified quadrilateral.
        /// </summary>
        /// <param name="Quadrilateral">The quadrilateral from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from quadrilateral.</param>
        public static Quadrilateral<T> operator -(Quadrilateral<T> Quadrilateral, Vector<T> Vector)
            => Offset(Quadrilateral, -Vector);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="Quadrilateral1">The first quadrilateral to compare.</param>
        /// <param name="Quadrilateral2">The second quadrilateral to compare.</param>
        public static bool operator ==(Quadrilateral<T> Quadrilateral1, Quadrilateral<T> Quadrilateral2)
            => Quadrilateral1.Equals(Quadrilateral2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="Quadrilateral1">The first quadrilateral to compare.</param>
        /// <param name="Quadrilateral2">The second quadrilateral to compare.</param>
        public static bool operator !=(Quadrilateral<T> Quadrilateral1, Quadrilateral<T> Quadrilateral2)
            => !Quadrilateral1.Equals(Quadrilateral2);

    }
}