using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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

            Normalize(Mode);

            int Index = 0;

            Expression Left = GetNextExpression(Mode, ref Index, Base, Parameters);
            for (; Index < Contexts.Count;)
            {
                IExpressionObject Context = Contexts[Index++];
                if (Context is not ExpressionIdentifier Identifier)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid syntax : {this}.");

                // Next
                Expression Right = GetNextExpression(Mode, ref Index, Base, Parameters);

                // Body
                Left = GetBodyExpression(Mode, ref Index, Identifier, Left, Right, Base, Parameters);
            }

            Block = Left;
            return Block;
        }

        private Expression GetNextExpression(ExpressionMode Mode, ref int Index, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            int Length = Contexts.Count;
            if (Index >= Length)
                throw new InvalidProgramException($"[Expression][{nameof(GetNextExpression)}]Invalid syntax : {this}.");

            IExpressionObject Next = Contexts[Index++];

            // Identifier
            if (Next.Type == ExpressionObjectType.Identifier)
            {
                // Check End
                if (Index >= Length)
                    throw new InvalidProgramException($"[Expression][{nameof(GetNextExpression)}]Invalid syntax : {this}.");

                ExpressionType Operator = Next.ExpressionType;
                Next = Contexts[Index++];

                // Check Right Object
                return Next.Type == ExpressionObjectType.Identifier
                    ? throw new InvalidProgramException($"[Expression][{nameof(GetNextExpression)}]Invalid syntax : {this}.")
                    : Operator switch
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

                        _ => throw new InvalidProgramException($"[Expression][{nameof(GetNextExpression)}]Invalid syntax : {this}."),
                    };
            }

            // Convert
            else if (Next is ExpressionConvert Convert)
            {
                return !Convert.TypeRoute.TryParseType(Base, Parameters, out Type Type)
                    ? throw new InvalidProgramException($"[Expression][{nameof(GetNextExpression)}]Invalid convert type : {Convert.TypeRoute}.")
                    : GetNextExpression(Mode, ref Index, Base, Parameters).Cast(Type);
            }

            return Next.Implement(Mode, Base, Parameters);
        }
        private Expression GetBodyExpression(ExpressionMode Mode, ref int Index, ExpressionIdentifier Operator, Expression Left, Expression Right, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
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
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

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
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Subtract(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // *
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Multiply(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // /
                case ExpressionType.Divide:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Divide(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // %
                case ExpressionType.Modulo:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, false, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.Modulo(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // &
                case ExpressionType.And:
                    {
                        // Checks Type
                        if (TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            return Expression.And(Left.Cast(MaxType), Right.Cast(MaxType));

                        if (Left.Type.SupportsBitwiseAnd() && Right.Type.SupportsBitwiseAnd())
                            return Expression.And(Left, Right);

                        throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");
                    }

                // |
                case ExpressionType.Or:
                    {
                        // Checks Type
                        if (TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            return Expression.Or(Left.Cast(MaxType), Right.Cast(MaxType));

                        if (Left.Type.SupportsBitwiseOr() && Right.Type.SupportsBitwiseOr())
                            return Expression.Or(Left, Right);

                        throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");
                    }

                // ^
                case ExpressionType.ExclusiveOr:
                    {
                        if (Mode == ExpressionMode.Math)
                            return Expression.Power(Left.Cast(typeof(double)), Right.Cast(typeof(double)));

                        // Checks Type
                        if (TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            return Expression.ExclusiveOr(Left.Cast(MaxType), Right.Cast(MaxType));

                        if (Left.Type.SupportsBitwiseXor() && Right.Type.SupportsBitwiseXor())
                            return Expression.ExclusiveOr(Left, Right);

                        throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");
                    }

                // <<
                case ExpressionType.LeftShift:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.LeftShift(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // >>
                case ExpressionType.RightShift:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.RightShift(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // <
                case ExpressionType.LessThan:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.LessThan(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // <=
                case ExpressionType.LessThanOrEqual:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.LessThanOrEqual(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // >
                case ExpressionType.GreaterThan:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.GreaterThan(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // >=
                case ExpressionType.GreaterThanOrEqual:
                    {
                        // Checks Type
                        if (!TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");

                        return Expression.GreaterThanOrEqual(Left.Cast(MaxType), Right.Cast(MaxType));
                    }

                // ==
                case ExpressionType.Equal:
                    {
                        // Checks Type
                        if (TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            return Expression.Equal(Left.Cast(MaxType), Right.Cast(MaxType));

                        if (Left.Type.SupportsComparisonEquality() && Right.Type.SupportsComparisonEquality())
                            return Expression.Equal(Left, Right);

                        throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");
                    }

                // !=
                case ExpressionType.NotEqual:
                    {
                        // Checks Type
                        if (TryGetValidType(Left.Type, Right.Type, true, out Type MaxType))
                            return Expression.NotEqual(Left.Cast(MaxType), Right.Cast(MaxType));

                        if (Left.Type.SupportsComparisonEquality() && Right.Type.SupportsComparisonEquality())
                            return Expression.NotEqual(Left, Right);

                        throw new InvalidCastException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {Left.Type.Name} {Operator} {Right.Type.Name}.");
                    }

                // &&
                case ExpressionType.AndAlso:
                    {
                        if (Left.Type != typeof(bool) && !Left.Type.SupportsLogicalAnd())
                            Left = Left.Cast(typeof(bool));

                        if (Right.Type != typeof(bool) && !Right.Type.SupportsLogicalAnd())
                            Right = Right.Cast(typeof(bool));

                        return Expression.AndAlso(Left, Right);
                    }

                // ||
                case ExpressionType.OrElse:
                    {
                        if (Left.Type != typeof(bool) && !Left.Type.SupportsLogicalOr())
                            Left = Left.Cast(typeof(bool));

                        if (Right.Type != typeof(bool) && !Right.Type.SupportsLogicalOr())
                            Right = Right.Cast(typeof(bool));

                        return Expression.OrElse(Left, Right);
                    }

                // ?
                case ExpressionType.Conditional when Operator.ToString() == "?":
                    {
                        int Length = Contexts.Count;
                        if (Index >= Length)
                            break;

                        IExpressionObject Context = Contexts[Index++];
                        if (Context is not ExpressionIdentifier Identifier || Index >= Length)
                            break;

                        Expression Right2 = GetNextExpression(Mode, ref Index, Base, Parameters);
                        string IdentifierSymbol = Identifier.ToString();
                        if (IdentifierSymbol == ":")
                            return Expression.Condition(Left, Right, Right2);

                        if (IdentifierSymbol == "?")
                        {
                            Right = GetBodyExpression(Mode, ref Index, Identifier, Right, Right2, Base, Parameters);

                            Identifier = Contexts[Index++] as ExpressionIdentifier;
                            if (Identifier is null || Index >= Length)
                                break;

                            Right2 = GetNextExpression(Mode, ref Index, Base, Parameters);
                            if (Identifier.ToString() == ":")
                                return Expression.Condition(Left, Right, Right2);
                        }

                        break;
                    }
            }

            throw new InvalidProgramException($"[Expression][{nameof(GetBodyExpression)}]Invalid syntax : {this}.");
        }

        private bool IsNormalized = false;
        private void Normalize(ExpressionMode Mode)
        {
            if (IsNormalized)
                return;

            // Explicit
            InternalNormalizeExplicit();

            // Abs
            if (Mode == ExpressionMode.Math)
                InternalNormalizeMathAbs();

            // Bitwise Operations
            InternalNormalizeBitwise();

            // Multiply and Divide
            InternalNormalizeArithmetic();

            // Comparison and Logical
            InternalNormalizeComparisonAndLogical();

            IsNormalized = true;
        }
        private void InternalNormalizeExplicit()
        {
            static bool IsBreak(IExpressionObject Item)
                => Item.Type is ExpressionObjectType.Block or
                   ExpressionObjectType.Route or
                   ExpressionObjectType.Const;

            if (Contexts.Count < 2)
                return;

            for (int i = 0; i < Contexts.Count; i++)
            {
                IExpressionObject Current = Contexts[i];
                if (Current.Type == ExpressionObjectType.Convert)
                {
                    int Start = i++;
                    if (!ExpressionHelper.TryGroupUntil(Contexts, ref i, IsBreak, false, true, out _, out IExpressionObject Group))
                        throw new InvalidProgramException($"[Epxression][{nameof(InternalNormalizeExplicit)}]Not found the end identifier of abs.");

                    ExpressionBlock Block = new();
                    Block.Contexts.Add(Current);
                    Block.Contexts.Add(Group);

                    Contexts.RemoveRange(Start, i - Start);
                    Contexts.Insert(Start, Block);

                    i = Start + 1;
                }
            }

            UnwrapIfOverwrapped(Contexts);
        }
        private void InternalNormalizeMathAbs()
        {
            void NormalizeMathAbs(ref int i, ref IExpressionObject Last, ref IExpressionObject Curt)
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
                            throw new InvalidProgramException($"[Epxression][{nameof(InternalNormalizeMathAbs)}]Not found the end identifier of abs.");

                        int Length = i - StartIndex + 1;
                        if (Length < 3)
                            throw new InvalidProgramException($"[Epxression][{nameof(InternalNormalizeMathAbs)}]Not found the variable in the identifiers of abs.");

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

            if (Contexts.Count < 3)
                return;

            int i = 0;
            IExpressionObject Last = null, Curt = null;
            NormalizeMathAbs(ref i, ref Last, ref Curt);
            UnwrapIfOverwrapped(Contexts);
        }
        private void InternalNormalizeBitwise()
        {
            if (Contexts.Count < 3)
                return;

            IExpressionObject Last = Contexts[0];
            for (int i = 1; i < Contexts.Count; i++)
            {
                IExpressionObject Curt = Contexts[i];
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
                                    throw new InvalidProgramException($"[Expression][{nameof(InternalNormalizeBitwise)}]Unknown Contexts : {{{this}}}.");

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

            UnwrapIfOverwrapped(Contexts);
        }
        private void InternalNormalizeArithmetic()
        {
            if (Contexts.Count < 3)
                return;

            IExpressionObject Last = Contexts[0];
            for (int i = 1; i < Contexts.Count; i++)
            {
                IExpressionObject Curt = Contexts[i];
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
                        throw new InvalidProgramException($"[Expression][{nameof(InternalNormalizeArithmetic)}]Unknown Contexts : {{{this}}}.");

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

            UnwrapIfOverwrapped(Contexts);
        }
        private void InternalNormalizeComparisonAndLogical()
        {
            static bool IsBreak(IExpressionObject Item)
                => Item.Type == ExpressionObjectType.Identifier &&
                   (Item.ExpressionType == ExpressionType.Equal ||
                    Item.ExpressionType == ExpressionType.NotEqual ||
                    Item.ExpressionType == ExpressionType.AndAlso ||
                    Item.ExpressionType == ExpressionType.OrElse ||
                    Item.ExpressionType == ExpressionType.GreaterThan ||
                    Item.ExpressionType == ExpressionType.GreaterThanOrEqual ||
                    Item.ExpressionType == ExpressionType.LessThan ||
                    Item.ExpressionType == ExpressionType.LessThanOrEqual ||
                    Item.ExpressionType == ExpressionType.Conditional);

            if (Contexts.Count < 3)
                return;

            int i = 0;
            if (!ExpressionHelper.TryGroupUntil(Contexts, ref i, IsBreak, false, false, out _, out IExpressionObject Group))
                return;

            try
            {
                if (i > 1)
                {
                    Contexts.RemoveRange(0, i);
                    Contexts.Insert(0, Group);

                    // Skip the operator.
                    i = 2;
                }
                else
                {
                    i++;
                }

                for (; i < Contexts.Count;)
                {
                    int Start = i;
                    if (!ExpressionHelper.TryGroupUntil(Contexts, ref i, IsBreak, true, false, out _, out Group))
                    {
                        string Syntax = string.Join(" ", Contexts.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()));
                        throw new InvalidProgramException($"[Expression][{nameof(InternalNormalizeComparisonAndLogical)}]Invalid syntax : {Syntax}.");
                    }

                    int Length = i - Start;
                    if (Length > 1)
                    {
                        Contexts.RemoveRange(Start, Length);
                        Contexts.Insert(Start, Group);

                        // Skip the operator.
                        i = Start + 2;
                        continue;
                    }

                    // Skip the operator.
                    i++;
                }
            }
            finally
            {
                UnwrapIfOverwrapped(Contexts);
            }
        }

        private static bool TryGetConverterType(ExpressionBlock Block, out ExpressionRoute Type)
        {
            if (Block.Contexts.Count != 1 ||
                Block.Contexts[0] is not ExpressionRoute Route ||
                Route.Contexts.Any(i => i.Type != ExpressionObjectType.Member))
            {
                Type = null;
                return false;
            }

            Type = Route;
            return true;
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

        private static void UnwrapIfOverwrapped(List<IExpressionObject> Contexts)
        {
            if (Contexts.Count == 1 &&
                Contexts[0] is ExpressionBlock Children)
            {
                Contexts.Clear();
                Contexts.AddRange(Children.Contexts);
            }
        }

        public static bool TryParse(string Code, out ExpressionBlock Block)
        {
            StringBuilder Builder = new();
            int Index = 0,
                Length = Code.Length;

            Block = new ExpressionBlock();
            while (ExpressionHelper.TryParseElement(Code, ref Index, Length, ref Builder, out IExpressionObject Element))
                Block.Contexts.Add(Element);

            if (Block.Contexts.Count == 0 || Index < Length)
            {
                Block = null;
                return false;
            }

            NormalizeTypeCasts(Block.Contexts);
            return true;
        }
        private static void NormalizeTypeCasts(List<IExpressionObject> Contexts)
        {
            for (int i = 0; i < Contexts.Count;)
            {
                IExpressionObject Curt = Contexts[i++];
                if (Curt is ExpressionBlock Group)
                {
                    // Check if it could be a converter.
                    if (!TryGetConverterType(Group, out ExpressionRoute Type))
                    {
                        NormalizeTypeCasts(Group.Contexts);
                        continue;
                    }

                    // Checks end.
                    if (i >= Contexts.Count)
                        break;

                    // Checks if the it is a variable by checking if next one is an operator.
                    if (Contexts[i].Type == ExpressionObjectType.Identifier)
                        continue;

                    Contexts.Insert(i - 1, new ExpressionConvert(Type));
                    Contexts.RemoveAt(i);
                }
                else if (Curt is ExpressionRoute Route)
                {
                    IExpressionRoute Context = Route.Contexts[Route.Contexts.Count - 1];
                    if (Context is ExpressionMethod Method)
                    {
                        NormalizeTypeCasts(Method.Parameters);
                    }

                    else if (Context is ExpressionIndexer Indexer)
                    {
                        NormalizeTypeCasts(Indexer.Parameters);
                    }
                }
            }
        }

        public override string ToString()
            => string.Join(" ", Contexts.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()));

    }
}