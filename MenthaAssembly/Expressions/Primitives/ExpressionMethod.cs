using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionMethod : IExpressionElement
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Method;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Call;

        public string Name { get; }

        public List<IExpressionObject> Parameters { get; }

        public ExpressionMethod(string Name)
        {
            this.Name = Name;
            Parameters = new List<IExpressionObject>();
        }
        public ExpressionMethod(string Name, IEnumerable<IExpressionObject> Parameters)
        {
            this.Name = Name;
            this.Parameters = new List<IExpressionObject>(Parameters);
        }

        private MethodCallExpression Method;
        public Expression Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => Implement(null, Base, Parameters);
        public Expression Implement(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (Method != null)
                return Method;

            Expression[] MethodParameters = this.Parameters.Select(i => i.Implement(Base, Parameters)).ToArray();
            Type[] ArgTypes = MethodParameters.Select(i => i.Type).ToArray();

            // Base Method
            if (Parent is null)
            {
                if (Base is null ||
                    Base.Type.GetMethod(Name, ArgTypes) is not MethodInfo Info)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}, with parameters : {{{string.Join(", ", ArgTypes.Select(i => i.Name))}}} in {Base.Type.Name}.");

                Method = Expression.Call(Base, Info, MethodParameters);
            }

            // Static Method
            else if (Parent is Type StaticType)
            {
                if (StaticType.GetMethod(Name, ArgTypes) is not MethodInfo Info)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}, with parameters : {{{string.Join(", ", ArgTypes.Select(i => i.Name))}}} in {StaticType.Name}.");

                Method = Expression.Call(Info, MethodParameters);
            }

            // Parent Method
            else if (Parent is Expression Expression)
            {
                if (Expression.Type.GetMethod(Name, ArgTypes) is not MethodInfo Info)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}, with parameters : {{{string.Join(", ", ArgTypes.Select(i => i.Name))}}} in {Expression.Type.Name}.");

                Method = Expression.Call(Expression, Info, MethodParameters);
            }

            // Unknown
            else
            {
                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown parent type : {Parent.GetType().Name}.");
            }

            return Method;
        }

        public override string ToString()
            => $"{Name}({string.Join(", ", Parameters.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()))})";

    }
}