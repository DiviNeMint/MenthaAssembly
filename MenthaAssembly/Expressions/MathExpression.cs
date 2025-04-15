using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class MathExpression
    {
        private readonly Expression Context;

        public Type Type
            => Context.Type;

        public ExpressionType NodeType
            => Context.NodeType;

        public IReadOnlyList<ParameterExpression> Parameters { get; }

        public MathExpression(string Formula, params ParameterExpression[] Parameters) : this(Formula, null, Parameters)
        {
        }
        public MathExpression(string Formula, object Base, params ParameterExpression[] Parameters)
        {
            if (!ExpressionBlock.TryParse(Formula, out ExpressionBlock Block))
                throw new InvalidProgramException($"[MathExpression][Parse]Invalid program {{ {Formula} }}.");

            this.Parameters = Parameters;
            Context = Block.Implement(ExpressionMode.Math, Expression.Constant(Base), Parameters);
        }

        public override string ToString()
            => Context.ToString();

        public static implicit operator Expression(MathExpression This)
            => This.Context;

    }
}