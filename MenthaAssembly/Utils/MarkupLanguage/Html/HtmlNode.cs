using System;
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

        public IReadOnlyList<IHtmlNode> Children { get; }

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
            this.Children = Children is List<HtmlNode> List ? List : Children.ToList();
        }
        public HtmlNode(string Name, Dictionary<string, object> Attributes, IEnumerable<IHtmlNode> Children)
        {
            this.Name = Name;
            this.Attributes = Attributes;
            this.Children = Children is List<IHtmlNode> List ? List : Children.ToList();
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

                if (Children.Count == 0)
                {
                    Builder.Append("/>");
                    return Builder.ToString();
                }

                Builder.Append(">");

                if (Children.Any(i => i.Name != "Text"))
                {
                    foreach (IHtmlNode Child in Children)
                    {
                        string HtmlCode = Child.ToString();
                        if (!string.IsNullOrEmpty(HtmlCode))
                        {
                            Builder.AppendLine();
                            Builder.Append(HtmlCode);
                        }
                    }

                    Builder.AppendLine();
                }
                else
                {
                    foreach (IHtmlNode Child in Children)
                    {
                        string HtmlCode = Child.ToString();
                        if (!string.IsNullOrEmpty(HtmlCode))
                            Builder.Append(HtmlCode);
                    }
                }

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

                return Parse(NodeName.ToLower(), Reader, ref Buffer, ref Index, BufferSize, ChildName => false, ref Builder, out Node, out _, out _);
            }

            Node = null;
            return false;
        }
        internal static bool Parse(string NodeName, TextReader Reader, ref char[] Buffer, ref int Index, int BufferSize, Predicate<string> ContainsNodeName, ref StringBuilder Builder, out HtmlNode Node, out string EndNodeName, out string UnknownContent)
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

                // Skip '='
                Index++;

                string StartChar = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, true, 1, '"'),
                       Value;
                if (StartChar == "\"")
                {
                    Value = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '"');
                    while (Buffer[Index] != '"')
                    {
                        Builder.Append(Value);
                        Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, c => !char.IsWhiteSpace(c));
                        Value = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '"');
                    }
                }
                else
                {
                    Builder.Append(StartChar);
                    Value = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', ' ', '/', '>');
                }

                Attributes.Add(Name, Value);

                Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, ' ', '/', '>');
                c = Buffer[Index];
            }

            Reader.MoveTo(ref Buffer, ref Index, BufferSize, true, '>');

            if (c == '/')
            {
                Node = new HtmlNode(NodeName, Attributes);
                EndNodeName = NodeName;
                UnknownContent = null;
                return true;
            }

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
                    {
                        Node = new HtmlNode(NodeName, Attributes);
                        EndNodeName = NodeName;
                        UnknownContent = null;
                        return true;
                    }
                case "script":
                    {
                        // Children
                        List<HtmlNode> Children = new List<HtmlNode>();

                        bool LoopValue = true;
                        string Content = null;
                        do
                        {
                            Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, c => !char.IsWhiteSpace(c));

                            Builder.Append(Content);
                            Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                            while (Buffer[Index] == '<')
                            {
                                // Skip '<';
                                Index++;

                                // Check Buffer
                                if (BufferSize == Index)
                                {
                                    if (Reader.Read(Buffer, 0, BufferSize) == 0)
                                        break;

                                    Index -= BufferSize;
                                }

                                if (Buffer[Index] != '/')
                                {
                                    Builder.Append(Content);
                                    Builder.Append('<');
                                    Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                                    continue;
                                }

                                // Skip '/';
                                Index++;

                                EndNodeName = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');
                                if (NodeName != StringHelper.Remove(EndNodeName, '\r', '\n', ' '))
                                {
                                    Builder.Append(Content);
                                    Builder.Append("</");
                                    Builder.Append(EndNodeName);
                                    Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                                    continue;
                                }

                                LoopValue = false;
                            }

                        } while (LoopValue);

                        if (!string.IsNullOrWhiteSpace(Content))
                            Children.Add(new HtmlNode("Text", Content));

                        // Skip '>'
                        Index++;

                        Node = new HtmlNode(NodeName, Attributes, Children);
                        EndNodeName = NodeName;
                        UnknownContent = null;
                        return true;
                    }
                case "head":
                    {
                        // Children
                        List<HtmlNode> Children = new List<HtmlNode>();

                        do
                        {
                            bool LoopValue = true;
                            do
                            {
                                Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, c => !char.IsWhiteSpace(c));

                                string Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                                while (Buffer[Index] == '<')
                                {
                                    Index++;

                                    // Check Buffer
                                    if (BufferSize == Index)
                                    {
                                        if (Reader.Read(Buffer, 0, BufferSize) == 0)
                                            break;

                                        Index -= BufferSize;
                                    }

                                    if (Buffer[Index] == ' ')
                                    {
                                        Builder.Append(Content);
                                        Builder.Append('<');
                                        Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                                        continue;
                                    }

                                    LoopValue = false;
                                }

                                if (!string.IsNullOrWhiteSpace(Content))
                                    Children.Add(new HtmlNode("Text", Content));

                            } while (LoopValue);

                            string ChildNodeName = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '!', ' ', '/', '>');
                            if (string.IsNullOrEmpty(ChildNodeName))
                            {
                                // Skip '!', ' ', '/', '>'
                                c = Buffer[Index++];

                                // Comments Node
                                if (c == '!')
                                {
                                    string Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');
                                    while (Content[Content.Length - 1] != '-')
                                    {
                                        // Skip '>';
                                        Index++;

                                        Builder.Append(Content);
                                        Builder.Append('>');
                                        Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');
                                    }

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

                                    Node = new HtmlNode(NodeName, Attributes, Children);
                                    EndNodeName = NodeName;
                                    UnknownContent = null;
                                    return true;
                                }

                                continue;
                            }

                            ChildNodeName = StringHelper.Remove(ChildNodeName, '\r', '\n', ' ');
                            switch (ChildNodeName)
                            {
                                case "title":
                                case "meta":
                                case "style":
                                case "link":
                                case "script":
                                case "noscript":
                                case "base":
                                    {
                                        Predicate<string> ParentContainsNodeName = ContainsNodeName != null ? ChildName => ChildName == ChildNodeName || ContainsNodeName(ChildName) :
                                                                                                              ChildName => ChildName == ChildNodeName;

                                        if (!Parse(ChildNodeName.ToLower(), Reader, ref Buffer, ref Index, BufferSize, ParentContainsNodeName, ref Builder, out HtmlNode ChildNode, out EndNodeName, out UnknownContent))
                                        {
                                            if (EndNodeName == NodeName)
                                            {
                                                Children.Add(new HtmlNode("Text", UnknownContent));
                                                Node = new HtmlNode(NodeName, Attributes, Children);
                                                EndNodeName = NodeName;
                                                UnknownContent = null;
                                                return true;
                                            }

                                            // Unknown Content
                                            try
                                            {
                                                // NodeName
                                                Builder.Append($"<head");

                                                // Attributes
                                                if (Attributes.Count > 0)
                                                    Builder.Append($" {string.Join(" ", Attributes.Where(i => i.Key != "Content").Select(i => $"{i.Key}=\"{i.Value}\""))}");

                                                Builder.Append(">");

                                                // Children
                                                foreach (HtmlNode Child in Children)
                                                    Builder.AppendLine(Child.ToString());

                                                Builder.AppendLine(UnknownContent);

                                                Node = null;
                                                return false;
                                            }
                                            finally
                                            {
                                                Builder.Clear();
                                            }
                                        }

                                        Children.Add(ChildNode);
                                        break;
                                    }
                                default:
                                    {
                                        Node = new HtmlNode("head", Attributes, Children);
                                        EndNodeName = ChildNodeName;
                                        UnknownContent = null;
                                        return true;
                                    }
                            }

                        } while (true);
                    }
                default:
                    {
                        // Children
                        List<HtmlNode> Children = new List<HtmlNode>();

                        do
                        {
                            bool LoopValue = true;
                            do
                            {
                                Reader.MoveTo(ref Buffer, ref Index, BufferSize, false, c => !char.IsWhiteSpace(c));

                                string Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                                while (Buffer[Index] == '<')
                                {
                                    Index++;

                                    // Check Buffer
                                    if (BufferSize == Index)
                                    {
                                        if (Reader.Read(Buffer, 0, BufferSize) == 0)
                                            break;

                                        Index -= BufferSize;
                                    }

                                    if (Buffer[Index] == ' ')
                                    {
                                        Builder.Append(Content);
                                        Builder.Append('<');
                                        Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '\r', '\n', '<');
                                        continue;
                                    }

                                    LoopValue = false;
                                }

                                if (!string.IsNullOrWhiteSpace(Content))
                                    Children.Add(new HtmlNode("Text", Content));

                            } while (LoopValue);

                            string ChildNodeName = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '!', ' ', '/', '>');
                            if (string.IsNullOrEmpty(ChildNodeName))
                            {
                                // Skip '!', ' ', '/', '>'
                                c = Buffer[Index++];

                                // Comments Node
                                if (c == '!')
                                {
                                    string Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');
                                    while (Content[Content.Length - 1] != '-')
                                    {
                                        // Skip '>';
                                        Index++;

                                        Builder.Append(Content);
                                        Builder.Append('>');
                                        Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');
                                    }

                                    // Skip '>'
                                    Index++;

                                    #region Complex
                                    //string Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');

                                    //// Simple Comments
                                    //if (Content[Content.Length - 1] == '-')
                                    //{
                                    //    // Skip '>'
                                    //    Index++;
                                    //    Children.Add(new HtmlNode("Comment", Content));
                                    //    continue;
                                    //}

                                    //// Conditional Comments
                                    //StringBuilder CommentBuilder = new StringBuilder();

                                    //// Content
                                    //do
                                    //{
                                    //    CommentBuilder.Append(Content);

                                    //    Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, true, '<');
                                    //    if (string.IsNullOrEmpty(Content))
                                    //        break;

                                    //    CommentBuilder.Append(Content);

                                    //    Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, 1, '!');

                                    //} while (!string.IsNullOrEmpty(Content));

                                    //// End Comments Node
                                    //Content = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>');
                                    //CommentBuilder.Append(Content);
                                    //Content = CommentBuilder.ToString();
                                    //CommentBuilder.Clear();

                                    //// Skip '>'
                                    //Index++;
                                    #endregion

                                    Children.Add(new HtmlNode("Comment", Content));
                                    continue;
                                }

                                // End Node
                                EndNodeName = string.Empty;
                                if (c == '/')
                                {
                                    EndNodeName = Reader.ReadTo(ref Buffer, ref Index, BufferSize, ref Builder, false, '>').ToLower();
                                    EndNodeName = StringHelper.Remove(EndNodeName, '\r', '\n', ' ');
                                    if (EndNodeName == NodeName)
                                    {
                                        // Skip '>'
                                        Index++;

                                        Node = new HtmlNode(NodeName, Attributes, Children);
                                        UnknownContent = null;
                                        return true;
                                    }

                                    if (!ContainsNodeName(EndNodeName))
                                    {
                                        // Skip '>'
                                        Index++;

                                        Debug.WriteLine($"Invalid EndHtmlNode {EndNodeName}.");
                                        continue;
                                    }
                                }

                                Debug.WriteLine($"Unknown HtmlNode Start : {NodeName} and End : {EndNodeName}.");

                                // Unknown Content
                                try
                                {
                                    // NodeName
                                    Builder.Append('<');
                                    Builder.Append(NodeName);

                                    // Attributes
                                    if (Attributes.Count > 0)
                                        Builder.Append($" {string.Join(" ", Attributes.Where(i => i.Key != "Content").Select(i => $"{i.Key}=\"{i.Value}\""))}");

                                    Builder.Append(">");

                                    // Children
                                    foreach (HtmlNode Child in Children)
                                        Builder.AppendLine(Child.ToString());

                                    Node = null;
                                    UnknownContent = Builder.ToString();
                                    return false;
                                }
                                finally
                                {
                                    Builder.Clear();
                                }
                            }

                            ChildNodeName = StringHelper.Remove(ChildNodeName, '\r', '\n', ' ').ToLower();

                            Predicate<string> ParentContainsNodeName = ContainsNodeName != null ? ChildName => ChildName == ChildNodeName || ContainsNodeName(ChildName) :
                                                                                                  ChildName => ChildName == ChildNodeName;

                            bool Success = Parse(ChildNodeName, Reader, ref Buffer, ref Index, BufferSize, ParentContainsNodeName, ref Builder, out HtmlNode ChildNode, out EndNodeName, out UnknownContent);
                            while (Success && ChildNodeName != EndNodeName)
                            {
                                Children.Add(ChildNode);
                                ChildNodeName = EndNodeName;
                                Success = Parse(ChildNodeName, Reader, ref Buffer, ref Index, BufferSize, ParentContainsNodeName, ref Builder, out ChildNode, out EndNodeName, out UnknownContent);
                            }

                            if (!Success)
                            {
                                if (EndNodeName == NodeName)
                                {
                                    Children.Add(new HtmlNode("Text", UnknownContent));
                                    Node = new HtmlNode(NodeName, Attributes, Children);
                                    UnknownContent = null;
                                    return true;
                                }

                                // Unknown Content
                                try
                                {
                                    // NodeName
                                    Builder.Append($"<{NodeName}");

                                    // Attributes
                                    if (Attributes.Count > 0)
                                        Builder.Append($" {string.Join(" ", Attributes.Where(i => i.Key != "Content").Select(i => $"{i.Key}=\"{i.Value}\""))}");

                                    Builder.Append(">");

                                    // Children
                                    foreach (HtmlNode Child in Children)
                                        Builder.AppendLine(Child.ToString());

                                    Builder.AppendLine(UnknownContent);

                                    Node = null;
                                    UnknownContent = Builder.ToString();
                                    return false;
                                }
                                finally
                                {
                                    Builder.Clear();
                                }
                            }

                            Children.Add(ChildNode);

                        } while (true);
                    }
            }
        }

    }
}