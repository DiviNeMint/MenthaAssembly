using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Offices.Primitives
{
    internal abstract unsafe class BiffReader : IDisposable
    {
        private readonly bool LeaveOpen;
        protected Stream Stream;
        private byte[] Buffer;
        protected BiffReader(Stream Stream, bool LeaveOpen = false)
        {
            if (Stream is null)
                throw new ArgumentNullException();

            this.Stream = Stream;
            Buffer = ArrayPool<byte>.Shared.Rent(512);
            this.LeaveOpen = LeaveOpen;
        }

        protected int VariableLength = 0,
                      VariableOffset = 0;
        public abstract bool ReadVariable(out int ID, out int Length);

        protected abstract bool ReadVariableContext(byte[] Buffer, int Length);

        public T Read<T>()
            where T : unmanaged
        {
            int Size = sizeof(T);

            int NextOffset = VariableOffset + Size;
            if (NextOffset > VariableLength)
                throw new OutOfMemoryException();

            if (Buffer.Length < Size)
                ResizeBuffer(Size);

            if (!ReadVariableContext(Buffer, Size))
                throw new EndOfStreamException();

            VariableOffset = NextOffset;

            return *(T*)Buffer.ToPointer();
        }
        public bool TryRead<T>(out T Value)
            where T : unmanaged
        {
            int Size = sizeof(T);

            int NextOffset = VariableOffset + Size;
            if (NextOffset > VariableLength)
            {
                Value = default;
                return false;
            }

            if (Buffer.Length < Size)
                ResizeBuffer(Size);

            if (!ReadVariableContext(Buffer, Size))
            {
                Value = default;
                return false;
            }

            VariableOffset = NextOffset;
            Value = *(T*)Buffer.ToPointer();
            return true;
        }

        public object ReadRkNumber()
        {
            int Data = Read<int>();

            bool fx100 = (Data & 0b01) != 0,
                 fInt = (Data & 0b10) != 0;

            Data >>= 2;
            if (fInt)
                return fx100 ? Data / 100 : Data;

            double FloatValue = BitConverter.Int64BitsToDouble(((long)Data) << 34);
            return fx100 ? FloatValue / 100d : FloatValue;
        }

        public byte[] ReadBuffer(int Length)
        {
            int NextOffset = VariableOffset + Length;
            if (NextOffset > VariableLength)
                throw new OutOfMemoryException();

            byte[] Buffer = new byte[Length];
            if (!ReadVariableContext(Buffer, Length))
                throw new EndOfStreamException();

            VariableOffset = NextOffset;
            return Buffer;
        }

        protected StringBuilder Builder = new StringBuilder();
        public string ReadString()
        {
            uint Length = Read<uint>();
            if (Length == uint.MaxValue)
                return null;

            int ByteLength = (int)(Length << 1);
            try
            {
                int NextOffset = VariableOffset + ByteLength;
                if (NextOffset > VariableLength)
                    throw new OutOfMemoryException();

                if (Buffer.Length < ByteLength)
                    ResizeBuffer(ByteLength);

                if (!ReadVariableContext(Buffer, ByteLength))
                    throw new EndOfStreamException();

                ushort* pUshort = (ushort*)Buffer.ToPointer();
                for (uint i = 0; i < Length; i++)
                    Builder.Append((char)*pUshort++);

                VariableOffset = NextOffset;
                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        public bool SkipVariable()
        {
            try
            {
                if (VariableOffset < VariableLength &&
                    !SkipVariableContext(VariableLength - VariableOffset))
                    return false;

                return true;
            }
            finally
            {
                VariableLength = 0;
                VariableOffset = 0;
            }
        }
        public void Skip(int Length)
        {
            int NextOffset = VariableOffset + Length;
            if (NextOffset > VariableLength)
                throw new OutOfMemoryException();

            if (!SkipVariableContext(Length))
                throw new EndOfStreamException();

            VariableOffset = NextOffset;
        }
        public bool TrySkip(int Length)
        {
            int NextOffset = VariableOffset + Length;
            if (NextOffset > VariableLength)
                return false;

            if (!SkipVariableContext(Length))
                return false;

            VariableOffset = NextOffset;
            return true;
        }

        protected virtual bool SkipVariableContext(int Length)
        {
            if (Stream.CanSeek)
            {
                Stream.Seek(Length, SeekOrigin.Current);
                return true;
            }

            if (Buffer.Length < Length)
                ResizeBuffer(Length);

            return ReadVariableContext(Buffer, Length);
        }

        private void ResizeBuffer(int Length)
        {
            ArrayPool<byte>.Shared.Return(Buffer);
            Buffer = ArrayPool<byte>.Shared.Rent(Length);
        }

        private bool IsDisposed = false;
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            if (!LeaveOpen)
                Stream.Dispose();
            Stream = null;

            Builder = null;

            ArrayPool<byte>.Shared.Return(Buffer);
            Buffer = null;

            IsDisposed = true;
        }

    }
}
