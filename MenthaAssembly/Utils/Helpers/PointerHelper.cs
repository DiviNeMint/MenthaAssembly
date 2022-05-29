namespace System.Runtime.InteropServices
{
    public static unsafe class PointerHelper
    {
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

        /// <summary>
        /// Cast the ponter to a special type instance.
        /// </summary>
        /// <typeparam name="T">The special type instance.</typeparam>
        public static T Cast<T>(this IntPtr This)
            where T : unmanaged
            => *(T*)This;

        /// <summary>
        /// Cast the ponter to a special type instance by <see cref="Marshal.PtrToStructure{T}(IntPtr)"/>
        /// </summary>
        /// <typeparam name="T">The special type instance.</typeparam>
        public static T CastByMarshal<T>(this IntPtr This)
            where T : unmanaged
            => Marshal.PtrToStructure<T>(This);

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
