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

        internal ExpressionBlock()
        {
            Contexts = [];
        }
        internal ExpressionBlock(IEnumerable<IExpressionObject> Contexts)
        {
            this.Contexts = new List<IExpressionObject>(Contexts);
        }

        private Expression Block;
        public Expression Implement(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (Block != null)
                return Block;

            Normalize(Mode, Base, Parameters);

            int Index = 0,
                Length = Contexts.Count;

            Expression Left = GetNextExpression(Mode, ref Index, Length, Base, Parameters);
            for (; Index < Length;)
            {
                IExpressionObject Context = Contexts[Index++];
                if (Context is not ExpressionIdentifier Identifier)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid syntax : {this}.");

                // Next
                Expression Right = GetNextExpression(Mode, ref Index, Length, Base, Parameters);

                // Body
                Left = GetBodyExpression(Mode, Identifier, Left, Right);
            }

            Block = Left;
            return Block;
        }
        private Expression GetNextExpression(ExpressionMode Mode, ref int Index, int Length, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (Index >= Length)
                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid syntax : {this}.");

            IExpressionObject Next = Contexts[Index++];

            // Identifier
            if (Next.Type == ExpressionObjectType.Identifier)
            {
                // Check End
                if (Index >= Length)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid syntax : {this}.");

                ExpressionType Operator = Next.ExpressionType;
                Next = Contexts[Index++];

                // Check Right Object
                if (Next.Type == ExpressionObjectType.Identifier)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid syntax : {this}.");

                return Operator switch
                {
                    // +
                    ExpressionType.Add or
                    ExpressionType.AddChecked or
                    ExpressionType.UnaryPlus => Next.Implement(Mode, Base, Parameters),

                    // -
                    ExpressionType.Negate or
                    ExpressionType.NegateChecked or
                    ExpressionType.Subtract or
                    ExpressionType.SubtractChecked => Expression.Negate(Next.Implement(Mode, Base, Parameters)),

                    // ~
                    ExpressionType.OnesComplement => Expression.OnesComplement(Next.Implement(Mode, Base, Parameters)),

                    // !
                    ExpressionType.Not => Expression.Not(Next.Implement(Mode, Base, Parameters)),

                    _ => throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid syntax : {this}."),
                };
            }

            // Convert
            else if (Next is ExpressionConvert Convert)
            {
                if (!Convert.TypeRoute.TryParseType(Base, Parameters, out Type Type))
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid convert type : {Convert.TypeRoute}.");

                return GetNextExpression(Mode, ref Index, Length, Base, Parameters).Cast(Type);
            }

            return Next.Implement(Mode, Base, Parameters);
        }
        private Expression GetBodyExpression(ExpressionMode Mode, ExpressionIdentifier Operator, Expression Left, Expression Right)
        {
            switch (Operator.ExpressionType)
            {
                // +
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.UnaryPlus:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Add(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // -
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Subtract(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // *
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Multiply(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // /
                case ExpressionType.Divide:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Divide(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // %
                case ExpressionType.Modulo:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Modulo(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // &
                case ExpressionType.And:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.And(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // |
                case ExpressionType.Or:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Or(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // ^
                case ExpressionType.ExclusiveOr:
                    {
                        if (Mode == ExpressionMode.Math)
                            return Expression.Power(Left.Cast(typeof(double)), Right.Cast(typeof(double)));

                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.ExclusiveOr(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // <<
                case ExpressionType.LeftShift:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.LeftShift(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // >>
                case ExpressionType.RightShift:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"{Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.RightShift(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

            }

            throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid syntax : {this}.");
        }

        private bool IsNormalized = false;
        private void Normalize(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (IsNormalized)
                return;

            IsNormalized = true;
            if (Contexts.Count > 3)
            {
                IExpressionObject Last, Curt;

                #region Abs
                if (Mode == ExpressionMode.Math)
                {
                    int i = 0;
                    Last = Curt = null;
                    NormalizeMathAbs(ref i, ref Last, ref Curt);

                    // Check Children
                    if (Contexts.Count == 1 &&
                        Contexts[0] is ExpressionBlock Children)
                    {
                        Contexts.Clear();
                        Contexts.AddRange(Children.Contexts);
                        return;
                    }
                }
                #endregion

                #region Bitwise Operations
                {
                    Last = Contexts[0];
                    for (int i = 1; i < Contexts.Count; i++)
                    {
                        Curt = Contexts[i];
                        if (Curt is ExpressionIdentifier Identifier)
                        {
                            switch (Identifier.ExpressionType)
                            {
                                case ExpressionType.And:
                                case ExpressionType.Or:
                                case ExpressionType.ExclusiveOr:
                                case ExpressionType.LeftShift:
                                case ExpressionType.RightShift:
                                    {
                                        if (Last.Type == ExpressionObjectType.Identifier)
                                            throw new InvalidProgramException($"[Expression][{nameof(Normalize)}]Unknown Contexts : {{{this}}}.");

                                        ExpressionBlock Group = new() { IsNormalized = true };
                                        int StartIndex = i - 1,
                                            Length = 2;

                                        Group.Contexts.Add(Last);
                                        Group.Contexts.Add(Curt);

                                        i++;
                                        for (; i < Contexts.Count; i++)
                                        {
                                            Curt = Contexts[i];
                                            if (Curt.Type == ExpressionObjectType.Identifier &&
                                                Curt.ExpressionType != ExpressionType.And &&
                                                Curt.ExpressionType != ExpressionType.Or &&
                                                Curt.ExpressionType != ExpressionType.ExclusiveOr &&
                                                Curt.ExpressionType != ExpressionType.LeftShift &&
                                                Curt.ExpressionType != ExpressionType.RightShift)
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
                            }
                        }

                        Last = Curt;
                    }

                    // Check Children
                    if (Contexts.Count == 1 &&
                        Contexts[0] is ExpressionBlock Children)
                    {
                        Contexts.Clear();
                        Contexts.AddRange(Children.Contexts);
                        return;
                    }
                }
                #endregion

                #region Multiply and Divide
                {
                    Last = Contexts[0];
                    for (int i = 1; i < Contexts.Count; i++)
                    {
                        Curt = Contexts[i];
                        if (Curt.Type == ExpressionObjectType.Identifier &&
                            (Curt.ExpressionType == ExpressionType.Multiply ||
                             Curt.ExpressionType == ExpressionType.MultiplyAssign ||
                             Curt.ExpressionType == ExpressionType.MultiplyChecked ||
                             Curt.ExpressionType == ExpressionType.MultiplyAssignChecked ||
                             Curt.ExpressionType == ExpressionType.Divide ||
                             Curt.ExpressionType == ExpressionType.DivideAssign ||
                             Curt.ExpressionType == ExpressionType.Modulo ||
                             Curt.ExpressionType == ExpressionType.ModuloAssign))
                        {
                            if (Last.Type == ExpressionObjectType.Identifier)
                                throw new InvalidProgramException($"[Expression][{nameof(Normalize)}]Unknown Contexts : {{{this}}}.");

                            ExpressionBlock Group = new() { IsNormalized = true };
                            int StartIndex = i - 1,
                                Length = 2;

                            Group.Contexts.Add(Last);
                            Group.Contexts.Add(Curt);

                            i++;
                            for (; i < Contexts.Count; i++)
                            {
                                Curt = Contexts[i];
                                if (Curt.Type == ExpressionObjectType.Identifier &&
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
                        }

                        Last = Curt;
                    }

                    // Check Children
                    if (Contexts.Count == 1 &&
                        Contexts[0] is ExpressionBlock Children)
                    {
                        Contexts.Clear();
                        Contexts.AddRange(Children.Contexts);
                        return;
                    }
                }
                #endregion
            }
        }
        private void NormalizeMathAbs(ref int i, ref IExpressionObject Last, ref IExpressionObject Curt)
        {
            for (; i < Contexts.Count; i++)
            {
                Curt = Contexts[i];
                if (Curt.Type == ExpressionObjectType.Identifier &&
                    Curt.ExpressionType == ExpressionType.Or)
                {
                    if (Last is not null &&
                        Last.Type != ExpressionObjectType.Identifier)
                        break;

                    int StartIndex = i++;
                    Last = Curt;

                    NormalizeMathAbs(ref i, ref Last, ref Curt);
                    if (Curt.Type != ExpressionObjectType.Identifier ||
                        Curt.ExpressionType != ExpressionType.Or)
                        throw new InvalidProgramException($"[Epxression][{nameof(NormalizeMathAbs)}]Not found the end identifier of abs.");

                    int Length = i - StartIndex + 1;
                    if (Length < 3)
                        throw new InvalidProgramException($"[Epxression][{nameof(NormalizeMathAbs)}]Not found the variable in the identifiers of abs.");

                    ExpressionBlock Group = new() { IsNormalized = true };
                    for (int j = StartIndex + 1; j < i; j++)
                        Group.Contexts.Add(Contexts[j]);

                    ExpressionRoute Route = new();
                    Route.Contexts.Add(new ExpressionMember("Math"));
                    Route.Contexts.Add(new ExpressionMethod("Abs", [Group]));

                    Contexts.RemoveRange(StartIndex, Length);
                    Contexts.Insert(StartIndex, Route);

                    i = StartIndex;
                    Curt = Group;
                }

                Last = Curt;
            }
        }

        private static bool TryGetValidType(Type NumberType1, Type NumberType2, bool OnlyInteger, out Type Result)
        {
            if (OnlyInteger)
            {
                if (NumberType1.IsIntegerType())
                {
                    Result = NumberType2.IsIntegerType() ? MaxNumberType(NumberType1, NumberType2) : NumberType1;
                    return true;
                }

                if (NumberType2.IsIntegerType())
                {
                    Result = MaxNumberType(NumberType1, NumberType2);
                    return true;
                }

                Result = null;
                return false;
            }

            Result = MaxNumberType(NumberType1, NumberType2);
            return Result != null;
        }
        private static Type MaxNumberType(Type NumberType1, Type NumberType2)
            => ReflectionHelper.NumberTypes.TryGetValue(NumberType1, out byte v1) ?
               ReflectionHelper.NumberTypes.TryGetValue(NumberType2, out byte v2) ? v1 > v2 ? NumberType1 : NumberType2 : NumberType1 :
               ReflectionHelper.NumberTypes.ContainsKey(NumberType2) ? NumberType2 : null;

        public override string ToString()
            => string.Join(" ", Contexts.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()));

    }
}