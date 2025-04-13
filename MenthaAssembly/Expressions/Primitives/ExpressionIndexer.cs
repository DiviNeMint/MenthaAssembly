using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionIndexer : IExpressionRoute
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Indexer;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.Index;

        public string Name
            => null;

        public List<ExpressionTypeInfo> GenericTypes
            => throw new NotSupportedException();

        public List<IExpressionObject> Parameters { get; }

        internal ExpressionIndexer()
        {
            Parameters = [];
        }
        internal ExpressionIndexer(IEnumerable<IExpressionObject> Parameters)
        {
            this.Parameters = new List<IExpressionObject>(Parameters);
        }

        private Expression Indexer;
        public Expression Implement(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => TryImplement(Mode, null, Base, Parameters, out Indexer) ? Indexer :
               throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found valid indexer : {this}.");
        public bool TryImplement(ExpressionMode Mode, object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Expression Expression)
        {
            if (Indexer != null)
            {
                Expression = Indexer;
                return true;
            }

            Expression[] IndexerParameters = this.Parameters.Select(i => i.Implement(Mode, Base, Parameters)).ToArray();
            Type[] ParameterTypes = IndexerParameters.Select(i => i.Type).ToArray();

            // Base Indexer
            if (Parent is null)
            {
                if (Base is null)
                {
                    Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Unknown route.");
                    Expression = null;
                    return false;
                }

                Type BaseType = Base.Type;
                if (BaseType.TryGetIndexerWithImplicitParameter(ParameterTypes, out PropertyInfo IndexerInfo, out Type[] DefinedParameterTypes))
                {
                    for (int i = 0; i < ParameterTypes.Length; i++)
                        IndexerParameters[i] = IndexerParameters[i].Cast(DefinedParameterTypes[i]);

                    Indexer = Expression.MakeIndex(Base, IndexerInfo, IndexerParameters);
                    Expression = Indexer;
                    return true;
                }

                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found valid indexer in {BaseType.Name}.");
            }

            // Static Indexer
            else if (Parent is Type StaticType)
            {
                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Indexers are not supported for static types.");
            }

            // Parent Indexer
            else if (Parent is Expression ParentExpression)
            {
                Type ParentType = ParentExpression.Type;
                if (ParentType.IsArray)
                {
                    int Dimension = ParentType.GetArrayRank();
                    if (ParameterTypes.Length == Dimension &&
                        !ParameterTypes.Any(i => !i.IsConvertibleTo<int>()))
                    {
                        for (int i = 0; i < ParameterTypes.Length; i++)
                            IndexerParameters[i] = IndexerParameters[i].Cast(typeof(int));

                        Indexer = Expression.ArrayIndex(ParentExpression, IndexerParameters);
                        Expression = Indexer;
                        return true;
                    }
                }
                else if (ParentType.TryGetIndexerWithImplicitParameter(ParameterTypes, out PropertyInfo IndexerInfo, out Type[] DefinedParameterTypes))
                {
                    for (int i = 0; i < ParameterTypes.Length; i++)
                        IndexerParameters[i] = IndexerParameters[i].Cast(DefinedParameterTypes[i]);

                    Indexer = Expression.MakeIndex(ParentExpression, IndexerInfo, IndexerParameters);
                    Expression = Indexer;
                    return true;
                }

                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found valid indexer in {ParentType.Name}.");
            }

            // Unknown
            else
            {
                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Unknown parent type : {Parent.GetType().Name}.");
            }

            Expression = null;
            return false;
        }

        public override string ToString()
            => $"[{string.Join(", ", Parameters.Select(i => i.Type == ExpressionObjectType.Block ? $"({i})" : i.ToString()))}]";

    }
}