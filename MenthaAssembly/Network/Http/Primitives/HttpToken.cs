using System.Net;
using System.Net.Security;

namespace MenthaAssembly.Network.Primitives
{
    public class HttpToken : ITcpToken
    {
        public IPEndPoint Address
            => Stream?.Address;

        public TcpStream Stream { set; get; }

        public SslStream SslStream { set; get; }

    }
}
