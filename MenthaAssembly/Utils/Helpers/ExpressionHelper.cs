using MenthaAssembly;
using System.Collections.Concurrent;
using System.Reflection;

namespace System.Linq.Expressions
{
    public static class ExpressionHelper
    {
        public static object GetValue(this Expression This)
            => This.GetValue<object>();
        public static T GetValue<T>(this Expression This)
        {
            if (This is ConstantExpression CExpression)
                return (T)CExpression.Value;

            UnaryExpression objectMember = Expression.Convert(This, typeof(T));
            return Expression.Lambda<Func<T>>(objectMember)
                             .Compile()
                             .Invoke();
        }
    }

    public static class ExpressionHelper<T>
    {
        private static readonly Type t = typeof(T);

        private static Func<T, T> _Neg;
        public static Func<T, T> CreateNeg()
        {
            if (_Neg is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");

                try
                {
                    _Neg = Expression.Lambda<Func<T, T>>(Expression.Negate(Arg1), Arg1)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Neg = T => throw Ex;
                }
            }

            return _Neg;
        }

        private static Func<T, T, T> _Add, _Sub, _Mul, _Div;
        public static Func<T, T, T> CreateAdd()
        {
            if (_Add is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");
                try
                {
                    _Add = Expression.Lambda<Func<T, T, T>>(Expression.Add(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Add = (a, b) => throw Ex;
                }
            }

            return _Add;
        }
        public static Func<T, T, T> CreateSub()
        {
            if (_Sub is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Sub = Expression.Lambda<Func<T, T, T>>(Expression.Subtract(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Sub = (a, b) => throw Ex;
                }
            }

            return _Sub;
        }
        public static Func<T, T, T> CreateMul()
        {
            if (_Mul is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Mul = Expression.Lambda<Func<T, T, T>>(Expression.Multiply(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Mul = (a, b) => throw Ex;
                }
            }

            return _Mul;
        }
        public static Func<T, T, T> CreateDiv()
        {
            if (_Div is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Div = Expression.Lambda<Func<T, T, T>>(Expression.Divide(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Div = (a, b) => throw Ex;
                }
            }

            return _Div;
        }

        public static readonly ConcurrentDictionary<object, Func<T, T>> _Muls = new(),
                                                                        _Divs = new();
        public static Func<T, T> CreateMul(object Constant)
        {
            if (_Muls.TryGetValue(Constant, out Func<T, T> MulFunc))
                return MulFunc;

            try
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");
                if (typeof(int).Equals(t))
                {
                    if (Constant is not int IntConst)
                        IntConst = Convert.ToInt32(Constant);

                    if (IntConst > 1 &&
                        (IntConst & (IntConst - 1)) == 0)
                    {
                        int Shift = 0;
                        while (IntConst > 1)
                        {
                            IntConst >>= 1;
                            Shift++;
                        }

                        MulFunc = Expression.Lambda<Func<T, T>>(Expression.LeftShift(Arg1, Expression.Constant(Shift)), Arg1)
                                            .Compile();
                    }
                    else
                    {
                        MulFunc = Expression.Lambda<Func<T, T>>(Expression.Multiply(Arg1, Expression.Constant(IntConst)), Arg1)
                                            .Compile();
                    }
                }
                else
                {
                    if (Constant is not T TConst)
                        TConst = (T)Convert.ChangeType(Constant, t);

                    MulFunc = Expression.Lambda<Func<T, T>>(Expression.Multiply(Arg1, Expression.Constant(TConst)), Arg1)
                                        .Compile();
                }

            }
            catch (Exception Ex)
            {
                MulFunc = a => throw Ex;
            }

            _Muls.AddOrUpdate(Constant, MulFunc, (k, v) => MulFunc);
            return MulFunc;
        }
        public static Func<T, T> CreateDiv(object Constant)
        {
            if (_Divs.TryGetValue(Constant, out Func<T, T> DivFunc))
                return DivFunc;

            try
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");
                if (typeof(int).Equals(t))
                {
                    if (Constant is not int IntConst)
                        IntConst = Convert.ToInt32(Constant);

                    if (IntConst > 1 &&
                        (IntConst & (IntConst - 1)) == 0)
                    {
                        int Shift = 0;
                        while (IntConst > 1)
                        {
                            IntConst >>= 1;
                            Shift++;
                        }

                        DivFunc = Expression.Lambda<Func<T, T>>(Expression.RightShift(Arg1, Expression.Constant(Shift)), Arg1)
                                            .Compile();
                    }
                    else
                    {
                        DivFunc = Expression.Lambda<Func<T, T>>(Expression.Divide(Arg1, Expression.Constant(IntConst)), Arg1)
                                            .Compile();
                    }
                }
                else
                {
                    if (Constant is not T TConst)
                        TConst = (T)Convert.ChangeType(Constant, t);

                    DivFunc = Expression.Lambda<Func<T, T>>(Expression.Divide(Arg1, Expression.Constant(TConst)), Arg1)
                                        .Compile();
                }

            }
            catch (Exception Ex)
            {
                DivFunc = a => throw Ex;
            }

            _Divs.AddOrUpdate(Constant, DivFunc, (k, v) => DivFunc);
            return DivFunc;
        }

        private static Func<T, T> _Abs;
        public static Func<T, T> CreateAbs()
        {
            if (_Abs is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");

                try
                {
                    if (t.Equals(typeof(int)) &&
                        typeof(MathHelper).TryGetStaticMethod(nameof(MathHelper.Abs), out MethodInfo IntAbsMethod))
                    {
                        _Abs = Expression.Lambda<Func<T, T>>(Expression.Call(IntAbsMethod, Arg1), Arg1)
                                         .Compile();
                    }
                    //else if (typeof(Math).TryGetStaticMethod(nameof(Math.Abs), new[] { t }, out MethodInfo CommonAbsMethod))
                    //{
                    //    _Abs = Expression.Lambda<Func<T, T>>(Expression.Call(CommonAbsMethod, Arg1), Arg1)
                    //                     .Compile();
                    //}
                    else
                    {
                        LabelTarget Label = Expression.Label(t);
                        ConstantExpression Arg2 = Expression.Constant(default(T));

                        BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Arg2),
                                                                Expression.Return(Label, Arg1, typeof(T)),
                                                                Expression.Return(Label, Expression.Negate(Arg1), typeof(T))),
                                                                Expression.Label(Label, Expression.New(t)));

                        _Abs = Expression.Lambda<Func<T, T>>(Body, Arg1)
                                        .Compile();
                    }
                }
                catch (Exception Ex)
                {
                    _Abs = T => throw Ex;
                }
            }

            return _Abs;
        }

