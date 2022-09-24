using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static unsafe class StreamHelper
    {
        /// <summary>
        /// Writes a data of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The specified type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Data">The specified type of data to write to the stream.</param>
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
        /// Writes an array of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The specified type of array.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Datas">The array of specified type to write to the stream.</param>
        public static void Write<T>(this Stream This, T[] Datas)
            where T : unmanaged
            => Write(This, Datas, 0, Datas.Length);
        /// <summary>
        /// Writes an array of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The specified type of array.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Datas">The array of specified type to be written.</param>
        /// <param name="Offset">The zero-based byte offset in datas at which to begin writing to the stream.</param>
        /// <param name="Length">The specified length of datas to be written.</param>
        public static void Write<T>(this Stream This, T[] Datas, int Offset, int Length)
            where T : unmanaged
        {
            int Size = sizeof(T) * Length;
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);

            try
            {
                T* pBuffer = (T*)Buffer.ToPointer();
                for (int i = 0; i < Length; i++)
                    *pBuffer++ = Datas[Offset++];

                This.Write(Buffer, 0, Size);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }
        /// <summary>
        /// Writes datas from a pointer of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of pointer.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="pBuffer">The pointer of specified type. This method copies datas from the pointer to the current stream.</param>
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

        /// <summary>
        /// Asynchronously writes a data of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The specified type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Data">The specified type of data to write to the stream.</param>
        public static Task WriteAsync<T>(this Stream This, T Data)
            where T : unmanaged
        {
            byte[] Buffer = new byte[sizeof(T)];
            *(T*)Buffer.ToPointer() = Data;

            return This.WriteAsync(Buffer, 0, Buffer.Length);
        }
        /// <summary>
        /// Asynchronously writes an array of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The specified type of array.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Datas">The array of specified type to write to the stream.</param>
        public static Task WriteAsync<T>(this Stream This, T[] Datas)
            where T : unmanaged
            => WriteAsync(This, Datas, 0, Datas.Length);
        /// <summary>
        /// Asynchronously writes an array of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The specified type of array.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Datas">The array of specified type to be written.</param>
        /// <param name="Offset">The zero-based byte offset in datas at which to begin writing to the stream.</param>
        /// <param name="Length">The specified length of datas to be written.</param>
        public static Task WriteAsync<T>(this Stream This, T[] Datas, int Offset, int Length)
            where T : unmanaged
        {
            byte[] Buffer = new byte[sizeof(T) * Length];
            T* pBuffer = (T*)Buffer.ToPointer();
            for (int i = 0; i < Length; i++)
                *pBuffer++ = Datas[Offset++];

            return This.WriteAsync(Buffer, 0, Buffer.Length);
        }

        /// <summary>
        /// Reads a data of specified type from the stream.
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
        /// Reads a  specified length buffer from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Length">The length of specified buffer.</param>
        public static byte[] Read(this Stream This, int Length)
        {
            byte[] Buffer = new byte[Length];
            if (!ReadBuffer(This, Buffer))
                throw new OutOfMemoryException();

            return Buffer;
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
        public static bool ReadBuffer(this Stream This, byte[] Buffer)
            => ReadBuffer(This, Buffer, 0, Buffer.Length);
        /// <summary>
        /// Reads a buffer from the stream until it fills the specified length of buffer.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Buffer">The buffer to be filled by the stream.</param>
        /// <param name="Length">The specified length of buffer to be filled.</param>
        public static bool ReadBuffer(this Stream This, byte[] Buffer, int Length)
            => ReadBuffer(This, Buffer, 0, Length);
        /// <summary>
        /// Reads a buffer from the stream until it fills the specified length of buffer.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Buffer">The buffer to be filled by the stream.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="Length">The specified length of buffer to be filled.</param>
        public static bool ReadBuffer(this Stream This, byte[] Buffer, int Offset, int Length)
        {
            int Index = This.Read(Buffer, Offset, Length),
                ReadLength;

            Length -= Index;
            Index += Offset;
            while (Length > 0)
            {
                ReadLength = This.Read(Buffer, Index, Length);

                if (ReadLength < 1)
                    return false;

                Index += ReadLength;
                Length -= ReadLength;
            }

            return true;
        }

    }
}
