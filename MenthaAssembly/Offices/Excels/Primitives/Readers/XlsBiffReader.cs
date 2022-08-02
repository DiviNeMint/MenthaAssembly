using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;

namespace MenthaAssembly.Offices.Primitives
{
    internal unsafe class XlsBiffReader : BiffReader
    {
        public byte[] SecretKey { set; get; }

        private SymmetricAlgorithm Cipher = null;
        private ICryptoTransform CipherTransform = null;
        private int CipherBlockNumber = -1;

        private Encryption _Encryption;
        public Encryption Encryption
        {
            get => _Encryption;
            set
            {
                CipherBlockNumber = -1;
                CipherTransform?.Dispose();
                Cipher?.Dispose();
                _Encryption = value;
            }
        }

        public XlsBiffReader(Stream Stream, bool LeaveOpen) : base(Stream, LeaveOpen)
        {
        }
        public XlsBiffReader(Stream Stream, Encryption Encryption, byte[] SecretKey, bool LeaveOpen) : base(Stream, LeaveOpen)
        {
            _Encryption = Encryption;
            this.SecretKey = SecretKey;
        }

        public override bool ReadVariable(out int ID, out int Length)
        {
            if (!SkipVariable())
            {
                ID = -1;
                Length = 0;
                return false;
            }

            return DecryptVariableContext(out ID, out Length);
        }

        private byte[] DecryptoBuffer;
        protected override bool ReadVariableContext(byte[] Buffer, int Length)
        {
            if (DecryptoBuffer is null)
                return Stream.ReadBuffer(Buffer, Length);

            Array.Copy(DecryptoBuffer, VariableOffset, Buffer, 0, Length);
            return true;
        }

        protected override bool SkipVariableContext(int Length)
            => DecryptoBuffer is not null || base.SkipVariableContext(Length);

        private bool DecryptVariableContext(out int ID, out int Length)
        {
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(4);
            try
            {
                // Read Variable Header
                if (!Stream.ReadBuffer(Buffer, 4))
                {
                    ID = -1;
                    Length = 0;
                    return false;
                }

                ID = BitConverter.ToUInt16(Buffer, 0);
                Length = BitConverter.ToUInt16(Buffer, 2);

                // Check Decrypt
                if (SecretKey is null ||
                    Encryption is null)
                {
                    if (DecryptoBuffer != null)
                    {
                        ArrayPool<byte>.Shared.Return(DecryptoBuffer);
                        DecryptoBuffer = null;
                    }

                    VariableOffset = 0;
                    VariableLength = Length;
                    return true;
                }

                VariableOffset = 4;
                VariableLength = Length + 4;

                // Set Buffer
                if (DecryptoBuffer is null)
                {
                    DecryptoBuffer = ArrayPool<byte>.Shared.Rent(VariableLength);
                }
                else if (DecryptoBuffer.Length < VariableLength)
                {
                    ArrayPool<byte>.Shared.Return(DecryptoBuffer);
                    DecryptoBuffer = ArrayPool<byte>.Shared.Rent(VariableLength);
                }

                // Read CryptoDatas
                long StartPosition = Stream.Position - 4;
                if (!Stream.ReadBuffer(DecryptoBuffer, 4, Length))
                    throw new EndOfStreamException();

                Array.Copy(Buffer, 0, DecryptoBuffer, 0, 4);

                // Decrypto
                int Index = ID switch
                {
                    0x0809 => VariableLength,   // Bof
                    0x00E1 => VariableLength,   // InterFaceHDR
                    0x002F => VariableLength,   // FilePass
                    0x0085 => 8,                // BoundSheet
                    _ => 4,                     // Skip ID & VariableLength
                };

                Decrypt((int)StartPosition, DecryptoBuffer, Index, VariableLength);
                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }

        private void Decrypt(int StreamPosition, byte[] Datas, int Offset, int Length)
        {
            int Position = 0,
                BlockNumber = StreamPosition >> 10,
                BlockOffset = StreamPosition & 1023;
            while (Position < Length)
            {
                if (BlockNumber != CipherBlockNumber)
                {
                    byte[] BlockKey = Encryption.GenerateBlockKey(BlockNumber, SecretKey);
                    CipherBlockNumber = BlockNumber;

                    if (Cipher is null)
                        Cipher = Encryption.CreateCipher();

                    if (CipherTransform is null)
                    {
                        CipherTransform = Cipher.CreateDecryptor(BlockKey, null);
                        byte[] OffsetBlock = new byte[BlockOffset];
                        Encryption.Decrypt(CipherTransform, OffsetBlock);
                    }
                    else
                    {
                        CipherTransform?.Dispose();
                        CipherTransform = Cipher.CreateDecryptor(BlockKey, null);
                    }
                }

                // Bypass everything and hook into the XorTransform instance to set the XorArrayIndex pr record.
                // This is a hack to use the XorTransform otherwise transparently to the other encryption methods.
                if (CipherTransform is XorEncryption.XorTransform XorTransform)
                    XorTransform.XorArrayIndex = StreamPosition + Length - 4;

                // Decrypt at most up to the next 1024 byte boundary
                int ChunkSize = Math.Min(Length - Position, 1024 - BlockOffset);
                byte[] Block = new byte[ChunkSize];

                Array.Copy(Datas, Position, Block, 0, ChunkSize);

                byte[] Decrypted = Encryption.Decrypt(CipherTransform, Block);
                if (Offset < Length)
                {
                    int DestIndex = Math.Max(Position, Offset),
                        SourIndex = DestIndex - Position;

                    if (SourIndex < ChunkSize)
                        Array.Copy(Decrypted, SourIndex, Datas, DestIndex, ChunkSize - SourIndex);
                }

                Position += ChunkSize;
                StreamPosition += ChunkSize;
                BlockOffset += ChunkSize;
                if (BlockOffset >= 1024)
                {
                    BlockOffset &= 1023;
                    BlockNumber++;
                }
            }
        }

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            Encryption = null;
            CipherTransform = null;
            Cipher = null;

            base.Dispose();
        }

    }

}