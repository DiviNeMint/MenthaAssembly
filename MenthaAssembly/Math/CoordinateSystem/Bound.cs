using System;
using static MenthaAssembly.OperatorHelper;

namespace MenthaAssembly
{
    [Serializable]
    public struct Bound<T> : ICloneable
        where T : unmanaged
    {
        public static Bound<T> Empty => new();

        public T Left { set; get; }

        public T Top { set; get; }

        public T Right { set; get; }

        public T Bottom { set; get; }

        public T Width
            => Subtract(Right, Left);

        public T Height
            => Subtract(Bottom, Top);

        public Point<T> Center
            => new(Half(Add(Left, Right)), Half(Add(Top, Bottom)));

        public Point<T> LeftTop
            => new(Left, Top);

        public Point<T> LeftBottom
            => new(Left, Bottom);

        public Point<T> RightTop
            => new(Right, Top);

        public Point<T> RightBottom
            => new(Right, Bottom);

        public bool IsEmpty
            => IsDefault(Width) && IsDefault(Height);

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
            Right = Add(Position.X, Size.Width);
            Bottom = Add(Position.Y, Size.Height);
        }

        public void Offset(Vector<T> Vector)
            => Offset(Vector.X, Vector.Y);
        public void Offset(T X, T Y)
        {
            Left = Add(Left, X);
            Right = Add(Right, X);
            Top = Add(Top, Y);
            Bottom = Add(Bottom, Y);
        }

        public void Scale(T Scale)
            => this.Scale(Scale, Scale);
        public void Scale(T XScale, T YScale)
        {
            Left = Multiply(Left, XScale);
            Top = Multiply(Top, YScale);
            Right = Multiply(Right, XScale);
            Bottom = Multiply(Bottom, YScale);
        }

        public void ScaleSize(T Scale)
            => ScaleSize(Scale, Scale);
        public void ScaleSize(T XScale, T YScale)
        {
            Right = Add(Left, Multiply(Width, XScale));
            Bottom = Add(Top, Multiply(Height, YScale));
        }

        public void Rotate(double Theta)
        {
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
        }
        public void Rotate(T Ox, T Oy, double Theta)
        {
            double DL = Cast<T, double>(Left),
                   DT = Cast<T, double>(Top),
                   DR = Cast<T, double>(Right),
                   DB = Cast<T, double>(Bottom),
                   DOx = Cast<T, double>(Ox),
                   DOy = Cast<T, double>(Oy),
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

            Left = Min(Left, Bound.Left);
            Right = Max(Right, Bound.Right);
            Top = Min(Top, Bound.Top);
            Bottom = Max(Bottom, Bound.Bottom);
        }
        public void Union(T Left, T Top, T Right, T Bottom)
        {
            if (IsEmpty)
            {
                this = new(Left, Top, Right, Bottom);
                return;
            }

            if (IsDefault(Subtract(Right, Left)) && IsDefault(Subtract(Bottom, Top)))
                return;

            this.Left = Min(this.Left, Left);
            this.Right = Max(this.Right, Right);
            this.Top = Min(this.Top, Top);
            this.Bottom = Max(this.Bottom, Bottom);
        }

