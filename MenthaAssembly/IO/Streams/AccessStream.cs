using System;
using System.IO;

namespace MenthaAssembly.IO
{
    public sealed class AccessStream : Stream
    {
        public override bool CanRead { get; }

        public override bool CanWrite { get; }

        public override bool CanSeek
            => !IsDisposed && Stream.CanSeek;

        public override long Length
            => IsDisposed ? 0 : Stream.Length;

        public override long Position
        {
            get => IsDisposed ? 0 : Stream.Position;
            set
            {
                CheckDispose();
                Stream.Position = value;
            }
        }

        private readonly bool LeaveOpen;
        private readonly Stream Stream;
        public AccessStream(Stream BaseStream, bool LeaveOpen, StreamAccess Access)
        {
            this.Stream = BaseStream;
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
            return CanRead ? Stream.Read(Buffer, Offset, Count) :
                             throw new NotSupportedException();
        }

        public override void Write(byte[] Buffer, int Offset, int Count)
        {
            CheckDispose();

            if (!CanWrite)
                throw new NotSupportedException();

            Stream.Write(Buffer, Offset, Count);
        }

        public override long Seek(long Offset, SeekOrigin Origin)
        {
            CheckDispose();
            return Stream.Seek(Offset, Origin);
        }

        public override void SetLength(long Value)
        {
            CheckDispose();
            Stream.SetLength(Value);
        }

        public override void Flush()
        {
            if (!CanWrite)
                throw new NotSupportedException();

            Stream.Flush();
        }

        public override void Close()
        {
            if (IsDisposed)
                return;

            if (!LeaveOpen)
                Stream.Close();

            base.Close();
        }

        private void CheckDispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(AccessStream));
        }

        private bool IsDisposed;
        protected override void Dispose(bool Disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                if (!LeaveOpen)
                    Stream.Dispose();

                base.Dispose(Disposing);
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}