using System;
using System.Buffers;
using System.IO;
using System.Linq;

namespace MenthaAssembly.IO
{
    public class HuffmanStream : Stream
    {
        private const int BufferSize = 8192;

        public override bool CanRead { get; }

        public override bool CanWrite { get; }

        public override bool CanSeek
            => false;

        public override long Length
            => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        private byte[] CoderBuffer;
        private readonly Stream Stream;
        private readonly bool LeaveOpen;
        public HuffmanStream(Stream Stream, HuffmanDecodeTable Table) : this(Stream, Table, false)
        {

        }
        public HuffmanStream(Stream Stream, HuffmanDecodeTable Table, bool LeaveOpen)
        {
            CanRead = true;

            DecodeTable = Table;

            ReadBitIndex = 0;
            ReadValue = 0;
            MaxBits = Table.Bits.Max();
            CoderBufferLength = CoderBufferIndex = BufferSize;
            CoderBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            this.Stream = Stream;
            this.LeaveOpen = LeaveOpen;
        }

        private byte[] LastData;
        private int LastIndex;
        private readonly int MaxBits;
        private readonly HuffmanDecodeTable DecodeTable;
        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            CheckDispose();

            if (!CanRead)
                throw new NotImplementedException();

            int Read = 0,
                Index = Offset,
                Bit = 0,
                Code = 0;

            if (LastData != null)
            {
                int DataLength = LastData.Length - LastIndex;
                if (DataLength < Count)
                {
                    Array.Copy(LastData, LastIndex, Buffer, Index, DataLength);
                    Index += DataLength;
                    Read = DataLength;
                    LastData = null;
                }
                else
                {
                    LastIndex += Count;
                    Array.Copy(LastData, LastIndex, Buffer, Index, Count);
                    return Read;
                }
            }

            while (TryReadBit(out int BitCode))
            {
                Bit++;
                if (MaxBits < Bit)
                    throw new IOException("Invalid decodetable.");

                Code |= BitCode;

                if (DecodeTable[Bit, Code] is byte[] Data)
                {
                    Bit = 0;
                    Code = 0;

                    int DataLength = Data.Length,
                        NewRead = Read + DataLength;
                    if (NewRead < Count)
                    {
                        Array.Copy(Data, 0, Buffer, Index, DataLength);
                        Index += DataLength;
                        Read = NewRead;
                        continue;
                    }
                    else
                    {
                        LastIndex = Count - Read;
                        LastData = Data;
                        Array.Copy(Data, 0, Buffer, Index, LastIndex);
                        return Count;
                    }
                }

                Code <<= 1;
            }

            return Read;
        }

        private int CoderBufferLength, CoderBufferIndex, ReadValue, ReadBitIndex = 8;
        private bool TryReadBit(out int Bit)
        {
            if (ReadBitIndex == 8)
            {
                if (CoderBufferIndex < CoderBufferLength)
                {
                    ReadValue = CoderBuffer[CoderBufferIndex++];
                    ReadBitIndex = 0;
                }
                else
                {
                    CoderBufferLength = Stream.Read(CoderBuffer, 0, BufferSize);
                    if (CoderBufferLength == 0)
                    {
                        Bit = 0;
                        return false;
                    }

                    ReadBitIndex = 0;
                    CoderBufferIndex = 1;
                    ReadValue = CoderBuffer[0];
                }
            }

            Bit = (ReadValue >> (7 - ReadBitIndex)) & 1;
            ReadBitIndex++;
            return true;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDispose();

            if (!CanWrite)
                throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Flush()
        {
            if (!CanWrite)
                throw new NotSupportedException();

        }

        public override void Close()
        {
            if (IsDisposed)
                return;

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
                ArrayPool<byte>.Shared.Return(CoderBuffer);

                if (!LeaveOpen)
                    Stream.Dispose();

                base.Dispose(disposing);
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}