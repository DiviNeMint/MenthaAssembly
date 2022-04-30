using MenthaAssembly.Network.Primitives;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class TcpClient : TcpSocket
    {
        protected TcpToken ServerToken;

        public IPEndPoint Server
            => ServerToken?.Address;

        public override bool IsDisposed => _IsDisposed;

        public TcpClient() : base(CommonProtocolCoder.Instance) { }
        public TcpClient(IProtocolCoder Protocol) : base(Protocol) { }

        public void Connect(string Address, int Port)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{GetType().Name} Connect Error\nIPAddress may not be correct format.");

            Connect(new IPEndPoint(TempIP, Port));
        }
        public void Connect(IPAddress IPAddress, int Port)
            => Connect(new IPEndPoint(IPAddress, Port));
        public void Connect(IPEndPoint IPEndPoint)
        {
            //Check if it had Connect server.
            ServerToken?.Dispose();

            // Connect Server
            Socket Server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Server.Connect(IPEndPoint);

            Debug.WriteLine($"[Info][{GetType().Name}]Connect to [{IPEndPoint.Address}:{IPEndPoint.Port}].");

            // Start Receive Server's Message
            SocketAsyncEventArgs e = Dequeue(true);
            ServerToken = new TcpToken(Server, true);
            e.UserToken = ServerToken;

            if (!Server.ReceiveAsync(e))
                OnReceiveProcess(e);
        }

        public async Task<IMessage> SendAsync(IMessage Request)
           => await SendAsync(Request, 5000);
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

        protected override void OnDisconnected(TcpToken Token)
        {
            // Clear ServerToken
            ServerToken = null;

            // Trigger Disconnected Event.
            base.OnDisconnected(Token);
        }

        private bool _IsDisposed = false;
        public override void Dispose()
        {
            if (_IsDisposed)
                return;

            try
            {
                ServerToken?.Dispose();
                ServerToken = null;
            }
            finally
            {
                _IsDisposed = true;
            }
        }

        ~TcpClient()
        {
            Dispose();
        }

    }
}
