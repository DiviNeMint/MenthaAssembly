using System;
using System.Linq.Expressions;

namespace MenthaAssembly
{
    public static class OperatorHelper
    {
        /// <summary>
        /// -A
        /// </summary>
        public static T Negate<T>(T A)
            => Operator<T>.Negate(A);

        /// <summary>
        /// A + B
        /// </summary>
        public static T Add<T>(T A, T B)
            => Operator<T>.Add(A, B);

        /// <summary>
        /// A - B
        /// </summary>
        public static T Subtract<T>(T A, T B)
            => Operator<T>.Subtract(A, B);

        /// <summary>
        /// A * B
        /// </summary>
        public static T Multiply<T>(T A, T B)
            => Operator<T>.Multiply(A, B);

        /// <summary>
        /// A / B
        /// </summary>
        public static T Divide<T>(T A, T B)
            => Operator<T>.Divide(A, B);

        /// <summary>
        /// A / 2
        /// </summary>
        public static T Half<T>(T A)
            => Operator<T>.Half(A);

        /// <summary>
        /// A * 2
        /// </summary>
        public static T Double<T>(T A)
            => Operator<T>.Double(A);

        /// <summary>
        /// | A |
        /// </summary>
        public static T Abs<T>(T A)
            => Operator<T>.Abs(A);

        /// <summary>
        /// Gets the maximum in A and B.
        /// </summary>
        public static T Max<T>(T A, T B)
            => Operator<T>.Max(A, B);

        /// <summary>
        /// Gets the minimum in A and B.
        /// </summary>
        public static T Min<T>(T A, T B)
            => Operator<T>.Min(A, B);

        /// <summary>
        /// A == B
        /// </summary>
        public static bool Equals<T>(T A, T B)
            => Operator<T>.Equals(A, B);

        /// <summary>
        /// A == default<T>(T)
        /// </summary>
        public static bool IsDefault<T>(T A)
            => Operator<T>.IsDefault(A);

        /// <summary>
        /// A &gt; B
        /// </summary>
        public static bool GreaterThan<T>(T A, T B)
            => Operator<T>.GreaterThan(A, B);

        /// <summary>
        /// A &gt;= B
        /// </summary>
        public static bool GreaterThanOrEqual<T>(T A, T B)
            => Operator<T>.GreaterThanOrEqual(A, B);

        /// <summary>
        /// A &lt; B
        /// </summary>
        public static bool LessThan<T>(T A, T B)
            => Operator<T>.LessThan(A, B);

        /// <summary>
        /// A &lt;= B
        /// </summary>
        public static bool LessThanOrEqual<T>(T A, T B)
            => Operator<T>.LessThanOrEqual(A, B);

        /// <summary>
        /// Convert T to U.
        /// </summary>
        public static U Cast<T, U>(T A)
            => Operator<T, U>.Cast(A);

        private static class Operator<T>
        {
            private static Func<T, T> _Neg, _Abs, _Half, _Double;
            private static Func<T, T, T> _Add, _Sub, _Mul, _Div, _Min, _Max;
            private static Func<T, T, bool> _Equals, _GreaterThan, _LessThan, _GreaterThanOrEqual, _LessThanOrEqual;
            private static Predicate<T> _IsDefault;

