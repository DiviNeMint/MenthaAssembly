using System.IO.Compression;

namespace System.IO
{
    public static class ArchiveHelper
    {
        public static bool IsZipArchive(byte[] Identifier)
            => Identifier.Length >= 4 &&
               Identifier[0] == 0x50 &&     // P
               Identifier[1] == 0x4B &&     // K
               Identifier[2] == 0x03 &&     // \3
               Identifier[3] == 0x04;       // ]4

    }
}