using MenthaAssembly.Network.Primitives;
using MenthaAssembly.Network.Utils;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class TcpClient : TcpSocketBase
    {
        protected SocketToken ServerToken;

        public TcpClient(IMessageHandler MessageHandler) : this(CommonProtocolHandler.Instance, MessageHandler) { }
        public TcpClient(IProtocolHandler Protocol, IMessageHandler MessageHandler) : this(Protocol, MessageHandler, 8192) { }
        public TcpClient(IProtocolHandler Protocol, IMessageHandler MessageHandler, int BufferSize) : base(Protocol, MessageHandler, BufferSize) { }

        public void Connect(string Address, int Port)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{this.GetType().Name} Connect Error\nIPAddress may not be correct format.");

            this.Connect(new IPEndPoint(TempIP, Port));
        }
        public void Connect(IPAddress IPAddress, int Port)
            => this.Connect(new IPEndPoint(IPAddress, Port));
        public void Connect(IPEndPoint IPEndPoint)
        {
            //Check if it had Connect server.
            ServerToken?.Dispose();

            // Connect Server
            Socket Server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Server.Connect(IPEndPoint);

            Debug.WriteLine($"[Info]{this.GetType().Name} Connect to [{IPEndPoint.Address}:{IPEndPoint.Port}].");

            // Start Receive Server's Message
            SocketAsyncEventArgs e = Dequeue();
            ServerToken = new SocketToken(Server, e);

            if (!Server.ReceiveAsync(e))
                OnReceiveProcess(e);
        }

        public async Task<IMessage> Send(IMessage Request)
           => await Send(Request, 5000);
        public async Task<IMessage> Send(IMessage Request, int TimeoutMileseconds)
           => await base.Send(ServerToken, Request, TimeoutMileseconds);

        public async Task<T> Send<T>(IMessage Request)
            where T : IMessage
           => await Send<T>(Request, 3000);
        public async Task<T> Send<T>(IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            IMessage Response = await base.Send(ServerToken, Request, TimeoutMileseconds);
            if (ErrorMessage.Timeout.Equals(Response))
                throw new TimeoutException();

            if (ErrorMessage.NotSupport.Equals(Response))
                throw new NotSupportedException();

            return (T)Response;
        }

        protected override void OnDisconnected(SocketToken Token)
        {
            // Clear ServerToken
            ServerToken = null;

            // Trigger Disconnected Event.
            base.OnDisconnected(Token);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                ServerToken?.Dispose();
                ServerToken = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

        ~TcpClient()
        {
            Dispose();
        }

    }
}
