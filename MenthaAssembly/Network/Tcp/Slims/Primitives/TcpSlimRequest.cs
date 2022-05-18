using MenthaAssembly.Network.Primitives;
using System;

namespace MenthaAssembly.Network
{
    public sealed class TcpSlimRequest
    {
        public IMessage Content { get; }

        private TcpSlimBase Base;
        private TcpSlimToken Token;
        private readonly int UID;
        internal TcpSlimRequest(TcpSlimBase Base, TcpSlimToken Token, int UID, IMessage Message)
        {
            this.Base = Base;
            this.Token = Token;
            this.UID = UID;
            this.Content = Message;
        }

        private bool HadReplied = false;

        public void Reply(IMessage Response)
            => Reply(Response, 3000);
        public void Reply(IMessage Response, int TimeoutMileseconds)
        {
            if (HadReplied)
                throw new InvalidOperationException("The request had replied.");

            HadReplied = true;

            try
            {
                Base.Reply(Token, UID, Response, TimeoutMileseconds);
            }
            finally
            {
                Base = null;
                Token = null;
            }
        }
    }
}
