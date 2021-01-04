using System.Net;

namespace MenthaAssembly.Network
{
    public interface IConnectionValidator
    {
        bool Validate(TcpServer Server, IPEndPoint Address);

    }
}
