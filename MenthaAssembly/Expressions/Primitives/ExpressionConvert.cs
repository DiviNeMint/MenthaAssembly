using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionConvert : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Convert;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Convert;

        public ExpressionRoute TypeRoute { get; }

        internal ExpressionConvert(ExpressionRoute TypeRoute)
        {
            this.TypeRoute = TypeRoute;
        }

        public Expression Implement(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => throw new NotSupportedException();

        public override string ToString()
            => $"({TypeRoute})";

    }
}