using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionMember : IExpressionRoute
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Member;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.MemberAccess;

        public string Name { get; }

        public List<ExpressionTypeInfo> GenericTypes { get; }

        internal ExpressionMember(string Name)
        {
            this.Name = Name;
            GenericTypes = [];
        }
        internal ExpressionMember(string Name, IEnumerable<ExpressionTypeInfo> GenericTypes)
        {
            this.Name = Name;
            this.GenericTypes = new List<ExpressionTypeInfo>(GenericTypes);
        }

        private Expression Member;
        public Expression Implement(ExpressionMode Mode, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => GenericTypes.Count == 0 ? TryImplement(Mode, null, Base, Parameters, out Member) ? Member :
               throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found member : {this}.") :
               throw new InvalidProgramException($"[Expression]{this} is not object's member.");
        public bool TryImplement(ExpressionMode Mode, object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Expression Expression)
        {
            if (GenericTypes.Count > 0)
            {
                Expression = null;
                return false;
            }

            if (Member != null)
            {
                Expression = Member;
                return true;
            }

            // Base Member
            if (Parent is null)
            {
                // null
                if (Name == "null")
                {
                    Member = ExpressionHelper.Null;
                    Expression = Member;
                    return true;
                }

                // true
                if (Name == "true")
                {
                    Member = ExpressionHelper.True;
                    Expression = Member;
                    return true;
                }

                // false
                if (Name == "false")
                {
                    Member = ExpressionHelper.False;
                    Expression = Member;
                    return true;
                }

                // Parameters
                if (Parameters.FirstOrDefault(i => i.Name == Name) is ParameterExpression Parameter)
                {
                    Member = Parameter;
                    Expression = Member;
                    return true;
                }

                if (Base is null)
                {
                    Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Unknown member : {Name}.");
                    Expression = null;
                    return false;
                }

                // this (base)
                else if (Name == "this")
                {
                    Member = Base;
                    Expression = Member;
                    return true;
                }

                Type BaseType = Base.Type;
                if (BaseType.GetProperty(Name) is PropertyInfo Property)
                {
                    Member = Expression.Property(Base, Property);
                    Expression = Member;
                    return true;
                }

                else if (BaseType.GetField(Name) is FieldInfo Field)
                {
                    Member = Expression.Field(Base, Field);
                    Expression = Member;
                    return true;
                }

                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found member : {Name} in {BaseType.Name}.");
            }

            // Static Member
            else if (Parent is Type StaticType)
            {
                if (StaticType.GetProperty(Name) is PropertyInfo Property)
                {
                    Member = Expression.Property(null, Property);
                    Expression = Member;
                    return true;
                }

                else if (StaticType.GetField(Name) is FieldInfo Field)
                {
                    Member = Expression.Field(null, Field);
                    Expression = Member;
                    return true;
                }

                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found property or field : {Name} in {StaticType.Name}.");
            }

            // Parent Member
            else if (Parent is Expression ParentExpression)
            {
                Type ParentType = ParentExpression.Type;
                if (ParentType.GetProperty(Name) is PropertyInfo Property)
                {
                    Member = Expression.Property(ParentExpression, Property);
                    Expression = Member;
                    return true;
                }

                else if (ParentType.GetField(Name) is FieldInfo Field)
                {
                    Member = Expression.Field(ParentExpression, Field);
                    Expression = Member;
                    return true;
                }

                Debug.WriteLine($"[Expression][{nameof(TryImplement)}]Not found property or field : {Name} in {ParentType.Name}.");
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
            => GenericTypes.Count == 0 ? Name : $"{Name}<{string.Join(", ", GenericTypes)}>";

    }
}