using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionMethod : IExpressionRoute
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Method;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Call;

        public string Name { get; }

        public List<ExpressionTypeInfo> GenericTypes { get; }

        public List<IExpressionObject> Parameters { get; }

        public ExpressionMethod(string Name)
        {
            this.Name = Name;
            GenericTypes = new List<ExpressionTypeInfo>(0);
            Parameters = new List<IExpressionObject>(0);
        }
        public ExpressionMethod(string Name, IEnumerable<IExpressionObject> Parameters)
        {
            this.Name = Name;
            GenericTypes = new List<ExpressionTypeInfo>(0);
            this.Parameters = new List<IExpressionObject>(Parameters);
        }
        public ExpressionMethod(string Name, IEnumerable<ExpressionTypeInfo> GenericTypes, IEnumerable<IExpressionObject> Parameters)
        {
            this.Name = Name;
            this.GenericTypes = new List<ExpressionTypeInfo>(GenericTypes);
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
            Type[] ParameterTypes = MethodParameters.Select(i => i.Type).ToArray(),
                   GenericTypes = this.GenericTypes.Select(i => i.Implement()).ToArray();

            // Base Method
            if (Parent is null)
            {
                if (Base is null)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Invalid route.");

                if (!ReflectionHelper.TryGetMethodWithImplicitParameter(Base.Type, Name, GenericTypes, ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes))
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}" +
                                                      $"{(GenericTypes.Length > 0 ? $"<{string.Join(", ", GenericTypes.Select(i => i.Name))}>" : string.Empty)}, " +
                                                      $"with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {Base.Type.Name}.");

                for (int i = 0; i < ParameterTypes.Length; i++)
                    MethodParameters[i] = MethodParameters[i].Cast(DefinedParameterTypes[i]);

                Method = Expression.Call(Base, Info, MethodParameters);
            }

            // Static Method
            else if (Parent is Type StaticType)
            {
                if (!ReflectionHelper.TryGetMethodWithImplicitParameter(StaticType, Name, GenericTypes, ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes))
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}" +
                                                      $"{(GenericTypes.Length > 0 ? $"<{string.Join(", ", GenericTypes.Select(i => i.Name))}>" : string.Empty)}, " +
                                                      $"with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {StaticType.Name}.");

                for (int i = 0; i < ParameterTypes.Length; i++)
                    MethodParameters[i] = MethodParameters[i].Cast(DefinedParameterTypes[i]);

                Method = Expression.Call(Info, MethodParameters);
            }

            // Parent Method
            else if (Parent is Expression Expression)
            {
                if (!ReflectionHelper.TryGetMethodWithImplicitParameter(Expression.Type, Name, GenericTypes, ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes))
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}" +
                                                      $"{(GenericTypes.Length > 0 ? $"<{string.Join(", ", GenericTypes.Select(i => i.Name))}>" : string.Empty)}, " +
                                                      $"with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {Expression.Type.Name}.");

                for (int i = 0; i < ParameterTypes.Length; i++)
                    MethodParameters[i] = MethodParameters[i].Cast(DefinedParameterTypes[i]);

                Method = Expression.Call(Expression, Info, MethodParameters);
            }

            // Unknown
            else
            {
                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown parent : {Parent.GetType().Name}.");
            }

            return Method;
        }

        public override string ToString()
        {
            StringBuilder Builder = new StringBuilder(Name);
            try
            {
                if (GenericTypes.Count > 0)
                    Builder.Append($"<{string.Join(", ", GenericTypes)}>");

                Builder.Append($"({string.Join(", ", Parameters.Select(i => i.Type == ExpressionObjectType.Block ? $"({i})" : i.ToString()))})");
                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

    }
}