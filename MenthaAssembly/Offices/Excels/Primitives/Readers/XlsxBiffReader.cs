using System.Buffers;
using System.IO;

namespace MenthaAssembly.Offices.Primitives
{
    internal class XlsxBiffReader : BiffReader
    {
        public XlsxBiffReader(Stream Stream) : base(Stream)
        {
            ByteBuffer = ArrayPool<byte>.Shared.Rent(1);
        }

        private byte[] ByteBuffer;
        public override bool ReadVariable(out int ID, out int Length)
        {
            Length = 0;
            if (!SkipVariable())
            {
                ID = -1;
                return false;
            }

            ID = 0;
            for (int i = 0; i < 4; i++)
            {
                if (Stream.Read(ByteBuffer, 0, 1) == 0)
                {
                    ID = -1;
                    return false;
                }

                byte Data = ByteBuffer[0];
                ID |= (Data & 0x7F) << (7 * i);

                if ((Data & 0x80) == 0)
                    break;
            }

            Length = 0;
            for (int i = 0; i < 4; i++)
            {
                if (Stream.Read(ByteBuffer, 0, 1) == 0)
                {
                    ID = -1;
                    return false;
                }

                byte Data = ByteBuffer[0];
                Length |= (Data & 0x7F) << (7 * i);

                if ((Data & 0x80) == 0)
                    break;
            }

            VariableLength = Length;
            return true;
        }

        protected override bool ReadVariableContext(byte[] Buffer, int Length)
            => Stream.ReadBuffer(Buffer, Length);

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                ArrayPool<byte>.Shared.Return(ByteBuffer);
                ByteBuffer = null;

                base.Dispose();
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
