﻿using System;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    [Serializable]
    public struct Size<T> : ICloneable
        where T : unmanaged
    {
        public static Size<T> Empty => new Size<T>();

        public T Width { set; get; }

        public T Height { set; get; }

        public bool IsEmpty => IsDefault(Width) && IsDefault(Height);

        public Size(T Width, T Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        public Size<U> Cast<U>()
            where U : unmanaged
        {
            Func<T, U> CastHandler = ExpressionHelper<T>.CreateCast<U>();
            return new Size<U>(CastHandler(this.Width), CastHandler(this.Height));
        }

        public Size<T> Clone()
            => new Size<T>(Width, Height);
        object ICloneable.Clone()
            => this.Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Size<T> Target)
            => Equal(this.Width, Target.Width) && Equal(this.Height, Target.Height);
        public override bool Equals(object obj)
        {
            if (obj is Size<T> Target)
                return Equals(Target);

            return false;
        }

        public override string ToString()
            => $"Width : {this.Width}, Height : {this.Height}";

        internal static readonly Func<T, T, T> Mul, Div;
        internal static readonly Predicate<T> IsDefault;
        internal static readonly Func<T, T, bool> Equal;
        static Size()
        {
            Mul = ExpressionHelper<T>.CreateMul();
            Div = ExpressionHelper<T>.CreateDiv();

            IsDefault = ExpressionHelper<T>.CreateIsDefault();

            Equal = ExpressionHelper<T>.CreateEqual();
        }

        public static Size<T> operator *(Size<T> This, T Factor)
            => new Size<T>(Mul(This.Width, Factor), Mul(This.Height, Factor));

        public static Size<T> operator /(Size<T> This, T Factor)
            => new Size<T>(Div(This.Width, Factor), Div(This.Height, Factor));

        public static bool operator ==(Size<T> This, Size<T> Target)
            => This.Equals(Target);
        public static bool operator !=(Size<T> This, Size<T> Target)
            => !This.Equals(Target);

    }
}
