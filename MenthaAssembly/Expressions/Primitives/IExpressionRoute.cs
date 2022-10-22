using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public interface IExpressionRoute : IExpressionObject
    {
        public Expression Implement(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters);

    }
}