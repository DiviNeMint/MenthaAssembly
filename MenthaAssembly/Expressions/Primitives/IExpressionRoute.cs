using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public interface IExpressionRoute : IExpressionObject
    {
        public string Name { get; }

        public List<ExpressionTypeInfo> GenericTypes { get; }

        public bool TryImplement(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Expression Expression);

    }
}