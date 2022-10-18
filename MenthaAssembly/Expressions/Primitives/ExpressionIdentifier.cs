using System;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionIdentifier : IExpressionObject
    {
        private readonly ExpressionObjectType BaseType;
        ExpressionObjectType IExpressionObject.Type
            => BaseType;

        public ExpressionType Type { get; }

        public ExpressionIdentifier(ExpressionType IdentifierType)
        {
            switch (IdentifierType)
            {
                #region MathIdentifier
                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.AddChecked:
                case ExpressionType.AddAssignChecked:
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
                        Type = IdentifierType;
                        BaseType = ExpressionObjectType.MathIdentifier;
                        break;
                    }
                #endregion
                default:
                    throw new NotSupportedException($"{IdentifierType} is not identifier.");
            }
        }

        public override string ToString()
            => Type switch
            {
                ExpressionType.Add or
                ExpressionType.AddAssign or
                ExpressionType.AddChecked or
                ExpressionType.AddAssignChecked => "+",
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
                _ => string.Empty,
            };

    }
}