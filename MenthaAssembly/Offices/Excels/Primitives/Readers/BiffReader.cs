using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Offices.Primitives
{
    public abstract unsafe class BiffReader : IDisposable
    {
        private readonly bool LeaveOpen;
        protected Stream Stream;
        protected byte[] Buffer;
        protected byte* pBuffer;
        protected BiffReader(Stream Stream, bool LeaveOpen = false)
        {
            if (Stream is null)
                throw new ArgumentNullException();

            this.Stream = Stream;
            Buffer = ArrayPool<byte>.Shared.Rent(512);
            pBuffer = Buffer.ToPointer();
            this.LeaveOpen = LeaveOpen;
        }

        protected int VariableLength = 0,
                      VariableOffset = 0;
        public abstract bool ReadVariable(out int ID);

        public T Read<T>()
            where T : unmanaged
        {
            int Size = sizeof(T);

            int NextOffset = VariableOffset + Size;
            if (NextOffset > VariableLength)
                throw new OutOfMemoryException();

            if (Buffer.Length < Size)
                ResizeBuffer(Size);

            if (!Stream.ReadBuffer(Buffer, Size))
                throw new EndOfStreamException();

            VariableOffset = NextOffset;
            return *(T*)pBuffer;
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

                if (!Stream.ReadBuffer(Buffer, ByteLength))
                    throw new EndOfStreamException();

                ushort* pUshort = (ushort*)pBuffer;
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
                    !Stream.ReadBuffer(Buffer, VariableLength - VariableOffset))
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

            if (Buffer.Length < Length)
                ResizeBuffer(Length);

            if (!Stream.ReadBuffer(Buffer, Length))
                throw new EndOfStreamException();

            VariableOffset = NextOffset;
        }

        protected void ResizeBuffer(int Length)
        {
            ArrayPool<byte>.Shared.Return(Buffer);
            Buffer = ArrayPool<byte>.Shared.Rent(Length);
            pBuffer = Buffer.ToPointer();
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
