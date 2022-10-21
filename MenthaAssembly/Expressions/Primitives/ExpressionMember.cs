using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionMember : IExpressionElement
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Member;

        ExpressionType IExpressionObject.ExpressionType
            => ExpressionType.MemberAccess;

        public string Name { get; }

        public ExpressionMember(string Name)
        {
            this.Name = Name;
        }

        private Expression Member;
        public Expression Implement(ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
            => Implement(null, Base, Parameters);
        public Expression Implement(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters)
        {
            if (Member != null)
                return Member;

            // Base Member
            if (Parent is null)
            {
                // Parameters
                if (Parameters.FirstOrDefault(i => i.Name == Name) is ParameterExpression Parameter)
                {
                    Member = Parameter;
                    return Member;
                }

                if (Base is null)
                    throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown member : {Name}.");

                // this (base)
                if (Name == "this")
                {
                    Member = Base;
                    return Base;
                }

                Type BaseType = Base.Type;
                if (BaseType.GetProperty(Name) is PropertyInfo Property)
                {
                    Member = Expression.Property(null, Property);
                    return Member;
                }

                else if (BaseType.GetField(Name) is FieldInfo Field)
                {
                    Member = Expression.Field(null, Field);
                    return Member;
                }

                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found member : {Name}. in {BaseType.Name}");
            }

            // Static Member
            else if (Parent is Type StaticType)
            {
                if (StaticType.GetProperty(Name) is PropertyInfo Property)
                {
                    Member = Expression.Property(null, Property);
                    return Member;
                }

                else if (StaticType.GetField(Name) is FieldInfo Field)
                {
                    Member = Expression.Field(null, Field);
                    return Member;
                }

                throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Not found property or field : {Name}. in {StaticType.Name}");
            }

            // Parent Method
            else if (Parent is Expression Expression)
            {
                Member = Expression.PropertyOrField(Expression, Name);
                return Member;
            }

            // Unknown
            throw new InvalidProgramException($"[Expression][{nameof(Implement)}]Unknown parent type : {Parent.GetType().Name}.");
        }
        public bool TryImplement(object Parent, ConstantExpression Base, IEnumerable<ParameterExpression> Parameters, out Expression Member)
        {
            if (this.Member != null)
            {
                Member = this.Member;
                return true;
            }

            // Base Member
            if (Parent is null)
            {
                // Parameters
                if (Parameters.FirstOrDefault(i => i.Name == Name) is ParameterExpression Parameter)
                {
                    this.Member = Parameter;
                    Member = this.Member;
                    return true;
                }

                if (Base is null)
                {
                    Member = null;
                    return false;
                }

                Type BaseType = Base.Type;
                if (BaseType.GetProperty(Name) is PropertyInfo Property)
                {
                    this.Member = Expression.Property(null, Property);
                    Member = this.Member;
                    return true;
                }

                else if (BaseType.GetField(Name) is FieldInfo Field)
                {
                    this.Member = Expression.Field(null, Field);
                    Member = this.Member;
                    return true;
                }

                Member = null;
                return false;
            }

            // Static Member
            else if (Parent is Type StaticType)
            {
                if (StaticType.GetProperty(Name) is PropertyInfo Property)
                {
                    this.Member = Expression.Property(null, Property);
                    Member = this.Member;
                    return true;
                }

                else if (StaticType.GetField(Name) is FieldInfo Field)
                {
                    this.Member = Expression.Field(null, Field);
                    Member = this.Member;
                    return true;
                }

                Member = null;
                return false;
            }

            // Parent Method
            else if (Parent is Expression Expression)
            {
                this.Member = System.Linq.Expressions.Expression.PropertyOrField(Expression, Name);
                Member = this.Member;
                return true;
            }

            // Unknown
            Member = null;
            return false;
        }

        public override string ToString()
            => Name;
    }
}