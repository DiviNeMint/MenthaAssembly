using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MenthaAssembly.Utils
{
    public class ConcatStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanSeek => true;

        public override long Length
            => throw new NotSupportedException();

        private long _Position = 0;
        public override long Position
        {
            get => _Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        private byte[] Datas;
        private readonly int DatasLength;

        private readonly bool LeaveStreamOpen;
        private readonly bool LeaveMergedStreamOpen;
        private readonly bool IsConcatStreams;
        private Stream Stream,
                       MergedStream;

        public ConcatStream(IEnumerable<byte> Datas, Stream Stream) : this(Datas, Stream, true)
        {
        }
        public ConcatStream(IEnumerable<byte> Datas, Stream Stream, bool LeaveOpen)
        {
            this.Datas = Datas.ToArray();
            DatasLength = this.Datas.Length;
            MergedStream = Stream;
            LeaveMergedStreamOpen = LeaveOpen;
        }
        public ConcatStream(byte[] Datas, int Offset, int Length, Stream Stream) : this(Datas, Offset, Length, Stream, true)
        {
        }
        public ConcatStream(byte[] Datas, int Offset, int Length, Stream Stream, bool LeaveOpen)
        {
            this.Datas = Datas.Skip(Offset)
                              .Take(Length)
                              .ToArray();
            DatasLength = Length;
            MergedStream = Stream;
            LeaveMergedStreamOpen = LeaveOpen;
        }
        public ConcatStream(Stream Stream, Stream MergedStream) : this(Stream, false, MergedStream, false)
        {

        }
        public ConcatStream(Stream Stream, bool LeaveStreamOpen, Stream MergedStream, bool LeaveMergedStreamOpen)
        {
            this.Stream = Stream;
            this.LeaveStreamOpen = LeaveStreamOpen;
            this.MergedStream = MergedStream;
            this.LeaveMergedStreamOpen = LeaveMergedStreamOpen;
            IsConcatStreams = true;
        }

        private bool IsBasePosition = true;
        public override int Read(byte[] Buffers, int Offset, int Count)
        {
            int ReadLength;
            if (IsBasePosition)
            {
                int IntPosition = (int)_Position;

                if (IsConcatStreams)
                {
                    ReadLength = Stream.Read(Buffers, Offset, Count);
                }
                else
                {
                    ReadLength = Math.Min(DatasLength - IntPosition, Count);
                    if (ReadLength > 0)
                        Buffer.BlockCopy(Datas, IntPosition, Buffers, Offset, ReadLength);
                }

                if (ReadLength < Count)
                {
                    IsBasePosition = false;
                    ReadLength += MergedStream.Read(Buffers, Offset + ReadLength, Count - ReadLength);
                }

                _Position += ReadLength;
                return ReadLength;
            }

            ReadLength = MergedStream.Read(Buffers, Offset, Count);

            _Position += ReadLength;
            return ReadLength;
        }

        public override void Write(byte[] Buffers, int Offset, int Count)
            => throw new NotSupportedException();

        public override long Seek(long Offset, SeekOrigin Origin)
        {
            if (Origin == SeekOrigin.Current)
                Offset += _Position;

            else if (Origin != SeekOrigin.Begin)
                throw new NotSupportedException();

            _Position = Offset;
            if (Offset < DatasLength)
            {
                IsBasePosition = true;

                if (MergedStream.CanSeek)
                    MergedStream.Position = 0;
            }
            else
            {
                IsBasePosition = false;
                Offset -= DatasLength;

                if (MergedStream.CanSeek)
                    MergedStream.Position = Offset;
            }

            return _Position;
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Flush()
        {
            if (IsDisposed)
                return;

            if (IsConcatStreams)
                Stream.Flush();

            MergedStream.Flush();
        }

        public override void Close()
        {
            if (IsDisposed)
                return;

            if (IsConcatStreams && !LeaveStreamOpen)
                Stream.Close();

            if (!LeaveMergedStreamOpen)
                MergedStream.Close();

            base.Close();
        }

        private bool IsDisposed = false;
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (IsConcatStreams)
            {
                if (!LeaveStreamOpen)
                    Stream.Dispose();
                Stream = null;
            }
            else
            {
                Datas = null;
            }

            if (!LeaveMergedStreamOpen)
                MergedStream.Dispose();
            MergedStream = null;

            base.Dispose(disposing);

            IsDisposed = true;
        }

    }
}
