using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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
            if (!TryParse(Formula, out ExpressionBlock Block))
                throw new InvalidProgramException($"[MathExpression][Parse]Invalid program {{ {Formula} }}.");

            this.Parameters = Parameters;
            Context = Block.Implement(Expression.Constant(Base), Parameters);
        }

        public static bool TryParse(string Formula, out ExpressionBlock Block)
        {
            StringBuilder Builder = new StringBuilder();
            int Index = 0,
                Length = Formula.Length;

            Block = new ExpressionBlock();
            while (TryParseMathElement(Formula, ref Index, Length, ref Builder, out IExpressionObject Element))
                Block.Contexts.Add(Element);

            if (Block.Contexts.Count == 0 || Index < Length)
            {
                Block = null;
                return false;
            }

            return true;
        }
        private static bool TryParseMathElement(string Formula, ref int Index, int Length, ref StringBuilder Builder, out IExpressionObject Element)
        {
            if (!ReaderHelper.MoveTo(Formula, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
            {
                Element = null;
                return false;
            }

            char c = Formula[Index];
            switch (c)
            {
                #region Block
                case '(':
                    {
                        Index++;

                        ExpressionBlock Block = new ExpressionBlock();
                        while (TryParseMathElement(Formula, ref Index, Length, ref Builder, out IExpressionObject Child))
                            Block.Contexts.Add(Child);

                        if (Index < Length)
                        {
                            c = Formula[Index];
                            if (c == ')')
                            {
                                Index++;
                                Element = Block;
                                return true;
                            }
                        }

                        Debug.WriteLine($"[MathExpression][Parser]Unknown Element.");
                        Element = null;
                        return false;
                    }
                case ')':
                case ',':
                    {
                        Element = null;
                        return false;
                    }
                #endregion
                #region Math Identifier
                case '+':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Add);
                        return true;
                    }
                case '-':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Subtract);
                        return true;
                    }
                case '*':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Multiply);
                        return true;
                    }
                case '/':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Divide);
                        return true;
                    }
                case '%':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Modulo);
                        return true;
                    }
                case '^':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Power);
                        return true;
                    }
                case '|':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionObjectType.MathIdentifier, ExpressionType.Or);
                        return true;
                    }
                #endregion
                #region Constant
                // Decimal
                case '.':
                    {
                        // Append decimal point '.'.
                        Index++;
                        Builder.Append(c);

                        Element = ExpressionHelper.ParseDecimalNumber(Formula, ref Index, Length, ref Builder);
                        return true;
                    }
                // Number
                case char n when n.IsArabicNumerals():
                    {
                        // Append the first number.
                        Index++;
                        Builder.Append(c);

                        Element = ExpressionHelper.ParseNumber(Formula, ref Index, Length, ref Builder);
                        return true;
                    }
                #endregion
                default:
                    {
                        Element = ExpressionHelper.ParseRoute(Formula, ref Index, Length, ref Builder);
                        return true;
                    }
            }
        }

        public static implicit operator Expression(MathExpression This) => This.Context;

    }
}