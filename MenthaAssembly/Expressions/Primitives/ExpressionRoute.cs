using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionRoute : IExpressionRoute
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Element;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Block;

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
            int Index = 0;
            if (Context is ExpressionMember Member)
            {
                Index++;
                if (!Member.TryImplement(Parent, Base, Parameters, out Current))
                {
                    if (AppDomain.CurrentDomain.GetAssemblies()
                                               .TrySelectMany(i => i.GetTypes())
                                               .FirstOrDefault(i => i.Name == Member.Name) is not Type StaticType)
                        throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown member : {Member.Name}.");

                    Current = Contexts[Index++].Implement(StaticType, Base, Parameters);
                }
            }

            for (; Index < Contexts.Count; Index++)
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
