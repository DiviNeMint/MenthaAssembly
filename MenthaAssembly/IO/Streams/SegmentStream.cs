using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace MenthaAssembly.Utils
{
    public sealed class SegmentStream : Stream
    {
        public override bool CanRead
            => true;

        public override bool CanSeek
            => Stream.CanSeek;

        public override bool CanWrite
            => false;

        public override long Length { get; }

        public override long Position
        {
            get => Current;
            set => Seek(value, SeekOrigin.Begin);
        }

        private long Current;
        private readonly long Begin;
        private readonly bool LeaveOpen;
        private readonly Stream Stream;
        public SegmentStream(Stream Stream, long Length) : this(Stream, Stream.CanSeek ? Stream.Position : 0L, Length)
        {
        }
        public SegmentStream(Stream Stream, long Length, bool LeaveOpen) : this(Stream, Stream.CanSeek ? Stream.Position : 0L, Length, LeaveOpen)
        {
        }
        public SegmentStream(Stream Stream, long Offset, long Length)
        {
            this.Stream = Stream;
            this.Length = Length;
            Begin = Offset;
        }
        public SegmentStream(Stream Stream, long Offset, long Length, bool LeaveOpen)
        {
            this.Stream = Stream;
            this.Length = Length;
            this.LeaveOpen = LeaveOpen;
            Begin = Offset;
        }

        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            CheckDispose();

            int Length = Math.Min((int)(this.Length - Current), Count);
            if (Length > 0)
            {
                Length = Stream.Read(Buffer, Offset, Length);
                Current += Length;
                return Length;
            }

            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override long Seek(long Offset, SeekOrigin Origin)
        {
            CheckDispose();
            if (!CanSeek)
                throw new NotSupportedException();

            long Position = Origin switch
            {
                SeekOrigin.Begin => Offset,
                SeekOrigin.Current => Current + Offset,
                SeekOrigin.End => Length - Offset,
                _ => throw new NotSupportedException()
            };

            Position = MathHelper.Clamp(Position, 0, Length);

            long Delta = Position - Current;
            Current = Position;
            return Stream.Seek(Delta, SeekOrigin.Current);
        }

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

        private void CheckDispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SegmentStream));
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