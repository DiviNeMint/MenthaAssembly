using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network.Primitives
{
    public class HttpChunkStream : Stream
    {
        public override bool CanRead
            => true;

        public override bool CanSeek
            => false;

        public override bool CanWrite
            => false;

        public override long Length
            => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        private Stream Stream;
        private readonly bool LeaveOpen;
        public HttpChunkStream(Stream Stream) : this(Stream, false)
        {
        }
        public HttpChunkStream(Stream Stream, bool LeaveOpen)
        {
            if (Stream is null)
                throw new ArgumentNullException();

            this.Stream = Stream;
            this.LeaveOpen = LeaveOpen;
        }

        private int ChunkOffset = -1,
                    ChunkLength = -1;
        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            if (IsDisposed)
                return 0;

            if (ChunkLength == 0)
                return 0;

            int So = Offset,
                ReadLength,
                ReserveLength;

            while (Count > 0)
            {
                ReserveLength = ChunkLength - ChunkOffset;
                if (ReserveLength <= 0)
                {
                    ChunkLength = ParseChunkSize();
                    ChunkOffset = 0;
                    ReserveLength = ChunkLength;
                }

                if (ChunkLength == 0)
                    return Offset - So;

                int MaxBufferLength = Math.Min(ReserveLength, Count),
                    BufferLength = MaxBufferLength,
                    TotalRead = 0;

                do
                {
                    ReadLength = Stream.Read(Buffer, Offset, BufferLength);

                    TotalRead += ReadLength;
                    Offset += ReadLength;
                    ChunkOffset += ReadLength;
                    Count -= ReadLength;
                    BufferLength -= ReadLength;

                } while (TotalRead < MaxBufferLength);
            }

            return Offset - So;
        }

        private int ParseChunkSize()
        {
            StringBuilder Builder = new StringBuilder();

            for (int i = 0; i < 16; i++)
            {
                if (!Stream.TryReadByte(out int c))
                    break;

                if (c == 13)
                {
                    c = Stream.ReadByte();
                    if (c == 10)
                    {
                        if (Builder.Length == 0)
                            continue;

                        return int.TryParse(Builder.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int Size) ? Size : throw new FormatException();
                    }

                    Builder.Append('\r');
                    Builder.Append((char)c);
                    continue;
                }

                Builder.Append((char)c);
            }

            throw new FormatException();
        }

        public override void Write(byte[] Buffer, int Offset, int Count)
            => throw new NotSupportedException();

        public override void Flush()
            => throw new NotSupportedException();

        public override long Seek(long Offset, SeekOrigin Origin)
            => throw new NotSupportedException();

        public override void SetLength(long Value)
            => throw new NotSupportedException();

        private bool IsDisposed = false;
        protected override void Dispose(bool Disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                if (!LeaveOpen)
                    Stream.Dispose();
                Stream = null;

                base.Dispose(Disposing);
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
