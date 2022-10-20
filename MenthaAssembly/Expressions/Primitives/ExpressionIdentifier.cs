using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionIdentifier : IExpressionObject
    {
        public ExpressionObjectType Type { get; }

        public ExpressionType ExpressionType { get; }

        public ExpressionIdentifier(ExpressionType IdentifierType)
        {
            switch (IdentifierType)
            {
                #region MathIdentifier
                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.AddChecked:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.Divide:
                case ExpressionType.DivideAssign:
                case ExpressionType.Modulo:
                case ExpressionType.ModuloAssign:
                case ExpressionType.Power:
                    {
                        Type = ExpressionObjectType.MathIdentifier;
                        ExpressionType = IdentifierType;
                        break;
                    }
                #endregion
                #region LogicIdentifier
                case ExpressionType.Or:
                case ExpressionType.And:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Not:
                case ExpressionType.OnesComplement:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    {
                        Type = ExpressionObjectType.LogicIdentifier;
                        ExpressionType = IdentifierType;
                        break;
                    }

                #endregion
                default:
                    throw new NotSupportedException($"[Expression]{IdentifierType} is not identifier.");
            }
        }
        public ExpressionIdentifier(ExpressionObjectType Type, ExpressionType IdentifierType)
        {
            if ((Type & ExpressionObjectType.Identifier) == 0)
                throw new ArgumentException($"{Type} is not identifier.");

            this.Type = Type;
            ExpressionType = IdentifierType;
        }

        Expression IExpressionObject.Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => throw new NotSupportedException();

        public override string ToString()
            => ExpressionType switch
            {
                #region MathIdentifier
                ExpressionType.Add or
                ExpressionType.AddAssign or
                ExpressionType.AddChecked or
                ExpressionType.AddAssignChecked or
                ExpressionType.UnaryPlus => "+",

                ExpressionType.Negate or
                ExpressionType.NegateChecked or
                ExpressionType.Subtract or
                ExpressionType.SubtractAssign or
                ExpressionType.SubtractChecked or
                ExpressionType.SubtractAssignChecked => "-",

                ExpressionType.Multiply or
                ExpressionType.MultiplyAssign or
                ExpressionType.MultiplyChecked or
                ExpressionType.MultiplyAssignChecked => "*",

                ExpressionType.Divide or
                ExpressionType.DivideAssign => "/",

                ExpressionType.Modulo or
                ExpressionType.ModuloAssign => "%",

                ExpressionType.Power => "^",
                #endregion
                #region LogicIdentifier
                ExpressionType.Or => "|",
                ExpressionType.And => "&",
                ExpressionType.ExclusiveOr => "^",
                ExpressionType.Not => "!",
                ExpressionType.OnesComplement => "~",

                ExpressionType.LeftShift => "<<",
                ExpressionType.RightShift => ">>",

                #endregion
                _ => string.Empty,
            };

    }
}