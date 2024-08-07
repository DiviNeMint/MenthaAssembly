using System;
using System.Reflection;
using static MenthaAssembly.OperatorHelper;

namespace MenthaAssembly
{
#if NET7_0_OR_GREATER
    [Obsolete("Microsoft allows operators to be defined in the interface in .Net7.0.")]
#endif
    public record struct Operatorable<T>(T Value) : IEquatable<Operatorable<T>>
    {
        public override readonly int GetHashCode()
            => Value.GetHashCode();

        public readonly bool Equals(Operatorable<T> obj)
            => OperatorHelper.Equals(Value, obj.Value);

        public override readonly string ToString()
            => $"{Value}";

        private static bool HasMinValue = false;
        private static T _MinValue;
        public static T MinValue
        {
            get
            {
                if (!HasMinValue)
                {
                    _MinValue = typeof(T).TryGetConstant(nameof(MinValue), out T Value) ? Value : default;
                    HasMinValue = true;
                }

                return _MinValue;
            }
        }

        private static bool HasMaxValue = false;
        private static T _MaxValue;
        public static T MaxValue
        {
            get
            {
                if (!HasMaxValue)
                {
                    _MaxValue = typeof(T).TryGetConstant(nameof(MaxValue), out T Value) ? Value : default;
                    HasMaxValue = true;
                }

                return _MaxValue;
            }
        }

        public static bool TryGetConstant(string Name, out T Value)
            => typeof(T).TryGetConstant(Name, out Value);
        public static T GetConstant(string Name)
            => typeof(T).TryGetConstant(Name, out T Value) ? Value : throw new NotSupportedException();

        public static Operatorable<T> operator -(Operatorable<T> This)
            => new(Negate(This.Value));
        public static Operatorable<T> operator +(Operatorable<T> This, Operatorable<T> Target)
            => new(Add(This.Value, Target.Value));
        public static Operatorable<T> operator -(Operatorable<T> This, Operatorable<T> Target)
            => new(Subtract(This.Value, Target.Value));
        public static Operatorable<T> operator *(Operatorable<T> This, Operatorable<T> Target)
            => new(Multiply(This.Value, Target.Value));
        public static Operatorable<T> operator /(Operatorable<T> This, Operatorable<T> Target)
            => new(Divide(This.Value, Target.Value));

        public static Operatorable<T> operator +(Operatorable<T> This, int Target)
            => new(Add(This.Value, Cast<int, T>(Target)));
        public static Operatorable<T> operator -(Operatorable<T> This, int Target)
            => new(Subtract(This.Value, Cast<int, T>(Target)));
        public static Operatorable<T> operator *(Operatorable<T> This, int Target)
            => new(Multiply(This.Value, Cast<int, T>(Target)));
        public static Operatorable<T> operator /(Operatorable<T> This, int Target)
            => new(Divide(This.Value, Cast<int, T>(Target)));

        public static Operatorable<T> operator ++(Operatorable<T> This)
        {
            This.Value = Add(This.Value, Cast<int, T>(1));
            return This;
        }
        public static Operatorable<T> operator --(Operatorable<T> This)
        {
            This.Value = Subtract(This.Value, Cast<int, T>(1));
            return This;
        }

        public static bool operator <(Operatorable<T> This, Operatorable<T> Target)
            => LessThan(This.Value, Target.Value);
        public static bool operator >(Operatorable<T> This, Operatorable<T> Target)
            => GreaterThan(This.Value, Target.Value);

        public static bool operator <=(Operatorable<T> This, Operatorable<T> Target)
            => LessThanOrEqual(This.Value, Target.Value);
        public static bool operator >=(Operatorable<T> This, Operatorable<T> Target)
            => GreaterThanOrEqual(This.Value, Target.Value);

        public static implicit operator Operatorable<T>(T Value)
            => new(Value);
        public static implicit operator T(Operatorable<T> This)
            => This.Value;

    }
}