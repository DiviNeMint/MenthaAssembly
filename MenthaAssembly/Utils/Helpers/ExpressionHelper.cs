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

        private static Func<T, T> _NegFunc;
        public static Func<T, T> CreateNegFunc()
        {
            if (_NegFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");

                _NegFunc = Expression.Lambda<Func<T, T>>(Expression.Negate(Arg1), Arg1)
                                     .Compile();
            }

            return _NegFunc;
        }

        private static Func<T, T, T> _AddFunc, _SubFunc, _MulFunc, _DivFunc;
        public static Func<T, T, T> CreateAddFunc()
        {
            if (_AddFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _AddFunc = Expression.Lambda<Func<T, T, T>>(Expression.Add(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _AddFunc;
        }
        public static Func<T, T, T> CreateSubFunc()
        {
            if (_SubFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _SubFunc = Expression.Lambda<Func<T, T, T>>(Expression.Subtract(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _SubFunc;
        }
        public static Func<T, T, T> CreateMulFunc()
        {
            if (_MulFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _MulFunc = Expression.Lambda<Func<T, T, T>>(Expression.Multiply(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _MulFunc;
        }
        public static Func<T, T, T> CreateDivFunc()
        {
            if (_DivFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _DivFunc = Expression.Lambda<Func<T, T, T>>(Expression.Divide(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _DivFunc;
        }

        private static Func<T, T, bool> _EqualFunc, _GreaterThanFunc, _LessThanFunc, _GreaterThanOrEqualFunc, _LessThanOrEqualFunc;
        public static Func<T, T, bool> CreateEqualFunc()
        {
            if (_EqualFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _EqualFunc = Expression.Lambda<Func<T, T, bool>>(Expression.Equal(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _EqualFunc;
        }
        public static Func<T, T, bool> CreateGreaterThanFunc()
        {
            if (_GreaterThanFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _GreaterThanFunc = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThan(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _GreaterThanFunc;
        }
        public static Func<T, T, bool> CreateLessThanFunc()
        {
            if (_LessThanFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _LessThanFunc = Expression.Lambda<Func<T, T, bool>>(Expression.LessThan(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _LessThanFunc;
        }
        public static Func<T, T, bool> CreateGreaterThanOrEqualFunc()
        {
            if (_GreaterThanOrEqualFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _GreaterThanOrEqualFunc = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThanOrEqual(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _GreaterThanOrEqualFunc;
        }
        public static Func<T, T, bool> CreateLessThanOrEqualFunc()
        {
            if (_LessThanOrEqualFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                _LessThanOrEqualFunc = Expression.Lambda<Func<T, T, bool>>(Expression.LessThanOrEqual(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
            }

            return _LessThanOrEqualFunc;
        }

        private static Func<T, bool> _IsDefaultFunc;
        public static Func<T, bool> CreateIsDefaultFunc()
        {
            if (_IsDefaultFunc is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");
                ConstantExpression Arg2 = Expression.Constant(default(T), t);

                _IsDefaultFunc = Expression.Lambda<Func<T, bool>>(Expression.Equal(Arg1, Arg2), Arg1)
                                           .Compile();
            }

            return _IsDefaultFunc;
        }

    }
}
