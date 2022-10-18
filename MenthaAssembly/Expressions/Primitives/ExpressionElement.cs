using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionElement : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Element;

        public Expression Expression { get; }

        public ExpressionElement(Expression Expression)
        {
            this.Expression = Expression;
        }

        public override string ToString()
            => Expression.ToString();

    }
}