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

        public static Func<T, T2> CreateCast<T,T2>()
        {
            Type t = typeof(T),
                 t2 = typeof(T2);
            LabelTarget Label = Expression.Label(t2);
            ParameterExpression Arg1 = Expression.Parameter(t, "a");

            if (t.Equals(t2))
                return Expression.Lambda<Func<T, T2>>(Expression.Block(Expression.Return(Label, Arg1, t2), 
                                                                       Expression.Label(Label, Expression.New(t2))), Arg1)
                                 .Compile();
            
            return Expression.Lambda<Func<T, T2>>(Expression.Convert(Arg1, t2), Arg1)
                             .Compile();
        }

        public static Func<T, T> CreateNegate<T>()
        {
            Type t = typeof(T);
            ParameterExpression Arg1 = Expression.Parameter(t, "a");

            return Expression.Lambda<Func<T, T>>(Expression.Negate(Arg1), Arg1)
                             .Compile();
        }

        public static Func<T, T, T> CreateAdd<T>()
        {
            Type t = typeof(T);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            return Expression.Lambda<Func<T, T, T>>(Expression.Add(Arg1, Arg2), Arg1, Arg2)
                             .Compile();
        }
        public static Func<T, T, T> CreateSubtract<T>()
        {
            Type t = typeof(T);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            return Expression.Lambda<Func<T, T, T>>(Expression.Subtract(Arg1, Arg2), Arg1, Arg2)
                             .Compile();
        }
        public static Func<T, T, T> CreateMultiply<T>()
        {
            Type t = typeof(T);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            return Expression.Lambda<Func<T, T, T>>(Expression.Multiply(Arg1, Arg2), Arg1, Arg2)
                             .Compile();
        }
        public static Func<T, T, T> CreateDivide<T>()
        {
            Type t = typeof(T);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            return Expression.Lambda<Func<T, T, T>>(Expression.Divide(Arg1, Arg2), Arg1, Arg2)
                             .Compile();
        }

        public static Func<T, T, bool> CreateEqual<T>()
        {
            Type t = typeof(T);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            return Expression.Lambda<Func<T, T, bool>>(Expression.Equal(Arg1, Arg2), Arg1, Arg2)
                             .Compile();
        }

        public static Func<T, T, T> CreateMin<T>()
        {
            Type t = typeof(T);
            LabelTarget Label = Expression.Label(t);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.LessThan(Arg1, Arg2), Expression.Return(Label, Arg1, typeof(T)), Expression.Return(Label, Arg2, typeof(T))),
                                                    Expression.Label(Label, Expression.New(t)));

            return Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                             .Compile();
        }
        public static Func<T, T, T> CreateMax<T>()
        {
            Type t = typeof(T);
            LabelTarget Label = Expression.Label(t);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Arg2), Expression.Return(Label, Arg1, typeof(T)), Expression.Return(Label, Arg2, typeof(T))),
                                                    Expression.Label(Label, Expression.New(t)));

            return Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                             .Compile();
        }

        public static Predicate<T> CreateIsDefault<T>()
        {
            Type t = typeof(T);
            ParameterExpression Arg1 = Expression.Parameter(t, "a");

            return Expression.Lambda<Predicate<T>>(Expression.Equal(Arg1, Expression.Constant(default(T), t)), Arg1)
                             .Compile();
        }

    }
}
