﻿using System;
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

            Normalize(Base, Parameters);

            int Length = Contexts.Count;
            Expression Left = null;
            IExpressionObject Curt;
            for (int i = 0; i < Length;)
            {
                Curt = Contexts[i++];
                if ((Curt.Type & ExpressionObjectType.Identifier) > 0)
                {
                    // Check End
                    if (Length <= i)
                        throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");

                    Expression Right = GetNextExpression(ref i, Length, Base, Parameters);

                    // Check Start
                    if (Left is null)
                    {
                        Left = Curt is ExpressionConvert Convert ?
                               Right.Cast(Convert.Type) :
                               Curt.ExpressionType switch
                               {
                                   // +
                                   ExpressionType.Add or
                                   ExpressionType.AddChecked or
                                   ExpressionType.UnaryPlus => Right,

                                   // -
                                   ExpressionType.Negate or
                                   ExpressionType.NegateChecked or
                                   ExpressionType.Subtract or
                                   ExpressionType.SubtractChecked => Expression.Negate(Right),

                                   _ => throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}."),
                               };

                        continue;
                    }

                    // Check Type
                    Type MaxType = MaxNumberType(Left.Type, Right.Type);

                    // Body
                    Left = Curt.ExpressionType switch
                    {
                        // +
                        ExpressionType.Add or
                        ExpressionType.AddChecked or
                        ExpressionType.UnaryPlus => MaxType != null ? Expression.Add(Left.Cast(MaxType), Right.Cast(MaxType)) :
                                                                      throw new InvalidCastException($"{Left.Type.Name} + {Right.Type.Name}."),

                        // -
                        ExpressionType.Negate or
                        ExpressionType.NegateChecked or
                        ExpressionType.Subtract or
                        ExpressionType.SubtractChecked => MaxType != null ? Expression.Subtract(Left.Cast(MaxType), Right.Cast(MaxType)) :
                                                                            throw new InvalidCastException($"{Left.Type.Name} - {Right.Type.Name}."),

                        // *
                        ExpressionType.Multiply or
                        ExpressionType.MultiplyChecked => MaxType != null ? Expression.Multiply(Left.Cast(MaxType), Right.Cast(MaxType)) :
                                                                            throw new InvalidCastException($"{Left.Type.Name} * {Right.Type.Name}."),

                        // /
                        ExpressionType.Divide => MaxType != null ? Expression.Divide(Left.Cast(MaxType), Right.Cast(MaxType)) :
                                                                   throw new InvalidCastException($"{Left.Type.Name} / {Right.Type.Name}."),

                        // %
                        ExpressionType.Modulo => MaxType != null ? Expression.Modulo(Left.Cast(MaxType), Right.Cast(MaxType)) :
                                                                   throw new InvalidCastException($"{Left.Type.Name} % {Right.Type.Name}."),

                        // ^
                        ExpressionType.Power => Expression.Power(Left.Cast(typeof(double)), Right.Cast(typeof(double))),

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
        private Expression GetNextExpression(ref int Index, int Count, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            IExpressionObject Next = Contexts[Index++];
            if ((Next.Type & ExpressionObjectType.Identifier) > 0)
            {
                // Check End.
                if (Count <= Index)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");

                // Math
                if (Next.Type == ExpressionObjectType.MathIdentifier)
                {
                    ExpressionType Operator = Next.ExpressionType;
                    Next = Contexts[Index++];

                    // Check Right Object
                    if ((Next.Type & ExpressionObjectType.Identifier) > 0)
                        throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");

                    return Operator switch
                    {
                        // +
                        ExpressionType.Add or
                        ExpressionType.AddChecked or
                        ExpressionType.UnaryPlus => Next.Implement(Base, Parameters),

                        // -
                        ExpressionType.Negate or
                        ExpressionType.NegateChecked or
                        ExpressionType.Subtract or
                        ExpressionType.SubtractChecked => Expression.Negate(Next.Implement(Base, Parameters)),

                        _ => throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}."),
                    };
                }

                // Convert
                else if (Next is ExpressionConvert Convert)
                {
                    Next = Contexts[Index++];

                    // Check Right Object
                    if ((Next.Type & ExpressionObjectType.Identifier) > 0)
                        throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");

                    return Next.Implement(Base, Parameters).Cast(Convert.Type);
                }

                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid operator : {this}.");
            }

            return Next.Implement(Base, Parameters);
        }

        private bool IsNormalized = false;
        private void Normalize(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (IsNormalized)
                return;

            IExpressionObject Last, Curt;

            #region Convert
            {
                for (int i = 0; i < Contexts.Count; i++)
                {
                    Curt = Contexts[i];
                    if (Curt is ExpressionBlock Block &&
                        Block.Contexts.Count == 1 &&
                        Block.Contexts[0] is ExpressionRoute Route &&
                        Route.TryParseType(Base, Parameters, out Type Type))
                    {
                        Contexts.RemoveAt(i);

                        ExpressionBlock Group = new ExpressionBlock();
                        Group.Contexts.Add(new ExpressionConvert(Type));

                        for (; i < Contexts.Count;)
                        {
                            Curt = Contexts[i];
                            Contexts.RemoveAt(i);
                            Group.Contexts.Add(Curt);

                            if ((Curt.Type & ExpressionObjectType.Identifier) == 0)
                                break;
                        }

                        Contexts.Insert(i, Group);
                    }
                }
            }
            #endregion

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

                        ExpressionRoute Route = new ExpressionRoute();
                        Route.Contexts.Add(new ExpressionMember("Math"));
                        Route.Contexts.Add(new ExpressionMethod("Abs", new IExpressionObject[] { Group }));

                        Contexts.RemoveRange(StartIndex, Length);
                        Contexts.Insert(StartIndex, Route);

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

        private static Type MaxNumberType(Type NumberType1, Type NumberType2)
            => ReflectionHelper.NumberTypes.TryGetValue(NumberType1, out byte v1) ?
               ReflectionHelper.NumberTypes.TryGetValue(NumberType2, out byte v2) ? v1 > v2 ? NumberType1 : NumberType2 : NumberType1 :
               ReflectionHelper.NumberTypes.ContainsKey(NumberType2) ? NumberType2 : null;

        public override string ToString()
            => string.Join(" ", Contexts.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()));

    }
}