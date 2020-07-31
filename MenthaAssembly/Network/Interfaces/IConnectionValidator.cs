using System.Net;

namespace MenthaAssembly.Network
{
    public interface IConnectionValidator
    {
        bool Validate(IPEndPoint Address);

    }
}
