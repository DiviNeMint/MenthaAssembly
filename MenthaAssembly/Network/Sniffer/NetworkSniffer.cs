using MenthaAssembly.Network.Primitives;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Network
{
    public unsafe class NetworkSniffer : IDisposable
    {
        public event EventHandler<PacketArrivedEventArgs> PacketArrived;

        private readonly int BufferSize;
        public NetworkSniffer()
        {
            BufferSize = NetworkHelper.GetNetorkMTUv4().Max();
            Parser = ParsePacket;
        }

        private IOCPSocket Listener;
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
            Stop();
            
            Listener = new IOCPSocket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP) { BufferSize = BufferSize };
            Listener.Bind(new IPEndPoint(Address, 0));

            if (!SetSocketOption())
                throw new NotSupportedException();

            // Listen
            Listener.Received += OnListenerReceived;
            for (int i = 0; i < 5; i++)
                Listener.Receive();
        }

        private void OnListenerReceived(object sender, Stream e)
        {
            // Event
            Parser.BeginInvoke(e, Parser.EndInvoke, null);

            // Loop Receive
            Listener?.Receive();
        }

        private readonly Action<Stream> Parser;
        protected void ParsePacket(Stream Stream)
        {
            byte[] Packet = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                int Length = Stream.Read(Packet, 0, Packet.Length);
                int* pPacket;

                fixed (byte* pDatas = &Packet[0])
                    pPacket = (int*)pDatas;

                IPHeader IPHeader = default;
                int* pIPHeader = IPHeader.Context;

                for (int i = 0; i < 5; i++)
                    *pIPHeader++ = *pPacket++;

                int OptionsLength = IPHeader.OptionsLength;
                if (OptionsLength > 0)
                {
                    int* pDest = IPHeader.Options;
                    for (int i = 0; i < OptionsLength; i++)
                        *pDest++ = *pPacket++;
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

                            OptionsLength = Header.OptionsLength;
                            if (OptionsLength > 0)
                            {
                                int* pDest = Header.Options;
                                for (int i = 0; i < OptionsLength; i++)
                                    *pDest++ = *pPacket++;
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

                Length -= IPHeader.Length;
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
                ArrayPool<byte>.Shared.Return(Packet);
            }
        }

        protected virtual void OnPacketArrived(PacketArrivedEventArgs e)
            => PacketArrived?.Invoke(this, e);
        private bool SetSocketOption()
        {
            try
            {
                // Enable Header Included
                Listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

                // Enable Receive All
                int Code = Listener.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), null);

                return Code == 0;
            }
            catch
            {
            }
            return false;
        }

        private bool IsStopping = false;
        public void Stop()
        {
            if (Listener != null)
            {
                try
                {
                    IsStopping = true;
                    Listener.Dispose();
                    Listener = null;
                }
                finally
                {
                    IsStopping = false;
                }
            }
        }

        private bool IsDisposed;
        public void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                // Listener
                Stop();
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}