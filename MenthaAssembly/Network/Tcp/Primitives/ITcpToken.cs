using MenthaAssembly.Network.Primitives;

namespace MenthaAssembly.Network
{
    public interface ITcpToken : IIOCPToken
    {
        public TcpStream Stream { get; }

    }
}
