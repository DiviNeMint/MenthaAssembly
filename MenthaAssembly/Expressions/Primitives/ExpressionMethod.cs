using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionMethod : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Method;

        public string Path { get; }

        public List<IExpressionObject> Parameters { get; }

        public ExpressionMethod(string Path)
        {
            this.Path = Path;
            Parameters = new List<IExpressionObject>();
        }
        public ExpressionMethod(string Path, IEnumerable<IExpressionObject> Parameters)
        {
            this.Path = Path;
            this.Parameters = new List<IExpressionObject>(Parameters);
        }

        public override string ToString()
            => $"{Path}({string.Join(", ", Parameters.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()))})";

    }
}