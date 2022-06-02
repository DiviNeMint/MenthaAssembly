using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static unsafe class StreamHelper
    {
        /// <summary>
        /// Writes a data of special type to the stream.
        /// </summary>
        /// <typeparam name="T">The special type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Data">The special type of data to write to the stream.</param>
        public static void Write<T>(this Stream This, T Data)
            where T : unmanaged
        {
            int Size = sizeof(T);
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);
            try
            {
                *(T*)Buffer.ToPointer() = Data;
                This.Write(Buffer, 0, Size);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }
        /// <summary>
        /// Writes an array of special type to the stream.
        /// </summary>
        /// <typeparam name="T">The special type of array.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Datas">The array of special type to write to the stream.</param>
        public static void Write<T>(this Stream This, T[] Datas)
            where T : unmanaged
        {
            int DataLength = Datas.Length,
                Size = sizeof(T) * DataLength;
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);

            try
            {
                T* pBuffer = (T*)Buffer.ToPointer();
                for (int i = 0; i < DataLength; i++)
                    *pBuffer++ = Datas[i];

                This.Write(Buffer, 0, Size);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }
        /// <summary>
        /// Writes datas from a pointer of special type to the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of pointer.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="pBuffer">The pointer of special type. This method copies datas from the pointer to the current stream.</param>
        /// <param name="Length">The number of bytes to be written to the current stream.</param>
        public static void Write<T>(this Stream This, T* pBuffer, int Length)
            where T : unmanaged
        {
            int ByteLength = sizeof(T) * Length;
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(ByteLength);
            try
            {
                Marshal.Copy((IntPtr)pBuffer, Buffer, 0, ByteLength);
                This.Write(Buffer, 0, ByteLength);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }

        public static Task WriteAsync<T>(this Stream This, T Datas)
            where T : unmanaged
        {
            byte[] Buffer = new byte[sizeof(T)];
            *(T*)Buffer.ToPointer() = Datas;

            return This.WriteAsync(Buffer, 0, Buffer.Length);
        }
        public static Task WriteAsync<T>(this Stream This, T[] Datas)
            where T : unmanaged
        {
            int DataLength = Datas.Length;
            byte[] Buffer = new byte[sizeof(T) * DataLength];
            T* pBuffer = (T*)Buffer.ToPointer();

            for (int i = 0; i < DataLength; i++)
                *pBuffer++ = Datas[i];

            return This.WriteAsync(Buffer, 0, Buffer.Length);
        }

        /// <summary>
        /// Reads a data of special type from the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        public static T Read<T>(this Stream This)
            where T : unmanaged
        {
            int Size = sizeof(T);
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);
            try
            {
                This.ReadBuffer(Buffer, Size);
                return *(T*)Buffer.ToPointer();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }

        /// <summary>
        /// Reads string of specified encoding from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="BytesLength">The byte length of string.</param>
        public static string ReadString(this Stream This, int BytesLength)
            => ReadString(This, BytesLength, Encoding.Default);
        /// <summary>
        /// Reads string of specified encoding from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="BytesLength">The byte length of string.</param>
        /// <param name="Encoding">The specified encoding of string.</param>
        public static string ReadString(this Stream This, int BytesLength, Encoding Encoding)
        {
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(BytesLength);
            try
            {
                This.ReadBuffer(Buffer, BytesLength);
                return Encoding.GetString(Buffer, 0, BytesLength);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }

        /// <summary>
        /// Reads a buffer from the stream until it fills the buffer.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Buffer">The buffer to be filled by the stream.</param>
        public static void ReadBuffer(this Stream This, byte[] Buffer)
            => ReadBuffer(This, Buffer, Buffer.Length);
        /// <summary>
        /// Reads a buffer from the stream until it fills the specified length of buffer.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Buffer">The buffer to be filled by the stream.</param>
        /// <param name="Length">The specified length of buffer to be filled.</param>
        public static void ReadBuffer(this Stream This, byte[] Buffer, int Length)
        {
            int Offset = This.Read(Buffer, 0, Length),
                ReadLength;

            Length -= Offset;
            while (Length > 0)
            {
                ReadLength = This.Read(Buffer, Offset, Length);

                if (ReadLength < 1)
                    throw new EndOfStreamException();

                Offset += ReadLength;
                Length -= ReadLength;
            }
        }

    }
}
