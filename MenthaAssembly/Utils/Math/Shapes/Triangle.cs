﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a triangle in 2-D space.
    /// </summary>
    [Serializable]
    public unsafe struct Triangle<T> : IPolygonShape<T>
        where T : unmanaged
    {
        private const int Vertices = 3;

        /// <summary>
        /// Gets a special value that represents a triangle with no position or area.
        /// </summary>
        public static Triangle<T> Empty => new();

        public Point<T>[] Points { set; get; }

        public bool IsEmpty
            => this.Points is null || this.Points.Length < Vertices || Line<T>.IsCollinear(this.Points[0], this.Points[1], this.Points[2]);

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
                         p2 = this.Points[2];

                return ToDouble(Abs(Vector<T>.Cross(Sub(p1.X, p0.X), Sub(p1.Y, p0.Y), Sub(p2.X, p0.X), Sub(p2.Y, p0.Y)))) / 2d;
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
            => this.Contain(Point.X, Point.Y);
        public bool Contain(T Px, T Py)
        {
            if (this.IsEmpty)
                return false;

            Point<T> p0 = this.Points[0],
                     p1 = this.Points[1],
                     p2 = this.Points[2];

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

            if (u < 0d)
                return false;

            double v = ToDouble(Sub(Mul(Dot00, Dot12), Mul(Dot01, Dot02))) * invDenom;
            return 0d <= v && u + v <= 1d;
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
        /// Creates a new casted <see cref="Triangle{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Triangle<U> Cast<U>() where U : unmanaged
            => this.IsEmpty ? Triangle<U>.Empty : new Triangle<U> { Points = this.Points.Select(i => i.Cast<U>()).ToArray() };
        IShape<U> IShape<T>.Cast<U>()
            => this.Cast<U>();
        IMathObject<U> IMathObject<T>.Cast<U>()
            => this.Cast<U>();

        /// <summary>
        /// Creates a new <see cref="Triangle{T}"/> that is a copy of the current instance.
        /// </summary>
        public Triangle<T> Clone()
            => this.IsEmpty ? Empty : new Triangle<T> { Points = this.Points.ToArray() };
        IShape<T> IShape<T>.Clone()
            => this.Clone();
        IMathObject<T> IMathObject<T>.Clone()
            => this.Clone();
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
        {
            if (this.Points is null || this.Points.Length < Vertices)
                return base.GetHashCode();

            int Code = this.Points[0].GetHashCode();
            for (int i = 1; i < Vertices; i++)
                Code ^= this.Points[i].GetHashCode();

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

            if (obj.IsEmpty)
                return false;

            int i = 0,
                Index = -1;
            Point<T> p = this.Points[0];
            for (; i < Vertices; i++)
            {
                if (p.Equals(obj.Points[i]))
                {
                    Index = i;
                    break;
                }
            }

            if (Index == -1)
                return false;

            i = Index + 1;
            if (i >= Vertices)
                i %= Vertices;

            if (this.Points[1].Equals(obj.Points[i]))
            {
                i++;
                if (i >= Vertices)
                    i %= Vertices;

                return this.Points[2].Equals(obj.Points[i]);
            }

            if (!this.Points[1].Equals(obj.Points[Vertices - i]))
                return false;

            i++;
            if (i >= Vertices)
                i %= Vertices;

            return this.Points[2].Equals(obj.Points[Vertices - i]);
        }
        bool IShape<T>.Equals(IShape<T> obj)
            => obj is Triangle<T> Tri && this.Equals(Tri);
        bool IMathObject<T>.Equals(IMathObject<T> obj)
            => obj is Triangle<T> Tri && this.Equals(Tri);
        public override bool Equals(object obj)
            => obj is Triangle<T> Tri && this.Equals(Tri);

        public override string ToString()
            => this.IsEmpty ? $"{nameof(Triangle<T>)}<{typeof(T).Name}>.Empty" :
                              string.Join(", ", this.Points.Select(i => $"{{{i}}}"));

        private static readonly Func<T, T, T> Add, Sub, Mul;
        private static readonly Func<T, T> Abs, DivVertices;
        private static readonly Func<T, double> ToDouble;
        static Triangle()
        {
            Abs = ExpressionHelper<T>.CreateAbs();

            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();

            DivVertices = ExpressionHelper<T>.CreateDiv(Vertices);

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
            => Triangle.IsEmpty ? Empty : new Triangle<T> { Points = Point<T>.Offset(Triangle.Points, Dx, Dy) };

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
            => Scale(Triangle, Triangle.Center, ScaleX, ScaleY);
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
            => Scale(Triangle, Center.X, Center.Y, ScaleX, ScaleY);
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
            => Triangle.IsEmpty ? Empty : new Triangle<T> { Points = LineSegment<T>.Scale(Triangle.Points, Cx, Cy, ScaleX, ScaleY) };

        /// <summary>
        /// Rotates the specified triangle about the origin.
        /// </summary>
        /// <param name="Triangle">The triangle to be rotated.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public static Triangle<T> Rotate(Triangle<T> Triangle, double Theta)
            => Triangle.IsEmpty ? Empty : new Triangle<T> { Points = Point<T>.Rotate(Triangle.Points, Theta) };
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
            => Triangle.IsEmpty ? Empty : new Triangle<T> { Points = Point<T>.Rotate(Triangle.Points, Cx, Cy, Theta) };

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
            => Triangle.IsEmpty ? Empty : new Triangle<T> { Points = Point<T>.Reflect(Triangle.Points, Lx1, Ly1, Lx2, Ly2) };

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