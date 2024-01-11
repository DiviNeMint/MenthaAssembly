using System;
#if NET7_0_OR_GREATER
using System.Numerics;
#else
using static MenthaAssembly.OperatorHelper;
#endif

namespace MenthaAssembly
{
    [Serializable]
    public struct Size<T> : ICloneable
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : unmanaged
#endif
    {
        public static Size<T> Empty => new();

        public T Width { set; get; }

        public T Height { set; get; }

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

        public Size(T Width, T Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        public Size<U> Cast<U>()
#if NET7_0_OR_GREATER
        where U : INumber<U>
#else
        where U : unmanaged
#endif
        {
#if NET7_0_OR_GREATER
            return new(U.CreateChecked(Width), U.CreateChecked(Height));
#else
            return new(Cast<T, U>(Width), Cast<T, U>(Height));
#endif
        }

        public Size<T> Clone()
            => new(Width, Height);
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Size<T> Target)
            => OperatorHelper.Equals(Width, Target.Width) && OperatorHelper.Equals(Height, Target.Height);
        public override bool Equals(object obj)
            => obj is Size<T> Target && Equals(Target);

        public override string ToString()
            => $"Width : {Width}, Height : {Height}";

        public static Size<T> operator *(Size<T> This, T Factor)
        {
#if NET7_0_OR_GREATER
            return new(This.Width * Factor, This.Height * Factor);
#else
            return new(Multiply(This.Width, Factor), Multiply(This.Height, Factor));
#endif
        }

        public static Size<T> operator /(Size<T> This, T Factor)
        {
#if NET7_0_OR_GREATER
            return new(This.Width / Factor, This.Height / Factor);
#else
            return new(Divide(This.Width, Factor), Divide(This.Height, Factor));
#endif
        }

        public static bool operator ==(Size<T> This, Size<T> Target)
            => This.Equals(Target);
        public static bool operator !=(Size<T> This, Size<T> Target)
            => !This.Equals(Target);

    }
}
