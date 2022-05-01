using MenthaAssembly.Network.Primitives;
using System;

namespace MenthaAssembly.Network
{
    public class PacketArrivedEventArgs : EventArgs
    {
        public IPHeader IPHeader { get; }

        public IProtocolHeader ProtocolHeader { get; }

        public byte[] Content { get; }

        public PacketArrivedEventArgs(IPHeader IPHeader, IProtocolHeader ProtocolHeader, byte[] Content)
        {
            this.IPHeader = IPHeader;
            this.ProtocolHeader = ProtocolHeader;
            this.Content = Content;
        }

    }
}
