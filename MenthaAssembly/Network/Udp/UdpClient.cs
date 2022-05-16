//using MenthaAssembly.Network.Primitives;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace MenthaAssembly.Network
//{
//    public class UdpClient : UdpSocket
//    {
//        public IPEndPoint Server { get; protected set; }

//        public UdpClient() : base(CommonProtocolCoder.Instance) { }
//        public UdpClient(IProtocolCoder Protocol) : base(Protocol) { }

//        public void Connect(string Address, int Port)
//        {
//            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
//                throw new Exception($"{GetType().Name} Start Error\nAddress may not be correct format.");

//            Connect(new IPEndPoint(TempIP, Port));
//        }
//        public void Connect(IPAddress IPAddress, int Port)
//            => Connect(new IPEndPoint(IPAddress, Port));
//        public void Connect(IPEndPoint IPEndPoint)
//        {
//            try
//            {
//                Reset();

//                // Create New Listener
//                Socket.Bind(IPEndPoint);
//                Server = IPEndPoint;

//                Debug.WriteLine($"[Info][{GetType().Name}]Connect to [{IPEndPoint.Address}:{IPEndPoint.Port}].");

//                // Start Listen
//                Listen();
//            }
//            finally
//            {
//                this.Server = IPEndPoint;
//            }
//        }

//        private void Listen()
//        {
//            SocketAsyncEventArgs e = Dequeue(true);
//            e.RemoteEndPoint = CreateEndPoint();

//            // Loop Receive
//            if (!Socket.ReceiveFromAsync(e))
//                OnReceiveFromProcess(e);
//        }

//        public void Send(IMessage Message)
//            => SendTo(Server, Message, 3000);
//        public void Send(IMessage Message, int TimeoutMileseconds)
//            => SendTo(Server, Message, TimeoutMileseconds);

//        public async Task SendAsync(IMessage Message)
//            => await SendToAsync(Server, Message, 3000);
//        public async Task SendAsync(IMessage Message, int TimeoutMileseconds)
//            => await SendToAsync(Server, Message, TimeoutMileseconds);

//        protected override IPEndPoint CreateEndPoint()
//            => Server;

//    }
//}
