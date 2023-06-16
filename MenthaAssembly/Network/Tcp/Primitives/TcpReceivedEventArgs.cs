using MenthaAssembly.Network.Primitives;
using System;
using System.Net;

namespace MenthaAssembly.Network
{
    public sealed class TcpReceivedEventArgs : EventArgs
    {
        public EndPoint Address
            => Session.Address;

        public IMessage Message { get; }

        private readonly TcpBase Remote;
        private readonly int UID;
        private readonly TcpBase.Session Session;
        internal TcpReceivedEventArgs(TcpBase Remote, TcpBase.Session Session, int UID, IMessage Message)
        {
            this.Remote = Remote;
            this.Session = Session;
            this.UID = UID;
            this.Message = Message;
        }

        public void Reply(IMessage Message)
        {
            lock (Session)
            {
                Remote.Reply(Session, UID, Message);
            }
        }

    }
}