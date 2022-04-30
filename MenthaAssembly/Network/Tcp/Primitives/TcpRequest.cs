using MenthaAssembly.Network.Primitives;

namespace MenthaAssembly.Network
{
    public sealed class TcpRequest
    {
        public IMessage Message { get; }

        private readonly TcpSocket Socket;
        private readonly TcpToken Token;
        private readonly int UID;
        internal TcpRequest(TcpSocket Socket, TcpToken Token, int UID, IMessage Message)
        {
            this.Socket = Socket;
            this.Token = Token;
            this.UID = UID;
            this.Message = Message;
        }

        public void Reply(IMessage Response)
            => Reply(Response, 3000);
        public void Reply(IMessage Response, int TimeoutMileseconds)
            => Socket.Reply(Token, UID, Response, TimeoutMileseconds);

    }
}
