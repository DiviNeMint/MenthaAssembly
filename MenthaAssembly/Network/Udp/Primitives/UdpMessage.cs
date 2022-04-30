using System.Net;

namespace MenthaAssembly.Network
{
    public class UdpMessage
    {
        public IPEndPoint Address { get; }

        public IMessage Message { get; }

        internal UdpMessage(IPEndPoint Address, IMessage Message)
        {
            this.Address = Address;
            this.Message = Message;
        }

    }
}