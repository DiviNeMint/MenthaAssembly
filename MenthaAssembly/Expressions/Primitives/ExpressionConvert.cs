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

        public Type Type { get; }

        public ExpressionConvert(Type Type)
        {
            this.Type = Type;
        }

        public Expression Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => throw new NotSupportedException();

        public override string ToString()
            => $"({Type.Name})";

    }
}