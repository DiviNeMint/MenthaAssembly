namespace System.Runtime.InteropServices
{
    public unsafe delegate void UnmanagedAction<in T>(T* pDatas) where T : unmanaged;
    public unsafe delegate U UnmanagedFunc<in T, out U>(T* pDatas) where T : unmanaged;
    public static unsafe class PointerHelper
    {
        public static T Cast<T>(this IntPtr This)
            where T : unmanaged
            => *(T*)This;

        /// <summary>
        /// Cast by <see cref="Marshal.PtrToStructure{T}(IntPtr)"/>
        /// </summary>
        public static T CastByMarshal<T>(this IntPtr This)
            where T : unmanaged
            => Marshal.PtrToStructure<T>(This);

        public static void Fixed<T>(T[] Datas, UnmanagedAction<T> Action)
            where T : unmanaged
        {
            fixed (T* pBuffer = &Datas[0])
            {
                Action(pBuffer);
            }
        }
        public static U Fixed<T, U>(T[] Datas, UnmanagedFunc<T, U> Function)
            where T : unmanaged
        {
            fixed (T* pBuffer = &Datas[0])
            {
                return Function(pBuffer);
            }
        }

        /// <summary>
        /// Copies data from an unmanaged memory pointer to an unmanaged memory pointer.
        /// </summary>
        /// <param name="Source">The memory pointer to copy from.</param>
        /// <param name="Destination">The memory pointer to copy to.</param>
        /// <param name="Length">The number of array elements to copy.</param>
        public static void Copy(IntPtr Source, IntPtr Destination, int Length)
        {
            byte* pSource = (byte*)Source,
                  pDestination = (byte*)Destination;

            for (int i = 0; i < Length; i++)
                *pDestination++ = *pSource++;
        }

        /// <summary>
        /// Copies data from an unmanaged memory pointer to an unmanaged memory pointer.
        /// </summary>
        /// <param name="Source">The datas to copy from.</param>
        /// <param name="Destination">The byte array to copy to.</param>
        /// <param name="Offset">The zero-based index in the <paramref name="Destination"/> where copying to should start.</param>
        public static void Copy<T>(T Source, byte[] Destination, int Offset)
            where T : unmanaged
        {
            fixed (byte* pBuffer = &Destination[Offset])
            {
                *(T*)pBuffer = Source;
            }
        }

    }
}
