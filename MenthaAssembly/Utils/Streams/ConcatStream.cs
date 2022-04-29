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

        private readonly bool IsConcatStreams;
        private Stream BaseStream,
                       MergedStream;

        public ConcatStream(IEnumerable<byte> Datas, Stream Stream)
        {
            this.Datas = Datas.ToArray();
            DatasLength = this.Datas.Length;
            MergedStream = Stream;
        }
        public ConcatStream(byte[] Datas, int Offset, int Count, Stream Stream)
        {
            this.Datas = Datas.Skip(Offset)
                              .Take(Count)
                              .ToArray();
            DatasLength = Count;
            MergedStream = Stream;
        }
        public ConcatStream(Stream Stream, Stream MergedStream)
        {
            BaseStream = Stream;
            this.MergedStream = MergedStream;
            IsConcatStreams = true;
        }

        public override void Flush()
        {
            BaseStream?.Flush();
            MergedStream?.Flush();
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
                    ReadLength = BaseStream.Read(Buffers, Offset, Count);
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

        public override long Seek(long Offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Current)
                Offset += _Position;

            else if (origin != SeekOrigin.Begin)
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

        public override void Close()
        {
            BaseStream?.Close();
            MergedStream?.Dispose();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            BaseStream?.Dispose();
            BaseStream = null;

            MergedStream?.Dispose();
            MergedStream = null;

            Datas = null;
            base.Dispose(disposing);
        }

    }
}
