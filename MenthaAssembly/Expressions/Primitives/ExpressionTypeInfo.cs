using System.Collections.Generic;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionTypeInfo
    {
        public string Name { get; }

        public string Namespace { get; }

        public List<ExpressionTypeInfo> GenericTypes { get; }

        public ExpressionTypeInfo(string Name, string Namespace)
        {
            this.Name = Name;
            this.Namespace = Namespace;
            GenericTypes = new List<ExpressionTypeInfo>();
        }
        public ExpressionTypeInfo(string Name, string Namespace, IEnumerable<ExpressionTypeInfo> GenericTypes)
        {
            this.Name = Name;
            this.Namespace = Namespace;
            this.GenericTypes = new List<ExpressionTypeInfo>(GenericTypes);
        }

        public override string ToString()
            => GenericTypes.Count == 0 ? Name : $"{Name}<{string.Join(", ", GenericTypes)}>";

    }
}