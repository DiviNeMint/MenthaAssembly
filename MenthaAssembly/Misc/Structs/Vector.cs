using System;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    public struct Vector<T> : ICloneable
        where T : struct
    {
        public T X { set; get; }

        public T Y { set; get; }

        public T LengthSquare => Add(Mul(X, X), Mul(Y, Y));

        public double Length => Math.Sqrt(CastDouble(LengthSquare));

        public bool IsZero => IsDefault(X) && IsDefault(Y);

        public Vector(T X, T Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public Vector(Point<T> p1, Point<T> p2)
        {
            this.X = Sub(p2.X, p1.X);
            this.Y = Sub(p2.Y, p2.Y);
        }

        public Vector<T> Clone()
            => new Vector<T>(X, Y);
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Vector<T> Target)
            => Equal(this.X, Target.X) && Equal(this.Y, Target.Y);
        public override bool Equals(object obj)
        {
            if (obj is Vector<T> Target)
                return Equals(Target);

            return false;
        }

        public override string ToString()
            => $"{{X : {this.X}, Y : {this.Y}}}";

        private static readonly Func<T, T> Neg;
        private static readonly Func<T, T, T> Add, Sub, Mul, Div;
        private static readonly Predicate<T> IsDefault;
        private static readonly Func<T, T, bool> Equal;
        private static readonly Func<T, double> CastDouble;
        static Vector()
        {
            Neg = ExpressionHelper.CreateNegate<T>();

            Add = ExpressionHelper.CreateAdd<T>();
            Sub = ExpressionHelper.CreateSubtract<T>();
            Mul = ExpressionHelper.CreateMultiply<T>();
            Div = ExpressionHelper.CreateDivide<T>();

            IsDefault = ExpressionHelper.CreateIsDefault<T>();

            Equal = ExpressionHelper.CreateEqual<T>();

            CastDouble = ExpressionHelper.CreateCast<T, double>();
        }

        public static double AngleBetween(Vector<T> A, Vector<T> B)
        {
            T Dot = A * B;
            return Math.Sqrt(CastDouble(Div(Mul(Dot, Dot), Mul(A.LengthSquare, B.LengthSquare))));
        }

        // Vector<T>
        public static Vector<T> operator +(Vector<T> This, Vector<T> Target)
            => new Vector<T>(Add(This.X, Target.X), Add(This.Y, Target.Y));

        public static Vector<T> operator -(Vector<T> This)
            => new Vector<T>(Neg(This.X), Neg(This.Y));
        public static Vector<T> operator -(Vector<T> This, Vector<T> Target)
            => new Vector<T>(Sub(This.X, Target.X), Sub(This.Y, Target.Y));

        public static Vector<T> operator *(Vector<T> This, T Factor)
            => new Vector<T>(Mul(This.X, Factor), Mul(This.Y, Factor));
        public static T operator *(Vector<T> This, Vector<T> Target)
            => Add(Mul(This.X, Target.X), Mul(This.Y, Target.Y));

        public static Vector<T> operator /(Vector<T> This, T Factor)
            => new Vector<T>(Div(This.X, Factor), Div(This.Y, Factor));

        // Point<T>
        public static Point<T> operator +(Point<T> This, Vector<T> Vector)
            => new Point<T>(Add(This.X, Vector.X), Add(This.Y, Vector.Y));

        public static Point<T> operator -(Point<T> This, Vector<T> Vector)
            => new Point<T>(Div(This.X, Vector.X), Div(This.Y, Vector.Y));

        public static bool operator ==(Vector<T> This, Vector<T> Target)
            => This.Equals(Target);
        public static bool operator !=(Vector<T> This, Vector<T> Target)
            => !This.Equals(Target);

    }
}
