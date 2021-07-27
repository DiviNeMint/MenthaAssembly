using System;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    [Serializable]
    public struct Bound<T> : ICloneable
        where T : struct
    {
        public static Bound<T> Empty => new Bound<T>();

        public T Left { set; get; }

        public T Top { set; get; }

        public T Right { set; get; }

        public T Bottom { set; get; }

        public T Width => Sub(this.Right, this.Left);

        public T Height => Sub(this.Bottom, this.Top);

        public Point<T> Center => new Point<T>(ToGeneric(ToDouble(Add(this.Left, this.Right)) * 0.5d), ToGeneric(ToDouble(Add(this.Top, this.Bottom)) * 0.5d));

        public bool IsEmpty => IsDefault(Width) && IsDefault(Height);

        public Bound(T Left, T Top, T Right, T Bottom)
        {
            this.Left = Left;
            this.Top = Top;
            this.Right = Right;
            this.Bottom = Bottom;
        }
        public Bound(Point<T> Position, Size<T> Size)
        {
            this.Left = Position.X;
            this.Top = Position.Y;
            this.Right = Add(Position.X, Size.Width);
            this.Bottom = Add(Position.Y, Size.Height);
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
            this.Left = Mul(this.Left, XScale);
            this.Top = Mul(this.Top, YScale);
            this.Right = Mul(this.Right, XScale);
            this.Bottom = Mul(this.Bottom, YScale);
        }

        public void ScaleSize(T Scale)
            => this.ScaleSize(Scale, Scale);
        public void ScaleSize(T XScale, T YScale)
        {
            this.Right = Add(this.Left, Mul(this.Width, XScale));
            this.Bottom = Add(this.Top, Mul(this.Height, YScale));
        }

        public void Rotate(double Theta)
        {
            double DL = ToDouble(this.Left),
                   DT = ToDouble(this.Top),
                   DR = ToDouble(this.Right),
                   DB = ToDouble(this.Bottom),
                   DOx = (DL + DR) * 0.5d,
                   DOy = (DT + DB) * 0.5d;

            MathHelper.Rotate(DL, DT, DOx, DOy, Theta, out double X0, out double Y0);
            MathHelper.Rotate(DL, DB, DOx, DOy, Theta, out double X1, out double Y1);
            MathHelper.Rotate(DR, DT, DOx, DOy, Theta, out double X2, out double Y2);
            MathHelper.Rotate(DR, DB, DOx, DOy, Theta, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            this.Left = ToGeneric(Min);
            this.Right = ToGeneric(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            this.Top = ToGeneric(Min);
            this.Bottom = ToGeneric(Max);
        }
        public void Rotate(T Ox, T Oy, double Theta)
        {
            double DL = ToDouble(this.Left),
                   DT = ToDouble(this.Top),
                   DR = ToDouble(this.Right),
                   DB = ToDouble(this.Bottom),
                   DOx = ToDouble(Ox),
                   DOy = ToDouble(Oy);

            MathHelper.Rotate(DL, DT, DOx, DOy, Theta, out double X0, out double Y0);
            MathHelper.Rotate(DL, DB, DOx, DOy, Theta, out double X1, out double Y1);
            MathHelper.Rotate(DR, DT, DOx, DOy, Theta, out double X2, out double Y2);
            MathHelper.Rotate(DR, DB, DOx, DOy, Theta, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            this.Left = ToGeneric(Min);
            this.Right = ToGeneric(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            this.Top = ToGeneric(Min);
            this.Bottom = ToGeneric(Max);
        }

        public void Union(Bound<T> Bound)
        {
            //if (IsEmpty)
            //{
            //    this = Bound;
            //    return;
            //}

            //if (Bound.IsEmpty)
            //    return;

            this.Left = Min(this.Left, Bound.Left);
            this.Right = Max(this.Right, Bound.Right);
            this.Top = Min(this.Top, Bound.Top);
            this.Bottom = Max(this.Bottom, Bound.Bottom);
        }
        public void Union(T Left, T Top, T Right, T Bottom)
        {
            //if (IsEmpty)
            //{
            //    this = new Bound<T>(Left, Top, Right, Bottom);
            //    return;
            //}

            //if (IsDefault(Sub(Right, Left)) && IsDefault(Sub(Bottom, Top)))
            //    return;

            this.Left = Min(this.Left, Left);
            this.Right = Max(this.Right, Right);
            this.Top = Min(this.Top, Top);
            this.Bottom = Max(this.Bottom, Bottom);
        }

        public void Intersect(Bound<T> Bound)
        {
            if (this.IntersectsWith(Bound))
            {
                this.Left = Max(this.Left, Bound.Left);
                this.Right = Max(Min(this.Right, Bound.Right), this.Left);
                this.Top = Max(this.Top, Bound.Top);
                this.Bottom = Max(Min(this.Bottom, Bound.Bottom), this.Top);
            }
            else
            {
                this = Empty;
            }
        }
        public void Intersect(T Left, T Top, T Right, T Bottom)
        {
            if (this.IntersectsWith(Left, Top, Right, Bottom))
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

            return LessThanOrEqual(Bound.Left, this.Right) &&
                   GreaterThanOrEqual(Bound.Right, this.Left) &&
                   LessThanOrEqual(Bound.Top, this.Bottom) &&
                   GreaterThanOrEqual(Bound.Bottom, this.Top);
        }
        public bool IntersectsWith(T Left, T Top, T Right, T Bottom)
        {
            if (IsEmpty || (IsDefault(Sub(Right, Left)) && IsDefault(Sub(Bottom, Top))))
                return false;

            return LessThanOrEqual(Left, this.Right) &&
                   GreaterThanOrEqual(Right, this.Left) &&
                   LessThanOrEqual(Top, this.Bottom) &&
                   GreaterThanOrEqual(Bottom, this.Top);
        }

        public bool Contains(Point<T> Point)
            => Contains(Point.X, Point.Y);
        public bool Contains(T X, T Y)
        {
            if (IsEmpty)
                return false;

            return LessThan(Left, X) && LessThan(X, Right) &&
                   LessThan(Top, Y) && LessThan(Y, Bottom);
        }

        public Bound<U> Cast<U>()
            where U : struct
        {
            Func<T, U> CastHandler = ExpressionHelper<T>.CreateCast<U>();
            return new Bound<U>(CastHandler(this.Left), CastHandler(this.Top), CastHandler(this.Right), CastHandler(this.Bottom));
        }

        public Bound<T> Clone()
            => new Bound<T>(this.Left, this.Top, this.Right, this.Bottom);
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Bound<T> Target)
            => Equal(this.Left, Target.Left) && Equal(this.Top, Target.Top) && Equal(this.Right, Target.Right) && Equal(this.Bottom, Target.Bottom);
        public override bool Equals(object obj)
        {
            if (obj is Bound<T> Target)
                return Equals(Target);

            return false;
        }

        public override string ToString()
            => $"{{Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom}}}";

        internal readonly static Func<T, T, T> Add, Sub, Mul, Div;
        internal readonly static Predicate<T> IsDefault;
        internal readonly static Func<T, double> ToDouble;
        internal readonly static Func<double, T> ToGeneric;
        internal readonly static Func<T, T, bool> Equal, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual;
        internal readonly static Func<T, T, T> Min, Max;
        static Bound()
        {
            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();
            Div = ExpressionHelper<T>.CreateDiv();

            IsDefault = ExpressionHelper<T>.CreateIsDefault();
            ToDouble = ExpressionHelper<T>.CreateCast<double>();
            ToGeneric = ExpressionHelper<double>.CreateCast<T>();

            Equal = ExpressionHelper<T>.CreateEqual();
            GreaterThan = ExpressionHelper<T>.CreateGreaterThan();
            LessThan = ExpressionHelper<T>.CreateLessThan();
            GreaterThanOrEqual = ExpressionHelper<T>.CreateGreaterThanOrEqual();
            LessThanOrEqual = ExpressionHelper<T>.CreateLessThanOrEqual();

            Min = (a, b) => LessThan(a, b) ? a : b;
            Max = (a, b) => GreaterThan(a, b) ? a : b;
        }

        public static Bound<T> Offset(Bound<T> Bound, Vector<T> Vector)
            => Offset(Bound, Vector.X, Vector.Y);
        public static Bound<T> Offset(Bound<T> Bound, T X, T Y)
            => new Bound<T>(Add(Bound.Left, X), Add(Bound.Top, Y), Add(Bound.Right, X), Add(Bound.Bottom, Y));

        public static Bound<T> Scale(Bound<T> Bound, T Scale)
            => Bound<T>.Scale(Bound, Scale, Scale);
        public static Bound<T> Scale(Bound<T> Bound, T XScale, T YScale)
            => new Bound<T>(Mul(Bound.Left, XScale), Mul(Bound.Top, YScale), Mul(Bound.Right, XScale), Mul(Bound.Bottom, YScale));

        public static Bound<T> ScaleSize(Bound<T> Bound, T Scale)
            => Bound<T>.ScaleSize(Bound, Scale, Scale);
        public static Bound<T> ScaleSize(Bound<T> Bound, T XScale, T YScale)
            => new Bound<T>(Bound.Left, Bound.Top, Add(Bound.Left, Mul(Bound.Width, XScale)), Add(Bound.Top, Mul(Bound.Height, YScale)));

        public static Bound<T> Rotate(Bound<T> Bound, double Theta)
        {
            Bound<T> R = new Bound<T>();

            double DL = ToDouble(Bound.Left),
                   DT = ToDouble(Bound.Top),
                   DR = ToDouble(Bound.Right),
                   DB = ToDouble(Bound.Bottom),
                   DOx = (DL + DR) * 0.5d,
                   DOy = (DT + DB) * 0.5d;

            MathHelper.Rotate(DL, DT, DOx, DOy, Theta, out double X0, out double Y0);
            MathHelper.Rotate(DL, DB, DOx, DOy, Theta, out double X1, out double Y1);
            MathHelper.Rotate(DR, DT, DOx, DOy, Theta, out double X2, out double Y2);
            MathHelper.Rotate(DR, DB, DOx, DOy, Theta, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            R.Left = ToGeneric(Min);
            R.Right = ToGeneric(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            R.Top = ToGeneric(Min);
            R.Bottom = ToGeneric(Max);

            return R;
        }
        public static Bound<T> Rotate(Bound<T> Bound, T Ox, T Oy, double Theta)
        {
            Bound<T> R = new Bound<T>();

            double DL = ToDouble(Bound.Left),
                   DT = ToDouble(Bound.Top),
                   DR = ToDouble(Bound.Right),
                   DB = ToDouble(Bound.Bottom),
                   DOx = ToDouble(Ox),
                   DOy = ToDouble(Oy);

            MathHelper.Rotate(DL, DT, DOx, DOy, Theta, out double X0, out double Y0);
            MathHelper.Rotate(DL, DB, DOx, DOy, Theta, out double X1, out double Y1);
            MathHelper.Rotate(DR, DT, DOx, DOy, Theta, out double X2, out double Y2);
            MathHelper.Rotate(DR, DB, DOx, DOy, Theta, out double X3, out double Y3);

            MathHelper.MinAndMax(out double Min, out double Max, X0, X1, X2, X3);
            R.Left = ToGeneric(Min);
            R.Right = ToGeneric(Max);

            MathHelper.MinAndMax(out Min, out Max, Y0, Y1, Y2, Y3);
            R.Top = ToGeneric(Min);
            R.Bottom = ToGeneric(Max);

            return R;
        }

        public static Bound<T> Union(Bound<T> Bound1, Bound<T> Bound2)
        {
            //if (Bound1.IsEmpty)
            //    return new Bound<T>(Bound2.Left, Bound2.Top, Bound2.Right, Bound2.Bottom);

            //if (Bound2.IsEmpty)
            //    return new Bound<T>(Bound1.Left, Bound1.Top, Bound1.Right, Bound1.Bottom);

            return new Bound<T>(Min(Bound1.Left, Bound2.Left),
                                Min(Bound1.Top, Bound2.Top),
                                Max(Bound1.Right, Bound2.Right),
                                Max(Bound1.Bottom, Bound2.Bottom));
        }
        public static Bound<T> Union(Bound<T> Bound, T Left, T Top, T Right, T Bottom)
        {
            //if (Bound.IsEmpty)
            //    return new Bound<T>(Left, Top, Right, Bottom);

            //if (IsDefault(Sub(Right, Left)) && IsDefault(Sub(Bottom, Top)))
            //    return new Bound<T>(Bound.Left, Bound.Top, Bound.Right, Bound.Bottom);

            return new Bound<T>(Min(Bound.Left, Left),
                                Min(Bound.Top, Top),
                                Max(Bound.Right, Right),
                                Max(Bound.Bottom, Bottom));
        }

        public static Bound<T> Intersect(Bound<T> Bound1, Bound<T> Bound2)
        {
            if (Bound1.IntersectsWith(Bound2))
                return new Bound<T>(Max(Bound1.Left, Bound2.Left),
                                    Max(Min(Bound1.Right, Bound2.Right), Bound1.Left),
                                    Max(Bound1.Top, Bound2.Top),
                                    Max(Min(Bound1.Bottom, Bound2.Bottom), Bound1.Top));

            return Empty;
        }
        public static Bound<T> Intersect(Bound<T> Bound, T Left, T Top, T Right, T Bottom)
        {
            if (Bound.IntersectsWith(Left, Top, Right, Bottom))
                return new Bound<T>(Max(Bound.Left, Left),
                                    Max(Min(Bound.Right, Right), Bound.Left),
                                    Max(Bound.Top, Top),
                                    Max(Min(Bound.Bottom, Bottom), Bound.Top));

            return Empty;
        }

        public static Bound<T> operator +(Bound<T> This, Bound<T> Target)
            => Union(This, Target);
        public static Bound<T> operator -(Bound<T> This, Bound<T> Target)
            => Intersect(This, Target);
        public static Bound<T> operator *(Bound<T> This, T Factor)
            => Scale(This, Factor);
        public static Bound<T> operator /(Bound<T> This, T Factor)
            => new Bound<T>(Div(This.Left, Factor), Div(This.Top, Factor), Div(This.Right, Factor), Div(This.Bottom, Factor));

        public static bool operator ==(Bound<T> This, Bound<T> Target)
                => This.Equals(Target);
        public static bool operator !=(Bound<T> This, Bound<T> Target)
            => !This.Equals(Target);

    }
}
