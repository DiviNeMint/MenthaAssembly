using System;
using static MenthaAssembly.OperatorHelper;

namespace MenthaAssembly
{
    [Serializable]
    public struct Size<T> : ICloneable
        where T : unmanaged
    {
        public static Size<T> Empty => new();

        public T Width { set; get; }

        public T Height { set; get; }

        public bool IsEmpty
            => IsDefault(Width) && IsDefault(Height);

        public Size(T Width, T Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        public Size<U> Cast<U>()
            where U : unmanaged
            => new(Cast<T, U>(Width), Cast<T, U>(Height));

        public Size<T> Clone()
            => new(Width, Height);
        object ICloneable.Clone()
            => Clone();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool Equals(Size<T> Target)
            => Equal(Width, Target.Width) && Equal(Height, Target.Height);
        public override bool Equals(object obj)
            => obj is Size<T> Target && Equals(Target);

        public override string ToString()
            => $"Width : {Width}, Height : {Height}";

        public static Size<T> operator *(Size<T> This, T Factor)
            => new(Multiply(This.Width, Factor), Multiply(This.Height, Factor));

        public static Size<T> operator /(Size<T> This, T Factor)
            => new(Divide(This.Width, Factor), Divide(This.Height, Factor));

        public static bool operator ==(Size<T> This, Size<T> Target)
            => This.Equals(Target);
        public static bool operator !=(Size<T> This, Size<T> Target)
            => !This.Equals(Target);

    }
}
