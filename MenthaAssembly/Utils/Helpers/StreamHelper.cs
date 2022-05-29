using System.Threading.Tasks;

namespace System.IO
{
    public unsafe static class StreamHelper
    {
        public static void Write<T>(this Stream This, T Datas)
            where T : unmanaged
        {
            byte[] Buffer = new byte[sizeof(T)];
            fixed (byte* pBuffer = &Buffer[0])
            {
                *(T*)pBuffer = Datas;
            }

            This.Write(Buffer, 0, Buffer.Length);
        }
        public static void Write<T>(this Stream This, T[] Datas)
            where T : unmanaged
        {
            int DataLength = Datas.Length;
            byte[] Buffer = new byte[sizeof(T) * DataLength];
            T* pTempBuffer;
            fixed (byte* pBuffer = &Buffer[0])
            {
                pTempBuffer = (T*)pBuffer;
            }

            for (int i = 0; i < DataLength; i++)
                *pTempBuffer++ = Datas[i];

            This.Write(Buffer, 0, Buffer.Length);
        }
        public static Task WriteAsync<T>(this Stream This, T Datas)
            where T : unmanaged
        {
            byte[] Buffer = new byte[sizeof(T)];
            fixed (byte* pBuffer = &Buffer[0])
            {
                *(T*)pBuffer = Datas;
            }

            return This.WriteAsync(Buffer, 0, Buffer.Length);
        }
        public static Task WriteAsync<T>(this Stream This, T[] Datas)
            where T : unmanaged
        {
            int DataLength = Datas.Length;
            byte[] Buffer = new byte[sizeof(T) * DataLength];
            T* pTempBuffer;
            fixed (byte* pBuffer = &Buffer[0])
            {
                pTempBuffer = (T*)pBuffer;
            }

            for (int i = 0; i < DataLength; i++)
                *pTempBuffer++ = Datas[i];

            return This.WriteAsync(Buffer, 0, Buffer.Length);
        }

        public static T Read<T>(this Stream This)
            where T : unmanaged
        {
            byte[] Buffer = new byte[sizeof(T)];
            This.ReadBuffer(Buffer);

            fixed (byte* pBuffer = &Buffer[0])
            {
                T* pInt = (T*)pBuffer;
                return *pInt;
            }
        }

        public static void ReadBuffer(this Stream This, byte[] Buffer)
        {
            int Length = Buffer.Length,
                Offset = This.Read(Buffer, 0, Length),
                ReadLength;

            Length -= Offset;
            while (Length > 0)
            {
                ReadLength = This.Read(Buffer, Offset, Length);

                Offset += ReadLength;
                Length -= ReadLength;
            }
        }

    }
}
