using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static unsafe class StreamHelper
    {
        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Offset">A byte offset relative to the <paramref name="Origin"/>.</param>
        /// <param name="Origin">Indicates the reference point used to obtain the new position.</param>
        public static bool TrySeek(this Stream This, long Offset, SeekOrigin Origin)
        {
            if (Offset == 0)
                return true;

            if (This.CanSeek)
            {
                This.Seek(Offset, Origin);
                return true;
            }

            const int BufferLength = 8192;
            if (Origin == SeekOrigin.Current)
            {
                if (Offset < 0)
                    return false;

                byte[] Buffer = ArrayPool<byte>.Shared.Rent(BufferLength);
                try
                {
                    // More than 2GB
                    const int Buffer2GBLoop = int.MaxValue / BufferLength;
                    const long IntMaxValue = int.MaxValue;
                    while (IntMaxValue <= Offset)
                    {
                        for (int i = 0; i < Buffer2GBLoop; i++)
                            if (!ReadBuffer(This, Buffer, 0, BufferLength))
                                return false;

                        Offset -= IntMaxValue;
                    }

                    // More than BufferLength
                    while (BufferLength <= Offset)
                    {
                        if (!ReadBuffer(This, Buffer, 0, BufferLength))
                            return false;

                        Offset -= BufferLength;
                    }

                    return ReadBuffer(This, Buffer, 0, (int)Offset);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(Buffer);
                }
            }

            return false;
        }

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
        public static void Write<T>(this Stream This, params T[] Datas) where T : unmanaged
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
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Datas">Tn array of bytes to write to the stream.</param>
        public static void WriteBytes(this Stream This, params byte[] Datas)
            => This.Write(Datas, 0, Datas.Length);
        /// <summary>
        /// Writes a reaverse data of specified type to the stream.
        /// </summary>
        /// <typeparam name="T">The specified type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Data">The specified type of data to write to the stream.</param>
        public static void ReverseWrite<T>(this Stream This, T Data)
            where T : unmanaged
        {
            int Size = sizeof(T);
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);
            try
            {
                *(T*)Buffer.ToPointer() = Data;
                Array.Reverse(Buffer, 0, Size);
                This.Write(Buffer, 0, Size);
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
        public static Task WriteAsync<T>(this Stream This, T[] Datas) where T : unmanaged
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
        public static T Read<T>(this Stream This) where T : struct
            => TryRead(This, out T Result) ? Result : throw new IOException();
        /// <summary>
        /// Reads a data of the specified type from the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Buffer">The buffer to be filled by the stream.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        public static T Read<T>(this Stream This, byte[] Buffer, int Offset) where T : struct
            => TryRead(This, Buffer, Offset, out T Result) ? Result : throw new IOException();
        /// <summary>
        /// Reads a  specified length buffer from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Length">The length of specified buffer.</param>
        public static byte[] Read(this Stream This, int Length)
            => TryRead(This, Length, out byte[] Buffer) ? Buffer : throw new IOException();

        /// <summary>
        /// Reads a data of the specified type from the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Result">The object read from the stream.</param>
        public static bool TryRead<T>(this Stream This, out T Result)
            where T : struct
        {
            int Size = Marshal.SizeOf<T>();
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);
            try
            {
                if (!This.ReadBuffer(Buffer, 0, Size))
                {
                    Result = default;
                    return false;
                }

                fixed (byte* pBuffer = Buffer)
#pragma warning disable CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標
                    Result = *(T*)pBuffer;
#pragma warning restore CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標

                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }
        /// <summary>
        /// Reads a data of the specified type from the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Buffer">The buffer to be filled by the stream.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="Result">The object read from the stream.</param>
        public static bool TryRead<T>(this Stream This, byte[] Buffer, int Offset, out T Result)
            where T : struct
        {
            int Size = Marshal.SizeOf<T>();
            if (Offset + Size > Buffer.Length ||
                !This.ReadBuffer(Buffer, Offset, Size))
            {
                Result = default;
                return false;
            }

            fixed (byte* pBuffer = Buffer)
#pragma warning disable CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標
                Result = *(T*)pBuffer;
#pragma warning restore CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標

            return true;
        }
        /// <summary>
        /// Reads a specified length buffer from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="Length">The length of specified buffer.</param>
        /// <param name="Buffer">Data read from the stream.</param>
        public static bool TryRead(this Stream This, int Length, out byte[] Buffer)
        {
            Buffer = new byte[Length];
            if (!ReadBuffer(This, Buffer, 0, Length))
            {
                Buffer = null;
                return false;
            }

            return true;
        }
        /// <summary>
        /// Reads a reversed data of the specified type from the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Result">The object read from the stream.</param>
        public static bool TryReverseRead<T>(this Stream This, out T Result)
            where T : struct
        {
            int Size = Marshal.SizeOf<T>();
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);
            try
            {
                if (!This.ReadBuffer(Buffer, 0, Size))
                {
                    Result = default;
                    return false;
                }

                Array.Reverse(Buffer, 0, Size);
                fixed (byte* pBuffer = Buffer)
#pragma warning disable CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標
                    Result = *(T*)pBuffer;
#pragma warning restore CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標

                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }
        /// <summary>
        /// Reads a reversed data of the specified type from the stream.
        /// </summary>
        /// <typeparam name="T">The sepecial type of data.</typeparam>
        /// <param name="This">The current stream.</param>
        /// <param name="Buffer">The buffer to be filled by the stream.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="Result">The object read from the stream.</param>
        public static bool TryReverseRead<T>(this Stream This, byte[] Buffer, int Offset, out T Result)
            where T : struct
        {
            int Size = Marshal.SizeOf<T>();
            if (!This.ReadBuffer(Buffer, Offset, Size))
            {
                Result = default;
                return false;
            }

            Array.Reverse(Buffer, 0, Size);
            fixed (byte* pBuffer = Buffer)
#pragma warning disable CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標
                Result = *(T*)pBuffer;
#pragma warning restore CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標

            return true;
        }

        /// <summary>
        /// Reads string of specified encoding from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="BytesLength">The byte length of string.</param>
        public static string ReadString(this Stream This, int BytesLength)
            => TryReadString(This, BytesLength, Encoding.Default, out string Result) ? Result : throw new IOException();
        /// <summary>
        /// Reads string of specified encoding from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="BytesLength">The byte length of string.</param>
        /// <param name="Encoding">The specified encoding of string.</param>
        public static string ReadString(this Stream This, int BytesLength, Encoding Encoding)
            => TryReadString(This, BytesLength, Encoding, out string Result) ? Result : throw new IOException();

        /// <summary>
        /// Reads string of specified encoding from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="BytesLength">The byte length of string.</param>
        /// <param name="Result">The string decoded from the stream.</param>
        public static bool TryReadString(this Stream This, int BytesLength, out string Result)
            => TryReadString(This, BytesLength, Encoding.Default, out Result);
        /// <summary>
        /// Reads string of specified encoding from the stream.
        /// </summary>
        /// <param name="This">The current stream.</param>
        /// <param name="BytesLength">The byte length of string.</param>
        /// <param name="Encoding">The specified encoding of string.</param>
        /// <param name="Result">The string decoded from the stream.</param>
        public static bool TryReadString(this Stream This, int BytesLength, Encoding Encoding, out string Result)
        {
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(BytesLength);
            try
            {
                if (!This.ReadBuffer(Buffer, 0, BytesLength))
                {
                    Result = string.Empty;
                    return false;
                }

                Result = Encoding.GetString(Buffer, 0, BytesLength);
                return true;
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