using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class IOCPToken : IDisposable
    {
        protected Socket Socket;

        public IPEndPoint Address { get; private set; }

        protected SemaphoreSlim SendLock;

        public IOCPToken(Socket Socket)
        {
            this.Socket = Socket;
            Address = (IPEndPoint)Socket.RemoteEndPoint;
            SendLock = new SemaphoreSlim(1);
        }

        public void WaitSend(CancellationToken CancelToken)
        {
            if (!IsDisposed)
                SendLock.Wait(CancelToken);
        }
        public async Task WaitSendAsync(CancellationToken CancelToken)
        {
            if (!IsDisposed)
                await SendLock.WaitAsync(CancelToken);
        }

        public void ReleaseSend()
        {
            if (!IsDisposed)
                SendLock.Release();
        }

        public bool SendAsync(SocketAsyncEventArgs e)
            => Socket.SendAsync(e);

        public bool ReceiveAsync(SocketAsyncEventArgs e)
            => Socket.ReceiveAsync(e);

        public Stream GetStream()
            => new NetworkStream(Socket);

        private bool IsDisposed = false;
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                Socket.Dispose();
                Socket = null;

                Address = null;

                SendLock.Dispose();
                SendLock = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
