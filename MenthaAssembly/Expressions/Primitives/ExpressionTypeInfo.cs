using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionTypeInfo
    {
        public string Name { get; }

        public string Namespace { get; }

        public List<ExpressionTypeInfo> GenericTypes { get; }

        internal ExpressionTypeInfo(string Name, string Namespace)
        {
            this.Name = Name;
            this.Namespace = Namespace;
            GenericTypes = [];
        }
        internal ExpressionTypeInfo(string Name, string Namespace, IEnumerable<ExpressionTypeInfo> GenericTypes)
        {
            this.Name = Name;
            this.Namespace = Namespace;
            this.GenericTypes = new List<ExpressionTypeInfo>(GenericTypes);
        }

        public Type Implement()
        {
            Type[] GenericTypes = this.GenericTypes.Select(i => i.Implement())
                                                   .ToArray();
            return ReflectionHelper.TryGetType(Name, Namespace, GenericTypes, out Type t) ? t :
                   throw new TypeLoadException($"Not found type : {this}");
        }

        public override string ToString()
            => GenericTypes.Count == 0 ? Name : $"{Name}<{string.Join(", ", GenericTypes)}>";

    }
}