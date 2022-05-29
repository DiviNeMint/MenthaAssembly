using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Media.Imaging
{
    public unsafe static class IcoCoder
    {
        public const int IdentifyHeaderSize = 6;

        private const int IconEnrtyLength = 16;

        public static bool TryDecode(string FilePath, out IImageContext[] Images)
        {
            using FileStream Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, out Images);
        }
        public static bool TryDecode(Stream Stream, out IImageContext[] Images)
        {
            byte[] Buffer = new byte[IdentifyHeaderSize];

            //if (Stream.Position != 0)
            //    Stream.Seek(0, SeekOrigin.Begin);

            Stream.Read(Buffer, 0, IdentifyHeaderSize);

            byte* pBuffer = Buffer.ToPointer(2);    // Skip Reserved
            ushort* pUShorts = (ushort*)pBuffer;

            ushort Type = *pUShorts++;
            if (Type == 1)
            {
                ushort ImagesLength = *pUShorts;
                Images = new IImageContext[ImagesLength];

                Buffer = new byte[IconEnrtyLength];
                ushort* pBitsPerPixel = (ushort*)Buffer.ToPointer(6);
                int* pOffset = (int*)(pBitsPerPixel + 3);

                for (int i = 0; i < ImagesLength; i++)
                {
                    Stream.Read(Buffer, 0, IconEnrtyLength);

                    int Offset = *pOffset;
                    if (Stream.Position != Offset)
                        Stream.Seek(Offset, SeekOrigin.Begin);

                    if (*pBitsPerPixel == 32)
                    {
                        if (PngCoder.TryDecode(Stream, out IImageContext Image))
                            Images[i] = Image;
                    }
                    else
                    {
                        if (BmpCoder.TryDecode(Stream, out IImageContext Image))
                            Images[i] = Image;
                    }

                    Stream.Seek(22 + IconEnrtyLength * i, SeekOrigin.Begin);
                }
            }

            Images = new IImageContext[0];
            return false;
        }

        public static void Encode(string FilePath, params IImageContext[] Images)
        {
            using FileStream Stream = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            Encode(Stream, Images);
        }
        public static void Encode(Stream Stream, params IImageContext[] Images)
        {
            int ImageLength = Images.Length;
            byte[] Buffer =
            {
                0x00, 0x00,                                     // Reserved      , 2 Bytes
                0x01, 0x00,                                     // Type          , 2 Bytes ; 1 : (.ICO), 2 : (.CUR)
                (byte)ImageLength, (byte)(ImageLength >> 8)     // Images Length , 2 Bytes
            };

            Stream.Write(Buffer, 0, 6);

            Buffer = new byte[IconEnrtyLength];
            byte* pBuffer = Buffer.ToPointer();

            int NextImageOffset = 6 + 16 * ImageLength;
            for (int i = 0; i < ImageLength; i++)
            {
                IImageContext Image = Images[i];

                int Width = Image.Width,
                    Height = Image.Height;
                if (Width > 256 || Height > 256)
                {
                    double Scale = 256 / Math.Max(Width, Height);
                    Width = (int)(Width * Scale);
                    Height = (int)(Height * Scale);

                    Image = Image.Resize<RGBA>(Width, Height, InterpolationTypes.Nearest);
                }

                // Write Entry
                byte* pBytes = pBuffer;
                *pBytes++ = (byte)(Width == 256 ? 0 : Width);       // ImageWidth  , 1 Bytes
                *pBytes++ = (byte)(Height == 256 ? 0 : Height);     // ImageHeight , 1 Bytes
                *pBytes++ = (byte)(Image.Palette?.Count ?? 0);      // Palette     , 1 Bytes
                pBytes++;                                           // Reserved    , 1 Bytes

                ushort* pUShorts = (ushort*)pBytes;
                *pUShorts++ = 0;                                    // ColorPlanes  , 2 Bytes ; Should be 0 or 1.
                *pUShorts++ = (ushort)Image.BitsPerPixel;           // BitsPerPixel , 2 Bytes

                int* pInt = (int*)pUShorts;
                *pInt++ = 0;                                        // ImageSize    , 4 Bytes
                *pInt = NextImageOffset;                            // Offset       , 4 Bytes

                Stream.Write(Buffer, 0, IconEnrtyLength);

                Stream.Seek(NextImageOffset, SeekOrigin.Begin);

                // Write Image Datas
                if (Image.BitsPerPixel == 32)
                    PngCoder.Encode(Image, Stream);
                else
                    BmpCoder.Encode(Image, Stream);

                // Going back Write ImageSize
                int ImageSize = (int)(Stream.Position - NextImageOffset);
                Stream.Seek(14 + IconEnrtyLength * i, SeekOrigin.Begin);
                Stream.Write(ImageSize);
                Stream.Seek(4, SeekOrigin.Current);

                NextImageOffset += ImageSize;
            }
        }

        [Conditional("DEBUG")]
        public static void Parse(string FilePath)
        {
            using FileStream Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            byte[] Buffer = new byte[IdentifyHeaderSize];

            //if (Stream.Position != 0)
            //    Stream.Seek(0, SeekOrigin.Begin);

            Stream.Read(Buffer, 0, IdentifyHeaderSize);

            byte* pBuffer = Buffer.ToPointer(2);    // Skip Reserved
            ushort* pUShorts = (ushort*)pBuffer;

            ushort Type = *pUShorts++;

            Debug.WriteLine($"Type         : {Type}");
            if (Type == 1)
            {
                ushort ImagesLength = *pUShorts;
                Debug.WriteLine($"ImagesLength : {ImagesLength}");

                Buffer = new byte[IconEnrtyLength];
                pBuffer = Buffer.ToPointer(0);

                for (int i = 0; i < ImagesLength; i++)
                {
                    Debug.WriteLine($"============================================");
                    Debug.WriteLine($"                 Entry {i}                    ");
                    Debug.WriteLine($"============================================");
                    Stream.Read(Buffer, 0, IconEnrtyLength);

                    // Write Entry
                    byte* pBytes = pBuffer;
                    Debug.WriteLine($"ImagesWidth  : {*pBytes++}");
                    Debug.WriteLine($"ImagesHeight : {*pBytes++}");
                    Debug.WriteLine($"Palette      : {*pBytes++}");
                    pBytes++;                                           // Reserved    , 1 Bytes

                    pUShorts = (ushort*)pBytes;
                    Debug.WriteLine($"ColorPlanes  : {*pUShorts++}");
                    Debug.WriteLine($"BitsPerPixel : {*pUShorts++}");

                    int* pInt = (int*)pUShorts;
                    Debug.WriteLine($"ImageSize    : {*pInt++}");
                    Debug.WriteLine($"Offset       : {*pInt}");
                }
                Debug.WriteLine($"============================================");
            }
        }
    }
}