        private static Func<T, T, bool> _Equal, _GreaterThan, _LessThan, _GreaterThanOrEqual, _LessThanOrEqual;
        public static Func<T, T, bool> CreateEqual()
        {
            if (_Equal is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Equal = Expression.Lambda<Func<T, T, bool>>(Expression.Equal(Arg1, Arg2), Arg1, Arg2)
                                       .Compile();
                }
                catch (Exception Ex)
                {
                    _Equal = (a, b) => throw Ex;
                }
            }

            return _Equal;
        }
        public static Func<T, T, bool> CreateGreaterThan()
        {
            if (_GreaterThan is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _GreaterThan = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThan(Arg1, Arg2), Arg1, Arg2)
                                             .Compile();
                }
                catch (Exception Ex)
                {
                    _GreaterThan = (a, b) => throw Ex;
                }
            }

            return _GreaterThan;
        }
        public static Func<T, T, bool> CreateLessThan()
        {
            if (_LessThan is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _LessThan = Expression.Lambda<Func<T, T, bool>>(Expression.LessThan(Arg1, Arg2), Arg1, Arg2)
                                          .Compile();
                }
                catch (Exception Ex)
                {
                    _LessThan = (a, b) => throw Ex;
                }
            }

            return _LessThan;
        }
        public static Func<T, T, bool> CreateGreaterThanOrEqual()
        {
            if (_GreaterThanOrEqual is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _GreaterThanOrEqual = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThanOrEqual(Arg1, Arg2), Arg1, Arg2)
                                                    .Compile();
                }
                catch (Exception Ex)
                {
                    _GreaterThanOrEqual = (a, b) => throw Ex;
                }
            }

            return _GreaterThanOrEqual;
        }
        public static Func<T, T, bool> CreateLessThanOrEqual()
        {
            if (_LessThanOrEqual is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _LessThanOrEqual = Expression.Lambda<Func<T, T, bool>>(Expression.LessThanOrEqual(Arg1, Arg2), Arg1, Arg2)
                                                 .Compile();
                }
                catch (Exception Ex)
                {
                    _LessThanOrEqual = (a, b) => throw Ex;
                }
            }

            return _LessThanOrEqual;
        }

        private static Predicate<T> _IsDefault;
        public static Predicate<T> CreateIsDefault()
        {
            if (_IsDefault is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");
                ConstantExpression Arg2 = Expression.Constant(default(T), t);

                try
                {
                    _IsDefault = Expression.Lambda<Predicate<T>>(Expression.Equal(Arg1, Arg2), Arg1)
                                           .Compile();
                }
                catch (Exception Ex)
                {
                    _IsDefault = a => throw Ex;
                }
            }

            return _IsDefault;
        }

        public static readonly ConcurrentDictionary<Type, Delegate> _Casts = new();
        public static Func<T, T2> CreateCast<T2>()
        {
            Type t2 = typeof(T2);
            if (_Casts.TryGetValue(t2, out Delegate CacheFunc))
                return (Func<T, T2>)CacheFunc;

            LabelTarget Label = Expression.Label(t2);
            ParameterExpression Arg1 = Expression.Parameter(t, "a");

            Func<T, T2> CastFunc;
            try
            {
                if (t.Equals(t2))
                {
                    CastFunc = Expression.Lambda<Func<T, T2>>(Expression.Block(Expression.Return(Label, Arg1, t2),
                                                                               Expression.Label(Label, Expression.New(t2))), Arg1)
                                         .Compile();
                }
                else if (t.IsDecimalType() && (t2.IsIntegerType() || t2.IsPositiveIntegerType()) &&
                         typeof(Math).TryGetStaticMethod(nameof(Math.Round), new[] { typeof(double) }, out MethodInfo Method))
                {
                    Type Dt = typeof(double);
                    MethodCallExpression RoundArg = Expression.Call(Method, t.Equals(Dt) ? Arg1 : Expression.Convert(Arg1, Dt));

                    CastFunc = Expression.Lambda<Func<T, T2>>(Expression.Convert(RoundArg, t2), Arg1)
                                         .Compile();
                }
                else
                {
                    CastFunc = Expression.Lambda<Func<T, T2>>(Expression.Convert(Arg1, t2), Arg1)
                                         .Compile();
                }
            }
            catch (Exception Ex)
            {
                CastFunc = a => throw Ex;
            }

            _Casts.AddOrUpdate(t2, CastFunc, (k, v) => CastFunc);
            return CastFunc;
        }

        public static Func<T, T, T> CreateMin()
        {
            LabelTarget Label = Expression.Label(t);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.LessThan(Arg1, Arg2), Expression.Return(Label, Arg1, typeof(T)), Expression.Return(Label, Arg2, typeof(T))),
                                                    Expression.Label(Label, Expression.New(t)));

            return Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                             .Compile();
        }
        public static Func<T, T, T> CreateMax()
        {
            LabelTarget Label = Expression.Label(t);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Arg2), Expression.Return(Label, Arg1, typeof(T)), Expression.Return(Label, Arg2, typeof(T))),
                                                    Expression.Label(Label, Expression.New(t)));

            return Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                             .Compile();
        }

    }
}
