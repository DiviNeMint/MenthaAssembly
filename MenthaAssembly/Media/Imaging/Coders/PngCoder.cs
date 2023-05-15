using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents an encoder for Png file format.
    /// </summary>
    public unsafe static class PngCoder
    {
        // PNG File Struct
        // https://www.w3.org/TR/PNG/
        // ============================================================
        //                          File Header
        // ============================================================
        // Identifier           , 8 Bytes
        // ============================================================
        //                            Chunks
        // ============================================================
        // Length               , 4 Bytes
        // TypeCode             , 4 Bytes
        // Datas                , n Bytes
        // CRC32                , 4 Bytes
        // ============================================================

        public const int IdentifierSize = 8;

        private const int ChunkTypeCodeSize = 4;
        private const int CRC32CodeSize = sizeof(int);

        /// <summary>
        /// Decode a png file from the specified path without verifying the datas.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(string Path, out IImageContext Image)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, false, out Image);
        }
        /// <summary>
        /// Decodes a png file from the specified path.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="VerifyDatas">Determine whether to verify the data.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(string Path, bool VerifyDatas, out IImageContext Image)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, VerifyDatas, out Image);
        }
        /// <summary>
        /// Decodes a png file from the specified stream without verifying the datas.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(Stream Stream, out IImageContext Image)
            => TryDecode(Stream, false, out Image);
        /// <summary>
        /// Decodes a png file from the specified stream.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="VerifyDatas">Determine whether to verify the data.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(Stream Stream, bool VerifyDatas, out IImageContext Image)
        {
            Image = null;
            long Begin = Stream.CanSeek ? Stream.Position : 0L;

            // Header
            if (!Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string Identifier) ||
                !Identify(Identifier))
            {
                Stream.TrySeek(Begin, SeekOrigin.Begin);
                return false;
            }

            int Width = 0,
                Height = 0,
                Bits = 0,
                Channels = 0,
                Stride = 0;
            byte[][] ImageDatas = null;
            IList<BGRA> Palette = null;
            //byte Compression,
            //Filter,
            //Interlace;

            MemoryStream DataStream = new();
            DeflateStream Decompressor = new(DataStream, CompressionMode.Decompress);
            int IDAT_Offset = 0,
                IDAT_DecodeLength = 0,
                IDAT_DecodeHeight = 0;
            byte[] DecodeDatas = null,
                   TypeCode = ArrayPool<byte>.Shared.Rent(ChunkTypeCodeSize),
                   Datas = null;
            try
            {
                do
                {
                    if (!Stream.TryReverseRead(out int Length) ||
                        !Stream.ReadBuffer(TypeCode, 0, ChunkTypeCodeSize))
                    {
                        Stream.TrySeek(Begin, SeekOrigin.Begin);
                        return false;
                    }

                    #region IEND
                    if (TypeCode[0].Equals(0x49) && TypeCode[1].Equals(0x45) && TypeCode[2].Equals(0x4E) && TypeCode[3].Equals(0x44))
                        break;

                    #endregion

                    #region IHDR
                    if (TypeCode[0].Equals(0x49) && TypeCode[1].Equals(0x48) && TypeCode[2].Equals(0x44) && TypeCode[3].Equals(0x52))
                    {
                        Datas = new byte[Length];
                        Stream.Read(Datas, 0, Datas.Length);

                        Width = Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3];
                        Height = Datas[4] << 24 | Datas[5] << 16 | Datas[6] << 8 | Datas[7];
                        switch (Datas[9])
                        {
                            case 0:
                            case 3:
                                Bits = Datas[8];
                                break;
                            case 2:
                                Bits = 24;
                                break;
                            case 4:
                            case 6:
                                Bits = 32;
                                break;
                        }

                        //Compression = Datas[10];           // Always be 0 (ZibCompress).
                        //Filter = Datas[11];                // Always be 0.
                        //Interlace = Datas[12];   
                        Channels = (Bits + 7) >> 3;
                        Stride = (Width * Bits + 7) >> 3; //(((Width * Bits) >> 3) + 3) >> 2 << 2;
                    }

                    #endregion

                    // sBIT
                    //if (TypeCode[0].Equals(0x73) && TypeCode[1].Equals(0x42) && TypeCode[2].Equals(0x49) && TypeCode[3].Equals(0x54)) { }

                    #region PLTE
                    else if (TypeCode[0].Equals(0x50) && TypeCode[1].Equals(0x4C) && TypeCode[2].Equals(0x54) && TypeCode[3].Equals(0x45))
                    {
                        Datas = new byte[Length];
                        Stream.Read(Datas, 0, Datas.Length);

                        Palette = new List<BGRA>();
                        for (int i = 0; i < Length; i += 3)
                            Palette.Add(new BGRA(Datas[i + 2], Datas[i + 1], Datas[i], byte.MaxValue));
                    }
                    #endregion

                    // pHYs
                    //if (TypeCode[0].Equals(0x70) && TypeCode[1].Equals(0x48) && TypeCode[2].Equals(0x59) && TypeCode[3].Equals(0x73)) { }
                    //else

                    #region tEXt
                    else if (TypeCode[0].Equals(0x74) && TypeCode[1].Equals(0x45) && TypeCode[2].Equals(0x58) && TypeCode[3].Equals(0x74))
                    {
                        Datas = new byte[Length];
                        Stream.Read(Datas, 0, Datas.Length);

                        int SeparatorIndex = Datas.IndexOf(i => i.Equals(0x00));
                        if (SeparatorIndex > 0)
                        {
                            string Property = Encoding.Default.GetString(Datas, 0, SeparatorIndex),
                                   Content = Encoding.Default.GetString(Datas, SeparatorIndex, Length - SeparatorIndex);
                            Debug.WriteLine($"{Property} : {Content}");
                        }
                    }

                    #endregion

                    #region IDAT
                    else if (TypeCode[0].Equals(0x49) && TypeCode[1].Equals(0x44) && TypeCode[2].Equals(0x41) && TypeCode[3].Equals(0x54))
                    {
                        Datas = new byte[Length];
                        Stream.Read(Datas, 0, Datas.Length);

                        DataStream.Position = 0;
#if NETSTANDARD2_1
                        DataStream.Write(Datas.AsSpan(IdentifyRFC1950(Datas) ? 2 : 0));
#else
                    if (IdentifyRFC1950(Datas))
                        DataStream.Write(Datas, 2, Datas.Length - 2);
                    else
                        DataStream.Write(Datas, 0, Datas.Length);
#endif
                        DataStream.Position = 0;

                        // ImageDatas
                        int ChannelStride = Channels > 1 ? Width : Stride,
                            ChannelSize = ChannelStride * Height;
                        if (ImageDatas is null)
                            ImageDatas = new byte[Channels][];
                        if (DecodeDatas is null)
                            DecodeDatas = new byte[Stride + 1];

                        for (int j = IDAT_DecodeHeight; j < Height; j++)
                        {
                            // Check First IDAT Chunks
                            if (IDAT_DecodeLength.Equals(0) ||
                                IDAT_DecodeLength.Equals(DecodeDatas.Length))
                            {
                                IDAT_DecodeLength = Decompressor.Read(DecodeDatas, 0, DecodeDatas.Length);
                            }
                            else
                            {
                                IDAT_DecodeLength = Decompressor.Read(DecodeDatas, IDAT_DecodeLength, DecodeDatas.Length - IDAT_DecodeLength);
                                IDAT_DecodeLength = DecodeDatas.Length;
                            }
                            if (IDAT_DecodeLength < DecodeDatas.Length)
                                break;

                            // IDAT Filter
                            //https://www.w3.org/TR/2003/REC-PNG-20031110/#7Filtering
                            switch (DecodeDatas[0])
                            {
                                case 1:     // Sub
                                    {
                                        Parallel.For(0, ImageDatas.Length,
                                            (c) =>
                                            {
                                                if (ImageDatas[c] is null)
                                                    ImageDatas[c] = new byte[ChannelSize];

                                                ImageDatas[c][IDAT_Offset] = DecodeDatas[c + 1];
                                                for (int i = 1; i < ChannelStride; i++)
                                                    ImageDatas[c][IDAT_Offset + i] = (byte)(DecodeDatas[i * Channels + c + 1] +
                                                                                            ImageDatas[c][IDAT_Offset + i - 1]);
                                            });
                                        break;
                                    }
                                case 2:     // LastLine
                                    {
                                        Parallel.For(0, ImageDatas.Length,
                                            (c) =>
                                            {
                                                if (ImageDatas[c] is null)
                                                    ImageDatas[c] = new byte[ChannelSize];
                                                for (int i = 0; i < ChannelStride; i++)
                                                    ImageDatas[c][IDAT_Offset + i] = (byte)(DecodeDatas[i * Channels + c + 1] +
                                                                                            ImageDatas[c][IDAT_Offset - ChannelStride + i]);
                                            });
                                        break;
                                    }
                                case 3:     // Average
                                    {
                                        Parallel.For(0, ImageDatas.Length,
                                            (c) =>
                                            {
                                                if (ImageDatas[c] is null)
                                                    ImageDatas[c] = new byte[ChannelSize];

                                                // FirstData (no last data)
                                                ImageDatas[c][IDAT_Offset] = (byte)(DecodeDatas[c + 1] +
                                                                                    Math.Floor(ImageDatas[c][IDAT_Offset - ChannelStride] / 2d));
                                                for (int i = 1; i < ChannelStride; i++)
                                                {
                                                    int Index = IDAT_Offset + i;
                                                    ImageDatas[c][Index] = (byte)(DecodeDatas[i * Channels + c + 1] +
                                                                                  Math.Floor((ImageDatas[c][Index - ChannelStride] + ImageDatas[c][Index - 1]) / 2d));

                                                }
                                            });
                                        break;
                                    }
                                case 4:     // Paeth
                                    {
                                        Parallel.For(0, ImageDatas.Length,
                                            (c) =>
                                            {
                                                if (ImageDatas[c] is null)
                                                    ImageDatas[c] = new byte[ChannelSize];

                                                // FirstData (no last data)
                                                ImageDatas[c][IDAT_Offset] = (byte)(DecodeDatas[c + 1] + ImageDatas[c][IDAT_Offset - ChannelStride]);
                                                for (int i = 1; i < ChannelStride; i++)
                                                {
                                                    int Index = IDAT_Offset + i;
                                                    byte Last = ImageDatas[c][Index - 1];
                                                    byte PreviousLine = ImageDatas[c][Index - ChannelStride];
                                                    byte PreviousLineLast = ImageDatas[c][Index - ChannelStride - 1];

                                                    ImageDatas[c][Index] = (byte)(DecodeDatas[i * Channels + c + 1] +
                                                                                  CalculatePaeth(ImageDatas[c][Index - 1], ImageDatas[c][Index - ChannelStride], ImageDatas[c][Index - ChannelStride - 1]));
                                                }
                                            });
                                        break;
                                    }
                                case 0:     // None
                                default:
                                    {
                                        Parallel.For(0, ImageDatas.Length,
                                            (c) =>
                                            {
                                                if (ImageDatas[c] is null)
                                                    ImageDatas[c] = new byte[ChannelSize];
                                                for (int i = 0; i < ChannelStride; i++)
                                                    ImageDatas[c][IDAT_Offset + i] = DecodeDatas[i * Channels + c + 1];
                                            });
                                        break;
                                    }
                            }
                            IDAT_Offset += ChannelStride;
                            IDAT_DecodeHeight++;
                        }
                    }

                    #endregion

                    #region Other
                    else if (Length > 0)
                    {
                        // Skip Datas & CRC32.
                        if (!Stream.TrySeek(Length + ChunkTypeCodeSize, SeekOrigin.Current))
                        {
                            Image = null;
                            return false;
                        }

                        continue;
                    }

                    #endregion

                    #region CRC32
                    // Verify CRC32
                    if (VerifyDatas)
                    {
                        CRC32.Calculate(TypeCode, 0, ChunkTypeCodeSize, out uint NextRegister);
                        CRC32.Calculate(Datas: Datas, out int CRCResult, NextRegister);
                        if (!Stream.TryReverseRead(out int CRC32Code) ||
                            CRCResult != CRC32Code)
                        {
                            Image = null;
                            return false;
                        }
                    }

                    // Skip CRC32.
                    else if (!Stream.TrySeek(CRC32CodeSize, SeekOrigin.Current))
                    {
                        Image = null;
                        return false;
                    }

                    #endregion

                } while (Stream.Position < Stream.Length);
            }
            finally
            {
                Decompressor.Dispose();
                DataStream.Dispose();
                //Stream.Dispose();

                //ArrayPool<byte>.Shared.Return(Datas);
                ArrayPool<byte>.Shared.Return(TypeCode);
            }

            switch (ImageDatas.Length)
            {
                case 1:
                    Image = new ImageContext<Gray8>(Width, Height, ImageDatas[0]);
                    //Image = new ImageContext<Gray8>(Width, Height, ImageDatas[0], Palette?.Cast<Gray8>().ToList());
                    return true;
                case 3:
                    Image = new ImageContext<BGR>(Width, Height, ImageDatas[0], ImageDatas[1], ImageDatas[2]);
                    return true;
                case 4:
                    Image = new ImageContext<BGRA>(Width, Height, ImageDatas[3], ImageDatas[0], ImageDatas[1], ImageDatas[2]);
                    return true;
            }
            Image = null;
            return false;
        }

        /// <summary>
        /// Encodes the specified image to the specified path.
        /// </summary>
        /// <param name="Image">The specified image.</param>
        /// <param name="Path">The specified path.</param>
        public static void Encode(IImageContext Image, string Path)
        {
            using FileStream Stream = new(Path, FileMode.CreateNew, FileAccess.Write);
            Encode(Image, Stream);
        }
        /// <summary>
        /// Encodes the specified image to the specified stream.
        /// </summary>
        /// <param name="Image">The specified image.</param>
        /// <param name="Stream">The specified stream.</param>
        public static void Encode(IImageContext Image, Stream Stream)
        {
            // IdentifyHeader
            byte[] Datas = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Stream.Write(Datas, 0, Datas.Length);

            #region IHDR
            WriteChunk(Stream,
                       0x49, 0x48, 0x44, 0x52,
                       (byte)(Image.Width >> 24), (byte)(Image.Width >> 16), (byte)(Image.Width >> 8), (byte)Image.Width,       // Width     , 4 Bytes
                       (byte)(Image.Height >> 24), (byte)(Image.Height >> 16), (byte)(Image.Height >> 8), (byte)Image.Height,   // Height    , 4 Bytes
                       Image.BitsPerPixel <= 8 ? (byte)Image.BitsPerPixel : (byte)0x08,                                         // Bit Depth , 1 Bytes
                       Image.BitsPerPixel <= 8 ? (byte)0x03 : (Image.BitsPerPixel.Equals(24) ? (byte)0x02 : (byte)0x06),        // ColorType
                       0x00,                                                                                                    // Compression
                       0x00,                                                                                                    // Filter
                       0x00);                                                                                                   // Interlace

            #endregion

            #region PLTE
            if (Image.BitsPerPixel <= 8)
            {
                if (Image is IImageIndexedContext IndexedContext)
                {
                    IImagePalette Palette = IndexedContext.Palette;
                    Datas = new byte[Palette.Count * 3];
                    for (int i = 0; i < Palette.Count; i++)
                    {
                        IReadOnlyPixel Value = Palette[i];
                        Datas[i * 3] = Value.R;
                        Datas[i * 3 + 1] = Value.G;
                        Datas[i * 3 + 2] = Value.B;
                    }
                }
                else
                {
                    Datas = new byte[3 << Image.BitsPerPixel];
                    int ColorStep = byte.MaxValue / ((1 << Image.BitsPerPixel) - 1);
                    for (int i = 0; i < 256; i += ColorStep)
                    {
                        Datas[i * 3] = (byte)i;
                        Datas[i * 3 + 1] = Datas[0];
                        Datas[i * 3 + 2] = Datas[0];
                    }
                }

                WriteChunk(Stream,
                           new byte[] { 0x50, 0x4C, 0x54, 0x45 },
                           Datas);
            }

            #endregion

            #region IDAT
            // ImageDatas
            int Stride = (Image.Width * Image.BitsPerPixel + 7) >> 3;
            byte[] ImageDatas = new byte[Stride + 1];

            using MemoryStream DataStream = new MemoryStream();
            // Mark LZ77 Compress
            DataStream.Write(new byte[] { 0x78, 0xDA }, 0, 2);

            Stream Compressor = new DeflateStream(DataStream, CompressionLevel.Optimal, true);

            byte* pImageDatas = ImageDatas.ToPointer(1);
            Action<int> DataCopyAction = Image.BitsPerPixel == 32 ? y => Image.ScanLineCopy<RGBA>(0, y, Image.Width, pImageDatas) :
                                                                    y => Image.ScanLineCopy<RGB>(0, y, Image.Width, pImageDatas);
            for (int j = 0; j < Image.Height; j++)
            {
                DataCopyAction(j);
                Compressor.Write(ImageDatas, 0, ImageDatas.Length);
            }

            Compressor.Dispose();
            WriteChunk(Stream,
                       new byte[] { 0x49, 0x44, 0x41, 0x54 },
                       DataStream.ToArray());
            #endregion

            #region tEXt
            //bool HasDescription = false;
            //if (HasDescription)
            //{
            //    List<byte> TextDatas = new List<byte>();
            //    TextDatas.AddRange(Encoding.Default.GetBytes("Property"));
            //    TextDatas.Add(0x00);
            //    TextDatas.AddRange(Encoding.Default.GetBytes("Content)"));
            //    WriteChunk(Stream,
            //               new byte[] { 0x74, 0x45, 0x58, 0x74 },
            //               TextDatas.ToArray());
            //}

            #endregion

            #region IEND
            byte[] IENDChunk =
            {
                0x00, 0x00, 0x00, 0x00,
                0x49, 0x45, 0x4E, 0x44,
                0xAE, 0x42, 0x60, 0x82
            };
            Stream.Write(IENDChunk, 0, IENDChunk.Length);

            #endregion

        }

        /// <summary>
        /// Chunk Struct<para></para>
        /// Length (Data's Length)<para></para>
        /// TypeCode (ASCII)<para></para>
        /// Datas<para></para>
        /// CRC32 ( Include TypeCode and Datas)
        /// </summary>
        private static void WriteChunk(Stream Stream, byte[] TypeCode, byte[] Datas)
        {
            // Length
            byte[] Buffers = { (byte)(Datas.Length >> 24), (byte)(Datas.Length >> 16), (byte)(Datas.Length >> 8), (byte)Datas.Length };
            Stream.Write(Buffers, 0, Buffers.Length);
            // TypeCode
            Stream.Write(TypeCode, 0, TypeCode.Length);
            // Datas
            Stream.Write(Datas, 0, Datas.Length);

            CRC32.Calculate(Datas: TypeCode, out uint NewRegister);
            CRC32.Calculate(Datas: Datas, out int CRCResult, NewRegister);
            Buffers = new byte[] { (byte)(CRCResult >> 24), (byte)(CRCResult >> 16), (byte)(CRCResult >> 8), (byte)CRCResult };
            Stream.Write(Buffers, 0, Buffers.Length);
        }
        private static void WriteChunk(Stream Stream, params byte[] ChunkDatas)
        {
            // Length
            int Length = ChunkDatas.Length - 4;
            byte[] Buffers = { (byte)(Length >> 24), (byte)(Length >> 16), (byte)(Length >> 8), (byte)Length };
            Stream.Write(Buffers, 0, Buffers.Length);

            // ChunkDatas
            Stream.Write(ChunkDatas, 0, ChunkDatas.Length);

            CRC32.Calculate(Datas: ChunkDatas, out int CRCResult);
            Buffers = new byte[] { (byte)(CRCResult >> 24), (byte)(CRCResult >> 16), (byte)(CRCResult >> 8), (byte)CRCResult };
            Stream.Write(Buffers, 0, Buffers.Length);
        }

        /// <summary>
        /// ∥c∥b∥
        /// <para></para>
        /// ∥a∥x∥
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        private static byte CalculatePaeth(byte a, byte b, byte c)
        {
            int pa = (b - c).Abs();
            int pb = (a - c).Abs();
            int pc = (a + b - 2 * c).Abs();

            if (pa <= pb && pa <= pc)
                return a;

            if (pb <= pc)
                return b;

            return c;
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
               Identifier is "?PNG\r\n\u001a\n";

        private static bool IdentifyRFC1950(byte[] Datas)
            => (Datas[0].Equals(0x08) && (Datas[1].Equals(0x1D) || Datas[1].Equals(0x5B) || Datas[1].Equals(0x99) || Datas[1].Equals(0xD7))) ||
               (Datas[0].Equals(0x18) && (Datas[1].Equals(0x19) || Datas[1].Equals(0x57) || Datas[1].Equals(0x95) || Datas[1].Equals(0xD3))) ||
               (Datas[0].Equals(0x28) && (Datas[1].Equals(0x15) || Datas[1].Equals(0x53) || Datas[1].Equals(0x91) || Datas[1].Equals(0xCF))) ||
               (Datas[0].Equals(0x38) && (Datas[1].Equals(0x11) || Datas[1].Equals(0x4F) || Datas[1].Equals(0x8D) || Datas[1].Equals(0xCB))) ||
               (Datas[0].Equals(0x48) && (Datas[1].Equals(0x0D) || Datas[1].Equals(0x4B) || Datas[1].Equals(0x89) || Datas[1].Equals(0xC7))) ||
               (Datas[0].Equals(0x58) && (Datas[1].Equals(0x09) || Datas[1].Equals(0x47) || Datas[1].Equals(0x85) || Datas[1].Equals(0xC3))) ||
               (Datas[0].Equals(0x68) && (Datas[1].Equals(0x05) || Datas[1].Equals(0x43) || Datas[1].Equals(0x81) || Datas[1].Equals(0xDE))) ||
               (Datas[0].Equals(0x78) && (Datas[1].Equals(0x01) || Datas[1].Equals(0x5E) || Datas[1].Equals(0x9C) || Datas[1].Equals(0xDA)));
        public static bool Identify(byte[] Data)
        {
            if (Data.Length < IdentifierSize)
                return false;

            return Data[0].Equals(0x89) &&
                   Data[1].Equals(0x50) &&
                   Data[2].Equals(0x4E) &&
                   Data[3].Equals(0x47) &&
                   Data[4].Equals(0x0D) &&
                   Data[5].Equals(0x0A) &&
                   Data[6].Equals(0x1A) &&
                   Data[7].Equals(0x0A);
        }

        [Conditional("DEBUG")]
        public static void Parse(string Path, bool ShowIDAT = true)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);

            // Header
            if (!Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string Identifier) ||
                !Identify(Identifier))
            {
                Debug.WriteLine("This is not Png file.");
                return;
            }

            MemoryStream DataStream = new();
            DeflateStream Decompressor = new(DataStream, CompressionMode.Decompress);

            do
            {
                if (!Stream.TryReverseRead(out int Length) ||
                    !Stream.TryReadString(4, Encoding.ASCII, out string TypeCode))
                {
                    Debug.WriteLine("Reads chunk error.");
                    return;
                }

                Debug.WriteLine($"==========================================");
                Debug.WriteLine($"                  {TypeCode}              ");
                Debug.WriteLine($"==========================================");
                Debug.WriteLine($"Length             : {Length}");

                switch (TypeCode)
                {
                    case "IHDR":
                        {
                            if (!Stream.TryReverseRead(out int Width) ||
                                !Stream.TryReverseRead(out int Height) ||
                                !Stream.TryRead(out byte BitsPerPixel) ||
                                !Stream.TryRead(out byte ColorType) ||
                                !Stream.TryRead(out byte Compression) ||
                                !Stream.TryRead(out byte FilterMethod) ||
                                !Stream.TryRead(out byte InterlaceMethod))
                            {
                                Debug.WriteLine("Reads chunk error.");
                                return;
                            }

                            Debug.WriteLine($"Width              : {Width}");
                            Debug.WriteLine($"Height             : {Height}");
                            Debug.WriteLine($"BitsPerPixel       : {BitsPerPixel}");
                            Debug.WriteLine($"ColorType          : {ColorType}");
                            Debug.WriteLine($"Compression        : {Compression}");
                            Debug.WriteLine($"FilterMethod       : {FilterMethod}");
                            Debug.WriteLine($"InterlaceMethod    : {InterlaceMethod}");
                            break;
                        }
                    case "IDAT":
                        {
                            byte[] Datas = new byte[Length];
                            Stream.Read(Datas, 0, Datas.Length);

                            if (!ShowIDAT)
                                break;

                            Debug.WriteLine($"Data               : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");

                            bool IsRFC1950Header = IdentifyRFC1950(Datas);

                            DataStream.Position = 0;
                            if (IsRFC1950Header)
                                DataStream.Write(Datas, 2, Datas.Length - 2);
                            else
                                DataStream.Write(Datas, 0, Datas.Length);
                            DataStream.Position = 0;

                            List<byte> DecodeDatas = new();
                            Datas = new byte[1024];

                            int ReadLength;
                            do
                            {
                                ReadLength = Decompressor.Read(Datas, 0, Datas.Length);
                                DecodeDatas.AddRange(Datas.Take(ReadLength));
                            } while (ReadLength.Equals(Datas.Length));

                            Debug.WriteLine($"DecodeLength       : {DecodeDatas.Count}");
                            Debug.WriteLine($"Decode             : {string.Join(", ", DecodeDatas.Select(i => i.ToString("X2")))}");
                            break;
                        }
                    case "cHRM":
                        {
                            if (!Stream.TryReverseRead(out int Wx) ||
                                !Stream.TryReverseRead(out int Wy) ||
                                !Stream.TryReverseRead(out int Rx) ||
                                !Stream.TryReverseRead(out int Ry) ||
                                !Stream.TryReverseRead(out int Gx) ||
                                !Stream.TryReverseRead(out int Gy) ||
                                !Stream.TryReverseRead(out int Bx) ||
                                !Stream.TryReverseRead(out int By))
                            {
                                Debug.WriteLine("Reads chunk error.");
                                return;
                            }

                            Debug.WriteLine($"White Point X      : {Wx / 100000d}");
                            Debug.WriteLine($"White Point Y      : {Wy / 100000d}");
                            Debug.WriteLine($"Red X              : {Rx / 100000d}");
                            Debug.WriteLine($"Red Y              : {Ry / 100000d}");
                            Debug.WriteLine($"Green X            : {Gx / 100000d}");
                            Debug.WriteLine($"Green Y            : {Gy / 100000d}");
                            Debug.WriteLine($"Blue X             : {Bx / 100000d}");
                            Debug.WriteLine($"Blue Y             : {By / 100000d}");
                            break;
                        }
                    case "gAMA":
                        {
                            if (!Stream.TryReverseRead(out int Gamma))
                            {
                                Debug.WriteLine("Reads chunk error.");
                                return;
                            }

                            Debug.WriteLine($"Gamma              : {Gamma / 100000d}");
                            break;
                        }
                    case "sBIT":
                        {
                            // 位元深度資訊
                            // 實際要透過 IHDR.ColorType 來判斷
                            // ColorType == 0      : Grayscale Bits         1 bytes
                            // ColorType == 2 or 3 : RGB Bits               3 bytes
                            // ColorType == 4      : Grayscale + Alpha Bits 2 bytes
                            // ColorType == 6      : RGBA Bits              4 bytes

                            switch (Length)
                            {
                                case 1:
                                    {
                                        if (!Stream.TryRead(out byte Bits))
                                        {
                                            Debug.WriteLine("Reads chunk error.");
                                            return;
                                        }

                                        Debug.WriteLine($"Grayscale Bits     : {Bits}");
                                        break;
                                    }
                                case 2:
                                    {
                                        if (!Stream.TryRead(out byte GrayBits) ||
                                            !Stream.TryRead(out byte ABits))
                                        {
                                            Debug.WriteLine("Reads chunk error.");
                                            return;
                                        }

                                        Debug.WriteLine($"Grayscale Bits     : {GrayBits}");
                                        Debug.WriteLine($"Alpha     Bits     : {ABits}");
                                        break;
                                    }
                                case 3:
                                    {
                                        if (!Stream.TryRead(out byte RBits) ||
                                            !Stream.TryRead(out byte GBits) ||
                                            !Stream.TryRead(out byte BBits))
                                        {
                                            Debug.WriteLine("Reads chunk error.");
                                            return;
                                        }

                                        Debug.WriteLine($"R Bits             : {RBits}");
                                        Debug.WriteLine($"G Bits             : {GBits}");
                                        Debug.WriteLine($"B Bits             : {BBits}");
                                        break;
                                    }
                                case 4:
                                    {
                                        if (!Stream.TryRead(out byte RBits) ||
                                            !Stream.TryRead(out byte GBits) ||
                                            !Stream.TryRead(out byte BBits) ||
                                            !Stream.TryRead(out byte ABits))
                                        {
                                            Debug.WriteLine("Reads chunk error.");
                                            return;
                                        }

                                        Debug.WriteLine($"R     Bits         : {RBits}");
                                        Debug.WriteLine($"G     Bits         : {GBits}");
                                        Debug.WriteLine($"B     Bits         : {BBits}");
                                        Debug.WriteLine($"Alpha Bits         : {ABits}");
                                        break;
                                    }
                                default:
                                    Debug.WriteLine("Reads chunk error.");
                                    return;
                            }
                            break;
                        }
                    case "bKGD":
                        {
                            // 畫布的背景顏色
                            // 實際要透過 IHDR.ColorType 來判斷
                            // ColorType == 0 or 4 : Grayscale      2 bytes
                            // ColorType == 2 or 6 : RGB            6 bytes
                            // ColorType == 3      : Palette Index  1 bytes

                            switch (Length)
                            {
                                case 1:
                                    {
                                        if (!Stream.TryRead(out byte Index))
                                        {
                                            Debug.WriteLine("Reads chunk error.");
                                            return;
                                        }

                                        Debug.WriteLine($"Palette Index      : {Index}");
                                        break;
                                    }
                                case 2:
                                    {
                                        if (!Stream.TryReverseRead(out short Gray))
                                        {
                                            Debug.WriteLine("Reads chunk error.");
                                            return;
                                        }

                                        Debug.WriteLine($"Grayscale          : {Gray}");
                                        break;
                                    }
                                case 6:
                                    {
                                        if (!Stream.TryReverseRead(out short R) ||
                                            !Stream.TryReverseRead(out short G) ||
                                            !Stream.TryReverseRead(out short B))
                                        {
                                            Debug.WriteLine("Reads chunk error.");
                                            return;
                                        }

                                        Debug.WriteLine($"R                  : {R}");
                                        Debug.WriteLine($"G                  : {G}");
                                        Debug.WriteLine($"B                  : {B}");
                                        break;
                                    }
                                default:
                                    Debug.WriteLine("Reads chunk error.");
                                    return;
                            }
                            break;
                        }
                    case "tIME":
                        {
                            if (!Stream.TryReverseRead(out short Year) ||
                                !Stream.TryRead(out byte Month) ||
                                !Stream.TryRead(out byte Day) ||
                                !Stream.TryRead(out byte Hour) ||
                                !Stream.TryRead(out byte Minute) ||
                                !Stream.TryRead(out byte Second))
                            {
                                Debug.WriteLine("Reads chunk error.");
                                return;
                            }

                            Debug.WriteLine($"Last Modify        : {Year}/{Month}/{Day} {Hour}:{Minute}:{Second}");
                            break;
                        }
                    case "tEXt":
                        {
                            byte[] Datas = ArrayPool<byte>.Shared.Rent(Length);
                            try
                            {
                                if (!Stream.ReadBuffer(Datas, 0, Length))
                                {
                                    Debug.WriteLine("Reads chunk error.");
                                    return;
                                }

                                int Index = Datas.IndexOf(i => i.Equals(0x00));
                                if (Index > 0)
                                {
                                    Encoding Encoding = Encoding.GetEncoding(28591);  // Latin-1 (ISO-8859-1)
                                    string Property = Encoding.GetString(Datas, 0, Index),
                                           Value = Encoding.GetString(Datas, Index, Length - Index);

                                    // 由於 Console 無法顯示控制字符，故將控制字符移除
                                    Property = new(Property.Where(c => !char.IsControl(c)).ToArray());
                                    Value = new(Value.Where(c => !char.IsControl(c)).ToArray());
                                    Debug.WriteLine($"{Property} : {Value}");
                                }
                            }
                            finally
                            {
                                ArrayPool<byte>.Shared.Return(Datas);
                            }
                            break;
                        }
                    default:
                        if (Length > 0)
                        {
                            byte[] Datas = new byte[Length];
                            Stream.Read(Datas, 0, Datas.Length);
                            Debug.WriteLine($"Data               : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                        }
                        break;
                }

                if (!Stream.TryReverseRead(out int CRC32Code))
                {
                    Debug.WriteLine("Reads chunk error.");
                    return;
                }

                Debug.WriteLine($"CRC32              : {CRC32Code}");

            } while (Stream.Position < Stream.Length);

            Decompressor.Dispose();
            DataStream.Dispose();
            Stream.Close();
        }

    }
}