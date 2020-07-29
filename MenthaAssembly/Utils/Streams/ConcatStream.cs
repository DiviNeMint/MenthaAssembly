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

        public override bool CanSeek => false;

        public override long Length
            => throw new NotSupportedException();

        private long _Position = 0;
        public override long Position
        {
            get => _Position;
            set => throw new NotSupportedException();
        }

        private byte[] Datas;
        private readonly int DatasLength;

        private readonly bool IsConcatStreams;
        private Stream BaseStream,
                       MergedStream;

        public ConcatStream(IEnumerable<byte> Datas, Stream Stream)
        {
            this.Datas = Datas.ToArray();
            this.DatasLength = this.Datas.Length;
            this.MergedStream = Stream;
        }
        public ConcatStream(byte[] Datas, int Offset, int Count, Stream Stream)
        {
            this.Datas = Datas.Skip(Offset)
                              .Take(Count)
                              .ToArray();
            this.DatasLength = Count;
            this.MergedStream = Stream;
        }
        public ConcatStream(Stream Stream, Stream MergedStream)
        {
            this.BaseStream = Stream;
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
            if (IsBasePosition)
            {
                int IntPosition = (int)_Position,
                    ReadLength;

                if (IsConcatStreams)
                {
                    ReadLength = BaseStream.Read(Buffers, Offset, Count);
                }
                else
                {
                    ReadLength = Math.Min(DatasLength - IntPosition, Count);
                    if (ReadLength > 0)
                    {
                        Buffer.BlockCopy(Datas, IntPosition, Buffers, Offset, ReadLength);
                        _Position += ReadLength;
                    }
                }
                IsBasePosition = ReadLength > 0;

                return ReadLength < Count ? ReadLength + MergedStream.Read(Buffers, Offset + ReadLength, Count - ReadLength) :
                                            ReadLength;
            }

            return MergedStream.Read(Buffers, Offset, Count);
        }

        public override void Write(byte[] Buffers, int Offset, int Count)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

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
