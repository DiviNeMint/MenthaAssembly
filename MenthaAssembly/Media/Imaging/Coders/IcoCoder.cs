using MenthaAssembly.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents an encoder for ICO file format.
    /// </summary>
    public unsafe static class IcoCoder
    {
        // ICO File Struct
        // ============================================================
        //                          File Header
        // ============================================================
        // Reserved             , 2 Bytes
        // Identifier           , 2 Bytes (1 for icon (.ICO) image, 2 for cursor (.CUR) image)
        // NumImages            , 2 Bytes
        // ============================================================
        //                           Entry * n                  
        // ============================================================
        // Width                , 2 Bytes
        // Height               , 2 Bytes
        // NumPaletteColors     , 2 Bytes
        // BitsPerPixel         , 2 Bytes
        // ImageSize            , 4 Bytes
        // DataOffset           , 4 Bytes
        // ============================================================
        //                            Context
        // ============================================================
        // ImageDatas[1]        , n Bytes
        // ...
        // ImageDatas[n]        , n Bytes
        // ============================================================

        /// <summary>
        /// The length in bytes of the ico file format identifier.
        /// </summary>
        public const int IdentifierSize = 4;

        /// <summary>
        /// Decodes a ico file from the specified path.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="Images">The decoded images.</param>
        public static bool TryDecode(string Path, out IImageContext[] Images)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, out Images);
        }
        /// <summary>
        /// Decodes a icon file from the specified stream.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Images">The decoded images.</param>
        public static bool TryDecode(Stream Stream, out IImageContext[] Images)
        {
            Images = null;
            long Begin = Stream.CanSeek ? Stream.Position : 0L;

            // Header
            if (!Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string Identifier) ||
                !Identify(Identifier) ||
                !Stream.TryRead(out ushort NumImages))
            {
                Stream.TrySeek(Begin, SeekOrigin.Begin);
                return false;
            }

            // Entries
            Dictionary<long, int> Entries = new();
            for (int i = 0; i < NumImages; i++)
            {
                // Reads ImageSize and DataOffset.
                if (!Stream.TrySeek(8, SeekOrigin.Current) ||
                    !Stream.TryRead(out int ImageSize) ||
                    !Stream.TryRead(out int DataOffset))
                    return false;

                Entries.Add(Begin + DataOffset, ImageSize);
            }

            // Images
            Images = new IImageContext[NumImages];
            const int MaxIdentifierSize = PngCoder.IdentifierSize;
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(MaxIdentifierSize);
            try
            {
                int Index = 0;
                while (Entries.Count > 0)
                {
                    long Position = Stream.Position;
                    if (!Entries.TryGetValue(Position, out int ImageSize) ||
                        !Stream.ReadBuffer(Buffer, 0, MaxIdentifierSize))
                    {
                        Images = null;
                        return false;
                    }

                    using SegmentStream Segment = new(Stream, ImageSize - MaxIdentifierSize, true);

                    Identifier = Encoding.ASCII.GetString(Buffer, 0, MaxIdentifierSize);
                    IImageContext Image;
                    if (PngCoder.Identify(Identifier))
                    {
                        using ConcatStream Concat = new(Buffer, 0, MaxIdentifierSize, Segment);
                        if (!PngCoder.TryDecode(Concat, out Image))
                        {
                            Images = null;
                            return false;
                        }
                    }
                    else if (BmpCoder.Identify(Identifier))
                    {
                        using ConcatStream Concat = new(Buffer, 0, MaxIdentifierSize, Segment);
                        if (!BmpCoder.TryDecode(Stream, out Image))
                        {
                            Images = null;
                            return false;
                        }
                    }
                    else
                    {
                        Images = null;
                        return false;
                    }

                    Entries.Remove(Position);
                    Images[Index++] = Image;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }

            return true;
        }

        /// <summary>
        /// Encodes the specified images to the specified path.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="Images">The specified image.</param>
        public static void Encode(string Path, params IImageContext[] Images)
        {
            using FileStream Stream = new(Path, FileMode.CreateNew, FileAccess.Write);
            Encode(Stream, Images);
        }
        /// <summary>
        /// Encodes the specified images to the specified stream.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Images">The specified images.</param>
        public static void Encode(Stream Stream, params IImageContext[] Images)
        {
            int NumImages = Images.Length;

            // Ico File Header
            IconFileHeader Header = new() { NumImages = (ushort)NumImages };
            Stream.Write(Header);

            MemoryStream Data = new(32768);
            try
            {
                long DataOffset0 = sizeof(IconFileHeader) + sizeof(IconEntry) * NumImages;
                for (int i = 0; i < NumImages; i++)
                {
                    IImageContext Image = Images[i];

                    // Checks Image size.
                    int Iw = Image.Width,
                        Ih = Image.Height;
                    if (Iw > 256 || Ih > 256)
                    {
                        if (Iw > Ih)
                        {
                            Ih = Ih * 256 / Iw;
                            Iw = 256;
                        }
                        else
                        {
                            Iw = Iw * 256 / Ih;
                            Ih = 256;
                        }

                        Image = Image.Resize<RGBA>(Iw, Ih, InterpolationTypes.Nearest);
                    }

                    // Encodes ImageDatas
                    long Position = Data.Position;
                    PngCoder.Encode(Image, Data);

                    // Entry
                    IconEntry Entry = new()
                    {
                        Width = (byte)Iw,
                        Height = (byte)Ih,
                        NumPaletteColors = (byte)(Image is IImageIndexedContext IndexedImage ? IndexedImage.Palette.Count : 0),
                        BitsPerPixel = (short)Image.BitsPerPixel,
                        ImageSize = (int)(Data.Position - Position),
                        DataOffset = (int)(DataOffset0 + Position)
                    };
                    Stream.Write(Entry);
                }

                Data.Position = 0;
                Data.CopyTo(Stream);
            }
            finally
            {
                Data.Dispose();
            }
        }

        /// <summary>
        /// Indicates whether the specified Identifier is Icon Identifier.
        /// </summary>
        /// <param name="Identifier">The specified Identifier.</param>
        public static bool Identify(string Identifier)
            => Identifier.Length == IdentifierSize &&
               Identifier == "\0\0\u0001\0";

        [Conditional("DEBUG")]
        public static void Parse(Stream Stream)
        {
            // Identifier
            if (!Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string Identifier) ||
                !Identify(Identifier))
            {
                Debug.WriteLine("This is not Ico file.");
                return;
            }

            // NumImages
            if (!Stream.TryRead(out ushort NumImages))
                return;

            Debug.WriteLine($"NumImages         : {NumImages}");
            for (int i = 0; i < NumImages; i++)
            {
                if (!Stream.TryRead(out IconEntry Entry))
                    return;

                Debug.WriteLine($"================ Entry {i} =================");
                Debug.WriteLine($"Width             : {(Entry.Width == 0 ? 256 : Entry.Width)}");
                Debug.WriteLine($"Height            : {(Entry.Height == 0 ? 256 : Entry.Height)}");
                Debug.WriteLine($"NumPaletteColors  : {Entry.NumPaletteColors}");
                Debug.WriteLine($"BitsPerPixel      : {Entry.BitsPerPixel}");
                Debug.WriteLine($"ImageSize         : {Entry.ImageSize}");
                Debug.WriteLine($"DataOffset        : {Entry.DataOffset}");
            }
            Debug.WriteLine($"============================================");
        }

        [StructLayout(LayoutKind.Explicit, Size = 6)]
        private struct IconFileHeader
        {
            [FieldOffset(0)]
            private fixed byte Reserved[2];

            [FieldOffset(2)]
            private readonly byte Identifier1 = 1;

            [FieldOffset(3)]
            private readonly byte Identifier2;

            [FieldOffset(4)]
            public ushort NumImages;

            public IconFileHeader()
            {
            }

        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct IconEntry
        {
            [FieldOffset(0)]
            public byte Width;

            [FieldOffset(1)]
            public byte Height;

            [FieldOffset(2)]
            public byte NumPaletteColors;

            [FieldOffset(3)]
            private readonly byte Reserved;

            [FieldOffset(4)]
            private readonly short Plane = 1;

            [FieldOffset(6)]
            public short BitsPerPixel;

            [FieldOffset(8)]
            public int ImageSize;

            [FieldOffset(12)]
            public int DataOffset;

            public IconEntry()
            {

            }

        }

    }
}