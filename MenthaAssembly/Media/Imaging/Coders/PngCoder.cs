﻿using MenthaAssembly.IO;
using MenthaAssembly.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
        //                             IDHR 
        // ============================================================
        // Length               , 4 Bytes
        // TypeCode             , 4 Bytes
        // Width                , 4 Bytes
        // Height               , 4 Bytes
        // BitDepth             , 1 Bytes
        // ColorType            , 1 Bytes
        // Compression          , 1 Bytes
        // FilterMethod         , 1 Bytes
        // InterlaceMethod      , 1 Bytes
        // CRC32                , 4 Bytes
        // ============================================================
        //                            Chunks
        // ============================================================
        // Length               , 4 Bytes
        // TypeCode             , 4 Bytes
        // Datas                , n Bytes
        // CRC32                , 4 Bytes
        // ============================================================
        //                             IEND 
        // ============================================================
        // Length               , 4 Bytes
        // TypeCode             , 4 Bytes
        // CRC32                , 4 Bytes
        // ============================================================

        public const int IdentifierSize = 8;

        /// <summary>
        /// Decode a png file from the specified path without verifying the datas.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(string Path, out IImageContext Image)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, out Image);
        }
        /// <summary>
        /// Decodes a png file from the specified stream.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(Stream Stream, out IImageContext Image)
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

            byte[] ImageData = null;
            List<BGRA> Palette = null;
            int Iw = -1,
                Ih = -1,
                Bits = -1,
                PixelSize = 0,
                Stride = -1;

            CRC32Stream CRCStream = new(Stream, StreamAccess.Read);
            MemoryStream IDATContent = new();
            DeflateStream DecodeStream = new(IDATContent, CompressionMode.Decompress);
            byte[] DecodeBuffer = null;
            int IDAT_Offset = 0,
                IDAT_Dl = -1,
                IDAT_Dh = 0,
                DecodeLength = -1;
            try
            {
                do
                {
                    try
                    {
                        // Chunk Header
                        if (!Stream.TryReverseRead(out int Length) ||
                            !CRCStream.TryReadString(4, Encoding.ASCII, out string TypeCode))
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        // IEND
                        if (TypeCode == "IEND")
                            break;

                        // IHDR
                        if (TypeCode == "IHDR")
                        {
                            byte[] Datas = ArrayPool<byte>.Shared.Rent(5);
                            try
                            {
                                if (!CRCStream.TryReverseRead(out Iw) ||
                                    !CRCStream.TryReverseRead(out Ih) ||
                                    !CRCStream.ReadBuffer(Datas, 0, 5))
                                {
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }
                                // Bit Depth
                                // Only less than or equal to 8 bit depths are implemented in this graphics system.
                                if (Datas[0] == 16)
                                {
                                    Debug.WriteLine($"Not implement Bit Depth : 16.");
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                // BitsPerPixel
                                byte ColorType = Datas[1];
                                if (ColorType == 4)
                                {
                                    Debug.WriteLine($"Not implement ColorType 4 : Grayscale + Alpha.");
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                Bits = ColorType switch
                                {
                                    0 => Datas[0],          // Grayscale
                                    2 => Datas[0] * 3,      // RGB
                                    3 => Datas[0],          // Indexed Color
                                    4 => Datas[0] << 1,     // Grayscale + Alpha
                                    6 => Datas[0] << 2,     // RGBA
                                    _ => -1,
                                };

                                if (Bits == -1)
                                {
                                    Debug.WriteLine($"Unknown ColorType {ColorType}.");
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                // Compression Method
                                // Only compression method 0 is defined in specification.
                                if (Datas[2] != 0)
                                {
                                    Debug.WriteLine($"Unknown compression method {Datas[2]}.");
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                // Filter Method
                                // Only filter method 0 is defined in specification.
                                if (Datas[3] != 0)
                                {
                                    Debug.WriteLine($"Unknown filter method {Datas[3]}.");
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                // Interlace Method
                                // Only interlace method 0 is implemented in this coder.
                                byte InterlaceMethod = Datas[4];
                                if (InterlaceMethod != 0)
                                {
                                    Debug.WriteLine($"Not implement interlace method {InterlaceMethod}.");
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                PixelSize = (Bits + 7) >> 3;
                                Stride = (Iw * Bits + 7) >> 3; //(((Width * Bits) >> 3) + 3) >> 2 << 2;
                                ImageData = new byte[Stride * Ih];
                                DecodeLength = Stride + 1;
                                DecodeBuffer = ArrayPool<byte>.Shared.Rent(DecodeLength);
                            }
                            finally
                            {
                                ArrayPool<byte>.Shared.Return(Datas);
                            }
                        }

                        // PLTE
                        else if (TypeCode == "PLTE")
                        {
                            if (Length % 3 != 0)
                            {
                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            byte[] Datas = ArrayPool<byte>.Shared.Rent(Length);
                            try
                            {
                                if (!CRCStream.ReadBuffer(Datas, 0, Length))
                                {
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                Palette = new();
                                for (int i = 0; i < Length; i += 3)
                                    Palette.Add(new BGRA(Datas[i + 2], Datas[i + 1], Datas[i], byte.MaxValue));
                            }
                            finally
                            {
                                ArrayPool<byte>.Shared.Return(Datas);
                            }
                        }

                        // IDAT
                        else if (TypeCode == "IDAT")
                        {
                            byte[] Datas = ArrayPool<byte>.Shared.Rent(Length);
                            try
                            {
                                if (!CRCStream.ReadBuffer(Datas, 0, Length))
                                {
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                // IDATContent
                                IDATContent.Position = 0;

                                if (IDAT_Dl == -1 && IdentifyRFC1950(Datas))
                                    IDATContent.Write(Datas, 2, Length - 2);
                                else
                                    IDATContent.Write(Datas, 0, Length);

                                IDATContent.Position = 0;

                                // ImageDatas
                                for (int j = IDAT_Dh; j < Ih; j++)
                                {
                                    // Decode
                                    if (IDAT_Dl > 0)
                                        IDAT_Dl += DecodeStream.Read(DecodeBuffer, IDAT_Dl, DecodeLength - IDAT_Dl);
                                    else
                                        IDAT_Dl = DecodeStream.Read(DecodeBuffer, 0, DecodeLength);

                                    if (IDAT_Dl < DecodeLength)
                                        break;

                                    // Reconstruction
                                    if (!ReconstructionDatas(ImageData, IDAT_Offset, DecodeBuffer, Stride, PixelSize))
                                    {
                                        Stream.TrySeek(Begin, SeekOrigin.Begin);
                                        return false;
                                    }

                                    IDAT_Offset += Stride;
                                    IDAT_Dl = 0;
                                    IDAT_Dh++;
                                }
                            }
                            finally
                            {
                                ArrayPool<byte>.Shared.Return(Datas);
                            }
                        }

                        // Skip Datas & CRC32.
                        else if (Length > 0)
                        {
                            if (Stream.TrySeek(Length + 4, SeekOrigin.Current))
                                continue;

                            Image = null;
                            return false;
                        }

                        // Verify CRC32
                        if (!Stream.TryReverseRead(out int Code) ||
                            CRCStream.CRC32Code != Code)
                        {
                            Image = null;
                            return false;
                        }
                    }
                    finally
                    {
                        // Reset CRC32 Code.
                        CRCStream.ResetCode();
                    }
                } while (true);
            }
            finally
            {
                CRCStream.Dispose();
                DecodeStream.Dispose();
                IDATContent.Dispose();

                if (DecodeLength != -1)
                    ArrayPool<byte>.Shared.Return(DecodeBuffer);
            }

            switch (Bits)
            {
                case 1:
                    {
                        Image = new ImageContext<BGRA, Indexed1>(Iw, Ih, ImageData, new ImagePalette<BGRA>(Bits, Palette));
                        return true;
                    }
                case 4:
                    {
                        Image = new ImageContext<BGRA, Indexed4>(Iw, Ih, ImageData, new ImagePalette<BGRA>(Bits, Palette));
                        return true;
                    }
                case 8:
                    {
                        Image = new ImageContext<Gray8>(Iw, Ih, ImageData);
                        return true;
                    }
                case 24:
                    {
                        Image = new ImageContext<RGB>(Iw, Ih, ImageData);
                        return true;
                    }
                case 32:
                    {
                        Image = new ImageContext<RGBA>(Iw, Ih, ImageData);
                        return true;
                    }
                default:
                    {
                        Image = null;
                        return false;
                    }
            }
        }

        // IDAT Filter
        //https://www.w3.org/TR/2003/REC-PNG-20031110/#7Filtering
        private static bool ReconstructionDatas(byte[] ImageBuffer, int Offset, byte[] DecodeBuffer, int Stride, int PixelSize)
        {
            switch (DecodeBuffer[0])
            {
                // None
                case 0:
                    {
                        Array.Copy(DecodeBuffer, 1, ImageBuffer, Offset, Stride);
                        return true;
                    }
                // Sub
                case 1:
                    {
                        Array.Copy(DecodeBuffer, 1, ImageBuffer, Offset, PixelSize);
                        for (int c = 0; c < PixelSize; c++)
                        {
                            int i = Offset + c,
                                Di = c + PixelSize + 1;

                            byte Prev = ImageBuffer[i];
                            for (i += PixelSize; Di <= Stride; i += PixelSize, Di += PixelSize)
                            {
                                Prev = (byte)(DecodeBuffer[Di] + Prev);
                                ImageBuffer[i] = Prev;
                            }
                        }
                        return true;
                    }
                // LastLine
                case 2:
                    {
                        for (int c = 0; c < PixelSize; c++)
                        {
                            int i = Offset + c,
                                Ui = i - Stride,
                                Di = c + 1;

                            for (; Di <= Stride; Di += PixelSize, i += PixelSize, Ui += PixelSize)
                                ImageBuffer[i] = (byte)(DecodeBuffer[Di] + ImageBuffer[Ui]);
                        }
                        return true;
                    }
                // Average
                case 3:
                    {
                        for (int c = 0; c < PixelSize; c++)
                        {
                            int i = Offset + c,
                                Ui = i - Stride,
                                Di = c + 1;

                            // First Pixel
                            byte Prev = (byte)(DecodeBuffer[Di] + (ImageBuffer[Ui] >> 1));
                            ImageBuffer[i] = Prev;

                            for (Di += PixelSize, i += PixelSize, Ui += PixelSize; Di <= Stride; Di += PixelSize, i += PixelSize, Ui += PixelSize)
                            {
                                Prev = (byte)(DecodeBuffer[Di] + ((ImageBuffer[Ui] + Prev) >> 1));
                                ImageBuffer[i] = Prev;
                            }
                        }
                        return true;
                    }
                // Paeth
                case 4:
                    {
                        for (int c = 0; c < PixelSize; c++)
                        {
                            int i = Offset + c,
                                Ui = i - Stride,
                                Di = c + 1;

                            // First Pixel
                            byte Prev = (byte)(DecodeBuffer[Di] + ImageBuffer[Ui]),
                                 PrevUp = ImageBuffer[Ui];
                            ImageBuffer[i] = Prev;

                            for (Di += PixelSize, i += PixelSize, Ui += PixelSize; Di <= Stride; Di += PixelSize, i += PixelSize, Ui += PixelSize)
                            {
                                byte Up = ImageBuffer[Ui];
                                Prev = ReconstructionPaeth(DecodeBuffer[Di], Prev, Up, PrevUp);
                                PrevUp = Up;
                                ImageBuffer[i] = Prev;
                            }
                        }
                        return true;
                    }
            }

            return false;
        }
        /// <summary>
        /// ∥c∥b∥
        /// <para></para>
        /// ∥a∥x∥
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        private static byte ReconstructionPaeth(byte Fx, byte a, byte b, byte c)
        {
            int pa = Math.Abs(b - c),
                pb = Math.Abs(a - c),
                pc = Math.Abs(a + b - 2 * c);

            return (byte)(Fx + (pa <= pb && pa <= pc ? a : pb <= pc ? b : c));
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

            int Iw = Image.Width,
                Ih = Image.Height,
                Bits = Image.BitsPerPixel;

            CRC32Stream CRCStream = new(Stream, StreamAccess.Write, true);
            try
            {
                #region IHDR
                {
                    Type PixelType = Image.PixelType;
                    byte BitDepth = (byte)Math.Min(Bits, 8),    // TODO : Checks 16 Bit depth.
                         ColorType = (byte)(PixelType == typeof(Gray8) ? 0 : Bits <= 8 ? 3 : Bits == 24 ? 2 : 6);

                    Stream.ReverseWrite(13);                            // Chunk Length
                    CRCStream.WriteBytes(0x49, 0x48, 0x44, 0x52);       // CodeType
                    CRCStream.ReverseWrite(Iw);                         // Width
                    CRCStream.ReverseWrite(Ih);                         // Height
                    CRCStream.WriteBytes(BitDepth,                      // BitDepth
                                         ColorType,                     // ColorType
                                         0,                             // Compression
                                         0,                             // Filter Method
                                         0);                            // Interlace Method
                    Stream.ReverseWrite(CRCStream.CRC32Code);           // CRC32
                    CRCStream.ResetCode();

                    //WriteChunk(Stream,
                    //           0x49, 0x48, 0x44, 0x52,
                    //           (byte)(Iw >> 24), (byte)(Iw >> 16), (byte)(Iw >> 8), (byte)Iw,       // Width     , 4 Bytes
                    //           (byte)(Image.Height >> 24), (byte)(Image.Height >> 16), (byte)(Image.Height >> 8), (byte)Image.Height,   // Height    , 4 Bytes
                    //           Bits <= 8 ? (byte)Bits : (byte)0x08,                                         // Bit Depth , 1 Bytes
                    //           Bits <= 8 ? (byte)0x03 : (Bits.Equals(24) ? (byte)0x02 : (byte)0x06),        // ColorType
                    //           0x00,                                                                                                    // Compression
                    //           0x00,                                                                                                    // Filter
                    //           0x00);                                                                                                   // Interlace
                }
                #endregion

                #region PLTE
                if (Bits <= 8)
                {
                    try
                    {
                        Datas = ArrayPool<byte>.Shared.Rent(3);

                        if (Image is IImageIndexedContext IndexedContext)
                        {
                            IImagePalette Palette = IndexedContext.Palette;
                            int Count = Palette.Count;

                            Stream.ReverseWrite(Count * 3);                     // Chunk Length
                            CRCStream.WriteBytes(0x50, 0x4C, 0x54, 0x45);       // CodeType

                            for (int i = 0; i < Count; i++)
                            {
                                IReadOnlyPixel Value = Palette[i];
                                Datas[0] = Value.R;
                                Datas[1] = Value.G;
                                Datas[2] = Value.B;
                                CRCStream.Write(Datas, 0, 3);                   // Color
                            }
                        }
                        else
                        {
                            int Count = 3 << Bits;

                            Stream.ReverseWrite(Count * 3);                     // Chunk Length
                            CRCStream.WriteBytes(0x50, 0x4C, 0x54, 0x45);       // CodeType

                            int ColorStep = byte.MaxValue / ((1 << Bits) - 1);
                            for (int i = 0; i < 256; i += ColorStep)
                            {
                                Datas[0] = Datas[1] = Datas[2] = (byte)i;
                                CRCStream.Write(Datas, 0, 3);                   // Color
                            }
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(Datas);
                    }

                    Stream.ReverseWrite(CRCStream.CRC32Code);                   // CRC32
                    CRCStream.ResetCode();

                    //if (Image is IImageIndexedContext IndexedContext)
                    //{
                    //    IImagePalette Palette = IndexedContext.Palette;
                    //    Datas = new byte[Palette.Count * 3];
                    //    for (int i = 0; i < Palette.Count; i++)
                    //    {
                    //        IReadOnlyPixel Value = Palette[i];
                    //        Datas[i * 3] = Value.R;
                    //        Datas[i * 3 + 1] = Value.G;
                    //        Datas[i * 3 + 2] = Value.B;
                    //    }
                    //}
                    //else
                    //{
                    //    Datas = new byte[3 << Bits];
                    //    int ColorStep = byte.MaxValue / ((1 << Bits) - 1);
                    //    for (int i = 0; i < 256; i += ColorStep)
                    //    {
                    //        Datas[i * 3] = (byte)i;
                    //        Datas[i * 3 + 1] = Datas[0];
                    //        Datas[i * 3 + 2] = Datas[0];
                    //    }
                    //}

                    //WriteChunk(Stream,
                    //           new byte[] { 0x50, 0x4C, 0x54, 0x45 },
                    //           Datas);
                }

                #endregion

                #region IDAT
                // ImageDatas
                int Stride = (Iw * Bits + 7) >> 3;
                byte[] ImageDatas = new byte[Stride + 1];

                using MemoryStream DataStream = new();
                // Mark LZ77 Compress
                DataStream.Write(new byte[] { 0x78, 0xDA }, 0, 2);

                Stream Compressor = new DeflateStream(DataStream, CompressionLevel.Optimal, true);

                byte* pImageDatas = ImageDatas.ToPointer(1);
                Action<int> DataCopyAction = Bits == 32 ? y => Image.ScanLineCopy<RGBA>(0, y, Iw, pImageDatas) :
                                                          y => Image.ScanLineCopy<RGB>(0, y, Iw, pImageDatas);
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

                #region IEND
                Stream.WriteBytes(0x00, 0x00, 0x00, 0x00,       // Length
                                  0x49, 0x45, 0x4E, 0x44,       // TypeCode
                                  0xAE, 0x42, 0x60, 0x82);      // CRC32

                //byte[] IENDChunk =
                //{
                //    0x00, 0x00, 0x00, 0x00,
                //    0x49, 0x45, 0x4E, 0x44,
                //    0xAE, 0x42, 0x60, 0x82
                //};
                //Stream.Write(IENDChunk, 0, IENDChunk.Length);

                #endregion

            }
            finally
            {
                CRCStream.Dispose();
            }
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

        [Conditional("DEBUG")]
        public static void Parse(Stream Stream, bool ShowIDAT = true)
        {
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