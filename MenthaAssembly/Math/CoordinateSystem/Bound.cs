using System;
#if NET7_0_OR_GREATER
using System.Numerics;
#else
using static MenthaAssembly.OperatorHelper;
#endif

namespace MenthaAssembly
{
    [Serializable]
    public struct Bound<T> : ICloneable
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : unmanaged
#endif
    {
        public static Bound<T> Empty => new();

        public T Left { set; get; }

        public T Top { set; get; }

        public T Right { set; get; }

        public T Bottom { set; get; }

        public T Width
        {
            get
            {
#if NET7_0_OR_GREATER
                return Right - Left;
#else
                return Subtract(Right, Left);
#endif
            }
        }

        public T Height
        {
            get
            {
#if NET7_0_OR_GREATER
                return Bottom - Top;
#else
                return Subtract(Bottom, Top);
#endif
            }
        }

        public Point<T> Center
        {
            get
            {
#if NET7_0_OR_GREATER
                T Two = T.CreateChecked(2);
                return new((Left + Right) / Two, (Top + Bottom) / Two);
#else
                return new(Half(Add(Left, Right)), Half(Add(Top, Bottom)));
#endif
            }
        }

        public Point<T> LeftTop
            => new(Left, Top);

        public Point<T> LeftBottom
            => new(Left, Bottom);

        public Point<T> RightTop
            => new(Right, Top);

        public Point<T> RightBottom
            => new(Right, Bottom);

        public bool IsEmpty
        {
            get
            {
#if NET7_0_OR_GREATER
                return T.IsZero(Width) && T.IsZero(Height);
#else
                return IsDefault(Width) && IsDefault(Height);
#endif
            }
        }

        public Bound(T Left, T Top, T Right, T Bottom)
        {
            this.Left = Left;
            this.Top = Top;
            this.Right = Right;
            this.Bottom = Bottom;
        }
        public Bound(Point<T> Position, Size<T> Size)
        {
            Left = Position.X;
            Top = Position.Y;
#if NET7_0_OR_GREATER
            Right = Position.X + Size.Width;
            Bottom = Position.Y + Size.Height;
#else
            Right = Add(Position.X, Size.Width);
            Bottom = Add(Position.Y, Size.Height);
#endif
        }

        public void Offset(Vector<T> Vector)
            => Offset(Vector.X, Vector.Y);
        public void Offset(T X, T Y)
        {
#if NET7_0_OR_GREATER
            Left = Left + X;
            Right = Right + X;
            Top = Top + Y;
            Bottom = Bottom + Y;
#else
            Left = Add(Left, X);
            Right = Add(Right, X);
            Top = Add(Top, Y);
            Bottom = Add(Bottom, Y);
#endif
        }

        public void Scale(T Scale)
            => this.Scale(Scale, Scale);
        public void Scale(T XScale, T YScale)
        {
#if NET7_0_OR_GREATER
            Left *= XScale;
            Top *= YScale;
            Right *= XScale;
            Bottom *= YScale;
#else
            Left = Multiply(Left, XScale);
            Top = Multiply(Top, YScale);
            Right = Multiply(Right, XScale);
            Bottom = Multiply(Bottom, YScale);
#endif
        }

        public void ScaleSize(T Scale)
            => ScaleSize(Scale, Scale);
        public void ScaleSize(T XScale, T YScale)
        {
#if NET7_0_OR_GREATER
            Right = Left + Width * XScale;
            Bottom = Top + Height * YScale;
#else
            Right = Add(Left, Multiply(Width, XScale));
            Bottom = Add(Top, Multiply(Height, YScale));
#endif
        }

        public void Rotate(double Theta)
        {
#if NET7_0_OR_GREATER
            double DL = double.CreateChecked(Left),
                   DT = double.CreateChecked(Top),
                   DR = double.CreateChecked(Right),
                   DB = double.CreateChecked(Bottom),
                   DOx = (DL + DR) * 0.5d,
                   DOy = (DT + DB) * 0.5d,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Point<double>.Rotate(DL, DT, DOx, DOy, Sin, Cos, out double X0, out double Y0);
            Point<double>.Rotate(DL, DB, DOx, DOy, Sin, Cos, out double X1, out double Y1);
            Point<double>.Rotate(DR, DT, DOx, DOy, Sin, Cos, out double X2, out double Y2);
            Point<double>.Rotate(DR, DB, DOx, DOy, Sin, Cos, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            Left = T.CreateChecked(Min);
            Right = T.CreateChecked(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            Top = T.CreateChecked(Min);
            Bottom = T.CreateChecked(Max);
#else
            double DL = Cast<T, double>(Left),
                   DT = Cast<T, double>(Top),
                   DR = Cast<T, double>(Right),
                   DB = Cast<T, double>(Bottom),
                   DOx = (DL + DR) * 0.5d,
                   DOy = (DT + DB) * 0.5d,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Point<double>.Rotate(DL, DT, DOx, DOy, Sin, Cos, out double X0, out double Y0);
            Point<double>.Rotate(DL, DB, DOx, DOy, Sin, Cos, out double X1, out double Y1);
            Point<double>.Rotate(DR, DT, DOx, DOy, Sin, Cos, out double X2, out double Y2);
            Point<double>.Rotate(DR, DB, DOx, DOy, Sin, Cos, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            Left = Cast<double, T>(Min);
            Right = Cast<double, T>(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            Top = Cast<double, T>(Min);
            Bottom = Cast<double, T>(Max);
#endif
        }
        public void Rotate(T Ox, T Oy, double Theta)
        {
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Point<T>.Rotate(Left, Top, Ox, Oy, Sin, Cos, out T X0, out T Y0);
            Point<T>.Rotate(Left, Bottom, Ox, Oy, Sin, Cos, out T X1, out T Y1);
            Point<T>.Rotate(Right, Top, Ox, Oy, Sin, Cos, out T X2, out T Y2);
            Point<T>.Rotate(Right, Bottom, Ox, Oy, Sin, Cos, out T X3, out T Y3);

            MathHelper.MinAndMax(out T Min, out T Max, X0, X1, X2, X3);
            Left = Min;
            Right = Max;

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            Top = Min;
            Bottom = Max;
        }

        public void Union(Bound<T> Bound)
        {
            if (IsEmpty)
            {
                this = Bound;
                return;
            }

            if (Bound.IsEmpty)
                return;

#if NET7_0_OR_GREATER
            Left = T.Min(Left, Bound.Left);
            Right = T.Max(Right, Bound.Right);
            Top = T.Min(Top, Bound.Top);
            Bottom = T.Max(Bottom, Bound.Bottom);
#else
            Left = Min(Left, Bound.Left);
            Right = Max(Right, Bound.Right);
            Top = Min(Top, Bound.Top);
            Bottom = Max(Bottom, Bound.Bottom);
#endif
        }
        public void Union(T Left, T Top, T Right, T Bottom)
        {
            if (IsEmpty)
            {
                this = new(Left, Top, Right, Bottom);
                return;
            }

#if NET7_0_OR_GREATER
            if (T.IsZero(Right - Left) && T.IsZero(Bottom - Top))
                return;

            this.Left = T.Min(this.Left, Left);
            this.Right = T.Max(this.Right, Right);
            this.Top = T.Min(this.Top, Top);
            this.Bottom = T.Max(this.Bottom, Bottom);
#else
            if (IsDefault(Subtract(Right, Left)) && IsDefault(Subtract(Bottom, Top)))
                return;

            this.Left = Min(this.Left, Left);
            this.Right = Max(this.Right, Right);
            this.Top = Min(this.Top, Top);
            this.Bottom = Max(this.Bottom, Bottom);
#endif
        }

        public void Intersect(Bound<T> Bound)
        {
            if (IntersectsWith(Bound))
            {
#if NET7_0_OR_GREATER
                Left = T.Max(Left, Bound.Left);
                Right = T.Clamp(Bound.Right, Left, Right);      // T.Max(T.Min(Right, Bound.Right), Left);
                Top = T.Max(Top, Bound.Top);
                Bottom = T.Clamp(Bound.Bottom, Top, Bottom);     // T.Max(T.Min(Bottom, Bound.Bottom), Top);
#else
                Left = Max(Left, Bound.Left);
                Right = Max(Min(Right, Bound.Right), Left);
                Top = Max(Top, Bound.Top);
                Bottom = Max(Min(Bottom, Bound.Bottom), Top);
#endif
            }
            else
            {
                this = Empty;
            }
        }
        public void Intersect(T Left, T Top, T Right, T Bottom)
        {
            if (IntersectsWith(Left, Top, Right, Bottom))
            {
#if NET7_0_OR_GREATER
                this.Left = T.Max(this.Left, Left);
                this.Right = T.Clamp(Right, this.Left, this.Right);     // T.Max(T.Min(this.Right, Right), this.Left);
                this.Top = T.Max(this.Top, Top);
                this.Bottom = T.Clamp(Bottom, this.Top, this.Bottom);    // T.Max(T.Min(this.Bottom, Bottom), this.Top);
#else
                this.Left = Max(this.Left, Left);
                this.Right = Max(Min(this.Right, Right), this.Left);
                this.Top = Max(this.Top, Top);
                this.Bottom = Max(Min(this.Bottom, Bottom), this.Top);
#endif
            }
            else
            {
                this = Empty;
            }
        }

        public bool IntersectsWith(Bound<T> Bound)
        {
            if (IsEmpty || Bound.IsEmpty)
                return false;

#if NET7_0_OR_GREATER
            return Bound.Left <= Right &&
                   Bound.Right >= Left &&
                   Bound.Top <= Bottom &&
                   Bound.Bottom >= Top;
#else
            return LessThanOrEqual(Bound.Left, Right) &&
                   GreaterThanOrEqual(Bound.Right, Left) &&
                   LessThanOrEqual(Bound.Top, Bottom) &&
                   GreaterThanOrEqual(Bound.Bottom, Top);
#endif
        }
        public bool IntersectsWith(T Left, T Top, T Right, T Bottom)
        {
#if NET7_0_OR_GREATER
            if (IsEmpty || (T.IsZero(Right - Left) && T.IsZero(Bottom - Top)))
                return false;

            return Left <= this.Right &&
                   Right >= this.Left &&
                   Top <= this.Bottom &&
                   Bottom >= this.Top;
#else
            if (IsEmpty || (IsDefault(Subtract(Right, Left)) && IsDefault(Subtract(Bottom, Top))))
                return false;

            return LessThanOrEqual(Left, this.Right) &&
                   GreaterThanOrEqual(Right, this.Left) &&
                   LessThanOrEqual(Top, this.Bottom) &&
                   GreaterThanOrEqual(Bottom, this.Top);
#endif
        }

        public bool Contains(Point<T> Point)
            => Contains(Point.X, Point.Y);
        public bool Contains(T X, T Y)
        {
#if NET7_0_OR_GREATER
            return !IsEmpty && Left < X && X < Right &&
                               Top < Y && Y < Bottom;
#else
            return !IsEmpty && LessThan(Left, X) && LessThan(X, Right) &&
                               LessThan(Top, Y) && LessThan(Y, Bottom);
#endif
        }

        public Bound<U> Cast<U>()
#if NET7_0_OR_GREATER
        where U : INumber<U>
#else
        where U : unmanaged
#endif
        {
#if NET7_0_OR_GREATER
            return new(U.CreateChecked(Left), U.CreateChecked(Top), U.CreateChecked(Right), U.CreateChecked(Bottom));
#else
            return new(Cast<T, U>(Left), Cast<T, U>(Top), Cast<T, U>(Right), Cast<T, U>(Bottom));
#endif
        }

        public Bound<T> Clone()
            => new(Left, Top, Right, Bottom);
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => Left.GetHashCode() ^ Top.GetHashCode() ^ Left.GetHashCode() ^ Bottom.GetHashCode();

        public bool Equals(Bound<T> Target)
        {
#if NET7_0_OR_GREATER
            return Left == Target.Left &&
                   Top == Target.Top &&
                   Right == Target.Right &&
                   Bottom == Target.Bottom;
#else
            return OperatorHelper.Equals(Left, Target.Left) &&
                   OperatorHelper.Equals(Top, Target.Top) &&
                   OperatorHelper.Equals(Right, Target.Right) &&
                   OperatorHelper.Equals(Bottom, Target.Bottom);
#endif
        }
        public override bool Equals(object obj)
            => obj is Bound<T> Target && Equals(Target);

        public override string ToString()
            => $"Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom}";

        public static Bound<T> Offset(Bound<T> Bound, Vector<T> Vector)
            => Offset(Bound, Vector.X, Vector.Y);
        public static Bound<T> Offset(Bound<T> Bound, T X, T Y)
        {
#if NET7_0_OR_GREATER
            return new(Bound.Left + X, Bound.Top + Y, Bound.Right + X, Bound.Bottom + Y);
#else
            return new(Add(Bound.Left, X), Add(Bound.Top, Y), Add(Bound.Right, X), Add(Bound.Bottom, Y));
#endif
        }

        public static Bound<T> Scale(Bound<T> Bound, T Scale)
            => Bound<T>.Scale(Bound, Scale, Scale);
        public static Bound<T> Scale(Bound<T> Bound, T XScale, T YScale)
        {
#if NET7_0_OR_GREATER
            return new(Bound.Left * XScale, Bound.Top * YScale, Bound.Right * XScale, Bound.Bottom * YScale);
#else
            return new(Multiply(Bound.Left, XScale), Multiply(Bound.Top, YScale), Multiply(Bound.Right, XScale), Multiply(Bound.Bottom, YScale));
#endif
        }

        public static Bound<T> ScaleSize(Bound<T> Bound, T Scale)
            => ScaleSize(Bound, Scale, Scale);
        public static Bound<T> ScaleSize(Bound<T> Bound, T XScale, T YScale)
        {
#if NET7_0_OR_GREATER
            return new(Bound.Left, Bound.Top, Bound.Left + Bound.Width * XScale, Bound.Top + Bound.Height * YScale);
#else
            return new(Bound.Left, Bound.Top, Add(Bound.Left, Multiply(Bound.Width, XScale)), Add(Bound.Top, Multiply(Bound.Height, YScale)));
#endif
        }

        public static Bound<T> Rotate(Bound<T> Bound, double Theta)
        {
#if NET7_0_OR_GREATER
            double Left = double.CreateChecked(Bound.Left),
                   Top = double.CreateChecked(Bound.Top),
                   Right = double.CreateChecked(Bound.Right),
                   Bottom = double.CreateChecked(Bound.Bottom),
                   Ox = (Left + Right) * 0.5d,
                   Oy = (Top + Bottom) * 0.5d,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);
#else
            double Left = Cast<T, double>(Bound.Left),
                   Top = Cast<T, double>(Bound.Top),
                   Right = Cast<T, double>(Bound.Right),
                   Bottom = Cast<T, double>(Bound.Bottom),
                   Ox = (Left + Right) * 0.5d,
                   Oy = (Top + Bottom) * 0.5d,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);
#endif

            Point<double>.Rotate(Left, Top, Ox, Oy, Sin, Cos, out double X0, out double Y0);
            Point<double>.Rotate(Left, Bottom, Ox, Oy, Sin, Cos, out double X1, out double Y1);
            Point<double>.Rotate(Right, Top, Ox, Oy, Sin, Cos, out double X2, out double Y2);
            Point<double>.Rotate(Right, Bottom, Ox, Oy, Sin, Cos, out double X3, out double Y3);

            MathHelper.MinAndMax(out Left, out Right, X0, X1, X2, X3);
            MathHelper.MinAndMax(out Top, out Bottom, Y0, Y1, Y2, Y3);

#if NET7_0_OR_GREATER
            return new(T.CreateChecked(Left), T.CreateChecked(Top), T.CreateChecked(Right), T.CreateChecked(Bottom));
#else
            return new(Cast<double, T>(Left), Cast<double, T>(Top), Cast<double, T>(Right), Cast<double, T>(Bottom));
#endif
        }
        public static Bound<T> Rotate(Bound<T> Bound, T Ox, T Oy, double Theta)
        {
            T Left = Bound.Left,
              Top = Bound.Top,
              Right = Bound.Right,
              Bottom = Bound.Bottom;
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Point<T>.Rotate(Left, Top, Ox, Oy, Sin, Cos, out T X0, out T Y0);
            Point<T>.Rotate(Left, Bottom, Ox, Oy, Sin, Cos, out T X1, out T Y1);
            Point<T>.Rotate(Right, Top, Ox, Oy, Sin, Cos, out T X2, out T Y2);
            Point<T>.Rotate(Right, Bottom, Ox, Oy, Sin, Cos, out T X3, out T Y3);

            MathHelper.MinAndMax(out Left, out Right, X0, X1, X2, X3);
            MathHelper.MinAndMax(out Top, out Bottom, Y0, Y1, Y2, Y3);

            return new(Left, Top, Right, Bottom);
        }

        public static Bound<T> Union(Bound<T> Bound1, Bound<T> Bound2)
        {
            //if (Bound1.IsEmpty)
            //    return new(Bound2.Left, Bound2.Top, Bound2.Right, Bound2.Bottom);

            //if (Bound2.IsEmpty)
            //    return new(Bound1.Left, Bound1.Top, Bound1.Right, Bound1.Bottom);

#if NET7_0_OR_GREATER
            return new(T.Min(Bound1.Left, Bound2.Left),
                       T.Min(Bound1.Top, Bound2.Top),
                       T.Max(Bound1.Right, Bound2.Right),
                       T.Max(Bound1.Bottom, Bound2.Bottom));
#else
            return new(Min(Bound1.Left, Bound2.Left),
                       Min(Bound1.Top, Bound2.Top),
                       Max(Bound1.Right, Bound2.Right),
                       Max(Bound1.Bottom, Bound2.Bottom));
#endif
        }
        public static Bound<T> Union(Bound<T> Bound, T Left, T Top, T Right, T Bottom)
        {
            //if (Bound.IsEmpty)
            //    return new(Left, Top, Right, Bottom);

            //if (IsDefault(Sub(Right, Left)) && IsDefault(Sub(Bottom, Top)))
            //    return new(Bound.Left, Bound.Top, Bound.Right, Bound.Bottom);

#if NET7_0_OR_GREATER
            return new(T.Min(Bound.Left, Left),
                       T.Min(Bound.Top, Top),
                       T.Max(Bound.Right, Right),
                       T.Max(Bound.Bottom, Bottom));
#else
            return new(Min(Bound.Left, Left),
                       Min(Bound.Top, Top),
                       Max(Bound.Right, Right),
                       Max(Bound.Bottom, Bottom));
#endif
        }

        public static Bound<T> Intersect(Bound<T> Bound1, Bound<T> Bound2)
        {
            if (Bound1.IntersectsWith(Bound2))
#if NET7_0_OR_GREATER
                return new(T.Max(Bound1.Left, Bound2.Left),
                           T.Clamp(Bound2.Right, Bound1.Left, Bound1.Right),    // T.Max(T.Min(Bound1.Right, Bound2.Right), Bound1.Left),
                           T.Max(Bound1.Top, Bound2.Top),
                           T.Clamp(Bound2.Bottom, Bound1.Top, Bound1.Bottom));  // T.Max(T.Min(Bound1.Bottom, Bound2.Bottom), Bound1.Top));
#else
                return new(Max(Bound1.Left, Bound2.Left),
                           Max(Min(Bound1.Right, Bound2.Right), Bound1.Left),
                           Max(Bound1.Top, Bound2.Top),
                           Max(Min(Bound1.Bottom, Bound2.Bottom), Bound1.Top));
#endif

            return Empty;
        }
        public static Bound<T> Intersect(Bound<T> Bound, T Left, T Top, T Right, T Bottom)
        {
            if (Bound.IntersectsWith(Left, Top, Right, Bottom))
#if NET7_0_OR_GREATER
                return new(T.Max(Bound.Left, Left),
                           T.Clamp(Right, Bound.Left, Bound.Right),     // T.Max(T.Min(Bound.Right, Right), Bound.Left),
                           T.Max(Bound.Top, Top),
                           T.Clamp(Bottom, Bound.Top, Bound.Bottom));   // T.Max(T.Min(Bound.Bottom, Bottom), Bound.Top));
#else
                return new(Max(Bound.Left, Left),
                           Max(Min(Bound.Right, Right), Bound.Left),
                           Max(Bound.Top, Top),
                           Max(Min(Bound.Bottom, Bottom), Bound.Top));
#endif

            return Empty;
        }

        public static Bound<T> operator +(Bound<T> Bound1, Bound<T> Bound2)
            => Union(Bound1, Bound2);
        public static Bound<T> operator -(Bound<T> Bound1, Bound<T> Bound2)
            => Intersect(Bound1, Bound2);
        public static Bound<T> operator *(Bound<T> Bound, T Factor)
            => Scale(Bound, Factor);
        public static Bound<T> operator /(Bound<T> Bound, T Factor)
        {
#if NET7_0_OR_GREATER
            return new(Bound.Left / Factor, Bound.Top / Factor, Bound.Right / Factor, Bound.Bottom / Factor);
#else
            return new(Divide(Bound.Left, Factor), Divide(Bound.Top, Factor), Divide(Bound.Right, Factor), Divide(Bound.Bottom, Factor));
#endif
        }

        public static bool operator ==(Bound<T> Bound1, Bound<T> Bound2)
            => Bound1.Equals(Bound2);
        public static bool operator !=(Bound<T> Bound1, Bound<T> Bound2)
            => !Bound1.Equals(Bound2);

    }
}