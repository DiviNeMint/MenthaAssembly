using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionRoute : IExpressionRoute
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Element;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Block;

        public string Name
            => ToString();

        public List<ExpressionTypeInfo> GenericTypes
            => Contexts.LastOrDefault()?.GenericTypes;

        public List<IExpressionRoute> Contexts { get; }

        public ExpressionRoute()
        {
            Contexts = new List<IExpressionRoute>();
        }
        public ExpressionRoute(IEnumerable<IExpressionRoute> Contexts)
        {
            this.Contexts = new List<IExpressionRoute>(Contexts);
        }

        private Expression Element;
        public Expression Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => Implement(null, Base, Parameters);
        public Expression Implement(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (Element != null)
                return Element;

            if (Contexts.Count < 1)
                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]No Contexts.");

            Debug.Assert(Parent == null);

            IExpressionRoute Context = Contexts[0];
            if (Contexts.Count == 1)
            {
                Element = Context.Implement(Parent, Base, Parameters);
                return Element;
            }

            Expression Current = null;
            int Index = 0,
                Count = Contexts.Count;
            if (Context is ExpressionMember Member)
            {
                Index++;
                if (!Member.TryImplement(Parent, Base, Parameters, out Current))
                {
                    Type[] GenericTypes = Member.GenericTypes.Select(i => i.Implement()).ToArray();
                    if (!ReflectionHelper.TryGetType(Member.Name, GenericTypes, out Type StaticType))
                    {
                        StringBuilder Builder = new StringBuilder();

                        try
                        {
                            do
                            {
                                if (Index >= Count ||
                                    GenericTypes.Length > 0)
                                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown route : {this}.");

                                // Namespace
                                Builder.Append(Context.Name);

                                // Next Member
                                Context = Contexts[Index++];
                                if (Context.Type != ExpressionObjectType.Member)
                                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown route : {this}.");

                                // GenericTypes
                                GenericTypes = Member.GenericTypes.Select(i => i.Implement()).ToArray();

                            } while (!ReflectionHelper.TryGetType(Context.Name, Builder.ToString(), out StaticType));
                        }
                        finally
                        {
                            Builder.Clear();
                        }
                    }

                    Current = Contexts[Index++].Implement(StaticType, Base, Parameters);
                }
            }

            for (; Index < Count; Index++)
                Current = Contexts[Index].Implement(Current, Base, Parameters);

            if (Current is null)
                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown element : {this}.");

            Element = Current;
            return Current;
        }

        public override string ToString()
            => string.Join(".", Contexts);

    }
}