using System;
using System.Collections.Concurrent;
using System.IO;

namespace MenthaAssembly.IO
{
    public sealed class CRCStream : Stream
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

        public ulong Checksum
            => Calculator.Checksum;

        private readonly Stream Stream;
        private readonly bool LeaveOpen;
        private readonly CRCCalculator Calculator;
        public CRCStream(Stream Stream, StreamAccess Access, CRCAlgorithm Algorithm) :
            this(Stream, Access, false, CRCCalculator.Create(Algorithm))
        {
        }
        public CRCStream(Stream Stream, StreamAccess Access, bool LeaveOpen, CRCAlgorithm Algorithm) :
            this(Stream, Access, LeaveOpen, CRCCalculator.Create(Algorithm))
        {
        }
        public CRCStream(Stream Stream, StreamAccess Access, bool LeaveOpen, int CRCBitCount, ulong Polynomial, ulong Init, bool RefIn, bool RefOut, ulong XorOut) :
            this(Stream, Access, LeaveOpen, new CRCCalculator(CRCBitCount, Polynomial, Init, RefIn, RefOut, XorOut))
        {
        }
        private CRCStream(Stream Stream, StreamAccess Access, bool LeaveOpen, CRCCalculator Calculator)
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

            this.Calculator = Calculator;
        }

        public override int ReadByte()
        {
            ThrowIfDisposed();
            ThrowIfUnaccess(StreamAccess.Read);

            if (Stream.TryReadByte(out int Result))
                Calculator.Update((byte)Result);

            return Result;
        }

        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            ThrowIfDisposed();
            ThrowIfUnaccess(StreamAccess.Read);

            int Length = Stream.Read(Buffer, Offset, Count);
            Calculator.Update(Buffer, Offset, Count);
            return Length;
        }

        public override void WriteByte(byte Value)
        {
            ThrowIfDisposed();
            ThrowIfUnaccess(StreamAccess.Write);

            Calculator.Update(Value);
            Stream.WriteByte(Value);
        }

        public override void Write(byte[] Buffer, int Offset, int Count)
        {
            ThrowIfDisposed();
            ThrowIfUnaccess(StreamAccess.Write);

            Calculator.Update(Buffer, Offset, Count);
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

        public void ResetChecksum()
            => throw new NotSupportedException();

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CRC32Stream));
        }
        private void ThrowIfUnaccess(StreamAccess Access)
        {
            switch (Access)
            {
                case StreamAccess.Read when !CanRead:
                case StreamAccess.Write when !CanWrite:
                case StreamAccess.ReadWrite:
                    throw new NotSupportedException();
            }
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

        private class CRCCalculator
        {
            public int Width { get; }

            public ulong Polynomial { get; }

            public ulong Init { get; }

            public bool RefIn { get; }

            public bool RefOut { get; }

            public ulong XorOut { get; }

            private ulong _Checksum;
            public ulong Checksum
            {
                get
                {
                    ulong CRC = RefIn != RefOut ? Reflect(_Checksum, Width) : _Checksum;
                    return (CRC ^ XorOut) & Mask;
                }
            }

            private readonly ulong Mask;
            private readonly ulong[] Table;
            private readonly Action<byte> UpdateAction;
            public CRCCalculator(int Width, ulong Polynomial, ulong Init, bool RefIn, bool RefOut, ulong XorOut)
            {
                this.Width = Width;
                this.Polynomial = Polynomial;
                this.Init = Init;
                this.RefIn = RefIn;
                this.RefOut = RefOut;
                this.XorOut = XorOut;
                _Checksum = Init;
                Mask = GenerateMask(Width);
                Table = GenerateTable(Width, Polynomial, RefIn);
                UpdateAction = RefIn ? InternalRefInUpdate : InternalUpdate;
            }

            public void Update(byte Data)
                => UpdateAction(Data);
            public void Update(byte[] Datas, int Offset, int Length)
            {
                for (int i = 0; i < Length; i++)
                    UpdateAction(Datas[Offset + i]);
            }
            private void InternalUpdate(byte Data)
            {
                int Index = (int)(_Checksum & 0xFF) ^ Data;
                _Checksum = (_Checksum >> 8) ^ Table[Index];
                _Checksum &= Mask;
            }
            private void InternalRefInUpdate(byte Data)
            {
                int Index = (int)(_Checksum & 0xFF) ^ Data;
                _Checksum = (_Checksum >> 8) ^ Table[Index];
                _Checksum &= Mask;
            }

            public void Reset()
                => _Checksum = Init;

            private static readonly ConcurrentDictionary<(int Width, ulong Polynomial, bool RefIn), ulong[]> Tables = [];
            private static ulong[] GenerateTable(int Width, ulong Polynomial, bool RefIn)
            {
                if (Tables.TryGetValue((Width, Polynomial, RefIn), out ulong[] Table))
                    return Table;

                const int TableSize = 256;
                ulong Mask = GenerateMask(Width);
                Table = new ulong[TableSize];

                if (RefIn)
                {
                    Polynomial = Reflect(Polynomial, Width);
                    for (int i = 0; i < TableSize; i++)
                    {
                        ulong CRC = (ulong)i;
                        for (int j = 0; j < 8; j++)
                            CRC = (CRC & 1) > 0 ? (CRC >> 1) ^ Polynomial : CRC >> 1;

                        Table[i] = CRC & Mask;
                    }
                }
                else
                {
                    ulong TopBit = (ulong)1 << (Width - 1);
                    for (int i = 0; i < TableSize; i++)
                    {
                        ulong CRC = (ulong)i << (Width - 8);
                        for (int j = 0; j < 8; j++)
                            CRC = (CRC & TopBit) > 0 ? (CRC << 1) ^ Polynomial : CRC << 1;

                        Table[i] = CRC & Mask;
                    }
                }

                Tables.TryAdd((Width, Polynomial, RefIn), Table);
                return Table;
            }

            private static readonly ConcurrentDictionary<int, ulong> Masks = [];
            private static ulong GenerateMask(int Width)
            {
                if (Masks.TryGetValue(Width, out ulong Mask))
                    return Mask;

                Mask = 1;
                for (int i = 1; i < Width; i++)
                    Mask |= (ulong)1 << i;

                Masks.TryAdd(Width, Mask);
                return Mask;
            }

            private static ulong Reflect(ulong data, int bitCount)
            {
                ulong reflection = 0;
                for (int i = 0; i < bitCount; i++)
                {
                    if ((data & 1) != 0)
                        reflection |= (ulong)1 << (bitCount - 1 - i);

                    data >>= 1;
                }
                return reflection;
            }

            public static CRCCalculator Create(CRCAlgorithm Algorithm)
                => Algorithm switch
                {
                    CRCAlgorithm.CRC8 => new CRCCalculator(8, 0x07, 0x00, false, false, 0x00),
                    CRCAlgorithm.CRC8Ccitt => new CRCCalculator(8, 0x07, 0x00, false, false, 0x55),
                    CRCAlgorithm.CRC8DallasMaxim => new CRCCalculator(8, 0x31, 0x00, true, true, 0x00),
                    CRCAlgorithm.CRC16Ibm => new CRCCalculator(16, 0x8005, 0xFFFF, true, true, 0x0000),
                    CRCAlgorithm.CRC16CcittFalse => new CRCCalculator(16, 0x1021, 0xFFFF, false, false, 0x0000),
                    CRCAlgorithm.CRC16Xmodem => new CRCCalculator(16, 0x1021, 0x0000, false, false, 0x0000),
                    CRCAlgorithm.CRC16Arc => new CRCCalculator(16, 0x8005, 0x0000, true, true, 0x0000),
                    CRCAlgorithm.CRC16Usb => new CRCCalculator(16, 0x8005, 0xFFFF, true, true, 0xFFFF),
                    CRCAlgorithm.CRC32 => new CRCCalculator(32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0xFFFFFFFF),
                    CRCAlgorithm.CRC32C => new CRCCalculator(32, 0x1EDC6F41, 0xFFFFFFFF, true, true, 0xFFFFFFFF),
                    CRCAlgorithm.CRC32K => new CRCCalculator(32, 0x741B8CD7, 0xFFFFFFFF, true, true, 0xFFFFFFFF),
                    CRCAlgorithm.CRC64Ecma => new CRCCalculator(64, 0x42F0E1EBA9EA3693, 0x0000000000000000, false, false, 0x0000000000000000),
                    CRCAlgorithm.CRC64Iso => new CRCCalculator(64, 0x000000000000001B, 0xFFFFFFFFFFFFFFFF, true, true, 0xFFFFFFFFFFFFFFFF),
                    _ => throw new NotSupportedException($"The specified CRC algorithm '{Algorithm}' is not supported."),
                };

        }

    }
}