        public void Intersect(Bound<T> Bound)
        {
            if (IntersectsWith(Bound))
            {
                Left = Max(Left, Bound.Left);
                Right = Max(Min(Right, Bound.Right), Left);
                Top = Max(Top, Bound.Top);
                Bottom = Max(Min(Bottom, Bound.Bottom), Top);
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
                this.Left = Max(this.Left, Left);
                this.Right = Max(Min(this.Right, Right), this.Left);
                this.Top = Max(this.Top, Top);
                this.Bottom = Max(Min(this.Bottom, Bottom), this.Top);
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

            return LessThanOrEqual(Bound.Left, Right) &&
                   GreaterThanOrEqual(Bound.Right, Left) &&
                   LessThanOrEqual(Bound.Top, Bottom) &&
                   GreaterThanOrEqual(Bound.Bottom, Top);
        }
        public bool IntersectsWith(T Left, T Top, T Right, T Bottom)
        {
            if (IsEmpty || (IsDefault(Subtract(Right, Left)) && IsDefault(Subtract(Bottom, Top))))
                return false;

            return LessThanOrEqual(Left, this.Right) &&
                   GreaterThanOrEqual(Right, this.Left) &&
                   LessThanOrEqual(Top, this.Bottom) &&
                   GreaterThanOrEqual(Bottom, this.Top);
        }

        public bool Contains(Point<T> Point)
            => Contains(Point.X, Point.Y);
        public bool Contains(T X, T Y)
            => !IsEmpty && LessThan(Left, X) && LessThan(X, Right) &&
                           LessThan(Top, Y) && LessThan(Y, Bottom);

        public Bound<U> Cast<U>()
            where U : unmanaged
            => new(Cast<T, U>(Left), Cast<T, U>(Top), Cast<T, U>(Right), Cast<T, U>(Bottom));

        public Bound<T> Clone()
            => new(Left, Top, Right, Bottom);
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Bound<T> Target)
            => Equal(Left, Target.Left) && Equal(Top, Target.Top) && Equal(Right, Target.Right) && Equal(Bottom, Target.Bottom);
        public override bool Equals(object obj)
            => obj is Bound<T> Target && Equals(Target);

        public override string ToString()
            => $"Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom}";

        public static Bound<T> Offset(Bound<T> Bound, Vector<T> Vector)
            => Offset(Bound, Vector.X, Vector.Y);
        public static Bound<T> Offset(Bound<T> Bound, T X, T Y)
            => new(Add(Bound.Left, X), Add(Bound.Top, Y), Add(Bound.Right, X), Add(Bound.Bottom, Y));

        public static Bound<T> Scale(Bound<T> Bound, T Scale)
            => Bound<T>.Scale(Bound, Scale, Scale);
        public static Bound<T> Scale(Bound<T> Bound, T XScale, T YScale)
            => new(Multiply(Bound.Left, XScale), Multiply(Bound.Top, YScale), Multiply(Bound.Right, XScale), Multiply(Bound.Bottom, YScale));

        public static Bound<T> ScaleSize(Bound<T> Bound, T Scale)
            => ScaleSize(Bound, Scale, Scale);
        public static Bound<T> ScaleSize(Bound<T> Bound, T XScale, T YScale)
            => new(Bound.Left, Bound.Top, Add(Bound.Left, Multiply(Bound.Width, XScale)), Add(Bound.Top, Multiply(Bound.Height, YScale)));

