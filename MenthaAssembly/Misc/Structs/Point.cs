using System;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    [Serializable]
    public struct Point<T> : ICloneable
        where T : struct
    {
        public T X { set; get; }

        public T Y { set; get; }

        public bool IsOriginalPoint => IsDefault(X) && IsDefault(Y);

        public Point(T X, T Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public void Rotate(Point<T> OriginalPoint, double Theta)
            => Rotate(OriginalPoint.X, OriginalPoint.Y, Theta);
        public void Rotate(T Ox, T Oy, double Theta)
        {
            MathHelper.Rotate(ToDouble(X), ToDouble(Y), ToDouble(Ox), ToDouble(Oy), Theta, out double Nx, out double Ny);
            this.X = ToGeneric(Nx);
            this.Y = ToGeneric(Ny);
        }

        public Point<U> Cast<U>()
            where U : struct
        {
            Func<T, U> CastHandler = ExpressionHelper<T>.CreateCast<U>();
            return new Point<U>(CastHandler(this.X), CastHandler(this.Y));
        }

        public Point<T> Clone()
            => new Point<T>(X, Y);
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Point<T> Target)
            => Equal(this.X, Target.X) && Equal(this.Y, Target.Y);
        public override bool Equals(object obj)
        {
            if (obj is Point<T> Target)
                return Equals(Target);

            return false;
        }

        public override string ToString()
            => $"{{X : {this.X}, Y : {this.Y}}}";

        internal static readonly Func<T, T> Neg;
        internal static readonly Func<T, T, T> Sub, Mul, Div;
        internal static readonly Predicate<T> IsDefault;
        internal static readonly Func<T, T, bool> Equal;
        internal static readonly Func<T, double> ToDouble;
        internal static readonly Func<double, T> ToGeneric;
        static Point()
        {
            Neg = ExpressionHelper<T>.CreateNeg();

            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();
            Div = ExpressionHelper<T>.CreateDiv();

            IsDefault = ExpressionHelper<T>.CreateIsDefault();
            Equal = ExpressionHelper<T>.CreateEqual();

            ToDouble = ExpressionHelper<T>.CreateCast<double>();
            ToGeneric = ExpressionHelper<double>.CreateCast<T>();
        }

        public static Point<T> Rotate(Point<T> Point, Point<T> OriginalPoint, double Theta)
            => Rotate(Point, OriginalPoint.X, OriginalPoint.Y, Theta);
        public static Point<T> Rotate(Point<T> Point, T Ox, T Oy, double Theta)
        {
            MathHelper.Rotate(ToDouble(Point.X), ToDouble(Point.Y), ToDouble(Ox), ToDouble(Oy), Theta, out double Nx, out double Ny);
            return new Point<T>(ToGeneric(Nx), ToGeneric(Ny));
        }

        public static Point<T> operator -(Point<T> This)
            => new Point<T>(Neg(This.X), Neg(This.Y));
        public static Point<T> operator *(Point<T> This, T Factor)
            => new Point<T>(Mul(This.X, Factor), Mul(This.Y, Factor));
        public static Point<T> operator /(Point<T> This, T Factor)
            => new Point<T>(Div(This.X, Factor), Div(This.Y, Factor));

        public static Vector<T> operator -(Point<T> This, Point<T> Target)
            => new Vector<T>(Sub(This.X, Target.X), Sub(This.Y, Target.Y));

        public static bool operator ==(Point<T> This, Point<T> Target)
            => This.Equals(Target);
        public static bool operator !=(Point<T> This, Point<T> Target)
            => !This.Equals(Target);

    }
}
