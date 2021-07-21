using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MenthaAssembly
{
    public struct Bound<T> : ICloneable
        where T : struct
    {
        public static Bound<T> Empty => new Bound<T>();

        public T Left { set; get; }

        public T Top { set; get; }

        public T Right { set; get; }

        public T Bottom { set; get; }

        public T Width => Sub(Right, Left);

        public T Height => Sub(Bottom, Top);

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
            Right = Add(Left, Mul(Width, XScale));
            Bottom = Add(Top, Mul(Height, YScale));
        }

        public Bound<T> Clone()
            => new Bound<T>(this.Left, this.Top, this.Right, this.Bottom);
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Bound<T> Target)
            => Equal(this.Width, Target.Width) && Equal(this.Height, Target.Height);
        public override bool Equals(object obj)
        {
            if (obj is Bound<T> Target)
                return Equals(Target);

            return false;
        }

        public override string ToString()
            => $"{{Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom}}}";

        private static readonly Func<T, T> Neg;
        private static readonly Func<T, T, T> Add, Sub, Mul, Div;
        private static readonly Predicate<T> IsDefault;
        private static readonly Func<T, T, bool> Equal;
        private static readonly Func<T, double> CastDouble;
        static Bound()
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

        public static Bound<T> Offset(Bound<T> Bound, Vector<T> Vector)
            => Offset(Bound, Vector.X, Vector.Y);
        public static Bound<T> Offset(Bound<T> Bound, T X, T Y)
            => new Bound<T>(Add(Bound.Left, X), Add(Bound.Top, Y), Add(Bound.Right, X), Add(Bound.Bottom, Y));

        public static Bound<T> Scale(Bound<T> Bound, T Scale)
            => Bound<T>.Scale(Bound, Scale, Scale);
        public static Bound<T> Scale(Bound<T> Bound, T XScale, T YScale)
            => new Bound<T>(Bound.Left, Bound.Top, Add(Bound.Left, Mul(Bound.Width, XScale)), Add(Bound.Top, Mul(Bound.Height, YScale)));

    }
}
