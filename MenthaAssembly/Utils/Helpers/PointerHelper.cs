namespace System.Runtime.InteropServices
{
    public static unsafe class PointerHelper
    {
        /// <summary>
        /// Gets a specified type instance from the specified ponter.
        /// </summary>
        /// <typeparam name="T">The specified type instance.</typeparam>
        /// <param name="This">The specified ponter.</param>
        public static T Get<T>(this IntPtr This)
            where T : struct
        {
#pragma warning disable CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標
            T* pStruct = (T*)This;
            return *pStruct;
#pragma warning restore CS8500
        }

        /// <summary>
        /// Copies data from an unmanaged memory pointer to an unmanaged memory pointer.
        /// </summary>
        /// <param name="Source">The memory pointer to copy from.</param>
        /// <param name="Destination">The memory pointer to copy to.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        public static void Copy(IntPtr Source, IntPtr Destination, int Length)
            => MenthaAssembly.Win32.System.MemoryCopy(Destination, Source, Length);
        /// <summary>
        /// Copies data from an unmanaged memory pointer to an unmanaged memory pointer.
        /// </summary>
        /// <param name="Source">The memory pointer to copy from.</param>
        /// <param name="Destination">The memory pointer to copy to.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        public static void Copy(void* Source, void* Destination, int Length)
            => MenthaAssembly.Win32.System.MemoryCopy(Destination, Source, Length);
        /// <summary>
        /// Copies data from an unmanaged memory pointer to an unmanaged memory pointer.
        /// </summary>
        /// <param name="Source">The datas to copy from.</param>
        /// <param name="Destination">The byte array to copy to.</param>
        /// <param name="Offset">The zero-based index in the <paramref name="Destination"/> where copying to should start.</param>
        public static void Copy<T>(T Source, byte[] Destination, int Offset)
            where T : struct
        {
            fixed (byte* pBuffer = &Destination[Offset])
#pragma warning disable CS8500 // 這會取得 Managed 類型的位址、大小，或宣告指向它的指標
                *(T*)pBuffer = Source;
#pragma warning restore CS8500
        }

        /// <summary>
        /// Converts the array to a pointer.
        /// </summary>
        public static T* ToPointer<T>(this T[] This)
            where T : unmanaged
        {
            fixed (T* pThis = &This[0])
            {
                return pThis;
            }
        }
        /// <summary>
        /// Converts the array to a pointer.
        /// </summary>
        public static T* ToPointer<T>(this T[] This, int Offset)
            where T : unmanaged
        {
            fixed (T* pThis = &This[Offset])
            {
                return pThis;
            }
        }

    }
}