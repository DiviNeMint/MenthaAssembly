using System.Collections.Generic;

namespace MenthaAssembly
{
    public interface IHtmlNode : IEnumerable<IHtmlNode>
    {
        public string Name { get; }

        public IReadOnlyDictionary<string, object> Attributes { get; }

        public object this[string Attribute] { get; }

        public IEnumerable<IHtmlNode> Children { get; }

    }
}