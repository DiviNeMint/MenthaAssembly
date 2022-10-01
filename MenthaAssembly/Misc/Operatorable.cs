﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MenthaAssembly
{
    public struct Operatorable<T>
    {
        public T Value { get; set; }

        public Operatorable(T Value)
        {
            this.Value = Value;
        }

        public override int GetHashCode()
            => Value.GetHashCode();

        public bool Equals(Operatorable<T> obj)
            => Equal(Value, obj.Value);
        public override bool Equals(object obj)
        {
            if (obj is Operatorable<T> Target)
                return Equals(Target);

            if (obj is T Value)
                return Equal(this.Value, Value);

            return false;
        }

        public override string ToString() => $"{Value}";

        private static readonly Func<T, T> Neg;
        private static readonly Func<T, T, T> Add, Sub, Mul, Div;
        private static readonly Func<T, T, bool> Equal, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual;
        internal static readonly Func<int, T> Cast;
        static Operatorable()
        {
            Neg = ExpressionHelper<T>.CreateNeg();

            Add = ExpressionHelper<T>.CreateAdd();
            Sub = ExpressionHelper<T>.CreateSub();
            Mul = ExpressionHelper<T>.CreateMul();
            Div = ExpressionHelper<T>.CreateDiv();

            Equal = ExpressionHelper<T>.CreateEqual();
            GreaterThan = ExpressionHelper<T>.CreateGreaterThan();
            LessThan = ExpressionHelper<T>.CreateLessThan();
            GreaterThanOrEqual = ExpressionHelper<T>.CreateGreaterThanOrEqual();
            LessThanOrEqual = ExpressionHelper<T>.CreateLessThanOrEqual();

            Cast = ExpressionHelper<int>.CreateCast<T>();
        }

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
        {
            if (typeof(T).TryGetConstant(Name, out T Value))
                return Value;

            throw new NotSupportedException();
        }

        public static Operatorable<T> operator -(Operatorable<T> This)
            => new Operatorable<T>(Neg(This.Value));
        public static Operatorable<T> operator +(Operatorable<T> This, Operatorable<T> Target)
            => new Operatorable<T>(Add(This.Value, Target.Value));
        public static Operatorable<T> operator -(Operatorable<T> This, Operatorable<T> Target)
            => new Operatorable<T>(Sub(This.Value, Target.Value));
        public static Operatorable<T> operator *(Operatorable<T> This, Operatorable<T> Target)
            => new Operatorable<T>(Mul(This.Value, Target.Value));
        public static Operatorable<T> operator /(Operatorable<T> This, Operatorable<T> Target)
            => new Operatorable<T>(Div(This.Value, Target.Value));

        public static Operatorable<T> operator +(Operatorable<T> This, int Target)
            => new Operatorable<T>(Add(This.Value, Cast(Target)));
        public static Operatorable<T> operator -(Operatorable<T> This, int Target)
            => new Operatorable<T>(Sub(This.Value, Cast(Target)));
        public static Operatorable<T> operator *(Operatorable<T> This, int Target)
            => new Operatorable<T>(Mul(This.Value, Cast(Target)));
        public static Operatorable<T> operator /(Operatorable<T> This, int Target)
            => new Operatorable<T>(Div(This.Value, Cast(Target)));

        public static Operatorable<T> operator ++(Operatorable<T> This)
        {
            This.Value = Add(This.Value, Cast(1));
            return This;
        }
        public static Operatorable<T> operator --(Operatorable<T> This)
        {
            This.Value = Sub(This.Value, Cast(1));
            return This;
        }

        public static bool operator ==(Operatorable<T> This, Operatorable<T> Target)
            => Equal(This.Value, Target.Value);
        public static bool operator !=(Operatorable<T> This, Operatorable<T> Target)
            => !Equal(This.Value, Target.Value);

        public static bool operator <(Operatorable<T> This, Operatorable<T> Target)
            => LessThan(This.Value, Target.Value);
        public static bool operator >(Operatorable<T> This, Operatorable<T> Target)
            => GreaterThan(This.Value, Target.Value);

        public static bool operator <=(Operatorable<T> This, Operatorable<T> Target)
            => LessThanOrEqual(This.Value, Target.Value);
        public static bool operator >=(Operatorable<T> This, Operatorable<T> Target)
            => GreaterThanOrEqual(This.Value, Target.Value);

        public static implicit operator Operatorable<T>(T Value) => new Operatorable<T>(Value);
        public static implicit operator T(Operatorable<T> This) => This.Value;

    }
}