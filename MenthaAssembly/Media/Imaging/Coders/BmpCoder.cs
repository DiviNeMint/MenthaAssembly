using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Buffers;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents an encoder for bitmap file format.
    /// </summary>
    public static unsafe class BmpCoder
    {
        // Bitmap File Struct
        // https://crazycat1130.pixnet.net/blog/post/1345538#mark-4
        // https://en.wikipedia.org/wiki/BMP_file_format
        // ============================================================
        //                          File Header
        // ============================================================
        // Identifier           , 2 Bytes
        // FileSize             , 4 Bytes
        // Reserved             , 4 Bytes
        // DataOffset           , 4 Bytes (54 bytes for common header struct)
        // ============================================================
        //                          Info Header
        // ============================================================
        // InfoSize             , 4 Bytes (40 Bytes for common info struce)
        // Width                , 4 Bytes
        // Height               , 4 Bytes
        // Planes               , 2 Bytes (forever be set 1.)
        // BitsPerPixel         , 2 Bytes
        // Compression          , 4 Bytes
        // ImageSize            , 4 Bytes
        // XResolution          , 4 Bytes (Dpi * 39.37)
        // YResolution          , 4 Bytes
        // NumPaletteColors     , 4 Bytes
        // ImportantColors      , 4 Bytes
        // ============================================================
        //                            Context
        // ============================================================
        // PaletteEntries       , 4 * NumPaletteColors Bytes
        // ImageDatas           , ImageSize Bytes
        // ============================================================

        public const int IdentifierSize = 2;

        /// <summary>
        /// Decodes a bitmap file from the specified path.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="Image">The decoded bitmap.</param>
        public static bool TryDecode(string Path, out IImageContext Image)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, out Image);
        }
        /// <summary>
        /// Decodes a bitmap file from the specified stream.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Image">The decoded bitmap.</param>
        public static bool TryDecode(Stream Stream, out IImageContext Image)
        {
            Image = null;
            long Begin = Stream.CanSeek ? Stream.Position : 0L;

            // Header
            if (!Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string Identifier) ||
                !Identify(Identifier) ||
                !Stream.TrySeek(8, SeekOrigin.Current) ||
                !Stream.TryRead(out int DataOffset) ||
                !Stream.TryRead(out int HeaderSize) ||
                !Stream.TryRead(out int Width) ||
                !Stream.TryRead(out int Height) ||
                !Stream.TrySeek(2, SeekOrigin.Current) ||
                !Stream.TryRead(out short Bits) ||
                !Stream.TrySeek(16, SeekOrigin.Current))
            {
                Stream.TrySeek(Begin, SeekOrigin.Begin);
                return false;
            }

            long Current = 46L;

            // Palette
            ImagePalette<BGRA> Palette = null;
            int PaletteOffset = 14 + HeaderSize;    // File Header + Info Header
            if (PaletteOffset < DataOffset)
            {
                // NColors
                if (!Stream.TryRead(out int NColors))
                {
                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                    return false;
                }

                if (NColors == 0)
                    NColors = 1 << Bits;

                Current += 4L;
                long Offset = Begin + PaletteOffset - Current;
                if (!Stream.TrySeek(Offset, SeekOrigin.Current))
                    return false;

                const int EntrySize = 4;
                byte[] Entry = ArrayPool<byte>.Shared.Rent(EntrySize);
                try
                {
                    Palette = new ImagePalette<BGRA>(Bits);
                    for (int i = 0; i < NColors; i++)
                    {
                        if (!Stream.ReadBuffer(Entry, 0, EntrySize))
                            return false;

                        Palette.Datas.Add(new BGRA(Entry[0], Entry[1], Entry[2], byte.MaxValue));
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(Entry);
                }

                Current += Offset + NColors * EntrySize;
            }

            // ImageDatas
            if (!Stream.TrySeek(Begin + DataOffset - Current, SeekOrigin.Current))
                return false;

            int Stride = (((Width * Bits) >> 3) + 3) & 2147483644;
            byte[] ImageDatas;
            if (Height < 0)
            {
                Height = ~Height + 1;   // Abs

                ImageDatas = new byte[Stride * Height];
                if (!Stream.ReadBuffer(ImageDatas))
                    return false;
            }
            else
            {
                ImageDatas = new byte[Stride * Height];
                for (int j = Height - 1, Offset = Stride * j; j >= 0; j--, Offset -= Stride)
                    if (!Stream.ReadBuffer(ImageDatas, Offset, Stride))
                        return false;
            }

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

            return false;
        }

        /// <summary>
        /// Encodes the specified image to the specified path.
        /// </summary>
        /// <param name="Image">The specified image.</param>
        /// <param name="Path">The specified path.</param>
        public static void Encode(IImageContext Image, string Path)
        {
            using Stream Stream = new FileStream(Path, FileMode.CreateNew, FileAccess.Write);
            Encode(Image, Stream);
        }
        /// <summary>
        /// Encodes the specified image to the specified stream.
        /// </summary>
        /// <param name="Image">The specified image.</param>
        /// <param name="Stream">The specified stream.</param>
        public static void Encode(IImageContext Image, Stream Stream)
        {
            int Iw = Image.Width,
                Ih = Image.Height,
                Bits = Image.BitsPerPixel,
                Stride = (((Iw * Bits) >> 3) + 3) & 2147483644,
                ImageSize = Stride * Ih,
                PaletteNum = 0,
                PaletteSize = 0,
                DataOffset = 54;

            if (Bits < 8)
            {
                PaletteNum = 1 << Bits;
                PaletteSize = PaletteNum << 2;
                DataOffset += PaletteSize;
            }

            // Bitmap File Header
            BitmapFileHeader Header = new()
            {
                FileSize = DataOffset + ImageSize,
                DataOffset = DataOffset,
                Width = Iw,
                Height = Ih,
                BitsPerPixel = (short)Bits,
                ImageSize = ImageSize,
            };
            Stream.Write(Header);

            // Palette
            if (DataOffset > 54)
            {
                const int EntryLength = 4;
                byte[] Entry = ArrayPool<byte>.Shared.Rent(EntryLength);
                Entry[3] = 0;

                try
                {
                    // Indexed Colors
                    if (Image is IImageIndexedContext IndexedContext)
                    {
                        IImagePalette Palette = IndexedContext.Palette;
                        for (int i = 0; i < Palette.Count; i++)
                        {
                            IReadOnlyPixel Value = Palette[i];
                            Entry[0] = Value.B;
                            Entry[1] = Value.G;
                            Entry[2] = Value.R;
                            Stream.Write(Entry, 0, EntryLength);
                        }

                        int Num = Palette.Count;
                        if (Num < PaletteNum)
                        {
                            int Length = PaletteSize - (Num << 2);
                            Stream.Write(new byte[Length], 0, Length);
                        }
                    }

                    // Gray Colors
                    else
                    {
                        int ColorStep = byte.MaxValue / (PaletteNum - 1);
                        for (int i = 0; i < 256; i += ColorStep)
                        {
                            Entry[0] = Entry[1] = Entry[2] = (byte)i;
                            Stream.Write(Entry, 0, EntryLength);
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(Entry);
                }
            }

            // ImageDatas
            void WriteImageGenericDatas<T>() where T : unmanaged, IPixel
            {
                byte[] ScanLineData = ArrayPool<byte>.Shared.Rent(Stride);
                try
                {
                    int Sy = Ih - 1,
                        Dx = -Iw;
                    fixed (byte* pScanLineData = ScanLineData)
                    {
                        T* pData0 = (T*)pScanLineData;
                        PixelAdapter<T> Adapter = Image.GetAdapter<T>(0, Sy);
                        for (; Sy >= 0; Sy--, Adapter.DangerousMovePreviousY())
                        {
                            T* pData = pData0;
                            for (int i = 0; i < Iw; i++, Adapter.DangerousMoveNextX())
                                Adapter.OverrideTo(pData++);

                            Adapter.DangerousOffsetX(Dx);
                            Stream.Write(ScanLineData, 0, Stride);
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(ScanLineData);
                }
            }

            Action WriteImageDatas = Bits switch
            {
                8 => WriteImageGenericDatas<Gray8>,
                32 => WriteImageGenericDatas<BGRA>,
                _ => WriteImageGenericDatas<BGR>,
            };
            WriteImageDatas();
        }

        /// <summary>
        /// Indicates whether the specified Identifier is bitmap Identifier.<para/>
        /// BM – Windows 3.1x, 95, NT, ... etc.<para/>
        /// BA – OS / 2 struct Bitmap Array<para/>
        /// CI – OS / 2 struct Color Icon<para/>
        /// CP – OS / 2 const Color Pointer<para/>
        /// IC – OS / 2 struct Icon<para/>
        /// PT – OS / 2 Pointer<para/>
        /// </summary>
        /// <param name="Identifier">The specified Identifier.</param>
        public static bool Identify(string Identifier)
            => Identifier.Length == IdentifierSize &&
               Identifier is "BM" or "BA" or "CI" or "CP" or "IC" or "PT";

        [Conditional("DEBUG")]
        public static void Parse(string Path)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);

            // Identifier
            if (!Stream.TryReadString(2, Encoding.ASCII, out string Identifier) ||
                !Identify(Identifier))
            {
                Debug.WriteLine("This is not Bmp file.");
                return;
            }

            // FileSize
            if (!Stream.TryRead(out int FileSize))
                return;

            Debug.WriteLine($"FileSize    : {FileSize}");

            // Reserved
            if (!Stream.TryRead(4, out _))
                return;

            // Offset
            if (!Stream.TryRead(out int Offset))
                return;

            Debug.WriteLine($"Offset      : {Offset}");

            // HeaderSize
            if (!Stream.TryRead(out int HeaderSize))
                return;

            Debug.WriteLine($"HeaderSize  : {HeaderSize}");

            // Width
            if (!Stream.TryRead(out int Width))
                return;

            Debug.WriteLine($"Width       : {Width}");

            // Height
            if (!Stream.TryRead(out int Height))
                return;

            Debug.WriteLine($"Height      : {Height}");

            // Planes
            if (!Stream.TryRead(out short Planes))
                return;

            Debug.WriteLine($"Planes      : {Planes}");

            // BitsPerPixel
            if (!Stream.TryRead(out short Bits))
                return;

            int Channels = (Bits + 7) >> 3;
            int Stride = (((Width * Bits) >> 3) + 3) >> 2 << 2;
            Debug.WriteLine($"Bits        : {Bits}\r\n" +
                            $"Channels    : {Channels}\r\n" +
                            $"Stride      : {Stride}");

            // Compression
            if (!Stream.TryRead(out int Compression))
                return;

            Debug.WriteLine($"Compression : {Compression}");

            // ImageSize
            if (!Stream.TryRead(out int ImageSize))
                return;

            Debug.WriteLine($"ImageSize   : {ImageSize}");

            // XResolution
            if (!Stream.TryRead(out int XResolution))
                return;

            Debug.WriteLine($"XResolution : {XResolution}");

            // YResolution
            if (!Stream.TryRead(out int YResolution))
                return;

            Debug.WriteLine($"YResolution : {YResolution}");

            // NColors
            if (!Stream.TryRead(out int NColors))
                return;

            Debug.WriteLine($"NColors     : {NColors}");

            // ImportantColors
            if (!Stream.TryRead(out int ImportantColors))
                return;

            Debug.WriteLine($"ImpColors   : {ImportantColors}");

            // Palette
            int PaletteSize = Offset - (int)Stream.Position;
            if (PaletteSize > 0)
            {
                Debug.WriteLine($"Palette     :");
                byte[] Datas = new byte[sizeof(int)];
                for (int i = 0; i < PaletteSize >> 2; i++)
                {
                    Stream.Read(Datas, 0, Datas.Length);
                    if (i > 99)
                        Debug.WriteLine($"{i} : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                    else if (i > 9)
                        Debug.WriteLine($"{i}  : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                    else
                        Debug.WriteLine($"{i}   : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                }
            }

            Stream.Close();
        }

        [StructLayout(LayoutKind.Explicit, Size = 54)]
        private struct BitmapFileHeader
        {
            [FieldOffset(0)]
            private readonly byte Identifier1 = (byte)'B';

            [FieldOffset(1)]
            private readonly byte Identifier2 = (byte)'M';

            [FieldOffset(2)]
            public int FileSize;

            [FieldOffset(6)]
            private fixed byte Reserved[4];

            [FieldOffset(10)]
            public int DataOffset;

            [FieldOffset(14)]
            private readonly int InfoSize = 40;

            [FieldOffset(18)]
            public int Width;

            [FieldOffset(22)]
            public int Height;

            [FieldOffset(26)]
            private readonly short Planes = 1;

            [FieldOffset(28)]
            public short BitsPerPixel;

            [FieldOffset(30)]
            public int Compression;

            [FieldOffset(34)]
            public int ImageSize;

            [FieldOffset(38)]
            public int XResolution;

            [FieldOffset(42)]
            public int YResolution;

            [FieldOffset(46)]
            public int NumPaletteColors;

            [FieldOffset(50)]
            public int ImportantColors;

            public BitmapFileHeader()
            {
            }

        }

    }
}