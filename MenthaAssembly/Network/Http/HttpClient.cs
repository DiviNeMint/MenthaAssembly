using MenthaAssembly.IO;
using MenthaAssembly.Network.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class HttpClient : TcpBase
    {
        private readonly List<string> Cookies = new();

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous
        /// </summary>
        public int ConnectTimeout { get; set; } = 100;

        public HttpClient() : base(null)
        {

        }

        public async Task<HttpMessage> GetAsync(string URL, HttpRequestHeader Header, int TimeoutMileseconds)
            => await GetAsync(new Uri(URL), Header, TimeoutMileseconds);
        public async Task<HttpMessage> GetAsync(Uri URL, HttpRequestHeader Header, int TimeoutMileseconds)
            => await GetAsync(URL.Host, URL.Port, URL.Scheme.Equals(Uri.UriSchemeHttps), Header, TimeoutMileseconds);
        public async Task<HttpMessage> GetAsync(string Host, int Port, bool SchemeHttps, HttpRequestHeader Header, int TimeoutMileseconds)
        {
            CancellationTokenSource CancelToken = new(TimeoutMileseconds);

            try
            {
                // Connects
                if (Connect(Host, Port, CancelToken.Token) is not Session Session)
                    return HttpMessage.NotFound;

                if (string.IsNullOrEmpty(Header.Host))
                    Header.Host = Host;

                // Checks HTTPS
                HttpMessage Request = new(Header);
                Stream Stream = Session.Socket.GetStream();  //= Session.SslStream is null ? Session.Stream : Session.SslStream;
                if (SchemeHttps)
                {
                    SslStream ssl = new(Stream, false, OnValidateServerCertificate, null);
#pragma warning disable CS0618 // 類型或成員已經過時
                    ssl.AuthenticateAsClient(Host, null, SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls, false);
#pragma warning restore CS0618 // 類型或成員已經過時

                    Stream = ssl;
                }

                // Header
                byte[] HeaderDatas = Request.Header.GetBytes(Cookies);

                Debug.WriteLine($"[Info][{GetType().Name}]Request to [{Session.Address}].");
                await Stream.WriteAsync(HeaderDatas, 0, HeaderDatas.Length, CancelToken.Token);
                await Stream.FlushAsync(CancelToken.Token);

                HttpResponseHeader ResponseHeader = HttpResponseHeader.Parse(Stream);
                Cookies.AddRange(ResponseHeader.Where(i => i.Key.Equals("Set-Cookie")).Select(i => i.Value));

                int ContentLength = ResponseHeader.ContentLength;
                if (ContentLength > 0)
                    return new HttpMessage(ResponseHeader, new SegmentStream(Stream, ContentLength));

                else if (ContentLength == 0 &&
                    ResponseHeader.TransferEncoding == "chunked")
                    return new HttpMessage(ResponseHeader, new HttpChunkStream(Stream));

                Debug.WriteLine($"[Warn][{GetType().Name}]Received unknown response.");
                return HttpMessage.BadRequest;
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                // Timeout
                return HttpMessage.RequestTimeout;
            }
        }

        protected Session Connect(string Host, int Port, CancellationToken CancelToken)
        {
            // Get host related information.
            IPHostEntry HostEntry = Dns.GetHostEntry(Host);

            // Loop through the AddressList to obtain the supported AddressFamily.
            // This is to avoid an exception that occurs when the host IP Address is not compatible with the address family (typical in the IPv6 case).
            foreach (IPAddress Address in HostEntry.AddressList)
            {
                CancelToken.ThrowIfCancellationRequested();

                IPEndPoint ipe = new(Address, Port);
                IOCPSocket e = new(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (e.TryConnect(ipe, ConnectTimeout))
                {
                    // Session
                    Session Session = new(e);

                    //// Init
                    //e.Received += (s, e) => OnReceived(Session, e);
                    //e.Disconnect += (s, e) =>
                    //{
                    //    OnDisconnected(Session.Address);
                    //};
                    //e.Receive();

                    return Session;
                }

                e.Dispose();
            }

            return null;
        }
        protected virtual bool OnValidateServerCertificate(object sender, X509Certificate Certificate, X509Chain Chain, SslPolicyErrors sslPolicyErrors)
            => sslPolicyErrors == SslPolicyErrors.None;

        public override void Dispose()
        {

        }

    }
}