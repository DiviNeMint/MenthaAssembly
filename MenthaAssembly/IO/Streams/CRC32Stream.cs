using System;
using System.IO;

namespace MenthaAssembly.IO
{
    public class CRC32Stream : Stream
    {
        public override bool CanRead { get; }

        public override bool CanWrite { get; }

        public override bool CanSeek
            => false;

        public override long Length
            => throw new NotSupportedException();

        public override long Position
        {
            get;
            set;
        }

        private uint _Code = 0xFFFFFFFFU;
        public int CRC32Code => unchecked((int)~_Code);

        private readonly Stream Stream;
        private readonly bool LeaveOpen;
        public CRC32Stream(Stream Stream, StreamAccess Access) : this(Stream, Access, false)
        {
        }
        public CRC32Stream(Stream Stream, StreamAccess Access, bool LeaveOpen)
        {
            this.Stream = Stream;
            this.LeaveOpen = LeaveOpen;
            switch (Access)
            {
                case StreamAccess.Read:
                    CanRead = true;
                    break;
                case StreamAccess.Write:
                    CanWrite = true;
                    break;
                case StreamAccess.ReadWrite:
                default:
                    throw new NotSupportedException();
            }
        }

        public override int ReadByte()
        {
            CheckDispose();
            if (!CanRead)
                throw new NotSupportedException();

            if (Stream.TryReadByte(out int Result))
                CRC32.Calculate((byte)Result, out _Code, _Code);

            return Result;
        }

        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            CheckDispose();
            if (!CanRead)
                throw new NotSupportedException();

            int Length = Stream.Read(Buffer, Offset, Count);
            CRC32.Calculate(Buffer, Offset, Length, out _Code, _Code);
            return Length;
        }

        public override void WriteByte(byte Value)
        {
            CheckDispose();
            if (!CanWrite)
                throw new NotSupportedException();

            CRC32.Calculate(Value, out _Code, _Code);
            Stream.WriteByte(Value);
        }

        public override void Write(byte[] Buffer, int Offset, int Count)
        {
            CheckDispose();
            if (!CanWrite)
                throw new NotSupportedException();

            CRC32.Calculate(Buffer, Offset, Count, out _Code, _Code);
            Stream.Write(Buffer, Offset, Count);
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Flush()
            => throw new NotSupportedException();

        public override void Close()
        {
            if (IsDisposed)
                return;

            if (!LeaveOpen)
                Stream.Close();

            base.Close();
        }

        public void ResetCode()
            => _Code = 0xFFFFFFFFU;

        private void CheckDispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CRC32Stream));
        }

        private bool IsDisposed = false;
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