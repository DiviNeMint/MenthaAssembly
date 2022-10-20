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
            if (!TryParseMathExpression(Formula, out ExpressionBlock Block))
                throw new InvalidProgramException($"[MathExpression][Parse]Invalid program {{ {Formula} }}.");

            this.Parameters = Parameters;
            Context = Block.Implement(Expression.Constant(Base), Parameters);
        }

        public static bool TryParseMathExpression(string Formula, out ExpressionBlock Block)
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
                #region Number Constant
                case '.':
                    {
                        Index++;

                        try
                        {
                            Builder.Append(c);
                            Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out bool IsEnd, c => !c.IsArabicNumerals()));

                            if (IsEnd)
                            {
                                double DoubleConst = Builder.ToString().ToDoubleFast();
                                Element = new ExpressionConst(DoubleConst);
                                return true;
                            }

                            c = Formula[Index];
                            if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                            {
                                double DoubleConst = Builder.ToString().ToDoubleFast();
                                Element = new ExpressionConst(DoubleConst);
                                return true;
                            }

                            Index++;

                            string Remained = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out _, c => !c.IsVariableChars());
                            if (Remained.Length > 0)
                            {
                                Builder.Append(c);
                                Builder.Append(Remained);
                                Element = new ExpressionObject(Builder.ToString());
                                return true;
                            }

                            // Sepeified Types
                            switch (c)
                            {
                                case 'f':
                                case 'F':
                                    {
                                        float FloatConst = Builder.ToString().ToFloatFast();
                                        Element = new ExpressionConst(FloatConst);
                                        return true;
                                    }
                                case 'd':
                                case 'D':
                                    {
                                        double DoubleConst = Builder.ToString().ToDoubleFast();
                                        Element = new ExpressionConst(DoubleConst);
                                        return true;
                                    }
                                case 'm':
                                case 'M':
                                    {
                                        decimal DecimalConst = decimal.Parse(Builder.ToString());
                                        Element = new ExpressionConst(DecimalConst);
                                        return true;
                                    }
                            }

                            // Unknown
                            Builder.Append(c);
                            Builder.Append(Remained);
                            Element = new ExpressionObject(Builder.ToString());
                            return true;
                        }
                        finally
                        {
                            Builder.Clear();
                        }
                    }
                case char n when n.IsArabicNumerals():
                    {
                        Index++;

                        try
                        {
                            Builder.Append(c);
                            Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out bool IsEnd, c => !c.IsArabicNumerals()));

                            // Integer
                            if (IsEnd)
                            {
                                int IntConst = Builder.ToString().ToInt32Fast();
                                Element = new ExpressionConst(IntConst);
                                return true;
                            }

                            c = Formula[Index];
                            if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                            {
                                int IntConst = Builder.ToString().ToInt32Fast();
                                Element = new ExpressionConst(IntConst);
                                return true;
                            }

                            Index++;

                            // Decimal
                            bool IsIntegerOrAllowSpecifiedType = true;
                            if (c == '.')
                            {
                                Builder.Append(c);
                                Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsArabicNumerals()));

                                if (IsEnd)
                                {
                                    double DoubleConst = Builder.ToString().ToDoubleFast();
                                    Element = new ExpressionConst(DoubleConst);
                                    return true;
                                }

                                c = Formula[Index];
                                if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                                {
                                    double DoubleConst = Builder.ToString().ToDoubleFast();
                                    Element = new ExpressionConst(DoubleConst);
                                    return true;
                                }

                                Index++;
                                IsIntegerOrAllowSpecifiedType = false;
                            }

                            // Specified Integer
                            else if (Builder.ToString() == "0")
                            {
                                Builder.Append(c);

                                // Hex
                                if (c == 'x' || c == 'X')
                                {
                                    // Format
                                    string Format = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsHexNumerals());
                                    if (Format.Length == 0)
                                    {
                                        Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars()));
                                        Element = new ExpressionObject(Builder.ToString());
                                        return true;
                                    }

                                    if (IsEnd)
                                    {
                                        int IntConst = int.Parse(Format, NumberStyles.HexNumber);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    c = Formula[Index];
                                    if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                                    {
                                        int IntConst = int.Parse(Format, NumberStyles.HexNumber);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    Index++;
                                    Builder.Append(Format);
                                    IsIntegerOrAllowSpecifiedType = false;
                                }

                                // Binary
                                else if (c == 'b' || c == 'B')
                                {
                                    // Format
                                    string Format = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsArabicNumerals());
                                    if (Format.Length == 0)
                                    {
                                        Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars()));
                                        Element = new ExpressionObject(Builder.ToString());
                                        return true;
                                    }

                                    if (IsEnd)
                                    {
                                        int IntConst = System.Convert.ToInt32(Format, 2);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    c = Formula[Index];
                                    if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                                    {
                                        int IntConst = System.Convert.ToInt32(Format, 2);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    Index++;
                                    Builder.Append(Format);
                                    IsIntegerOrAllowSpecifiedType = false;
                                }
                            }

                            string Remained = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out _, c => !c.IsVariableChars());
                            if (Remained.Length > 0)
                            {
                                Builder.Append(c);
                                Builder.Append(Remained);
                                Element = new ExpressionObject(Builder.ToString());
                                return true;
                            }

                            // Sepeified Types
                            switch (c)
                            {
                                case 'f':
                                case 'F':
                                    {
                                        float FloatConst = Builder.ToString().ToFloatFast();
                                        Element = new ExpressionConst(FloatConst);
                                        return true;
                                    }
                                case 'd':
                                case 'D':
                                    {
                                        double DoubleConst = Builder.ToString().ToDoubleFast();
                                        Element = new ExpressionConst(DoubleConst);
                                        return true;
                                    }
                                case 'm':
                                case 'M':
                                    {
                                        decimal DecimalConst = decimal.Parse(Builder.ToString());
                                        Element = new ExpressionConst(DecimalConst);
                                        return true;
                                    }
                                case 'l' when IsIntegerOrAllowSpecifiedType:
                                case 'L' when IsIntegerOrAllowSpecifiedType:
                                    {
                                        long Int64Const = long.Parse(Builder.ToString());
                                        Element = new ExpressionConst(Int64Const);
                                        return true;
                                    }
                                case 'u' when IsIntegerOrAllowSpecifiedType:
                                case 'U' when IsIntegerOrAllowSpecifiedType:
                                    {
                                        uint UIntConst = Builder.ToString().ToUInt32Fast();
                                        Element = new ExpressionConst(UIntConst);
                                        return true;
                                    }
                            }

                            // Unknown
                            Builder.Append(c);
                            Builder.Append(Remained);
                            Element = new ExpressionObject(Builder.ToString());
                            return true;
                        }
                        finally
                        {
                            Builder.Clear();
                        }
                    }
                #endregion
                default:
                    {
                        try
                        {
                            Builder.Append(c);
                            ExpressionElement Paths = new ExpressionElement();

                            do
                            {
                                // Skip '.' or First appended char.
                                Index++;

                                string Variable = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out bool IsEnd, c => !c.IsVariableChars());
                                if (IsEnd)
                                {
                                    Builder.Append(Variable);
                                    Paths.Contexts.Add(new ExpressionMember(Builder.ToString()));
                                    break;
                                }

                                c = Formula[Index];

                                // Check Space
                                if (char.IsWhiteSpace(c))
                                {
                                    if (!ReaderHelper.MoveTo(Formula, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
                                        break;

                                    // Tail Space
                                    if (string.IsNullOrEmpty(Variable))
                                    {
                                        Variable = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars());
                                        ReaderHelper.MoveTo(Formula, ref Index, Length, false, c => !char.IsWhiteSpace(c));
                                    }

                                    c = Formula[Index];
                                }

                                Builder.Append(Variable);

                                // Method
                                if (c == '(')
                                {
                                    ExpressionMethod Method = new ExpressionMethod(Builder.ToString());
                                    if (string.IsNullOrEmpty(Method.Name))
                                        break;

                                    List<IExpressionObject> ArgContext = new List<IExpressionObject>();

                                    Builder.Clear();
                                    do
                                    {
                                        Index++;    // Skip ',' 、 '('

                                        while (TryParseMathElement(Formula, ref Index, Length, ref Builder, out IExpressionObject Arg))
                                            ArgContext.Add(Arg);

                                        if (ArgContext.Count == 0)
                                            break;

                                        Method.Parameters.Add(ArgContext.Count == 1 ? ArgContext[0] : new ExpressionBlock(ArgContext));
                                        if (Index >= Length)
                                        {
                                            Paths.Contexts.Add(Method);
                                            Element = Paths;
                                            return true;
                                        }

                                        c = Formula[Index];
                                        ArgContext.Clear();

                                    } while (c != ')');

                                    Index++;    // Skip ')'

                                    Paths.Contexts.Add(Method);
                                    c = Formula[Index];
                                }

                                // Member
                                else
                                {
                                    ExpressionMember Member = new ExpressionMember(Builder.ToString());
                                    if (string.IsNullOrEmpty(Member.Name))
                                        break;

                                    Paths.Contexts.Add(Member);
                                    Builder.Clear();
                                }

                            } while (c == '.');

                            Element = Paths;
                            return true;
                        }
                        finally
                        {
                            Builder.Clear();
                        }
                    }
            }
        }

        public static implicit operator Expression(MathExpression This) => This.Context;
        //public static explicit operator Expression(MathExpression This) => This.Context;

    }
}