using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionBlock : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Block;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Block;

        public List<IExpressionObject> Contexts { get; }

        public ExpressionBlock()
        {
            Contexts = new List<IExpressionObject>();
        }
        public ExpressionBlock(IEnumerable<IExpressionObject> Contexts)
        {
            this.Contexts = new List<IExpressionObject>(Contexts);
        }

        private Expression Block;
        public Expression Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (Block != null)
                return Block;

            Normalize();

            int Length = Contexts.Count;
            Expression Left = null;
            IExpressionObject Curt, Next;
            for (int i = 0; i < Length;)
            {
                Curt = Contexts[i++];
                if ((Curt.Type & ExpressionObjectType.Identifier) > 0)
                {
                    // Check End
                    if (Length <= i)
                        throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");

                    Expression Right;

                    Next = Contexts[i++];
                    if ((Next.Type & ExpressionObjectType.Identifier) > 0)
                    {
                        // Check End and MathIdentifier
                        if (Length <= i ||
                            Next.Type != ExpressionObjectType.MathIdentifier)
                            throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");

                        Next = Contexts[i++];

                        // Check Right Object
                        if ((Next.Type & ExpressionObjectType.Identifier) > 0)
                            throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");

                        Right = Next.ExpressionType switch
                        {
                            // +
                            ExpressionType.Add or
                            ExpressionType.AddAssign or
                            ExpressionType.AddChecked or
                            ExpressionType.AddAssignChecked or
                            ExpressionType.UnaryPlus => Next.Implement(Base, Parameters),

                            // -
                            ExpressionType.Negate or
                            ExpressionType.NegateChecked or
                            ExpressionType.Subtract or
                            ExpressionType.SubtractAssign or
                            ExpressionType.SubtractChecked or
                            ExpressionType.SubtractAssignChecked => Expression.Negate(Next.Implement(Base, Parameters)),

                            _ => throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}."),
                        };
                    }
                    else
                    {
                        Right = Next.Implement(Base, Parameters);
                    }

                    // Check Start
                    if (Left is null)
                    {
                        Left = Curt.ExpressionType switch
                        {
                            // +
                            ExpressionType.Add or
                            ExpressionType.AddAssign or
                            ExpressionType.AddChecked or
                            ExpressionType.AddAssignChecked or
                            ExpressionType.UnaryPlus => Right,

                            // -
                            ExpressionType.Negate or
                            ExpressionType.NegateChecked or
                            ExpressionType.Subtract or
                            ExpressionType.SubtractAssign or
                            ExpressionType.SubtractChecked or
                            ExpressionType.SubtractAssignChecked => Expression.Negate(Right),

                            _ => throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}."),
                        };

                        continue;
                    }

                    // Check Type
                    Type MaxType = ReflectionHelper.MaxNumberType(Left.Type, Right.Type);
                    
                    // Body
                    Left = Curt.ExpressionType switch
                    {
                        // +
                        ExpressionType.Add or
                        ExpressionType.AddAssign or
                        ExpressionType.AddChecked or
                        ExpressionType.AddAssignChecked or
                        ExpressionType.UnaryPlus => MaxType != null ? Expression.Add(Left.CastExpression(MaxType), Right.CastExpression(MaxType)) :
                                                                      throw new InvalidCastException($"{Left.Type.Name} + {Right.Type.Name}."),

                        // -
                        ExpressionType.Negate or
                        ExpressionType.NegateChecked or
                        ExpressionType.Subtract or
                        ExpressionType.SubtractAssign or
                        ExpressionType.SubtractChecked or
                        ExpressionType.SubtractAssignChecked => MaxType != null ? Expression.Subtract(Left.CastExpression(MaxType), Right.CastExpression(MaxType)) :
                                                                                  throw new InvalidCastException($"{Left.Type.Name} - {Right.Type.Name}."),

                        // *
                        ExpressionType.Multiply or
                        ExpressionType.MultiplyAssign or
                        ExpressionType.MultiplyChecked or
                        ExpressionType.MultiplyAssignChecked => MaxType != null ? Expression.Multiply(Left.CastExpression(MaxType), Right.CastExpression(MaxType)) :
                                                                                  throw new InvalidCastException($"{Left.Type.Name} * {Right.Type.Name}."),

                        // /
                        ExpressionType.Divide or
                        ExpressionType.DivideAssign => MaxType != null ? Expression.Divide(Left.CastExpression(MaxType), Right.CastExpression(MaxType)) :
                                                                         throw new InvalidCastException($"{Left.Type.Name} / {Right.Type.Name}."),

                        // %
                        ExpressionType.Modulo or
                        ExpressionType.ModuloAssign => MaxType != null ? Expression.Modulo(Left.CastExpression(MaxType), Right.CastExpression(MaxType)) :
                                                                         throw new InvalidCastException($"{Left.Type.Name} % {Right.Type.Name}."),

                        // ^
                        ExpressionType.Power => Expression.Power(Left.CastExpression(typeof(double)), Right.CastExpression(typeof(double))),

                        _ => throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}."),
                    };
                }
                else if (Left is null)
                {
                    Left = Curt.Implement(Base, Parameters);
                }
                else
                {
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");
                }
            }

            Block = Left;
            return Block;
        }

        private bool IsNormalized = false;
        private void Normalize()
        {
            if (IsNormalized)
                return;

            IExpressionObject Last, Curt;

            #region Abs
            {
                int i = 0;
                Last = Curt = null;
                NormalizeMathAbs(ref i, ref Last, ref Curt);
            }
            #endregion
            #region Multiply and Divide
            if (Contexts.Count > 3)
            {
                Last = Contexts[0];
                for (int i = 1; i < Contexts.Count; i++)
                {
                    Curt = Contexts[i];
                    if (Curt.Type == ExpressionObjectType.MathIdentifier &&
                        (Curt.ExpressionType == ExpressionType.Multiply ||
                         Curt.ExpressionType == ExpressionType.MultiplyAssign ||
                         Curt.ExpressionType == ExpressionType.MultiplyChecked ||
                         Curt.ExpressionType == ExpressionType.MultiplyAssignChecked ||
                         Curt.ExpressionType == ExpressionType.Divide ||
                         Curt.ExpressionType == ExpressionType.DivideAssign ||
                         Curt.ExpressionType == ExpressionType.Modulo ||
                         Curt.ExpressionType == ExpressionType.ModuloAssign))
                    {
                        if ((Last.Type & ExpressionObjectType.Identifier) > 0)
                            throw new InvalidProgramException($"[Expression][{nameof(Normalize)}]Unknown Contexts : {{{ this }}}.");

                        ExpressionBlock Group = new ExpressionBlock { IsNormalized = true };
                        int StartIndex = i - 1,
                            Length = 2;

                        Group.Contexts.Add(Last);
                        Group.Contexts.Add(Curt);

                        i++;
                        for (; i < Contexts.Count; i++)
                        {
                            Curt = Contexts[i];
                            if (Curt.Type == ExpressionObjectType.MathIdentifier &&
                                Curt.ExpressionType != ExpressionType.Multiply &&
                                Curt.ExpressionType != ExpressionType.MultiplyAssign &&
                                Curt.ExpressionType != ExpressionType.MultiplyChecked &&
                                Curt.ExpressionType != ExpressionType.MultiplyAssignChecked &&
                                Curt.ExpressionType != ExpressionType.Divide &&
                                Curt.ExpressionType != ExpressionType.DivideAssign &&
                                Curt.ExpressionType != ExpressionType.Modulo &&
                                Curt.ExpressionType != ExpressionType.ModuloAssign)
                                break;

                            Length++;
                            Group.Contexts.Add(Curt);
                            Last = Curt;
                        }

                        Contexts.RemoveRange(StartIndex, Length);
                        Contexts.Insert(StartIndex, Group);

                        i = StartIndex;
                        Curt = Group;
                        break;
                    }

                    Last = Curt;
                }
            }
            #endregion

            IsNormalized = true;
        }
        private void NormalizeMathAbs(ref int i, ref IExpressionObject Last, ref IExpressionObject Curt)
        {
            for (; i < Contexts.Count; i++)
            {
                Curt = Contexts[i];
                if (Curt.Type == ExpressionObjectType.MathIdentifier &&
                    Curt.ExpressionType == ExpressionType.Or)
                {
                    if (Last is null ||
                        (Last.Type & ExpressionObjectType.Identifier) > 0)
                    {
                        int StartIndex = i++;
                        Last = Curt;

                        NormalizeMathAbs(ref i, ref Last, ref Curt);
                        if (Curt.Type != ExpressionObjectType.MathIdentifier ||
                            Curt.ExpressionType != ExpressionType.Or)
                            throw new InvalidProgramException($"[Epxression][{nameof(NormalizeMathAbs)}]Not found the end identifier of abs.");

                        int Length = i - StartIndex + 1;
                        if (Length < 3)
                            throw new InvalidProgramException($"[Epxression][{nameof(NormalizeMathAbs)}]Not found the variable in the identifiers of abs.");

                        ExpressionBlock Group = new ExpressionBlock() { IsNormalized = true };
                        for (int j = StartIndex + 1; j < i; j++)
                            Group.Contexts.Add(Contexts[j]);

                        ExpressionElement Element = new ExpressionElement();
                        Element.Contexts.Add(new ExpressionMember("Math"));
                        Element.Contexts.Add(new ExpressionMethod("Abs", new IExpressionObject[] { Group }));

                        Contexts.RemoveRange(StartIndex, Length);
                        Contexts.Insert(StartIndex, Element);

                        i = StartIndex;
                        Curt = Group;
                    }
                    else
                    {
                        break;
                    }
                }
                Last = Curt;
            }
        }

        public override string ToString()
            => string.Join(" ", Contexts.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()));

    }
}