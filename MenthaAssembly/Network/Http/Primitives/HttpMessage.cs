using System.IO;
using System.Net;

namespace MenthaAssembly.Network
{
    public class HttpMessage
    {
        public static HttpMessage NotFound
            => new HttpMessage(new HttpResponseHeader { StatusCode = HttpStatusCode.NotFound });

        public static HttpMessage BadRequest
            => new HttpMessage(new HttpResponseHeader { StatusCode = HttpStatusCode.BadRequest });

        public static HttpMessage RequestTimeout
            => new HttpMessage(new HttpResponseHeader { StatusCode = HttpStatusCode.RequestTimeout });

        public HttpHeader Header { get; }

        protected Stream _Content;
        public Stream Content => _Content;

        public HttpMessage(HttpHeader Header)
        {
            this.Header = Header;
        }
        public HttpMessage(HttpHeader Header, Stream Content)
        {
            this.Header = Header;
            _Content = Content;
        }

    }
}
