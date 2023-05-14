using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MenthaAssembly.Utils
{
    public sealed class ConcatStream : Stream
    {
        public override bool CanRead
            => true;

        public override bool CanWrite
            => false;

        public override bool CanSeek
            => Stream2.CanSeek;

        public override long Length
            => CanSeek ? ConcatLength : throw new NotSupportedException();

        private long Current = 0L;
        public override long Position
        {
            get => Current;
            set => Seek(value, SeekOrigin.Begin);
        }

        private readonly byte[] Datas;
        private readonly bool LeaveOpen1, LeaveOpen2;
        private readonly bool IsConcatStreams;
        private readonly long Begin1, Begin2, Length1, ConcatLength;
        private readonly Stream Stream1, Stream2;
        public ConcatStream(IEnumerable<byte> Datas, Stream Stream) : this(Datas, Stream, false)
        {
        }
        public ConcatStream(IEnumerable<byte> Datas, Stream Stream, bool LeaveOpen)
        {
            if (!Stream.CanRead)
                throw new NotSupportedException();

            this.Datas = Datas.ToArray();
            Stream2 = Stream;
            LeaveOpen2 = LeaveOpen;

            Length1 = this.Datas.Length;
            if (Stream.CanSeek)
            {
                Begin2 = Stream.Position;
                ConcatLength = Length1 + Stream2.Length - Begin2;
            }
        }
        public ConcatStream(byte[] Datas, int Offset, int Length, Stream Stream) : this(Datas, Offset, Length, Stream, false)
        {
        }
        public ConcatStream(byte[] Datas, int Offset, int Length, Stream Stream, bool LeaveOpen)
        {
            if (!Stream.CanRead)
                throw new NotSupportedException();

            this.Datas = Datas;
            Stream2 = Stream;
            LeaveOpen2 = LeaveOpen;

            Begin1 = Offset;
            Length1 = Length;
            if (Stream.CanSeek)
            {
                Begin2 = Stream.Position;
                ConcatLength = Length1 + Stream2.Length - Begin2;
            }
        }
        public ConcatStream(Stream Stream1, Stream Stream2) : this(Stream1, false, Stream2, false)
        {

        }
        public ConcatStream(Stream Stream1, bool LeaveStreamOpen1, Stream Stream2, bool LeaveStreamOpen2)
        {
            if (!Stream1.CanSeek ||
                !Stream1.CanRead ||
                !Stream2.CanRead)
                throw new NotSupportedException();

            this.Stream1 = Stream1;
            this.Stream2 = Stream2;
            LeaveOpen1 = LeaveStreamOpen1;
            LeaveOpen2 = LeaveStreamOpen2;
            IsConcatStreams = true;

            Begin1 = Stream1.Position;
            Length1 = Stream1.Length - Begin1;
            if (Stream2.CanSeek)
            {
                Begin2 = Stream2.Position;
                ConcatLength = Length1 + Stream2.Length - Begin2;
            }
        }

        private bool IsBasePosition = true;
        public override int Read(byte[] Buffers, int Offset, int Count)
        {
            CheckDispose();

            int ReadLength;
            if (IsBasePosition)
            {
                if (IsConcatStreams)
                {
                    ReadLength = Stream1.Read(Buffers, Offset, Count);
                }
                else
                {
                    ReadLength = Math.Min((int)(Length1 - Current), Count);
                    if (ReadLength > 0)
                        Buffer.BlockCopy(Datas, (int)Current, Buffers, Offset, ReadLength);
                }

                if (ReadLength < Count)
                {
                    IsBasePosition = false;
                    ReadLength += Stream2.Read(Buffers, Offset + ReadLength, Count - ReadLength);
                }

                Current += ReadLength;
                return ReadLength;
            }

            ReadLength = Stream2.Read(Buffers, Offset, Count);

            Current += ReadLength;
            return ReadLength;
        }

        public override void Write(byte[] Buffers, int Offset, int Count)
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
                SeekOrigin.End => Stream2.Length - Begin2 - Offset,
                _ => throw new NotSupportedException()
            };

            if (Length1 < Position)
            {
                Stream2.Seek(Begin2 + Position - Length1, SeekOrigin.Begin);
            }
            else
            {
                if (IsConcatStreams)
                    Stream1.Seek(Begin1 + Position, SeekOrigin.Begin);

                if (!IsBasePosition)
                {
                    IsBasePosition = true;
                    Stream2.Seek(Begin2, SeekOrigin.Begin);
                }
            }

            Current = Position;
            return Current;
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Flush()
            => throw new NotSupportedException();

        public override void Close()
        {
            if (IsDisposed)
                return;

            if (IsConcatStreams && !LeaveOpen1)
                Stream1.Close();

            if (!LeaveOpen2)
                Stream2.Close();

            base.Close();
        }

        private void CheckDispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ConcatStream));
        }

        private bool IsDisposed = false;
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                if (IsConcatStreams && !LeaveOpen1)
                    Stream1.Dispose();

                if (!LeaveOpen2)
                    Stream2.Dispose();

                base.Dispose(disposing);
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }

    //public class ConcatStream : Stream
    //{
    //    public override bool CanRead => true;

    //    public override bool CanWrite => false;

    //    public override bool CanSeek => true;

    //    public override long Length
    //        => throw new NotSupportedException();

    //    private long _Position = 0;
    //    public override long Position
    //    {
    //        get => _Position;
    //        set => Seek(value, SeekOrigin.Begin);
    //    }

    //    private byte[] Datas;
    //    private readonly int DatasLength;

    //    private readonly bool LeaveStreamOpen;
    //    private readonly bool LeaveMergedStreamOpen;
    //    private readonly bool IsConcatStreams;
    //    private Stream Stream,
    //                   MergedStream;
    //    public ConcatStream(IEnumerable<byte> Datas, Stream Stream) : this(Datas, Stream, false)
    //    {
    //    }
    //    public ConcatStream(IEnumerable<byte> Datas, Stream Stream, bool LeaveOpen)
    //    {
    //        this.Datas = Datas.ToArray();
    //        DatasLength = this.Datas.Length;
    //        MergedStream = Stream;
    //        LeaveMergedStreamOpen = LeaveOpen;
    //    }
    //    public ConcatStream(byte[] Datas, int Offset, int Length, Stream Stream) : this(Datas, Offset, Length, Stream, false)
    //    {
    //    }
    //    public ConcatStream(byte[] Datas, int Offset, int Length, Stream Stream, bool LeaveOpen)
    //    {
    //        this.Datas = Datas.Skip(Offset)
    //                          .Take(Length)
    //                          .ToArray();
    //        DatasLength = Length;
    //        MergedStream = Stream;
    //        LeaveMergedStreamOpen = LeaveOpen;
    //    }
    //    public ConcatStream(Stream Stream, Stream MergedStream) : this(Stream, false, MergedStream, false)
    //    {

    //    }
    //    public ConcatStream(Stream Stream, bool LeaveStreamOpen, Stream MergedStream, bool LeaveMergedStreamOpen)
    //    {
    //        this.Stream = Stream;
    //        this.LeaveStreamOpen = LeaveStreamOpen;
    //        this.MergedStream = MergedStream;
    //        this.LeaveMergedStreamOpen = LeaveMergedStreamOpen;
    //        IsConcatStreams = true;
    //    }

    //    private bool IsBasePosition = true;
    //    public override int Read(byte[] Buffers, int Offset, int Count)
    //    {
    //        int ReadLength;
    //        if (IsBasePosition)
    //        {
    //            int IntPosition = (int)_Position;

    //            if (IsConcatStreams)
    //            {
    //                ReadLength = Stream.Read(Buffers, Offset, Count);
    //            }
    //            else
    //            {
    //                ReadLength = Math.Min(DatasLength - IntPosition, Count);
    //                if (ReadLength > 0)
    //                    Buffer.BlockCopy(Datas, IntPosition, Buffers, Offset, ReadLength);
    //            }

    //            if (ReadLength < Count)
    //            {
    //                IsBasePosition = false;
    //                ReadLength += MergedStream.Read(Buffers, Offset + ReadLength, Count - ReadLength);
    //            }

    //            _Position += ReadLength;
    //            return ReadLength;
    //        }

    //        ReadLength = MergedStream.Read(Buffers, Offset, Count);

    //        _Position += ReadLength;
    //        return ReadLength;
    //    }

    //    public override void Write(byte[] Buffers, int Offset, int Count)
    //        => throw new NotSupportedException();

    //    public override long Seek(long Offset, SeekOrigin Origin)
    //    {
    //        if (Origin == SeekOrigin.Current)
    //            Offset += _Position;

    //        else if (Origin != SeekOrigin.Begin)
    //            throw new NotSupportedException();

    //        _Position = Offset;
    //        if (Offset < DatasLength)
    //        {
    //            IsBasePosition = true;

    //            if (MergedStream.CanSeek)
    //                MergedStream.Position = 0;
    //        }
    //        else
    //        {
    //            IsBasePosition = false;
    //            Offset -= DatasLength;

    //            if (MergedStream.CanSeek)
    //                MergedStream.Position = Offset;
    //        }

    //        return _Position;
    //    }

    //    public override void SetLength(long value)
    //        => throw new NotSupportedException();

    //    public override void Flush()
    //    {
    //        if (IsDisposed)
    //            return;

    //        if (IsConcatStreams)
    //            Stream.Flush();

    //        MergedStream.Flush();
    //    }

    //    public override void Close()
    //    {
    //        if (IsDisposed)
    //            return;

    //        if (IsConcatStreams && !LeaveStreamOpen)
    //            Stream.Close();

    //        if (!LeaveMergedStreamOpen)
    //            MergedStream.Close();

    //        base.Close();
    //    }

    //    private bool IsDisposed = false;
    //    protected override void Dispose(bool disposing)
    //    {
    //        if (IsDisposed)
    //            return;

    //        if (IsConcatStreams)
    //        {
    //            if (!LeaveStreamOpen)
    //                Stream.Dispose();
    //            Stream = null;
    //        }
    //        else
    //        {
    //            Datas = null;
    //        }

    //        if (!LeaveMergedStreamOpen)
    //            MergedStream.Dispose();
    //        MergedStream = null;

    //        base.Dispose(disposing);

    //        IsDisposed = true;
    //    }

    //}

}