using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace MenthaAssembly.Offices.Primitives
{
    public static class ZipArchiveHelper
    {
        public static bool TryExtract(this ZipArchiveEntry This, out Stream CompressedStream, out Func<Stream, Stream> GetDecompressor)
        {
            if (!This.Archive.TryGetInternalFieldValue("_archiveStream", out Stream BaseStream) ||
                !This.TryGetInternalFieldValue("_compressedSize", out long CompressedSize) ||
                !This.TryGetInternalPropertyValue("OffsetOfCompressedData", out long Offset) ||
                !This.TryGetInternalFieldValue("_storedCompressionMethod", out object CompressionMethod))
            {
                CompressedStream = null;
                GetDecompressor = null;
                return false;
            }

            ushort Method = (ushort)CompressionMethod;

            // Stored
            if (Method == 0x0)
            {
                GetDecompressor = s =>
                {
                    if (s is MemoryStream Memory)
                        return new MemoryStream(Memory.GetBuffer());

                    s.Seek(0, SeekOrigin.Begin);
                    MemoryStream Stream = new MemoryStream();
                    s.CopyTo(Stream);
                    return Stream;
                };
            }

            // Deflate
            else if (Method == 0x8)
            {
                GetDecompressor = s =>
                {
                    MemoryStream Stream;
                    if (s is MemoryStream Memory)
                        Stream = new MemoryStream(Memory.GetBuffer());
                    else
                    {
                        s.Seek(0, SeekOrigin.Begin);
                        Stream = new MemoryStream();
                        s.CopyTo(Stream);
                    }

                    return new DeflateStream(Stream, CompressionMode.Decompress);
                };
            }

            // Deflate64
            else if (Method == 0x9 &&
                     Assembly.GetAssembly(typeof(ZipArchive)).GetTypes().FirstOrDefault(i => i.Name.Equals("DeflateManagedStream")) is Type Type)
            {
                GetDecompressor = s =>
                {
                    MemoryStream Stream;
                    if (s is MemoryStream Memory)
                        Stream = new MemoryStream(Memory.GetBuffer());
                    else
                    {
                        s.Seek(0, SeekOrigin.Begin);
                        Stream = new MemoryStream();
                        s.CopyTo(Stream);
                    }

                    // new DeflateManagedStream(compressedStreamToRead, CompressionMethodValues.Deflate64, _uncompressedSize);
                    return (Stream)Activator.CreateInstance(Type, Stream, CompressionMethod);
                };
            }

            else
            {
                Debug.WriteLine($"Not support {Method}.");
                CompressedStream = null;
                GetDecompressor = null;
                return false;
            }

            byte[] Buffer = new byte[CompressedSize];
            BaseStream.ReadBuffer(Buffer);

            CompressedStream = new MemoryStream(Buffer, 0, Buffer.Length, false, true);
            return true;
        }

    }
}
