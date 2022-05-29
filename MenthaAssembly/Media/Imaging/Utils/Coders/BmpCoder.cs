using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    public static class BmpCoder
    {
        public const int IdentifyHeaderSize = 2;

        public static bool TryDecode(string FilePath, out IImageContext Image)
        {
            using Stream Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, out Image);
        }
        public static bool TryDecode(Stream Stream, out IImageContext Image)
        {
            byte[] Datas = new byte[IdentifyHeaderSize];

            //if (Stream.Position != 0)
            //    Stream.Seek(0, SeekOrigin.Begin);

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

            // HeaderSize
            Stream.Read(Datas, 0, Datas.Length);
            int HeaderSize = (Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24) + 14; // ImageStruct + FileHeader 

            // Width
            Stream.Read(Datas, 0, Datas.Length);
            int Width = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // Height
            Stream.Read(Datas, 0, Datas.Length);
            int Height = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // BitsPerPixel
            Stream.Seek(28, SeekOrigin.Begin);
            Stream.Read(Datas, 0, sizeof(short));
            int Bits = Datas[0] | Datas[1] << 8,
                Stride = (((Width * Bits) >> 3) + 3) & 2147483644;

            //// Compression
            //Stream.Read(Datas, 0, Datas.Length);
            //int Compression = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // Palette
            IList<BGRA> Palette = null;
            if (Offset > HeaderSize)
            {
                Palette = new List<BGRA>();

                // NColors
                Stream.Seek(46, SeekOrigin.Begin);
                Stream.Read(Datas, 0, Datas.Length);
                int NColors = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

                if (NColors.Equals(0))
                    NColors = 1 << Bits;

                Stream.Seek(HeaderSize, SeekOrigin.Begin);
                BGRA BlackColor = new BGRA(0, 0, 0, 255),
                     Color;
                for (int i = 0; i < NColors; i++)
                {
                    Stream.Read(Datas, 0, Datas.Length);
                    Color = new BGRA(Datas[0], Datas[1], Datas[2], byte.MaxValue);

                    if (!(Color == BlackColor && Datas[3] == 0) &&
                        !Palette.Contains(Color))
                        Palette.Add(Color);
                }
            }

            // ImageDatas
            byte[] ImageDatas = new byte[Stride * Height];

            bool IsHeightNegative = Height < 0;
            if (IsHeightNegative)
                Height = ~Height + 1;   // Abs

            Stream.Seek(Offset, SeekOrigin.Begin);
            if (IsHeightNegative)
                Stream.Read(ImageDatas, 0, ImageDatas.Length);
            else
                for (int j = Height - 1; j >= 0; j--)
                    Stream.Read(ImageDatas, j * Stride, Stride);

            switch (Bits)
            {
                case 1:
                    Image = new ImageContext<BGRA, Indexed1>(Width, Height, ImageDatas, Palette);
                    return true;
                case 4:
                    Image = new ImageContext<BGRA, Indexed4>(Width, Height, ImageDatas, Palette);
                    return true;
                case 8:
                    Image = Palette is null ? new ImageContext<Gray8>(Width, Height, ImageDatas) :
                                              new ImageContext<BGRA, Indexed8>(Width, Height, ImageDatas, Palette);
                    return true;
                case 24:
                    Image = new ImageContext<BGR>(Width, Height, ImageDatas);
                    return true;
                case 32:
                    Image = new ImageContext<BGRA>(Width, Height, ImageDatas);
                    return true;
            }


            Image = null;
            return false;
        }

        public static void Encode(IImageContext Image, string FilePath)
        {
            using Stream Stream = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            Encode(Image, Stream);
        }
        public static void Encode(IImageContext Image, Stream Stream)
        {
            // Bitmap File Struct
            // https://crazycat1130.pixnet.net/blog/post/1345538#mark-4

            int Stride = (((Image.Width * Image.BitsPerPixel) >> 3) + 3) >> 2 << 2,
                PaletteByteLength = 4 << Image.BitsPerPixel,
                ImageSize = Stride * Image.Height,
                HeaderOffset = Image.BitsPerPixel > 8 ? 54 : 54 + PaletteByteLength,
                FileSize = ImageSize + HeaderOffset;
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
                0, 0, 0, 0,                                                                                             // NColors      , 4 Bytes
                0, 0, 0, 0,                                                                                             // ImportantColours  , 4 Bytes
            };
            Stream.Write(InfoDatas, 0, InfoDatas.Length);

            // Palette
            if (HeaderOffset > 54)
            {
                IImagePalette Palette = Image.Palette;
                byte[] Datas = new byte[sizeof(int)];
                if (Palette is null ||
                    Palette.Count == 0)
                {
                    int ColorStep = byte.MaxValue / ((1 << Image.BitsPerPixel) - 1);
                    for (int i = 0; i < 256; i += ColorStep)
                    {
                        Datas[0] = (byte)i;
                        Datas[1] = Datas[0];
                        Datas[2] = Datas[0];
                        Stream.Write(Datas, 0, Datas.Length);
                    }
                }
                else
                {
                    for (int i = 0; i < Palette.Count; i++)
                    {
                        IPixel Value = Palette[i];
                        Datas[0] = Value.B;
                        Datas[1] = Value.G;
                        Datas[2] = Value.R;
                        Stream.Write(Datas, 0, Datas.Length);
                    }

                    int CurrentLength = Palette.Count << 2;
                    if (CurrentLength < PaletteByteLength)
                    {
                        int EmptyLength = PaletteByteLength - CurrentLength;
                        Stream.Write(new byte[EmptyLength], 0, EmptyLength);
                    }
                }
            }

            // Datas
            byte[] ImageDatas = new byte[Stride];
            Action<int> DataCopyAction = Image.BitsPerPixel == 32 ? new Action<int>((j) => Image.ScanLineCopy<BGRA>(0, j, Image.Width, ImageDatas, 0)) :
                                         Image.BitsPerPixel == 8 ? (y) => Image.ScanLineCopy<Gray8>(0, y, Image.Width, ImageDatas, 0) :
                                         (y) => Image.ScanLineCopy<BGR>(0, y, Image.Width, ImageDatas, 0);

            for (int j = Image.Height - 1; j >= 0; j--)
            {
                DataCopyAction(j);
                Stream.Write(ImageDatas, 0, ImageDatas.Length);
            }

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

            // ImageSize
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

            // Palette
            int PaletteSize = Offset - (int)FS.Position;
            if (PaletteSize > 0)
            {
                Debug.WriteLine($"Palette     :");
                Datas = new byte[sizeof(int)];
                for (int i = 0; i < PaletteSize >> 2; i++)
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
