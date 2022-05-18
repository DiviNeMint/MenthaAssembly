using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class IOCPStream : Stream
    {
        private IPEndPoint _Address;
        public IPEndPoint Address => _Address;

        public override bool CanRead
            => true;

        public override bool CanWrite
            => true;

        public override bool CanTimeout
            => true;

        public override int ReadTimeout { set; get; } = 3000;

        public override int WriteTimeout { set; get; } = 3000;

        public override bool CanSeek
            => false;

        public override long Length
            => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        private IOCPPool Pool;
        protected internal Socket Socket;
        public IOCPStream(Socket Socket, IOCPPool Pool)
        {
            this.Socket = Socket;
            this._Address = (IPEndPoint)Socket.RemoteEndPoint;
            this.Pool = Pool;
        }

        public abstract void Write(Stream Buffer, CancellationToken CancellationToken);
        public abstract Task WriteAsync(Stream Buffer, CancellationToken CancellationToken);

        private SemaphoreSlim ReadSemaphore = new SemaphoreSlim(1);
        protected void LockRead()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            ReadSemaphore.Wait();
        }
        protected void LockRead(CancellationToken CancellationToken)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            ReadSemaphore.Wait(CancellationToken);
        }
        protected Task LockReadAsync(CancellationToken CancellationToken)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            return ReadSemaphore.WaitAsync(CancellationToken);
        }
        protected void ReleaseRead()
            => ReadSemaphore?.Release();

        private SemaphoreSlim WriteSemaphore = new SemaphoreSlim(1);
        protected void LockWrite(CancellationToken CancellationToken)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            WriteSemaphore.Wait(CancellationToken);
        }
        protected Task LockWriteAsync(CancellationToken CancellationToken)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            return WriteSemaphore.WaitAsync(CancellationToken);
        }
        protected void ReleaseWrite()
            => WriteSemaphore?.Release();

        protected abstract void OnIOCompleted(object sender, SocketAsyncEventArgs e);

        protected void Enqueue(SocketAsyncEventArgs e)
        {
            e.Completed -= OnIOCompleted;
            Pool?.Enqueue(e);
        }
        protected SocketAsyncEventArgs Dequeue()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            SocketAsyncEventArgs e = Pool.Dequeue();
            e.Completed += OnIOCompleted;
            return e;
        }

        protected void EnqueueBuffer(byte[] Buffer)
            => Pool?.EnqueueBuffer(Buffer);
        protected byte[] DequeueBuffer()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);

            return Pool.DequeueBuffer();
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Flush()
        {

        }

        private bool IsDisposed = false;
        protected override void Dispose(bool Disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                ReadSemaphore.Dispose();
                ReadSemaphore = null;

                WriteSemaphore.Dispose();
                WriteSemaphore = null;

                Socket.Dispose();
                Socket = null;

                _Address = null;

                Pool = null;

                base.Dispose(Disposing);
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
