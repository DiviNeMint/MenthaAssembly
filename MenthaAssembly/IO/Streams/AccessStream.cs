using System;
using System.IO;

namespace MenthaAssembly.IO
{
    public class AccessStream : Stream
    {
        public override bool CanRead { get; }

        public override bool CanWrite { get; }

        public override bool CanSeek
            => !IsDisposed && BaseStream.CanSeek;

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
                    {
                        CanRead = true;
                        break;
                    }
                case StreamAccess.Write:
                    {
                        CanWrite = true;
                        break;
                    }
                case StreamAccess.ReadWrite:
                default:
                    {
                        CanRead = true;
                        CanWrite = true;
                        break;
                    }
            }

        }

        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            CheckDispose();
            return CanRead ? BaseStream.Read(Buffer, Offset, Count) :
                             throw new NotSupportedException();
        }

        public override void Write(byte[] Buffer, int Offset, int Count)
        {
            CheckDispose();

            if (!CanWrite)
                throw new NotSupportedException();

            BaseStream.Write(Buffer, Offset, Count);
        }

        public override void Flush()
        {
            CheckDispose();

            if (!CanWrite)
                throw new NotSupportedException();

            BaseStream.Flush();
        }

        public override long Seek(long Offset, SeekOrigin Origin)
        {
            CheckDispose();
            return BaseStream.Seek(Offset, Origin);
        }

        public override void SetLength(long Value)
        {
            CheckDispose();
            BaseStream.SetLength(Value);
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