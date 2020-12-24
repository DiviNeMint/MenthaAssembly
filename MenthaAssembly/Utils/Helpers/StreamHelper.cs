namespace System.IO
{
    public static class StreamHelper
    {
        public static unsafe T Read<T>(this Stream This)
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
