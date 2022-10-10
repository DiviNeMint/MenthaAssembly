using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MenthaAssembly
{
    public sealed class HtmlDocument : IHtmlNode
    {
        private readonly IHtmlNode Html;

        string IHtmlNode.Name
            => Html.Name;

        public IHtmlNode Head { get; }

        public IHtmlNode Body { get; }

        public IReadOnlyDictionary<string, object> Attributes
            => Html.Attributes;

        public IReadOnlyList<IHtmlNode> Children
            => Html.Children;

        public object this[string Attribute]
            => Html[Attribute];

        public HtmlDocument(IHtmlNode Html)
        {
            this.Html = Html;

            int HeadIndex = Html.Children.IndexOf(c => c.Name == "head");
            if (HeadIndex != -1)
                Head = Html.Children[HeadIndex];

            int BodyIndex = Html.Children.IndexOf(c => c.Name == "body");
            Body = BodyIndex != -1 ? Html.Children[BodyIndex] :
                                     new HtmlNode("body", new Dictionary<string, object>(0), Html.Children.Skip(HeadIndex + 1).ToList());
        }

        public IEnumerator<IHtmlNode> GetEnumerator()
            => Html.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => Html.ToString();

        public static bool TryParse(TextReader Reader, out HtmlDocument Document)
        {
            const int BufferSize = 1024;
            char[] Buffer = ArrayPool<char>.Shared.Rent(BufferSize);
            int i = BufferSize;

            try
            {
                StringBuilder Builder = new StringBuilder();
                if (Reader.MoveTo(ref Buffer, ref i, BufferSize, true, '<') &&
                    Reader.ReadTo(ref Buffer, ref i, BufferSize, ref Builder, true, '>').ToLower().StartsWith("!doctype html"))
                {
                    while (HtmlNode.TryParse(Reader, ref Buffer, ref i, BufferSize, ref Builder, out HtmlNode Html))
                    {
                        if (Html.Name == "html")
                        {
                            Document = new HtmlDocument(Html);
                            return true;
                        }
                    }
                }

                Document = null;
                return false;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(Buffer);
            }
        }

    }
}