using MenthaAssembly.Network.Primitives;
using MenthaAssembly.Utils;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class HttpClient : TcpBase<HttpToken>
    {
        private List<string> Cookies = new List<string>();

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous
        /// </summary>
        public int ConnectTimeout { get; set; } = 100;

        public async Task<HttpMessage> GetAsync(string URL, HttpRequestHeader Header, int TimeoutMileseconds)
            => await GetAsync(new Uri(URL), Header, TimeoutMileseconds);
        public async Task<HttpMessage> GetAsync(Uri URL, HttpRequestHeader Header, int TimeoutMileseconds)
            => await GetAsync(URL.Host, URL.Port, URL.Scheme.Equals(Uri.UriSchemeHttps), Header, TimeoutMileseconds);
        public async Task<HttpMessage> GetAsync(string Host, int Port, bool SchemeHttps, HttpRequestHeader Header, int TimeoutMileseconds)
        {
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            try
            {
                if (Connect(Host, Port, SchemeHttps, CancelToken.Token) is not HttpToken Token)
                    return HttpMessage.NotFound;

                if (string.IsNullOrEmpty(Header.Host))
                    Header.Host = Host;

                HttpMessage Request = new HttpMessage(Header);
                Stream Stream = Token.SslStream is null ? Token.Stream : Token.SslStream;

                // Header
                byte[] HeaderDatas = Request.Header.GetBytes(Cookies);

                Debug.WriteLine($"[Info][{GetType().Name}]Request to [{Token.Address}].");
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

        protected HttpToken Connect(string Host, int Port, bool SchemeHttps, CancellationToken CancelToken)
        {
            // Get host related information.
            IPHostEntry HostEntry = Dns.GetHostEntry(Host);

            // Loop through the AddressList to obtain the supported AddressFamily.
            // This is to avoid an exception that occurs when the host IP Address is not compatible with the address family (typical in the IPv6 case).
            foreach (IPAddress Address in HostEntry.AddressList)
            {
                CancelToken.ThrowIfCancellationRequested();

                IPEndPoint ipe = new IPEndPoint(Address, Port);
                Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult Result = s.BeginConnect(ipe, null, null);
                if (Result.AsyncWaitHandle.WaitOne(ConnectTimeout, true))
                {
                    s.EndConnect(Result);
                    if (s.Connected)
                    {
                        TcpStream Stream = new TcpStream(s, Pool);

                        HttpToken Token = DequeueToken();
                        PrepareToken(Token, Stream);

                        if (SchemeHttps)
                        {
                            SslStream ssl = new SslStream(Stream, false, OnValidateServerCertificate, null);
                            ssl.AuthenticateAsClient(Host, null, SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls, false);

                            Token.SslStream = ssl;
                        }

                        Debug.WriteLine($"[Info][{GetType().Name}]Connect to [{Address}:{Port}].");
                        return Token;
                    }
                }
                else
                {
                    s.Dispose();
                }
            }

            return null;
        }
        protected virtual bool OnValidateServerCertificate(object sender, X509Certificate Certificate, X509Chain Chain, SslPolicyErrors sslPolicyErrors)
            => sslPolicyErrors == SslPolicyErrors.None;

        protected override HttpToken CreateToken()
            => new HttpToken();
        protected override void PrepareToken(HttpToken Token, TcpStream Stream)
        {
            Token.Stream = Stream;
        }
        protected override void ResetToken(HttpToken Token)
        {
            Token.Stream.Dispose();
            Token.Stream = null;
        }

        protected override void OnReceived(HttpToken Token, Stream Stream) { }

        //public async Task<HttpMessage> GetAsync(string URL, HttpRequestHeader Header, int TimeoutMileseconds)
        //    => await GetAsync(new Uri(URL), Header, TimeoutMileseconds);
        //public async Task<HttpMessage> GetAsync(Uri URL, HttpRequestHeader Header, int TimeoutMileseconds)
        //    => await GetAsync(URL.Host, URL.Port, Header, TimeoutMileseconds);
        //public async Task<HttpMessage> GetAsync(string HostName, int Port, HttpRequestHeader Header, int TimeoutMileseconds)
        //{
        //    if (Connect(HostName, Port) is not HttpToken Token)
        //        return HttpMessage.NotFound;

        //    if (string.IsNullOrEmpty(Header.Host))
        //        Header.Host = HostName;

        //    //Debug.WriteLine($"------------------------");
        //    //Debug.WriteLine($"Request Header");
        //    //Debug.WriteLine($"------------------------");
        //    //Debug.WriteLine($"{Header}");
        //    //Debug.WriteLine($"------------------------");

        //    HttpMessage Request = new HttpMessage(Header);


        //    Task<HttpMessage> Task = SendAsync(Token, Request, TimeoutMileseconds);

        //    // Start Receive Server's Message
        //    SocketAsyncEventArgs e = Dequeue(true);
        //    e.UserToken = Token;

        //    if (!Token.ReceiveAsync(e))
        //        OnReceiveProcess(e);

        //    return await Task;
        //}

        //public async Task GetAsync2(string URL, HttpRequestHeader Header, int TimeoutMileseconds)
        //{
        //    Uri HostData = new Uri(URL);
        //    if (Connect2(HostData.Host, HostData.Port) is not Socket Socket)
        //        return;

        //    if (string.IsNullOrEmpty(Header.Host))
        //        Header.Host = HostData.Host;

        //    Debug.WriteLine($"------------------------");
        //    Debug.WriteLine($"Request Header");
        //    Debug.WriteLine($"------------------------");
        //    Debug.WriteLine($"{Header}");
        //    Debug.WriteLine($"------------------------");

        //    HttpMessage Request = new HttpMessage(Header);

        //    IOCPStream Stream = new TcpStream(Socket, null);

        //    // Header
        //    {
        //        string Host = Request.Header["Host"];

        //        byte[] HeaderDatas = Request.Header.GetBytes();

        //        await Stream.WriteAsync(HeaderDatas, 0, HeaderDatas.Length);
        //        await Stream.FlushAsync();

        //        var a = ParseHttpResponseHeader(Stream, true);


        //        GZipStream GZIP = new GZipStream(Stream, CompressionMode.Decompress);

        //        StreamReader Reader = new StreamReader(Stream, Encoding.ASCII);
        //        string Line = Reader.ReadLine();


        //        //SslStream ssl = new SslStream(Stream, false, OnValidateServerCertificate, null);
        //        //ssl.AuthenticateAsClient(Host, null, SslProtocols.Default, false);
        //        //if (ssl.IsAuthenticated)
        //        //{
        //        //    byte[] HeaderDatas = Request.Header.GetBytes();

        //        //    await ssl.WriteAsync(HeaderDatas, 0, HeaderDatas.Length);
        //        //    await ssl.FlushAsync();

        //        //    var a = ParseHttpResponseHeader(ssl);

        //        //}
        //    }
        //}

        //protected HttpToken Connect(string HostName, int Port)
        //{
        //    // Get host related information.
        //    IPHostEntry HostEntry = Dns.GetHostEntry(HostName);

        //    // Loop through the AddressList to obtain the supported AddressFamily.
        //    // This is to avoid an exception that occurs when the host IP Address is not compatible with the address family (typical in the IPv6 case).
        //    foreach (IPAddress address in HostEntry.AddressList)
        //    {
        //        IPEndPoint ipe = new IPEndPoint(address, Port);
        //        Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //        s.Connect(ipe);

        //        if (s.Connected)
        //        {
        //            Debug.WriteLine($"[Info][{GetType().Name}]Connect to [{address}:{Port}].");
        //            return new HttpToken(s);
        //        }
        //    }

        //    return null;
        //}
        //protected Socket Connect2(string HostName, int Port)
        //{
        //    // Get host related information.
        //    IPHostEntry HostEntry = Dns.GetHostEntry(HostName);

        //    // Loop through the AddressList to obtain the supported AddressFamily.
        //    // This is to avoid an exception that occurs when the host IP Address is not compatible with the address family (typical in the IPv6 case).
        //    foreach (IPAddress address in HostEntry.AddressList)
        //    {
        //        IPEndPoint ipe = new IPEndPoint(address, Port);
        //        Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //        s.Connect(ipe);

        //        if (s.Connected)
        //        {
        //            Debug.WriteLine($"[Info][{GetType().Name}]Connect to [{address}:{Port}].");
        //            return s;
        //        }
        //    }

        //    return null;
        //}

        ////public async Task<IMessage> SendAsync(IMessage Request)
        ////   => await SendAsync(Request, 5000);
        ////public async Task<IMessage> SendAsync(IMessage Request, int TimeoutMileseconds)
        ////{
        ////    if (ServerToken is null)
        ////        return ErrorMessage.NotConnected;

        ////    return await base.SendAsync(ServerToken, Request, TimeoutMileseconds);
        ////}

        ////public async Task<T> SendAsync<T>(IMessage Request)
        ////    where T : IMessage
        ////   => await SendAsync<T>(Request, 3000);
        ////public async Task<T> SendAsync<T>(IMessage Request, int TimeoutMileseconds)
        ////    where T : IMessage
        ////{
        ////    if (ServerToken is null)
        ////        throw new NotConnectedException();

        ////    IMessage Response = await base.SendAsync(ServerToken, Request, TimeoutMileseconds);
        ////    if (ErrorMessage.Timeout.Equals(Response))
        ////        throw new TimeoutException();

        ////    if (ErrorMessage.NotSupport.Equals(Response))
        ////        throw new NotSupportedException();

        ////    return (T)Response;
        ////}

        ////public IMessage Send(IMessage Request)
        ////    => Send(Request, 3000);
        ////public IMessage Send(IMessage Request, int TimeoutMileseconds)
        ////{
        ////    if (ServerToken is null)
        ////        return ErrorMessage.NotConnected;

        ////    return base.Send(ServerToken, Request, TimeoutMileseconds);
        ////}

        ////public T Send<T>(IMessage Request)
        ////    where T : IMessage
        ////   => Send<T>(Request, 3000);
        ////public T Send<T>(IMessage Request, int TimeoutMileseconds)
        ////    where T : IMessage
        ////{
        ////    if (ServerToken is null)
        ////        throw new NotConnectedException();

        ////    IMessage Response = base.Send(ServerToken, Request, TimeoutMileseconds);
        ////    if (ErrorMessage.Timeout.Equals(Response))
        ////        throw new TimeoutException();

        ////    if (ErrorMessage.NotSupport.Equals(Response))
        ////        throw new NotSupportedException();

        ////    return (T)Response;
        ////}

        //protected override void OnReceiveProcess(SocketAsyncEventArgs e)
        //{
        //    if (e.UserToken is HttpToken Token)
        //    {
        //        // Check Client's Connection Status
        //        if (e.SocketError == SocketError.Success &&
        //            e.BytesTransferred > 0)
        //        {
        //            try
        //            {
        //                // Decode Message
        //                Stream NetStream = Token.GetStream();
        //                ConcatStream s = new ConcatStream(e.Buffer, 0, e.BytesTransferred, NetStream, true);

        //                SslStream ssl = new SslStream(s, true, OnValidateServerCertificate, null);
        //                ssl.AuthenticateAsClient(Token.HostName, Token.X509Certificates, SslProtocols.Default, false);


        //                HttpResponseHeader Header = ParseHttpResponseHeader(s);
        //                s.Dispose();

        //                Debug.WriteLine($"[Info][{GetType().Name}]Receive Response.");

        //                HttpMessage Response = new HttpMessage(Header);

        //                try
        //                {
        //                    if (Header.ContentLength > 0)
        //                    {
        //                        SegmentStream Content = new SegmentStream(NetStream, Header.ContentLength);
        //                        Content.Ended += OnContentEnded;

        //                        void OnContentEnded(object sender, EventArgs Arg)
        //                        {
        //                            Content.Ended -= OnContentEnded;
        //                            OnDisconnected(Token);

        //                            // Push Resource to pool.
        //                            Enqueue(ref e);
        //                        }

        //                        Response.Content = Content;
        //                        return;
        //                    }
        //                }
        //                finally
        //                {
        //                    Token.ResponseTaskSource.TrySetResult(Response);
        //                    Token.ResponseTaskSource = null;

        //                    Token.ResponseCancelToken.Dispose();
        //                    Token.ResponseCancelToken = null;
        //                }
        //            }
        //            catch (Exception Ex)
        //            {
        //                if (!(Ex is IOException IOEx &&
        //                     IOEx.InnerException is ObjectDisposedException ODEx &&
        //                     ODEx.ObjectName == typeof(Socket).FullName) &&
        //                     !(Ex is SocketException))
        //                    Debug.WriteLine($"[Error][{GetType().Name}]Decode exception.");
        //            }
        //        }

        //        OnDisconnected(Token);
        //    }

        //    // Push Resource to pool.
        //    Enqueue(ref e);
        //}

        //protected override void OnDisconnected(HttpToken Token)
        //{
        //    //// Clear ServerToken
        //    //ServerToken = null;

        //    // Trigger Disconnected Event.
        //    base.OnDisconnected(Token);
        //}

        //private bool _IsDisposed = false;
        //public override void Dispose()
        //{
        //    if (_IsDisposed)
        //        return;

        //    try
        //    {
        //        //ServerToken?.Dispose();
        //        //ServerToken = null;
        //    }
        //    finally
        //    {
        //        _IsDisposed = true;
        //    }
        //}

        //~HttpClient()
        //{
        //    Dispose();
        //}

    }
}