            public static T Negate(T A)
            {
                if (_Neg is null)
                {
                    Type t = typeof(T);
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

                return _Neg(A);
            }

            public static T Add(T A, T B)
            {
                if (_Add is null)
                {
                    Type t = typeof(T);
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

                return _Add(A, B);
            }

            public static T Subtract(T A, T B)
            {
                if (_Sub is null)
                {
                    Type t = typeof(T);
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

                return _Sub(A, B);
            }

            public static T Multiply(T A, T B)
            {
                if (_Mul is null)
                {
                    Type t = typeof(T);
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

                return _Mul(A, B);
            }

            public static T Divide(T A, T B)
            {
                if (_Div is null)
                {
                    Type t = typeof(T);
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

                return _Div(A, B);
            }

            public static T Half(T A)
            {
                if (_Half is null)
                {
                    Type t = typeof(T);
                    ParameterExpression Arg1 = Expression.Parameter(t, "a");
                    Expression Number2 = Expression.Constant(2, typeof(int)).Cast(t);

                    try
                    {
                        _Half = Expression.Lambda<Func<T, T>>(Expression.Divide(Arg1, Number2), Arg1)
                                          .Compile();
                    }
                    catch (Exception Ex)
                    {
                        _Half = a => throw Ex;
                    }
                }

                return _Half(A);
            }

            public static T Double(T A)
            {
                if (_Double is null)
                {
                    Type t = typeof(T);
                    ParameterExpression Arg1 = Expression.Parameter(t, "a");
                    Expression Number2 = Expression.Constant(2, typeof(int)).Cast(t);

                    try
                    {
                        _Double = Expression.Lambda<Func<T, T>>(Expression.Multiply(Arg1, Number2), Arg1)
                                            .Compile();
                    }
                    catch (Exception Ex)
                    {
                        _Double = a => throw Ex;
                    }
                }

                return _Double(A);
            }

            public static T Abs(T A)
            {
                if (_Abs is null)
                {
                    Type t = typeof(T);
                    ParameterExpression Arg1 = Expression.Parameter(t, "a");

                    try
                    {
                        LabelTarget Label = Expression.Label(t);
                        ConstantExpression Default = Expression.Constant(default(T));

                        BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Default),
                                                                Expression.Return(Label, Arg1, t),
                                                                Expression.Return(Label, Expression.Negate(Arg1), t)),
                                                                Expression.Label(Label, Default));

                        _Abs = Expression.Lambda<Func<T, T>>(Body, Arg1)
                                         .Compile();
                    }
                    catch (Exception Ex)
                    {
                        _Abs = T => throw Ex;
                    }
                }

                return _Abs(A);
            }

            public static T Max(T A, T B)
            {
                if (_Max is null)
                {
                    Type t = typeof(T);
                    ConstantExpression Default = Expression.Constant(default(T));
                    ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                        Arg2 = Expression.Parameter(t, "b");

                    try
                    {
                        LabelTarget Label = Expression.Label(t);

                        BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Arg2),
                                                                Expression.Return(Label, Arg2, t),
                                                                Expression.Return(Label, Arg1, t)),
                                                                Expression.Label(Label, Default));

                        _Max = Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                                         .Compile();
                    }
                    catch (Exception Ex)
                    {
                        _Max = (a, b) => throw Ex;
                    }
                }

                return _Max(A, B);
            }

            public static T Min(T A, T B)
            {
                if (_Min is null)
                {
                    Type t = typeof(T);
                    ConstantExpression Default = Expression.Constant(default(T));
                    ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                        Arg2 = Expression.Parameter(t, "b");

                    try
                    {
                        LabelTarget Label = Expression.Label(t);

                        BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Arg2),
                                                                Expression.Return(Label, Arg2, t),
                                                                Expression.Return(Label, Arg1, t)),
                                                                Expression.Label(Label, Default));

                        _Min = Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                                         .Compile();
                    }
                    catch (Exception Ex)
                    {
                        _Min = (a, b) => throw Ex;
                    }
                }

                return _Min(A, B);
            }

            public static bool Equals(T A, T B)
            {
                if (_Equals is null)
                {
                    Type t = typeof(T);
                    ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                        Arg2 = Expression.Parameter(t, "b");

                    try
                    {
                        _Equals = Expression.Lambda<Func<T, T, bool>>(Expression.Equal(Arg1, Arg2), Arg1, Arg2)
                                           .Compile();
                    }
                    catch (Exception Ex)
                    {
                        _Equals = (a, b) => throw Ex;
                    }
                }

                return _Equals(A, B);
            }

            public static bool IsDefault(T A)
            {
                if (_IsDefault is null)
                {
                    Type t = typeof(T);
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

                return _IsDefault(A);
            }

            public static bool GreaterThan(T A, T B)
            {
                if (_GreaterThan is null)
                {
                    Type t = typeof(T);
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

                return _GreaterThan(A, B);
            }

            public static bool GreaterThanOrEqual(T A, T B)
            {
                if (_GreaterThanOrEqual is null)
                {
                    Type t = typeof(T);
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

                return _GreaterThanOrEqual(A, B);
            }

            public static bool LessThan(T A, T B)
            {
                if (_LessThan is null)
                {
                    Type t = typeof(T);
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

                return _LessThan(A, B);
            }

            public static bool LessThanOrEqual(T A, T B)
            {
                if (_LessThanOrEqual is null)
                {
                    Type t = typeof(T);
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

                return _LessThanOrEqual(A, B);
            }

        }

        private static class Operator<T, U>
        {
            private static Func<T, U> _Cast;
            public static U Cast(T A)
            {
                if (_Cast is null)
                {
                    ParameterExpression Arg1 = Expression.Parameter(typeof(T), "a");
                    try
                    {
                        _Cast = Expression.Lambda<Func<T, U>>(ExpressionHelper.Cast(Arg1, typeof(U)), Arg1)
                                          .Compile();
                    }
                    catch (Exception Ex)
                    {
                        _Cast = a => throw Ex;
                    }
                }

                return _Cast(A);
            }

        }

    }
}