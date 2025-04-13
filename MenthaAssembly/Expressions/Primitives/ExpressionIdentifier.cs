using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionIdentifier : IExpressionObject
    {
        public ExpressionObjectType Type
            => ExpressionObjectType.Identifier;

        public ExpressionType ExpressionType { get; }

        private readonly string Identifier;
        internal ExpressionIdentifier(string Identifier, ExpressionType ExpressionType)
        {
            this.ExpressionType = ExpressionType;
            this.Identifier = Identifier;
        }

        Expression IExpressionObject.Implement(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => throw new NotSupportedException();

        public override string ToString()
            => Identifier;
    }
}