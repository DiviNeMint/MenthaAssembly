namespace System.Runtime.InteropServices
{
    public static unsafe class PointerHelper
    {
        public static T Cast<T>(this IntPtr This)
            where T : unmanaged
            => *(T*)This;

        /// <summary>
        /// Cast by <see cref="Marshal.PtrToStructure{T}(IntPtr)"/>
        /// </summary>
        public static T Cast2<T>(this IntPtr This)
            where T : unmanaged
            => Marshal.PtrToStructure<T>(This);

    }
}
