using System;
using System.Linq.Expressions;

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
            => Equal(this.Value, obj.Value);
        public override bool Equals(object obj)
        {
            if (obj is Operatorable<T> Target)
                return Equals(Target);

            if (obj is T Value)
                return Equal(this.Value, Value);

            return false;
        }

        public override string ToString() => $"{Value}";

        private readonly static Func<T, T> Neg;
        private readonly static Func<T, T, T> Add, Sub, Mul, Div;
        private readonly static Func<T, T, bool> Equal, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual;
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
