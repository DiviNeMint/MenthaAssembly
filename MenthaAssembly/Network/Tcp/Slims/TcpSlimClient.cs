using MenthaAssembly.Network.Primitives;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class TcpSlimClient : TcpSlimBase
    {
        protected TcpSlimToken ServerToken;

        public IPEndPoint Server
            => _IsDisposed ? throw new ObjectDisposedException(GetType().Name) :
                             ServerToken?.Address;

        public int ConnectTimeout { get; set; } = 3000;

        public TcpSlimClient() : base(CommonProtocolCoder.Instance) { }
        public TcpSlimClient(IProtocolCoder Protocol) : base(Protocol) { }

        public void Connect(string Address, int Port)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{GetType().Name} Connect Error\nIPAddress may not be correct format.");

            Connect(new IPEndPoint(TempIP, Port));
        }
        public void Connect(IPAddress IPAddress, int Port)
            => Connect(new IPEndPoint(IPAddress, Port));
        public virtual void Connect(IPEndPoint IPEndPoint)
        {
            // Reset Last Connecting
            if (ServerToken != null)
            {
                EnqueueToken(ServerToken);
                ServerToken = default;
            }

            // Connect Server
            Socket Server = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IAsyncResult Result = Server.BeginConnect(IPEndPoint, null, null);
            if (!Result.AsyncWaitHandle.WaitOne(ConnectTimeout, true))
            {
                Server.Dispose();
                throw new TimeoutException();
            }

            Server.EndConnect(Result);

            if (!Server.Connected)
                throw new ConnectingFailedException();

            Debug.WriteLine($"[Info][{GetType().Name}]Connect to [{IPEndPoint.Address}:{IPEndPoint.Port}].");

            // Start Receive Server's Message
            TcpStream Stream = new TcpStream(Server, Pool);

            // Build Token
            TcpSlimToken Token = DequeueToken();
            PrepareToken(Token, Stream);
            ServerToken = Token;

            // Events
            void OnStreamDisconnected(IPEndPoint Address, Stream e)
            {
                OnDisconnected(Address);

                Stream.ReceiveCompleted -= OnStreamReceiveCompleted;
                Stream.Disconnected -= OnStreamDisconnected;
            }

            void OnStreamReceiveCompleted(IPEndPoint sender, Stream e)
                => OnReceived(Token, e);

            Stream.ReceiveCompleted += OnStreamReceiveCompleted;
            Stream.Disconnected += OnStreamDisconnected;

            // Start Receive
            Stream.Receive();
        }

        public async Task<IMessage> SendAsync(IMessage Request)
           => await SendAsync(Request, 3000);
        public async Task<IMessage> SendAsync(IMessage Request, int TimeoutMileseconds)
        {
            if (ServerToken is null)
                return ErrorMessage.NotConnected;

            return await base.SendAsync(ServerToken, Request, TimeoutMileseconds);
        }

        public async Task<T> SendAsync<T>(IMessage Request)
            where T : IMessage
           => await SendAsync<T>(Request, 3000);
        public async Task<T> SendAsync<T>(IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (ServerToken is null)
                throw new NotConnectedException();

            IMessage Response = await base.SendAsync(ServerToken, Request, TimeoutMileseconds);
            if (ErrorMessage.Timeout.Equals(Response))
                throw new TimeoutException();

            if (ErrorMessage.NotSupport.Equals(Response))
                throw new NotSupportedException();

            return (T)Response;
        }

        public IMessage Send(IMessage Request)
            => Send(Request, 3000);
        public IMessage Send(IMessage Request, int TimeoutMileseconds)
        {
            if (ServerToken is null)
                return ErrorMessage.NotConnected;

            return base.Send(ServerToken, Request, TimeoutMileseconds);
        }

        public T Send<T>(IMessage Request)
            where T : IMessage
           => Send<T>(Request, 3000);
        public T Send<T>(IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (ServerToken is null)
                throw new NotConnectedException();

            IMessage Response = base.Send(ServerToken, Request, TimeoutMileseconds);
            if (ErrorMessage.Timeout.Equals(Response))
                throw new TimeoutException();

            if (ErrorMessage.NotSupport.Equals(Response))
                throw new NotSupportedException();

            return (T)Response;
        }

        protected override TcpSlimToken CreateToken()
            => new TcpSlimToken(true);
        protected override void PrepareToken(TcpSlimToken Token, TcpStream Stream)
            => Token.Prepare(Stream);
        protected override void ResetToken(TcpSlimToken Token)
            => Token.Clear();

        protected override void OnDisconnected(IPEndPoint Address)
        {
            EnqueueToken(ServerToken);
            ServerToken = null;

            // Trigger Disconnected Event.
            base.OnDisconnected(Address);
            Debug.WriteLine($"[Info][{GetType().Name}]Server[{Address}] is disconnected.");
        }

        private bool _IsDisposed = false;
        public override void Dispose()
        {
            if (_IsDisposed)
                return;

            try
            {
                if (ServerToken != null)
                {
                    ResetToken(ServerToken);
                    ServerToken = null;
                }

                base.Dispose();
            }
            finally
            {
                _IsDisposed = true;
            }
        }

    }
}
