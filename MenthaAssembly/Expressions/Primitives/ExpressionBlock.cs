using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionBlock : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Block;

        public List<IExpressionObject> Contexts { get; }

        public ExpressionBlock()
        {
            Contexts = new List<IExpressionObject>();
        }
        public ExpressionBlock(IEnumerable<IExpressionObject> Contexts)
        {
            this.Contexts = new List<IExpressionObject>(Contexts);
        }

        public override string ToString()
            => string.Join(" ", Contexts.Select(i => i.Type == ExpressionObjectType.Block ? $"( {i} )" : i.ToString()));

    }
}