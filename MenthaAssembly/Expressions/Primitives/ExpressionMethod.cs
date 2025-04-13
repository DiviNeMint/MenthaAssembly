using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal ExpressionMethod(string Name)
        {
            this.Name = Name;
            GenericTypes = [];
            Parameters = [];
        }
        internal ExpressionMethod(string Name, IEnumerable<IExpressionObject> Parameters)
        {
            this.Name = Name;
            GenericTypes = [];
            this.Parameters = new List<IExpressionObject>(Parameters);
        }
        internal ExpressionMethod(string Name, IEnumerable<ExpressionTypeInfo> GenericTypes, IEnumerable<IExpressionObject> Parameters)
        {
            this.Name = Name;
            this.GenericTypes = new List<ExpressionTypeInfo>(GenericTypes);
            this.Parameters = new List<IExpressionObject>(Parameters);
        }

        private Expression Method;
        public Expression Implement(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => TryImplement(Mode, null, Base, Parameters, out Method) ? Method :
               throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found valid method : {this}.");
        public bool TryImplement(ExpressionMode Mode, object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Expression Expression)
        {
            if (Method != null)
            {
                Expression = Method;
                return true;
            }

            Expression[] MethodParameters = this.Parameters.Select(i => i.Implement(Mode, Base, Parameters)).ToArray();
            Type[] ParameterTypes = MethodParameters.Select(i => i.Type).ToArray(),
                   GenericTypes = this.GenericTypes.Select(i => i.Implement()).ToArray();

            // Base Method
            if (Parent is null)
            {
                if (Base is null)
                {
                    Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Invalid route.");
                    Expression = null;
                    return false;
                }

                if (!ReflectionHelper.TryGetMethodWithImplicitParameter(Base.Type, Name, GenericTypes, ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes))
                {
                    Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found valid method : {Name}" +
                                    $"{(GenericTypes.Length > 0 ? $"<{string.Join(", ", GenericTypes.Select(i => i.Name))}>" : string.Empty)}, " +
                                    $"with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {Base.Type.Name}.");
                    Expression = null;
                    return false;
                }

                for (int i = 0; i < ParameterTypes.Length; i++)
                    MethodParameters[i] = MethodParameters[i].Cast(DefinedParameterTypes[i]);

                Method = Expression.Call(Base, Info, MethodParameters);
                Expression = Method;
                return true;
            }

            // Static Method
            else if (Parent is Type StaticType)
            {
                if (!ReflectionHelper.TryGetMethodWithImplicitParameter(StaticType, Name, GenericTypes, ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes))
                {
                    Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found valid method : {Name}" +
                                    $"{(GenericTypes.Length > 0 ? $"<{string.Join(", ", GenericTypes.Select(i => i.Name))}>" : string.Empty)}, " +
                                    $"with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {StaticType.Name}.");
                    Expression = null;
                    return false;
                }

                for (int i = 0; i < ParameterTypes.Length; i++)
                    MethodParameters[i] = MethodParameters[i].Cast(DefinedParameterTypes[i]);

                Method = Expression.Call(Info, MethodParameters);
                Expression = Method;
                return true;
            }

            // Parent Method
            else if (Parent is Expression ParentExpression)
            {
                if (!ReflectionHelper.TryGetMethodWithImplicitParameter(ParentExpression.Type, Name, GenericTypes, ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes))
                {
                    Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found valid method : {Name}" +
                                    $"{(GenericTypes.Length > 0 ? $"<{string.Join(", ", GenericTypes.Select(i => i.Name))}>" : string.Empty)}, " +
                                    $"with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {ParentExpression.Type.Name}.");
                    Expression = null;
                    return false;
                }

                for (int i = 0; i < ParameterTypes.Length; i++)
                    MethodParameters[i] = MethodParameters[i].Cast(DefinedParameterTypes[i]);

                Method = Expression.Call(ParentExpression, Info, MethodParameters);
                Expression = Method;
                return true;
            }

            // Unknown
            else
            {
                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Unknown parent : {Parent.GetType().Name}.");
            }

            Expression = null;
            return false;
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