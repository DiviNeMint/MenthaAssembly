using System;
using System.IO;

namespace MenthaAssembly.Utils
{
    public class SegmentStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanWrite => false;

        public override long Length { get; }

        private long _Position;
        private readonly long _Offset;
        public override long Position
        {
            get => _Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        private readonly bool LeaveOpen;

        protected Stream Stream;
        public SegmentStream(Stream Stream, long Length) : this(Stream, Stream.CanSeek ? Stream.Position : 0L, Length)
        {
        }
        public SegmentStream(Stream Stream, long Offset, long Length)
        {
            this.Stream = Stream;
            _Offset = Offset;
            this.Length = Length;
        }
        public SegmentStream(Stream Stream, long Offset, long Length, bool LeaveOpen)
        {
            this.Stream = Stream;
            _Offset = Offset;
            this.Length = Length;
            this.LeaveOpen = LeaveOpen;
        }

        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            int Length = Math.Min((int)(this.Length - _Position), Count);
            if (Length > 0)
            {
                Length = Stream.Read(Buffer, Offset, Length);
                _Position += Length;
                return Length;
            }

            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override long Seek(long Offset, SeekOrigin Origin)
        {
            if (!CanSeek)
                throw new NotSupportedException();

            switch (Origin)
            {
                case SeekOrigin.Begin:
                    {
                        _Position = _Offset + Offset;
                        return Stream.Seek(_Position, SeekOrigin.Begin);
                    }
                case SeekOrigin.Current:
                    {
                        _Position = MathHelper.Clamp(_Position + Offset, _Offset, Length);
                        return Stream.Seek(_Position, SeekOrigin.Begin);
                    }
                case SeekOrigin.End:
                    {
                        _Position = _Offset + Length + Offset;
                        return Stream.Seek(_Position, SeekOrigin.Begin);
                    }
                default:
                    return _Position;
            }

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
