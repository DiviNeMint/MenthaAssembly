using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class TcpBase<Token> : IOCPBase
        where Token : ITcpToken
    {
        public event EventHandler<IPEndPoint> Disconnected;

        private ConcurrentQueue<Token> TokenPool = new ConcurrentQueue<Token>();

        protected Token DequeueToken()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            return TokenPool.TryDequeue(out Token Token) ? Token : CreateToken();
        }
        protected void EnqueueToken(Token Token)
        {
            if (IsDisposed)
                return;

            ResetToken(Token);
            TokenPool.Enqueue(Token);
        }

        protected abstract Token CreateToken();
        protected abstract void PrepareToken(Token Token, TcpStream Stream);
        protected abstract void ResetToken(Token Token);

        protected abstract void OnReceived(Token Token, Stream Stream);

        protected virtual void OnDisconnected(IPEndPoint Address)
            => Disconnected?.Invoke(this, Address);

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                TokenPool = null;

                base.Dispose();
            }
            finally
            {
                IsDisposed = true;
            }

        }

    }
}
