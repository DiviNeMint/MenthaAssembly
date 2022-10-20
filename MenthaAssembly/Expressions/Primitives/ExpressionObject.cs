using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionObject : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Unknown;

        ExpressionType IExpressionObject.ExpressionType
            => (ExpressionType)(-1);

        public string Context { get; }

        public ExpressionObject(string Context)
        {
            this.Context = Context;
        }

        public Expression Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown Context : {{{ Context }}}.");

        public override string ToString()
            => Context;

    }
}