        public static Bound<T> Rotate(Bound<T> Bound, double Theta)
        {
            Bound<T> R = new();

            double DL = Cast<T, double>(Bound.Left),
                   DT = Cast<T, double>(Bound.Top),
                   DR = Cast<T, double>(Bound.Right),
                   DB = Cast<T, double>(Bound.Bottom),
                   DOx = (DL + DR) * 0.5d,
                   DOy = (DT + DB) * 0.5d,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Point<double>.Rotate(DL, DT, DOx, DOy, Sin, Cos, out double X0, out double Y0);
            Point<double>.Rotate(DL, DB, DOx, DOy, Sin, Cos, out double X1, out double Y1);
            Point<double>.Rotate(DR, DT, DOx, DOy, Sin, Cos, out double X2, out double Y2);
            Point<double>.Rotate(DR, DB, DOx, DOy, Sin, Cos, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            R.Left = Cast<double, T>(Min);
            R.Right = Cast<double, T>(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            R.Top = Cast<double, T>(Min);
            R.Bottom = Cast<double, T>(Max);

            return R;
        }
        public static Bound<T> Rotate(Bound<T> Bound, T Ox, T Oy, double Theta)
        {
            Bound<T> R = new();

            double DL = Cast<T, double>(Bound.Left),
                   DT = Cast<T, double>(Bound.Top),
                   DR = Cast<T, double>(Bound.Right),
                   DB = Cast<T, double>(Bound.Bottom),
                   DOx = Cast<T, double>(Ox),
                   DOy = Cast<T, double>(Oy),
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Point<double>.Rotate(DL, DT, DOx, DOy, Sin, Cos, out double X0, out double Y0);
            Point<double>.Rotate(DL, DB, DOx, DOy, Sin, Cos, out double X1, out double Y1);
            Point<double>.Rotate(DR, DT, DOx, DOy, Sin, Cos, out double X2, out double Y2);
            Point<double>.Rotate(DR, DB, DOx, DOy, Sin, Cos, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            R.Left = Cast<double, T>(Min);
            R.Right = Cast<double, T>(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            R.Top = Cast<double, T>(Min);
            R.Bottom = Cast<double, T>(Max);

            return R;
        }

        public static Bound<T> Union(Bound<T> Bound1, Bound<T> Bound2)
        {
            //if (Bound1.IsEmpty)
            //    return new(Bound2.Left, Bound2.Top, Bound2.Right, Bound2.Bottom);

            //if (Bound2.IsEmpty)
            //    return new(Bound1.Left, Bound1.Top, Bound1.Right, Bound1.Bottom);

            return new(Min(Bound1.Left, Bound2.Left),
                                Min(Bound1.Top, Bound2.Top),
                                Max(Bound1.Right, Bound2.Right),
                                Max(Bound1.Bottom, Bound2.Bottom));
        }
        public static Bound<T> Union(Bound<T> Bound, T Left, T Top, T Right, T Bottom)
        {
            //if (Bound.IsEmpty)
            //    return new(Left, Top, Right, Bottom);

            //if (IsDefault(Sub(Right, Left)) && IsDefault(Sub(Bottom, Top)))
            //    return new(Bound.Left, Bound.Top, Bound.Right, Bound.Bottom);

            return new(Min(Bound.Left, Left),
                                Min(Bound.Top, Top),
                                Max(Bound.Right, Right),
                                Max(Bound.Bottom, Bottom));
        }

        public static Bound<T> Intersect(Bound<T> Bound1, Bound<T> Bound2)
        {
            if (Bound1.IntersectsWith(Bound2))
                return new(Max(Bound1.Left, Bound2.Left),
                                    Max(Min(Bound1.Right, Bound2.Right), Bound1.Left),
                                    Max(Bound1.Top, Bound2.Top),
                                    Max(Min(Bound1.Bottom, Bound2.Bottom), Bound1.Top));

            return Empty;
        }
        public static Bound<T> Intersect(Bound<T> Bound, T Left, T Top, T Right, T Bottom)
        {
            if (Bound.IntersectsWith(Left, Top, Right, Bottom))
                return new(Max(Bound.Left, Left),
                                    Max(Min(Bound.Right, Right), Bound.Left),
                                    Max(Bound.Top, Top),
                                    Max(Min(Bound.Bottom, Bottom), Bound.Top));

            return Empty;
        }

        public static Bound<T> operator +(Bound<T> Bound1, Bound<T> Bound2)
            => Union(Bound1, Bound2);
        public static Bound<T> operator -(Bound<T> Bound1, Bound<T> Bound2)
            => Intersect(Bound1, Bound2);
        public static Bound<T> operator *(Bound<T> Bound, T Factor)
            => Scale(Bound, Factor);
        public static Bound<T> operator /(Bound<T> Bound, T Factor)
            => new(Divide(Bound.Left, Factor), Divide(Bound.Top, Factor), Divide(Bound.Right, Factor), Divide(Bound.Bottom, Factor));

        public static bool operator ==(Bound<T> Bound1, Bound<T> Bound2)
            => Bound1.Equals(Bound2);
        public static bool operator !=(Bound<T> Bound1, Bound<T> Bound2)
            => !Bound1.Equals(Bound2);

    }
}
