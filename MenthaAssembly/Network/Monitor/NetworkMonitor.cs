using MenthaAssembly.Network.Primitives;
using MenthaAssembly.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MenthaAssembly.Network
{
    public partial class NetworkMonitor : IDisposable
    {
        public event EventHandler<PacketArrivedEventArgs> PacketArrived;

        private Socket Listener;
        private readonly BufferPool BufferPool = new BufferPool(ushort.MaxValue);
        public void Start()
            => Start(NetworkHelper.GetLocalhostInterNetworkAddresses().FirstOrDefault());
        public void Start(string Address)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{this.GetType().Name} Start Error\nAddress may not be correct format.");

            this.Start(TempIP);
        }
        public void Start(IPAddress Address)
        {
            // Reset
            this.Dispose();
            IsDisposed = false;

            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            Listener.Bind(new IPEndPoint(Address, 0));

            if (!SetSocketOption())
                throw new NotSupportedException();
            
            Listen();
        }

        private bool SetSocketOption()
        {
            try
            {
                Listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
                byte[] IN = new byte[4] { 1, 0, 0, 0 },
                       OUT = new byte[4];

                //低級別操作模式,接受所有的資料包，這一步是關鍵，必須把socket設成raw和IP Level才可用SIO_RCVALL
                int Code = Listener.IOControl(IOControlCode.ReceiveAll, IN, OUT);

                Code = OUT[0] + OUT[1] + OUT[2] + OUT[3]; //把4個8位元位元組合成一個32位元整數

                return Code == 0;
            }
            catch
            {
            }
            return false;
        }

        private void Listen()
        {
            byte[] Buffer = BufferPool.Dequeue();
            Listener.BeginReceive(Buffer, 0, ushort.MaxValue, SocketFlags.None, OnReceiveProcess, Buffer);
        }

        private unsafe void OnReceiveProcess(IAsyncResult ar)
        {
            int ReceiveLength = Listener.EndReceive(ar);

            Listen();

            if (ar.AsyncState is byte[] Buffer)
            {
                try
                {
                    byte* pBuffer;

                    fixed (byte* pbuffer = &Buffer[0])
                        pBuffer = pbuffer;

                    IPHeader* pIPHeader = (IPHeader*)pBuffer;
                    IProtocolHeader ProtocolHeader = null;
                    switch (pIPHeader->Protocol)
                    {
                        case ProtocolType.Icmp:
                            ProtocolHeader = *(IcmpHeader*)(pBuffer + pIPHeader->HeaderLength);
                            break;
                        case ProtocolType.Ggp:
                            ProtocolHeader = *(GgpHeader*)(pBuffer + pIPHeader->HeaderLength);
                            break;
                        case ProtocolType.Tcp:
                            ProtocolHeader = *(TcpHeader*)(pBuffer + pIPHeader->HeaderLength);
                            break;
                        case ProtocolType.Udp:
                            ProtocolHeader = *(UdpHeader*)(pBuffer + pIPHeader->HeaderLength);
                            break;
                    }

                    int Offset = pIPHeader->HeaderLength + (ProtocolHeader?.Length ?? 0);
                    byte[] Content = Buffer.Skip(Offset)
                                           .Take(pIPHeader->PacketLength - Offset)
                                           .ToArray();
                    OnPacketArrived(new PacketArrivedEventArgs(*pIPHeader, ProtocolHeader, Content));
                }
                finally
                {
                    BufferPool.Enqueue(ref Buffer);
                }
            }
        }

        protected virtual void OnPacketArrived(PacketArrivedEventArgs e)
            => PacketArrived?.Invoke(this, e);

        protected bool IsDisposed;
        public void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                // Listener
                Listener?.Close();
                Listener?.Dispose();
                Listener = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

        ~NetworkMonitor()
        {
            Dispose();
        }

    }
}
