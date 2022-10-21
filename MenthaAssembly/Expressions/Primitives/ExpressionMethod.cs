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
            Type[] ParameterTypes = MethodParameters.Select(i => i.Type).ToArray();

            // Base Method
            if (Parent is null)
            {
                if (Base is null ||
                    !TryGetMethod(Name, Base.Type, ParameterTypes, out MethodInfo Info))
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}, with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {Base.Type.Name}.");

                Method = Expression.Call(Base, Info, MethodParameters);
            }

            // Static Method
            else if (Parent is Type StaticType)
            {
                if (!TryGetMethod(Name, StaticType, ParameterTypes, out MethodInfo Info))
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}, with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {StaticType.Name}.");

                Method = Expression.Call(Info, MethodParameters);
            }

            // Parent Method
            else if (Parent is Expression Expression)
            {
                if (!TryGetMethod(Name, Expression.Type, ParameterTypes, out MethodInfo Info))
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found method : {Name}, with parameters : {{{string.Join(", ", ParameterTypes.Select(i => i.Name))}}} in {Expression.Type.Name}.");

                Method = Expression.Call(Expression, Info, MethodParameters);
            }

            // Unknown
            else
            {
                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown parent type : {Parent.GetType().Name}.");
            }

            return Method;
        }

        private static bool TryGetMethod(string MethodName, Type Base, Type[] ParameterTypes, out MethodInfo Info)
        {
            if (Base.GetMethod(MethodName, ParameterTypes) is MethodInfo StandardMethod)
            {
                Info = StandardMethod;
                return true;
            }

            else if (ParameterTypes.Length == 1)
            {
                MethodInfo TempMethod = null;

                // Checks is NumberType.
                if (ReflectionHelper.NumberTypes.TryGetValue(Base, out byte BaseKey))
                {
                    Type TempParameterType;
                    byte TempKey = 0;

                    foreach (MethodInfo Method in Base.GetMethods()
                                                      .Where(i => i.Name == MethodName))
                    {
                        ParameterInfo[] Parameters = Method.GetParameters();
                        if (Parameters.Length == 1)
                        {
                            Type ParameterType = Parameters[0].ParameterType;
                            if (ReflectionHelper.NumberTypes.TryGetValue(ParameterType, out byte MethodKey))
                            {
                                if (TempMethod is null)
                                {
                                    TempMethod = Method;
                                    TempParameterType = ParameterType;
                                    TempKey = MethodKey;
                                    continue;
                                }

                                // Finds the closest number type.
                                if (TempKey < BaseKey)
                                {
                                    if (TempKey < MethodKey)
                                    {
                                        TempMethod = Method;
                                        TempParameterType = ParameterType;
                                        TempKey = MethodKey;
                                    }
                                }
                                else
                                {
                                    if (MethodKey < TempKey)
                                    {
                                        TempMethod = Method;
                                        TempParameterType = ParameterType;
                                        TempKey = MethodKey;
                                    }
                                }
                            }
                        }
                    }

                    if (TempMethod != null)
                    {
                        Info = TempMethod;
                        return true;
                    }
                }
                else
                {
                    foreach (MethodInfo Method in Base.GetMethods()
                                                      .Where(i => i.Name == MethodName))
                    {
                        ParameterInfo[] Parameters = Method.GetParameters();
                        if (Parameters.Length == 1 &&
                            Base.IsConvertibleTo(Parameters[0].ParameterType))
                        {
                            Info = Method;
                            return true;
                        }
                    }
                }
            }

            Info = null;
            return false;
        }

        public override string ToString()
            => $"{Name}({string.Join(", ", Parameters.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()))})";

    }
}