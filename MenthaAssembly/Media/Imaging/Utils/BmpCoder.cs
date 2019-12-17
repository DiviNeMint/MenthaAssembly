using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging
{
    public static class BmpCoder
    {
        public static int IdentifyHeaderSize => 2;

        public static bool TryDecode(string FilePath, out ImageContext Image)
            => TryDecode(new FileStream(FilePath, FileMode.Open, FileAccess.Read), out Image);
        public static bool TryDecode(Stream Stream, out ImageContext Image)
        {
            byte[] Datas = new byte[IdentifyHeaderSize];

            if (Stream.Position != 0)
                Stream.Seek(0, SeekOrigin.Begin);

            Stream.Read(Datas, 0, Datas.Length);

            // Identify
            if (!Identify(Datas))
            {
                Image = null;
                return false;
            }

            Datas = new byte[sizeof(int)];
            // Offset
            Stream.Seek(10, SeekOrigin.Begin);
            Stream.Read(Datas, 0, Datas.Length);
            int Offset = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // Width
            Stream.Seek(18, SeekOrigin.Begin);
            Stream.Read(Datas, 0, Datas.Length);
            int Width = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // Height
            Stream.Read(Datas, 0, Datas.Length);
            int Height = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // BitsPerPixel
            Stream.Seek(28, SeekOrigin.Begin);
            Stream.Read(Datas, 0, sizeof(short));
            int Bits = Datas[0] | Datas[1] << 8;
            int Channels = (Bits + 7) >> 3;
            int Stride = (((Width * Bits) >> 3) + 3) >> 2 << 2;

            // ImageDatas
            int ChannelStride = Channels > 1 ? Width : Stride;
            int ChannelSize = ChannelStride * Height;
            byte[][] ImageDatas = new byte[Channels][];

            Datas = new byte[Stride];
            Stream.Seek(Offset, SeekOrigin.Begin);

            bool IsHeightNegative = Height < 0;
            if (IsHeightNegative)
                Height = ~Height + 1;   // Abs

            for (int j = 0; j < Height; j++)
            {
                Stream.Read(Datas, 0, Datas.Length);

                Offset = (IsHeightNegative ? j : Height - j - 1) * ChannelStride;
                Parallel.For(0, ImageDatas.Length,
                    (c) =>
                    {
                        if (ImageDatas[c] is null)
                            ImageDatas[c] = new byte[ChannelSize];

                        for (int i = 0; i < ChannelStride; i++)
                            ImageDatas[c][Offset + i] = Datas[i * Channels + c];
                    });
            }

            Stream.Close();

            switch (ImageDatas.Length)
            {
                case 1:
                    Image = new ImageContext(Width, Height, ImageDatas[0]);
                    return true;
                case 3:
                    Image = new ImageContext(Width, Height, ImageDatas[2], ImageDatas[1], ImageDatas[0]);
                    return true;
                case 4:
                    Image = new ImageContext(Width, Height, ImageDatas[3], ImageDatas[2], ImageDatas[1], ImageDatas[0]);
                    return true;
            }

            Image = null;
            return false;
        }

        public static void Encode(ImageContext Image, string FilePath)
            => Encode(Image, new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write));
        public static void Encode(ImageContext Image, Stream Stream)
        {
            // Bitmap File Struct
            // https://crazycat1130.pixnet.net/blog/post/1345538#mark-4

            int Stride = (((Image.Width * Image.BitsPerPixel) >> 3) + 3) >> 2 << 2;
            int ImageSize = Stride * Image.Height;
            int HeaderOffset = Image.BitsPerPixel > 8 ? 54 : 54 + (4 << (Image.BitsPerPixel));
            int FileSize = ImageSize + HeaderOffset;
            byte[] InfoDatas =
            {
                66, 77,                                                                                                 // Format       , 2 Bytes
                (byte)FileSize, (byte)(FileSize >> 8), (byte)(FileSize >> 16), (byte)(FileSize >> 24),                  // FileSize     , 4 Bytes
                0, 0, 0, 0,                                                                                             // Reserved     , 4 Bytes
                (byte)HeaderOffset, (byte)(HeaderOffset >> 8), (byte)(HeaderOffset >> 16), (byte)(HeaderOffset >> 24),  // Offset       , 4 Bytes (54 Bytes for Header Struct)
                40, 0, 0, 0,                                                                                            // InfoSize     , 4 Bytes (40 Bytes for Info Struce)
                (byte)Image.Width, (byte)(Image.Width >> 8), (byte)(Image.Width >> 16), (byte)(Image.Width >> 24),      // Width        , 4 Bytes
                (byte)Image.Height, (byte)(Image.Height >> 8), (byte)(Image.Height >> 16), (byte)(Image.Height >> 24),  // Height       , 4 Bytes
                1, 0,                                                                                                   // Planes       , 2 Bytes (forever be set 1.)
                (byte)Image.BitsPerPixel, (byte)(Image.BitsPerPixel >> 8),                                              // BitsPerPixel , 2 Bytes
                0, 0, 0, 0,                                                                                             // Compression  , 4 Bytes
                (byte)ImageSize, (byte)(ImageSize >> 8), (byte)(ImageSize >> 16), (byte)(ImageSize >> 24),              // ImageSize    , 4 Bytes
                0, 0, 0, 0,                                                                                             // XResolution  , 4 Bytes (Dpi * 39.37)
                0, 0, 0, 0,                                                                                             // YResolution  , 4 Bytes
                0, 0, 0, 0,                                                                                             // NColors     , 4 Bytes
                0, 0, 0, 0,                                                                                             // ImportantColours  , 4 Bytes
            };
            Stream.Write(InfoDatas, 0, InfoDatas.Length);

            // Plate
            if (HeaderOffset > 54)
            {
                int ColorStep = byte.MaxValue / ((1 << Image.BitsPerPixel) - 1);
                for (int i = 0; i < 256; i += ColorStep)
                {
                    byte Value = (byte)i;
                    Stream.Write(new byte[] { Value, Value, Value, 0 }, 0, 4);
                }
            }

            switch (Image.Channels)
            {
                case 1:
                    unsafe
                    {
                        byte[] ImageDatas = new byte[Stride];
                        for (int j = Image.Height - 1; j >= 0; j--)
                        {
                            byte* Source = (byte*)(Image.Scan0 + Image.Stride * j);
                            for (int i = 0; i < Image.Stride; i++)
                                ImageDatas[i] = *Source++;

                            Stream.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
                case 3:
                    unsafe
                    {
                        byte[] ImageDatas = new byte[Stride];
                        for (int j = Image.Height - 1; j >= 0; j--)
                        {
                            int Offset = Image.Stride * j;
                            byte* SourceR = (byte*)(Image.ScanR + Offset),
                                  SourceG = (byte*)(Image.ScanG + Offset),
                                  SourceB = (byte*)(Image.ScanB + Offset);
                            for (int i = 0; i < ImageDatas.Length - 2; i += 3)
                            {
                                ImageDatas[i] = *SourceB++;         // B
                                ImageDatas[i + 1] = *SourceG++;     // G
                                ImageDatas[i + 2] = *SourceR++;     // R
                            }
                            Stream.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
                case 4:
                    unsafe
                    {
                        byte[] ImageDatas = new byte[Stride];
                        IntPtr DataPointer;
                        fixed (byte* DataScan = &ImageDatas[0])
                            DataPointer = (IntPtr)DataScan;

                        for (int j = Image.Height - 1; j >= 0; j--)
                        {
                            int Offset = Image.Stride * j;
                            int* DataScan0 = (int*)DataPointer;
                            byte* SourceA = (byte*)(Image.ScanA + Offset),
                                  SourceR = (byte*)(Image.ScanR + Offset),
                                  SourceG = (byte*)(Image.ScanG + Offset),
                                  SourceB = (byte*)(Image.ScanB + Offset);
                            for (int i = 0; i < ImageDatas.Length; i += 4)
                                *DataScan0++ = *SourceA++ << 24 |  // A
                                               *SourceR++ << 16 |  // R
                                               *SourceG++ << 8 |   // G
                                               *SourceB++;         // B

                            Stream.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
            }

            Stream.Close();
            Stream.Dispose();
        }

        public static bool Identify(byte[] Data)
        {
            if (Data.Length < IdentifyHeaderSize)
                return false;

            return Data[0].Equals(0x42) && Data[1].Equals(0x4D);    // BM – Windows 3.1x, 95, NT, ... etc.

            //return (Data[0].Equals(0x42) && Data[1].Equals(0x4D)) ||    // BM – Windows 3.1x, 95, NT, ... etc.
            //       (Data[0].Equals(0x42) && Data[1].Equals(0x41)) ||    // BA – OS / 2 struct Bitmap Array
            //       (Data[0].Equals(0x43) && Data[1].Equals(0x49)) ||    // CI – OS / 2 struct Color Icon
            //       (Data[0].Equals(0x43) && Data[1].Equals(0x50)) ||    // CP – OS / 2 const Color Pointer
            //       (Data[0].Equals(0x49) && Data[1].Equals(0x43)) ||    // IC – OS / 2 struct Icon
            //       (Data[0].Equals(0x50) && Data[1].Equals(0x54));      // PT – OS / 2 Pointer
        }


        [Conditional("DEBUG")]
        public static void Parse(string FilePath)
        {
            FileStream FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            byte[] Datas = new byte[IdentifyHeaderSize];
            FS.Read(Datas, 0, Datas.Length);

            if (!Identify(Datas))
            {
                Debug.WriteLine("This is not Bmp file.");
                return;
            }

            Datas = new byte[sizeof(int)];
            // FileSize
            FS.Read(Datas, 0, Datas.Length);
            int FileSize = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"FileSize    : {FileSize}");

            // Offset
            FS.Seek(10, SeekOrigin.Begin);
            FS.Read(Datas, 0, Datas.Length);
            int Offset = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"Offset      : {Offset}");

            // HeaderSize
            FS.Read(Datas, 0, Datas.Length);
            int HeaderSize = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"HeaderSize  : {HeaderSize}");

            // Width
            FS.Read(Datas, 0, Datas.Length);
            int Width = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"Width       : {Width}");

            // Height
            FS.Read(Datas, 0, Datas.Length);
            int Height = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"Height      : {Height}");

            // Planes
            FS.Read(Datas, 0, sizeof(short));
            int Planes = Datas[0] | Datas[1] << 8;
            Debug.WriteLine($"Planes      : {Planes}");

            // BitsPerPixel
            FS.Read(Datas, 0, sizeof(short));
            int Bits = Datas[0] | Datas[1] << 8;
            int Channels = (Bits + 7) >> 3;
            int Stride = (((Width * Bits) >> 3) + 3) >> 2 << 2;
            Debug.WriteLine($"Bits        : {Bits}\r\n" +
                            $"Channels    : {Channels}\r\n" +
                            $"Stride      : {Stride}");

            // Compression
            FS.Read(Datas, 0, Datas.Length);
            int Compression = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"Compression : {Compression}");

            // Compression
            FS.Read(Datas, 0, Datas.Length);
            int ImageSize = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"ImageSize   : {ImageSize}");

            // XResolution
            FS.Read(Datas, 0, Datas.Length);
            int XResolution = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"XResolution : {XResolution}");

            // YResolution
            FS.Read(Datas, 0, Datas.Length);
            int YResolution = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"YResolution : {YResolution}");

            // NColors
            FS.Read(Datas, 0, Datas.Length);
            int NColors = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"NColors     : {NColors}");

            // ImportantColors
            FS.Read(Datas, 0, Datas.Length);
            int ImportantColors = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;
            Debug.WriteLine($"ImpColors   : {ImportantColors}");

            // Plate
            int PlateSize = Offset - (int)FS.Position;
            if (PlateSize > 0)
            {
                Debug.WriteLine($"Plate       :");
                Datas = new byte[sizeof(int)];
                for (int i = 0; i < PlateSize >> 2; i++)
                {
                    FS.Read(Datas, 0, Datas.Length);
                    if (i > 99)
                        Debug.WriteLine($"{i} : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                    else if (i > 9)
                        Debug.WriteLine($"{i}  : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                    else
                        Debug.WriteLine($"{i}   : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                }
            }

            FS.Close();
        }

    }
}
