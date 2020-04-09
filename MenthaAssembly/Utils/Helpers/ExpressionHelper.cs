namespace System.Linq.Expressions
{
    public static class ExpressionHelper
    {
        public static object GetValue(this Expression This)
            => This.GetValue<object>();
        public static T GetValue<T>(this Expression This)
        {
            UnaryExpression objectMember = Expression.Convert(This, typeof(T));
            return Expression.Lambda<Func<T>>(objectMember)
                             .Compile()
                             .Invoke();
        }

    }
}
