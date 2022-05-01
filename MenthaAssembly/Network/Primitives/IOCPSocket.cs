using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class IOCPSocket : IDisposable
    {
        private readonly ConcurrentQueue<SocketAsyncEventArgs> Pool = new ConcurrentQueue<SocketAsyncEventArgs>();
        private ConcurrentQueue<byte[]> BufferPool = new ConcurrentQueue<byte[]>();

        private int _BufferSize = 8192;
        public virtual int BufferSize
        {
            get => _BufferSize;
            set
            {
                _BufferSize = value;
                BufferPool = new ConcurrentQueue<byte[]>();
            }
        }

        protected abstract void OnIOCompleted(object sender, SocketAsyncEventArgs e);

        protected void Enqueue(ref SocketAsyncEventArgs e)
        {
            e.SocketError = SocketError.Success;
            e.AcceptSocket = null;
            e.UserToken = null;

            // Enqueue Buffer
            if (e.Buffer != null && e.Buffer.Length == BufferSize)
                BufferPool.Enqueue(e.Buffer);

            e.SetBuffer(null, 0, 0);

            Pool.Enqueue(e);
        }
        protected SocketAsyncEventArgs Dequeue(bool AllocBuffer)
        {
            if (!Pool.TryDequeue(out SocketAsyncEventArgs e))
            {
                e = new SocketAsyncEventArgs();
                e.Completed += OnIOCompleted;
            }

            if (AllocBuffer)
            {
                byte[] Buffer = Dequeue();
                e.SetBuffer(Buffer, 0, Buffer.Length);
            }

            return e;
        }

        protected void Enqueue(ref byte[] Buffer)
        {
            if (Buffer.Length == BufferSize)
                BufferPool.Enqueue(Buffer);
        }
        protected byte[] Dequeue()
            => BufferPool.TryDequeue(out byte[] Buffer) ? Buffer : new byte[_BufferSize];

        public abstract void Dispose();

    }
}
