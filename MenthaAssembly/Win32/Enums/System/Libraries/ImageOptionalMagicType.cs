namespace MenthaAssembly.Win32
{
    internal enum ImageOptionalMagicType : ushort
    {
        /// <summary>
        /// x86
        /// </summary>
        HDR32_MAGIC = 0x10b,

        /// <summary>
        /// x64
        /// </summary>
        HDR64_MAGIC = 0x20b,

        /// <summary>
        /// ROM image.
        /// </summary>
        ROM_OPTIONAL_HDR_MAGIC = 0x107,

    }
}