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
            => ExpressionObjectType.Route;

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
            => TryImplement(null, Base, Parameters, out Element) ? Element :
               throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown route : {this}.");
        public bool TryImplement(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Expression Expression)
        {
            if (Element != null)
            {
                Expression = Element;
                return true; ;
            }

            else if (Contexts.Count < 1)
            {
                Expression = null;
                return false;
            }

            Debug.Assert(Parent == null);

            IExpressionRoute Context = Contexts[0];
            if (Contexts.Count == 1)
                return Context.TryImplement(Parent, Base, Parameters, out Expression);

            Expression = null;
            int Index = 0,
                Count = Contexts.Count;
            if (Context.Type == ExpressionObjectType.Member &&
                !Context.TryImplement(Parent, Base, Parameters, out Expression) &&
                !TryImplementFirstRoute(Parent, Base, Parameters, out Expression, out Index))
                return false;

            for (; Index < Count; Index++)
                if (!Contexts[Index].TryImplement(Expression, Base, Parameters, out Expression))
                    return false;

            if (Expression is null)
                return false;

            Element = Expression;
            return true;
        }
        private bool TryImplementFirstRoute(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Expression Expression, out int Index)
        {
            StringBuilder Builder = new StringBuilder();
            try
            {
                Index = 0;
                int Count = Contexts.Count;
                IExpressionRoute Context = Contexts[Index++];

                string MemberName = Context.Name;
                Type[] GenericTypes = Context.GenericTypes.Select(i => i.Implement()).ToArray();

                if (ReflectionHelper.TryGetType(MemberName, GenericTypes, out Type Type))
                {
                    if (Index >= Count)
                    {
                        Expression = null;
                        return false;
                    }

                    if (Contexts[Index].TryImplement(Type, Base, Parameters, out Expression))
                        return true;
                }

                Builder.Append(MemberName);
                while (Index < Count)
                {
                    Context = Contexts[Index++];
                    if (Context.Type != ExpressionObjectType.Member)
                        break;

                    MemberName = Context.Name;
                    GenericTypes = Context.GenericTypes.Select(i => i.Implement()).ToArray();
                    if (ReflectionHelper.TryGetType(MemberName, Builder.ToString(), GenericTypes, out Type))
                    {
                        if (Index >= Count)
                        {
                            Expression = null;
                            return false;
                        }

                        if (Contexts[Index].TryImplement(Type, Base, Parameters, out Expression))
                            return true;
                    }

                    if (GenericTypes.Length > 0)
                        break;

                    Builder.Append('.');
                    Builder.Append(MemberName);
                }

                Expression = null;
                return false;
            }
            finally
            {
                Builder.Clear();
            }
        }

        public bool TryParseType(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Type Type)
        {
            IExpressionRoute Context = Contexts[0];

            string Name = Context.Name;
            if (Context.Type != ExpressionObjectType.Member ||
                Context.GenericTypes.Count > 0)
            {
                Type = null;
                return false;
            }

            int Count = Contexts.Count;
            if (Count == 1)
            {
                if (Parameters.Any(i => i.Name == Name) ||
                    Base.Type.GetProperty(Name) != null ||
                    Base.Type.GetField(Name) != null)
                {
                    Type = null;
                    return false;
                }

                return ReflectionHelper.TryGetType(Context.Name, string.Empty, out Type);
            }

            StringBuilder Builder = new StringBuilder();
            try
            {
                Builder.Append(Name);
                int LastIndex = Count - 1;
                for (int i = 1; i < LastIndex; i++)
                {
                    Context = Contexts[i];
                    if (Context.Type != ExpressionObjectType.Member ||
                        Context.GenericTypes.Count > 0)
                    {
                        Type = null;
                        return false;
                    }

                    Builder.Append('.');
                    Builder.Append(Context.Name);
                }

                Context = Contexts[LastIndex];
                if (Context.Type != ExpressionObjectType.Member)
                {
                    Type = null;
                    return false;
                }

                return ReflectionHelper.TryGetType(Context.Name, Builder.ToString(), out Type);
            }
            finally
            {
                Builder.Clear();
            }
        }

        public override string ToString()
            => string.Join(".", Contexts);

    }
}