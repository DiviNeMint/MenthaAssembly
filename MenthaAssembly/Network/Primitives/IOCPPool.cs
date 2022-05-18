using MenthaAssembly.Utils;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MenthaAssembly.Network.Primitives
{
    public class IOCPPool : Pool<SocketAsyncEventArgs>
    {
        private ConcurrentQueue<byte[]> BufferPool = new ConcurrentQueue<byte[]>();

        private int _BufferSize = 8192;
        public int BufferSize
        {
            get => _BufferSize;
            set
            {
                _BufferSize = value;
                BufferPool = new ConcurrentQueue<byte[]>();
            }
        }

        public void EnqueueBuffer(byte[] Buffer)
        {
            if (IsDisposed)
                return;

            if (Buffer.Length == BufferSize)
                BufferPool.Enqueue(Buffer);
        }

        public byte[] DequeueBuffer()
        {
            if (IsDisposed)
                return null;

            return BufferPool.TryDequeue(out byte[] Buffer) ? Buffer : new byte[BufferSize];
        }

        public override void Enqueue(SocketAsyncEventArgs e)
        {
            e.SocketError = SocketError.Success;
            e.UserToken = null;
            e.SetBuffer(null, 0, 0);

            base.Enqueue(e);
        }

        public SocketAsyncEventArgs Dequeue()
            => base.TryDequeue(out SocketAsyncEventArgs e) ? e : IsDisposed ? null : new SocketAsyncEventArgs();

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                BufferPool = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
