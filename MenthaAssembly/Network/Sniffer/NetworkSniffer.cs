using MenthaAssembly.Network.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Network
{
    public unsafe class NetworkSniffer : IOCPSocket
    {
        public override int BufferSize => base.BufferSize;

        public event EventHandler<PacketArrivedEventArgs> PacketArrived;

        public NetworkSniffer()
        {
            base.BufferSize = ushort.MaxValue;
            Parser = ParsePacket;
        }

        private Socket Listener;
        public void Start()
            => Start(NetworkHelper.GetLocalhostInterNetworkAddresses().FirstOrDefault());
        public void Start(string Address)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{GetType().Name} Start Error\nAddress may not be correct format.");

            Start(TempIP);
        }
        public void Start(IPAddress Address)
        {
            // Reset
            if (Listener != null)
                Listener.Dispose();

            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            Listener.Bind(new IPEndPoint(Address, 0));

            if (!SetSocketOption())
                throw new NotSupportedException();

            // Listen
            SocketAsyncEventArgs e = Dequeue(true);
            if (!Listener.ReceiveAsync(e))
                OnReceiveProcess(e);
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

        private void OnReceiveProcess(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success &&
                e.BytesTransferred > 19)
            {
                byte[] Packet = e.Buffer;
                int Length = e.BytesTransferred;
                Parser.BeginInvoke(Packet, Length, ar => Parser.EndInvoke(ar), null);

                byte[] Buffer = Dequeue();
                e.SetBuffer(Buffer, 0, ushort.MaxValue);
            }

            // Loop Receive
            if (!Listener.ReceiveAsync(e))
                OnReceiveProcess(e);
        }

        private readonly Action<byte[], int> Parser;
        protected void ParsePacket(byte[] Packet, int Length)
        {
            try
            {
                int* pPacket;

                fixed (byte* pDatas = &Packet[0])
                    pPacket = (int*)pDatas;

                IPHeader IPHeader = default;
                int* pIPHeader = IPHeader.Context;

                for (int i = 0; i < 5; i++)
                    *pIPHeader++ = *pPacket++;

                int IPOptions4Bits = IPHeader.Length4Bits - 5;
                if (IPOptions4Bits > 0)
                {
                    byte[] IPOptions = new byte[IPOptions4Bits << 2];

                    int* pDest;
                    fixed (byte* pOptions = &IPOptions[0])
                        pDest = (int*)pOptions;

                    for (int i = 0; i < IPOptions4Bits; i++)
                        *pDest++ = *pPacket++;

                    IPHeader._Options = IPOptions;
                }

                IProtocolHeader ProtocolHeader = null;
                switch (IPHeader.Protocol)
                {
                    case ProtocolType.Icmp:
                        {
                            IcmpHeader Header = default;
                            *Header.Context = *pPacket++;
                            ProtocolHeader = Header;
                            break;
                        }
                    case ProtocolType.Ggp:
                        {
                            GgpHeader Header = default;
                            *Header.Context = *pPacket++;
                            ProtocolHeader = Header;
                            break;
                        }
                    case ProtocolType.Tcp:
                        {
                            TcpHeader Header = default;
                            int* pHeader = Header.Context;

                            for (int i = 0; i < 5; i++)
                                *pHeader++ = *pPacket++;

                            int Options4Bits = Header.Length4Bits - 5;
                            if (Options4Bits > 0)
                            {
                                byte[] Options = new byte[Options4Bits << 2];

                                int* pDest;
                                fixed (byte* pOptions = &Options[0])
                                    pDest = (int*)pOptions;

                                for (int i = 0; i < Options4Bits; i++)
                                    *pDest++ = *pPacket++;

                                Header._Options = Options;
                            }

                            ProtocolHeader = Header;
                            break;
                        }
                    case ProtocolType.Udp:
                        {
                            UdpHeader Header = default;
                            *Header.Context = *pPacket++;
                            ProtocolHeader = Header;
                            break;
                        }
                }

                Length -= IPHeader.HeaderLength;
                if (ProtocolHeader != null)
                    Length -= ProtocolHeader.Length;

                byte[] Content = null;
                if (Length > 0)
                {
                    Content = new byte[Length];
                    Marshal.Copy((IntPtr)pPacket, Content, 0, Length);
                }

                OnPacketArrived(new PacketArrivedEventArgs(IPHeader, ProtocolHeader, Content));
            }
            finally
            {
                Enqueue(ref Packet);
            }
        }

        protected virtual void OnPacketArrived(PacketArrivedEventArgs e)
            => PacketArrived?.Invoke(this, e);

        protected sealed override void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    OnReceiveProcess(e);
                    break;
                default:
                    {
                        Debug.WriteLine($"[Error][{GetType().Name}Not support the operation {e.LastOperation}.");
                        throw new ArgumentException("The last operation completed on the socket was not Receive");
                    }
            }
        }

        private bool IsDisposed;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                // Listener
                Listener.Close();
                Listener.Dispose();
                Listener = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
