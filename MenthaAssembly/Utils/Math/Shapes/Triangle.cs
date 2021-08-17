using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MenthaAssembly
{
    [Serializable]
    public unsafe struct Triangle<T> : IPolygonShape<T>
        where T : unmanaged
    {
        /// <summary>
        /// Gets a special value that represents a triangle with no position or area.
        /// </summary>
        public static Triangle<T> Empty => new Triangle<T>();

        public Point<T>[] Points { set; get; }

        public bool IsEmpty
        {
            get
            {
                if (Points is null)
                    return true;

                int Length = Points.Length;
                if (Length == 0)
                    return true;

                for (int i = 0; i < Length; i++)
                {
                    Point<T> p = Points[i];
                    for (int j = i + 1; j < Length; j++)
                        if (p.Equals(Points[j]))
                            return true;
                }

                return Line<T>.IsCollinear(Points[0], Points[1], Points[2]);
            }
        }

        public Point<T> Center
        {
            get
            {
                if (this.IsEmpty)
                    return new Point<T>();

                T Cx = Points[0].X,
                  Cy = Points[0].Y;
                for (int i = 1; i < Points.Length; i++)
                {
                    Point<T> p = Points[i];
                    Cx = Add(Cx, p.X);
                    Cy = Add(Cy, p.Y);
                }

                return new Point<T>(Div3(Cx), Div3(Cy));
            }
        }

        public double Area
        {
            get
            {
                if (this.IsEmpty)
                    return default;

                Point<T> p0 = Points[0];
                return ToDouble(Abs(Vector<T>.Cross(Points[1] - p0, Points[2] - p0))) / 2d;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new triangle must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new triangle must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new triangle must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new triangle must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new triangle must contain.</param>
        public Triangle(Point<T> Point1, T Px2, T Py2, T Px3, T Py3)
        {
            this.Points = new[] { Point1, new Point<T>(Px2, Py2), new Point<T>(Px3, Py3) };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new triangle must contain.</param>
        /// <param name="Point2">The second point that the new triangle must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new triangle must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new triangle must contain.</param>
        public Triangle(Point<T> Point1, Point<T> Point2, T Px3, T Py3)
        {
            this.Points = new[] { Point1, Point2, new Point<T>(Px3, Py3) };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle{T}"/> structure.
        /// </summary>
        /// <param name="Point1">The first point that the new triangle must contain.</param>
        /// <param name="Point2">The second point that the new triangle must contain.</param>
        /// <param name="Point3">The third point that the new triangle must contain.</param>
        public Triangle(Point<T> Point1, Point<T> Point2, Point<T> Point3)
        {
            this.Points = new[] { Point1, Point2, Point3 };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle{T}"/> structure.
        /// </summary>
        /// <param name="Line1">The first bound of the new triangle.</param>
        /// <param name="Line2">The second bound of the new triangle.</param>
        /// <param name="Line3">The third bound of the new triangle.</param>
        public Triangle(Line<T> Line1, Line<T> Line2, Line<T> Line3)
        {
            IEnumerable<Point<T>> Points;
            CrossPoints<T> Cross = Line<T>.CrossPoint(Line1, Line2);
            if (Cross.IsInfinity || Cross.Count == 0)
            {
                this = Empty;
                return;
            }
            Points = Cross;

            Cross = Line<T>.CrossPoint(Line1, Line3);
            if (Cross.IsInfinity || Cross.Count == 0)
            {
                this = Empty;
                return;
            }
            Points = Points.Concat(Cross);

            Cross = Line<T>.CrossPoint(Line2, Line3);
            if (Cross.IsInfinity || Cross.Count == 0)
            {
                this = Empty;
                return;
            }

            this.Points = Points.Concat(Cross).ToArray();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle{T}"/> structure.
        /// </summary>
        /// <param name="Px1">The x-coordinate of the first point that the new triangle must contain.</param>
        /// <param name="Py1">The y-coordinate of the first point that the new triangle must contain.</param>
        /// <param name="Px2">The x-coordinate of the second point that the new triangle must contain.</param>
        /// <param name="Py2">The y-coordinate of the second point that the new triangle must contain.</param>
        /// <param name="Px3">The x-coordinate of the third point that the new triangle must contain.</param>
        /// <param name="Py3">The y-coordinate of the third point that the new triangle must contain.</param>
        public Triangle(T Px1, T Py1, T Px2, T Py2, T Px3, T Py3)
        {
            this.Points = new[] { new Point<T>(Px1, Py1), new Point<T>(Px2, Py2), new Point<T>(Px3, Py3) };
        }

        public bool Contain(Point<T> Point)
            => Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
        {
            if (IsEmpty)
                return false;

            // Compute Vectors
            T Vx0 = Sub(Points[2].X, Points[0].X),
              Vy0 = Sub(Points[2].Y, Points[0].Y),
              Vx1 = Sub(Points[1].X, Points[0].X),
              Vy1 = Sub(Points[1].Y, Points[0].Y),
              Vx2 = Sub(Px, Points[0].X),
              Vy2 = Sub(Py, Points[0].Y);

            // Compute Dot
            T Dot00 = Add(Mul(Vx0, Vx0), Mul(Vy0, Vy0)),
              Dot01 = Add(Mul(Vx0, Vx1), Mul(Vy0, Vy1)),
              Dot02 = Add(Mul(Vx0, Vx2), Mul(Vy0, Vy2)),
              Dot11 = Add(Mul(Vx1, Vx1), Mul(Vy1, Vy1)),
              Dot12 = Add(Mul(Vx1, Vx2), Mul(Vy1, Vy2));

            // Compute barycentric coordinates
            double invDenom = 1d / ToDouble(Sub(Mul(Dot00, Dot11), Mul(Dot01, Dot01))),
                   u = ToDouble(Sub(Mul(Dot11, Dot02), Mul(Dot01, Dot12))) * invDenom,
                   v = ToDouble(Sub(Mul(Dot00, Dot12), Mul(Dot01, Dot02))) * invDenom;

            return 0d <= u && 0d <= v && u + v <= 1d;
        }

        public void Offset(Vector<T> Vector)
            => Offset(Vector.X, Vector.Y);
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
                Vector<T>.Scale(pPoints, Points.Length, Cx, Cy, ScaleX, ScaleY);
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
            => Rotate(Center.X, Center.Y, Theta);
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

            Reflect(P1.X, P1.Y, P2.X, P2.Y);
        }
        public void Reflect(Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
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
        /// Creates a new casted <see cref="Triangle{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Triangle<U> Cast<U>()
            where U : unmanaged
        {
            if (this.IsEmpty)
                return Triangle<U>.Empty;

            return new Triangle<U>(this.Points[0].Cast<U>(), this.Points[1].Cast<U>(), this.Points[2].Cast<U>());
        }
        IShape<U> IShape<T>.Cast<U>()
            => this.Cast<U>();

        /// <summary>
        /// Creates a new <see cref="Triangle{T}"/> that is a copy of the current instance.
        /// </summary>
        public Triangle<T> Clone()
            => this.IsEmpty ? Empty : new Triangle<T>(Points[0], Points[1], Points[2]);
        IShape<T> IShape<T>.Clone()
            => this.Clone();
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
        {
            if (Points is null)
                return base.GetHashCode();

            int Code = Points[0].GetHashCode();
            for (int i = 1; i < Points.Length; i++)
                Code ^= Points[i].GetHashCode();

            return Code;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Triangle{T}"/>
        /// </summary>
        /// <param name="obj">The obj to compare to the current instance.</param>
        public bool Equals(Triangle<T> obj)
        {
            if (this.IsEmpty)
                return obj.IsEmpty;

            for (int i = 0; i < Points.Length; i++)
                if (!this.Points[i].Equals(obj.Points[i]))
                    return false;

            return true;
        }
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is Triangle<T> Tri && this.Equals(Tri);
        public override bool Equals(object obj)
            => obj is Triangle<T> Tri && this.Equals(Tri);

        public override string ToString()
        {
            if (this.IsEmpty)
                return $"{nameof(Triangle<T>)}<{typeof(T).Name}>.Empty";

            StringBuilder Builder = new StringBuilder(128);
            try
            {
                Point<T> p = Points[0];
                Builder.Append($"Px1 : {p.X}, Py1 : {p.Y}");

                for (int i = 1; i < Points.Length; i++)
                {
                    p = Points[i];

                    int Index = i + 1;
                    Builder.Append($", Px{Index} : {p.X}, Py{Index} : {p.Y}");
                }

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        private static readonly Func<T, T> Abs, Div3;
        private static readonly Func<T, T, T> Add, Sub, Mul;
        private static readonly Func<T, double> ToDouble;
        static Triangle()
        {
            Abs = ExpressionHelper<T>.CreateAbs();

            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();

            Div3 = ExpressionHelper<T>.CreateDiv(3);

            ToDouble = ExpressionHelper<T>.CreateCast<double>();
        }

        /// <summary>
        /// Offsets the specified triangle's coordinates by the specified vector.
        /// </summary>
        /// <param name="Triangle">The triangle to be offsetted.</param>
        /// <param name="Vector">The vector to be added to the specified triangle.</param>
        public static Triangle<T> Offset(Triangle<T> Triangle, Vector<T> Vector)
            => Offset(Triangle, Vector.X, Vector.Y);
        /// <summary>
        /// Offsets the specified triangle's coordinates by the specified amounts.
        /// </summary>
        /// <param name="Triangle">The triangle to be offsetted.</param>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public static Triangle<T> Offset(Triangle<T> Triangle, T Dx, T Dy)
        {
            if (Triangle.IsEmpty)
                return Empty;

            return new Triangle<T> { Points = Point<T>.Offset(Triangle.Points, Dx, Dy) };
        }

        /// <summary>
        /// Scales the specified triangle around the origin.
        /// </summary>
        /// <param name="Triangle">The triangle to be scaled.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Triangle<T> Scale(Triangle<T> Triangle, T Scale)
            => Triangle<T>.Scale(Triangle, Triangle.Center, Scale);
        /// <summary>
        /// Scales the specified triangle around the origin.
        /// </summary>
        /// <param name="Triangle">The triangle to be scaled.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Triangle<T> Scale(Triangle<T> Triangle, T ScaleX, T ScaleY)
            => Triangle<T>.Scale(Triangle, Triangle.Center, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified triangle around the specified point.
        /// </summary>
        /// <param name="Triangle">The triangle to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Triangle<T> Scale(Triangle<T> Triangle, Point<T> Center, T Scale)
            => Triangle<T>.Scale(Triangle, Center.X, Center.Y, Scale, Scale);
        /// <summary>
        /// Scales the specified triangle around the specified point.
        /// </summary>
        /// <param name="Triangle">The triangle to be scaled.</param>
        /// <param name="Center">The center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Triangle<T> Scale(Triangle<T> Triangle, Point<T> Center, T ScaleX, T ScaleY)
            => Triangle<T>.Scale(Triangle, Center.X, Center.Y, ScaleX, ScaleY);
        /// <summary>
        /// Scales the specified triangle around the specified point.
        /// </summary>
        /// <param name="Triangle">The triangle to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="Scale">The scale factor.</param>
        public static Triangle<T> Scale(Triangle<T> Triangle, T Cx, T Cy, T Scale)
            => Triangle<T>.Scale(Triangle, Cx, Cy, Scale, Scale);
        /// <summary>
        /// Scales the specified triangle around the specified point.
        /// </summary>
        /// <param name="Triangle">The triangle to be scaled.</param>
        /// <param name="Cx">The x-coordinate of the center about which to scale.</param>
        /// <param name="Cy">The y-coordinate of the center about which to scale.</param>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public static Triangle<T> Scale(Triangle<T> Triangle, T Cx, T Cy, T ScaleX, T ScaleY)
        {
            if (Triangle.IsEmpty)
                return Empty;

            return new Triangle<T> { Points = Vector<T>.Scale(Triangle.Points, Cx, Cy, ScaleX, ScaleY) };
        }

        /// <summary>
        /// Rotates the specified triangle about the origin.
        /// </summary>
        /// <param name="Triangle">The triangle to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Triangle<T> Rotate(Triangle<T> Triangle, double Theta)
        {
            if (Triangle.IsEmpty)
                return Empty;

            return new Triangle<T> { Points = Point<T>.Rotate(Triangle.Points, Theta) };
        }
        /// <summary>
        /// Rotates the specified triangle about the specified point.
        /// </summary>
        /// <param name="Triangle">The triangle to be rotated.</param>
        /// <param name="Center">The center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Triangle<T> Rotate(Triangle<T> Triangle, Point<T> Center, double Theta)
            => Rotate(Triangle, Center.X, Center.Y, Theta);
        /// <summary>
        /// Rotates the specified triangle about the specified point.
        /// </summary>
        /// <param name="Triangle">The triangle to be rotated.</param>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Triangle<T> Rotate(Triangle<T> Triangle, T Cx, T Cy, double Theta)
        {
            if (Triangle.IsEmpty)
                return Empty;

            return new Triangle<T> { Points = Point<T>.Rotate(Triangle.Points, Cx, Cy, Theta) };
        }

        /// <summary>
        /// Reflects the specified triangle over the specified line.
        /// </summary>
        /// <param name="Triangle">The triangle to be reflects.</param>
        /// <param name="Line">The projection line.</param>
        public static Triangle<T> Reflect(Triangle<T> Triangle, Line<T> Line)
        {
            if (Line.Points is null || Line.Points.Length < 2)
                return Triangle.Clone();

            Point<T> P1 = Line.Points[0],
                     P2 = Line.Points[1];

            return Reflect(Triangle, P1.X, P1.Y, P2.X, P2.Y);
        }
        /// <summary>
        /// Reflects the specified triangle over the specified line.
        /// </summary>
        /// <param name="Triangle">The triangle to be reflects.</param>
        /// <param name="LinePoint1">The triangle on the projection line.</param>
        /// <param name="LinePoint2">The another triangle on the projection line.</param>
        public static Triangle<T> Reflect(Triangle<T> Triangle, Point<T> LinePoint1, Point<T> LinePoint2)
            => Reflect(Triangle, LinePoint1.X, LinePoint1.Y, LinePoint2.X, LinePoint2.Y);
        /// <summary>
        /// Reflects the specified triangle over the specified line.
        /// </summary>
        /// <param name="Triangle">The triangle to be reflects.</param>
        /// <param name="Lx1">The x-coordinate of a triangle on the projection line.</param>
        /// <param name="Ly1">The y-coordinate of a triangle on the projection line.</param>
        /// <param name="Lx2">The x-coordinate of a another triangle on the projection line.</param>
        /// <param name="Ly2">The y-coordinate of a another triangle on the projection line.</param>
        public static Triangle<T> Reflect(Triangle<T> Triangle, T Lx1, T Ly1, T Lx2, T Ly2)
        {
            if (Triangle.IsEmpty)
                return Empty;

            return new Triangle<T> { Points = Point<T>.Reflect(Triangle.Points, Lx1, Ly1, Lx2, Ly2) };
        }

        /// <summary>
        /// Adds the specified vector to the specified triangle.
        /// </summary>
        /// <param name="Triangle">The triangle structure to add.</param>
        /// <param name="Vector">The vector structure to add.</param>
        public static Triangle<T> operator +(Triangle<T> Triangle, Vector<T> Vector)
            => Offset(Triangle, Vector);
        /// <summary>
        /// Subtracts the specified vector from the specified triangle.
        /// </summary>
        /// <param name="Triangle">The triangle from which vector is subtracted.</param>
        /// <param name="Vector">The vector to subtract from triangle.</param>
        public static Triangle<T> operator -(Triangle<T> Triangle, Vector<T> Vector)
            => Offset(Triangle, -Vector);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        /// <param name="Triangle1">The first triangle to compare.</param>
        /// <param name="Triangle2">The second triangle to compare.</param>
        public static bool operator ==(Triangle<T> Triangle1, Triangle<T> Triangle2)
            => Triangle1.Equals(Triangle2);
        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        /// <param name="Triangle1">The first triangle to compare.</param>
        /// <param name="Triangle2">The second triangle to compare.</param>
        public static bool operator !=(Triangle<T> Triangle1, Triangle<T> Triangle2)
            => !Triangle1.Equals(Triangle2);

    }
}