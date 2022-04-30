using MenthaAssembly.Network.Primitives;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class UdpServer : UdpSocket
    {
        public IPEndPoint IPEndPoint { get; protected set; }

        public UdpServer() : base(CommonProtocolCoder.Instance) { }
        public UdpServer(IProtocolCoder Protocol) : base(Protocol) { }

        public void Start(int Port)
            => Start(new IPEndPoint(IPAddress.Any, Port));
        public void Start(string Address, int Port)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{GetType().Name} Start Error\nAddress may not be correct format.");

            Start(new IPEndPoint(TempIP, Port));
        }
        public void Start(IPAddress IPAddress, int Port)
            => Start(new IPEndPoint(IPAddress, Port));
        public void Start(IPEndPoint IPEndPoint)
        {
            try
            {
                Reset();

                // Create New Listener
                Socket.Bind(IPEndPoint);
                this.IPEndPoint = IPEndPoint;

                Debug.WriteLine($"[Info][{GetType().Name}]Start at [{IPEndPoint.Address}:{IPEndPoint.Port}].");

                // Start Listen
                Listen();
            }
            finally
            {
                this.IPEndPoint = IPEndPoint;
            }
        }

        private void Listen()
        {
            SocketAsyncEventArgs e = Dequeue(true);
            e.RemoteEndPoint = CreateEndPoint();

            // Loop Receive
            if (!Socket.ReceiveFromAsync(e))
                OnReceiveFromProcess(e);
        }

        public void Send(IPEndPoint Address, IMessage Message)
            => SendTo(Address, Message, 3000);
        public void Send(IPEndPoint Address, IMessage Message, int TimeoutMileseconds)
            => SendTo(Address, Message, TimeoutMileseconds);

        public async Task SendAsync(IPEndPoint Address, IMessage Message)
            => await SendToAsync(Address, Message, 3000);
        public async Task SendAsync(IPEndPoint Address, IMessage Message, int TimeoutMileseconds)
            => await SendToAsync(Address, Message, TimeoutMileseconds);

        protected override IPEndPoint CreateEndPoint()
            => IPEndPoint;

    }
}
