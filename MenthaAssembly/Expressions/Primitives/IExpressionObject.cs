using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public interface IExpressionObject
    {
        public ExpressionObjectType Type { get; }

        public ExpressionType ExpressionType { get; }

        public Expression Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters);

    }
}