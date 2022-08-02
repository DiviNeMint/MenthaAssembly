using System;
using System.IO;

namespace MenthaAssembly.IO
{
    public class AccessStream : Stream
    {
        public override bool CanRead { get; }

        public override bool CanWrite { get; }

        public override bool CanSeek
            => IsDisposed ? false : BaseStream.CanSeek;

        public override long Length
            => IsDisposed ? 0 : BaseStream.Length;

        public override long Position
        {
            get => IsDisposed ? 0 : BaseStream.Position;
            set
            {
                CheckDispose();
                BaseStream.Position = value;
            }
        }

        private Stream BaseStream;
        private readonly bool LeaveOpen;
        public AccessStream(Stream BaseStream, bool LeaveOpen, StreamAccess Access)
        {
            this.BaseStream = BaseStream;
            this.LeaveOpen = LeaveOpen;
            switch (Access)
            {
                case StreamAccess.Read:

                    break;
                case StreamAccess.Write:
                    break;
                case StreamAccess.ReadWrite:
                default:
                    break;
            }

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDispose();

            if (!CanRead)
                throw new NotSupportedException();

            return BaseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDispose();

            if (!CanWrite)
                throw new NotSupportedException();

            BaseStream.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            CheckDispose();

            if (!CanWrite)
                throw new NotSupportedException();

            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDispose();
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            CheckDispose();
            BaseStream.SetLength(value);
        }

        private bool IsDisposed;
        protected override void Dispose(bool Disposing)
        {
            if (!IsDisposed)
            {
                if (!LeaveOpen)
                    BaseStream.Dispose();
                BaseStream = null;

                IsDisposed = true;
            }

            base.Dispose(Disposing);
        }

        private void CheckDispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(AccessStream));
        }

    }
}
