using MenthaAssembly.Network.Messages;

namespace MenthaAssembly.Network.Utils
{
    public class CommonPingProvider : IPingProvider
    {
        public static CommonPingProvider Instance { get; } = new CommonPingProvider();

        public IMessage Provide() => new PingRequest();

    }
}
