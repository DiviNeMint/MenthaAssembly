using System.Buffers;
using System.IO;
using System.Text;

namespace MenthaAssembly
{
    public class MenthaFileCodec(Stream Stream)
    {
        private readonly Stream Stream = Stream;

        public void Write<T>(T Value) where T : unmanaged
            => Stream.Write(Value);
        public void Write(string Value)
            => Write(Value, Encoding.Unicode);
        public void Write(string Value, Encoding Encoding)
        {
            int Length = Encoding.GetByteCount(Value);
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Length);
            try
            {
                Length = Encoding.GetBytes(Value, 0, Value.Length, Buffer, 0);

                Stream.Write(Length);
                Stream.Write(Buffer, 0, Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }

        public void Read<T>() where T : unmanaged
            => Stream.Read<T>();
        public string Read()
            => Read(Encoding.Unicode);
        public string Read(Encoding Encoding)
        {
            int Length = Stream.Read<int>();
            return Stream.ReadString(Length, Encoding);
        }

    }
}