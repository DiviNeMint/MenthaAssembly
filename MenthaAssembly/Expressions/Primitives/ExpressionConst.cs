using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionConst : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Const;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Constant;

        public object Value { get; }

        internal ExpressionConst(object Value)
        {
            this.Value = Value;
        }

        private ConstantExpression Const;
        public Expression Implement(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (Const != null)
                return Const;

            Const = Expression.Constant(Value);
            return Const;
        }

        public override string ToString()
            => Value.ToString();

    }
}