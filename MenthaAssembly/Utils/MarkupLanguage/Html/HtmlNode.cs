using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MenthaAssembly
{
    public sealed class HtmlNode : IHtmlNode
    {
        public string Name { get; }

        public IReadOnlyDictionary<string, object> Attributes { get; }

        public object this[string Attribute]
            => Attributes.TryGetValue(Attribute, out object value) ? value : null;

        public IEnumerable<IHtmlNode> Children { get; }

        public HtmlNode(string Name)
        {
            this.Name = Name;
            Attributes = new Dictionary<string, object>();
            Children = new HtmlNode[0];
        }
        public HtmlNode(string Name, object Value)
        {
            this.Name = Name;
            Attributes = new Dictionary<string, object>
            {
                { "Content", Value}
            };
            Children = new HtmlNode[0];
        }
        public HtmlNode(string Name, Dictionary<string, object> Attributes)
        {
            this.Name = Name;
            this.Attributes = Attributes;
            Children = new HtmlNode[0];
        }
        public HtmlNode(string Name, Dictionary<string, object> Attributes, IEnumerable<HtmlNode> Children)
        {
            this.Name = Name;
            this.Attributes = Attributes;
            this.Children = Children;
        }

        public IEnumerator<IHtmlNode> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            if (Name == "Text")
                return Attributes.TryGetValue("Content", out object Value) ? Value.ToString() : null;

            if (Name == "Comment")
                return Attributes.TryGetValue("Content", out object Value) ? $"<!{Value}>" : null;

            StringBuilder Builder = new StringBuilder($"<{Name}");
            try
            {
                if (Attributes.Count > 0)
                    Builder.Append($" {string.Join(" ", Attributes.Where(i => i.Key != "Content").Select(i => $"{i.Key}=\"{i.Value}\""))}");

                IEnumerator<IHtmlNode> Enumerator = Children.GetEnumerator();
                if (!Enumerator.MoveNext())
                {
                    Builder.Append("/>");
                    return Builder.ToString();
                }

                Builder.Append(">");

                IHtmlNode Current = Enumerator.Current;
                bool NewLine = Current.Name != "Text";

                do
                {
                    Current = Enumerator.Current;

                    if (NewLine)
                        Builder.AppendLine();

                    NewLine = Current.Name != "Text";

                    string HtmlCode = Current.ToString();
                    if (!string.IsNullOrEmpty(HtmlCode))
                        Builder.Append(HtmlCode);

                } while (Enumerator.MoveNext());

                if (NewLine)
                    Builder.AppendLine();

                Builder.Append($"</{Name}>");
                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        public static bool TryParse(TextReader Reader, out HtmlNode Node)
        {
            const int BufferSize = 1024;
            char[] Buffer = ArrayPool<char>.Shared.Rent(BufferSize);
            StringBuilder Builder = new StringBuilder();

            try
            {
                int Index = 0;
                return TryParse(Reader, ref Buffer, ref Index, BufferSize, ref Builder, out Node);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(Buffer);
                Builder.Clear();
            }
        }
        internal static bool TryParse(TextReader Reader, ref char[] Buffer, ref int Index, int BufferSize, ref StringBuilder Builder, out HtmlNode Node)
        {
            while (Reader.MoveTo(ref Buffer, ref Index, BufferSize, true, '<'))
            {
                string NodeName = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, ' ', '/', '>');
                if (string.IsNullOrEmpty(NodeName) && Buffer[Index] == '/')
                    continue;

                if (Parse(NodeName.ToLower(), Reader, ref Buffer, ref Index, BufferSize, ref Builder) is not HtmlNode Html)
                    break;

                Node = Html;
                return true;
            }

            Node = null;
            return false;
        }
        internal static HtmlNode Parse(string NodeName, TextReader Reader, ref char[] Buffer, ref int Index, int BufferSize, ref StringBuilder Builder)
        {
            // Attribute
            Dictionary<string, object> Attributes = new Dictionary<string, object>();
            char c = Buffer[Index];
            while (c != '/' && c != '>')
            {
                if (!Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, c => !char.IsWhiteSpace(c)))
                    break;

                string Name = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '=', ' ', '/', '>');
                if (string.IsNullOrEmpty(Name))
                    break;

                Name = Name.ToLower();
                if (Buffer[Index] != '=')
                {
                    Attributes.Add(Name, true);
                    continue;
                }

                Reader.MoveTo(ref Buffer, ref Index, BufferSize, true, '"');

                string Value = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '"');
                if (Buffer[Index] != '"')
                {
                    StringBuilder ValueBuilder = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(Value))
                        ValueBuilder.Append(Value);

                    do
                    {
                        Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, c => !char.IsWhiteSpace(c));
                        Value = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '"');
                        ValueBuilder.Append(Value);

                    } while (Buffer[Index] != '"');

                    Value = ValueBuilder.ToString();
                    ValueBuilder.Clear();
                }

                Attributes.Add(Name, Value);

                Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, ' ', '/', '>');
                c = Buffer[Index];
            }

            Reader.MoveTo(ref Buffer, ref Index, BufferSize, true, '>');

            if (c == '/')
                return new HtmlNode(NodeName, Attributes);

            switch (NodeName)
            {
                case "area":
                case "base":
                case "br":
                case "col":
                case "embed":
                case "hr":
                case "img":
                case "input":
                case "link":
                case "meta":
                case "param":
                case "source":
                case "track":
                case "wbr":
                    return new HtmlNode(NodeName, Attributes);
                default:
                    {
                        // Children
                        List<HtmlNode> Children = new List<HtmlNode>();

                        do
                        {
                            do
                            {
                                Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, c => !char.IsWhiteSpace(c));

                                string Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                                if (!string.IsNullOrWhiteSpace(Content))
                                    Children.Add(new HtmlNode("Text", Content));

                            } while (Buffer[Index] != '<');

                            // Skip '<'
                            Index++;

                            string ChildNodeName = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '!', ' ', '/', '>');
                            if (string.IsNullOrEmpty(ChildNodeName))
                            {
                                // Skip '!', ' ', '/', '>'
                                c = Buffer[Index++];

                                // Comments Node
                                if (c == '!')
                                {
                                    string Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');

                                    // Simple Comments
                                    if (Content[Content.Length - 1] == '-')
                                    {
                                        // Skip '>'
                                        Index++;
                                        Children.Add(new HtmlNode("Comment", Content));
                                        continue;
                                    }

                                    // Conditional Comments
                                    StringBuilder CommentBuilder = new StringBuilder();

                                    // Content
                                    do
                                    {
                                        CommentBuilder.Append(Content);

                                        Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, true, '<');
                                        if (string.IsNullOrEmpty(Content))
                                            break;

                                        CommentBuilder.Append(Content);

                                        Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, 1, '!');

                                    } while (!string.IsNullOrEmpty(Content));

                                    // End Comments Node
                                    Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');
                                    CommentBuilder.Append(Content);
                                    Content = CommentBuilder.ToString();
                                    CommentBuilder.Clear();

                                    // Skip '>'
                                    Index++;
                                    Children.Add(new HtmlNode("Comment", Content));
                                    continue;
                                }

                                // End Node
                                if (c == '/' && Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>').ToLower() == NodeName)
                                {
                                    // Skip '>'
                                    Index++;
                                    return new HtmlNode(NodeName, Attributes, Children);
                                }

                                Debug.WriteLine($"Unknown HtmlNode at {NodeName}.");
                                return null;
                            }

                            if (Parse(ChildNodeName.ToLower(), Reader, ref Buffer, ref Index, BufferSize, ref Builder) is not HtmlNode Node)
                                return null;

                            Children.Add(Node);

                        } while (true);
                    }
            }
        }

    }
}