﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public static class PngCoder
    {
        public static int IdentifyHeaderSize => 8;

        public static bool TryDecode(string FilePath, out ImageContext Image)
        {
            return TryDecode(new FileStream(FilePath, FileMode.Open, FileAccess.Read), out Image);
        }
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

            // IHDR
            Datas = new byte[10];
            Stream.Seek(8, SeekOrigin.Current);     // Skip Length & TypeCode (8 Bytes), Length = 13 & TypeCode = IHDR(0x49, 0x48, 0x44, 0x52)
            Stream.Read(Datas, 0, Datas.Length);
            int Width = Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3];
            int Height = Datas[4] << 24 | Datas[5] << 16 | Datas[6] << 8 | Datas[7];
            byte BitDepth = Datas[8];
            byte ColorType = Datas[9];
            Stream.Seek(7, SeekOrigin.Current);     // Skip Compression & Filter & Interlace & CRC32 (7 Bytes).
                                                    //byte Compression = Datas[10];           // Always be 0 (ZibCompress).
                                                    //byte Filter = Datas[11];                // Always be 0.
                                                    //byte Interlace = Datas[12];             //

            do
            {
                Datas = new byte[sizeof(int)];

                Stream.Read(Datas, 0, Datas.Length);
                int Length = Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3];

                Stream.Read(Datas, 0, Datas.Length);

                // IEND
                if (Datas[0].Equals(0x49) && Datas[1].Equals(0x45) && Datas[2].Equals(0x4E) && Datas[3].Equals(0x44))
                    break;

                // sBIT
                //if (Datas[0].Equals(0x73) && Datas[1].Equals(0x42) && Datas[2].Equals(0x49) && Datas[3].Equals(0x54)) { }
                // PLTE
                if (Datas[0].Equals(0x50) && Datas[1].Equals(0x4C) && Datas[2].Equals(0x54) && Datas[3].Equals(0x45))
                {
                    Datas = new byte[Length];
                    Stream.Read(Datas, 0, Datas.Length);
                }
                else
                // pHYs
                //if (Datas[0].Equals(0x70) && Datas[1].Equals(0x48) && Datas[2].Equals(0x59) && Datas[3].Equals(0x73)) { }
                // tEXt
                //if (Datas[0].Equals(0x74) && Datas[1].Equals(0x45) && Datas[2].Equals(0x58) && Datas[3].Equals(0x74)) { }

                // IDAT
                if (Datas[0].Equals(0x49) && Datas[1].Equals(0x44) && Datas[2].Equals(0x41) && Datas[3].Equals(0x54))
                {

                    MemoryStream DataStream = new MemoryStream();

                    Datas = new byte[2];
                    Stream.Read(Datas, 0, Datas.Length);

                    bool IsRFC1950Header = IdentifyRFC1950(Datas);

                    if (!IdentifyRFC1950(Datas))
                        DataStream.Write(Datas, 0, Datas.Length);

                    Datas = new byte[Length - 2];
                    Stream.Read(Datas, 0, Datas.Length);

                    DataStream.Write(Datas, 0, Datas.Length);
                    DataStream.Position = 0;

                    List<byte> DecodeDatas = new List<byte>();
                    Datas = new byte[1024];
                    using DeflateStream Decompressor = new DeflateStream(DataStream, CompressionMode.Decompress);
                    int ReadLength;
                    do
                    {
                        ReadLength = Decompressor.Read(Datas, 0, Datas.Length);
                        DecodeDatas.AddRange(Datas.Take(ReadLength));
                    } while (ReadLength.Equals(Datas.Length));
                }
                else if (Length > 0)
                {
                    Datas = new byte[Length];
                    Stream.Read(Datas, 0, Datas.Length);
                }

                Stream.Seek(sizeof(int), SeekOrigin.Current);   // Skip 4 Bytes, This is CRC32.
            } while (Stream.Position < Stream.Length);
            Stream.Close();

            Image = null;
            return true;
        }

        public static void Encode(ImageContext Image, string FilePath)
        {
            Encode(Image, new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write));
        }
        public static void Encode(ImageContext Image, Stream Stream)
        {
            // Png File Struct
            // https://ifun01.com/WM2YFYH.html

            // IdentifyHeader
            Stream.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, IdentifyHeaderSize);

            // IHDR
            WriteChunk(Stream,
                       new byte[] { 0x49, 0x48, 0x44, 0x52 },
                       new byte[]
                       {
                           (byte)(Image.Width >> 24), (byte)(Image.Width >> 16), (byte)(Image.Width >> 8),(byte)Image.Width,        // Width     , 4 Bytes
                           (byte)(Image.Height >> 24), (byte)(Image.Height >> 16), (byte)(Image.Height >> 8),(byte)Image.Height,    // Height    , 4 Bytes
                           0x08,                                                                                                    // Bit Depth , 1 Bytes
                           Image.BitsPerPixel >= 32 ? (byte)0x06 : (Image.BitsPerPixel <= 8 ? (byte)0x00 :(byte)0x02),              // ColorType
                           0x00,                                                                                                    // Compression
                           0x00,                                                                                                    // Filter
                           0x00                                                                                                     // Interlace
                       });
            // IDAT
            using MemoryStream ms = new MemoryStream();

            Stream Compressor = new DeflateStream(ms, CompressionLevel.Optimal, true);
            switch (Image.Channels)
            {
                case 1:
                    //if (Image.Datas[0] is null)
                    //{
                    //    unsafe
                    //    {
                    //        byte[] ImageDatas = new byte[Image.Stride + 1];
                    //        ImageDatas[0] = 0x00;
                    //        byte* Source = (byte*)Image.Scan0;
                    //        for (int j = 0; j < Image.Height; j++)
                    //        {
                    //            for (int i = 1; i <= Image.Stride; i += Image.PixelBytes)
                    //                for (int k = Image.PixelBytes - 1; k >= 0; k--)
                    //                    ImageDatas[i + k] = *Source++;

                    //            Compressor.Write(ImageDatas, 0, ImageDatas.Length);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    byte[] ImageDatas = new byte[Image.Stride + 1];
                    //    ImageDatas[0] = 0x00;
                    //    for (int j = 0; j < Image.Height; j++)
                    //    {
                    //        int Offset = j * Image.Stride;
                    //        for (int i = 0; i < Image.Stride; i += Image.PixelBytes)
                    //        {
                    //            ImageDatas[i + 1] = Image.Datas[0][Offset + i + 2];     // B
                    //            ImageDatas[i + 2] = Image.Datas[0][Offset + i + 1];     // G
                    //            ImageDatas[i + 3] = Image.Datas[0][Offset + i];         // R
                    //            ImageDatas[i + 4] = Image.Datas[0][Offset + i + 3];     // A
                    //        }
                    //        Compressor.Write(ImageDatas, 0, ImageDatas.Length);
                    //    }
                    //}
                    break;
                case 3:
                    unsafe
                    {
                        byte[] ImageDatas = new byte[Image.Stride * 3 + 1];
                        ImageDatas[0] = 0x00;
                        for (int j = 0; j < Image.Height; j++)
                        {
                            int Offset = Image.Stride * j;
                            byte* SourceR = (byte*)(Image.ScanR + Offset),
                                  SourceG = (byte*)(Image.ScanG + Offset),
                                  SourceB = (byte*)(Image.ScanB + Offset);
                            for (int i = 1; i < ImageDatas.Length - 1; i += 3)
                            {
                                ImageDatas[i] = *SourceR++;         // R
                                ImageDatas[i + 1] = *SourceG++;     // G
                                ImageDatas[i + 2] = *SourceB++;     // B
                            }

                            Compressor.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
                case 4:
                    unsafe
                    {
                        byte[] ImageDatas = new byte[Image.Stride * 4 + 1];
                        ImageDatas[0] = 0x00;
                        IntPtr DataPointer;
                        fixed (byte* DataScan = &ImageDatas[1])
                            DataPointer = (IntPtr)DataScan;

                        for (int j = 0; j < Image.Height; j++)
                        {
                            int Offset = Image.Stride * j;
                            int* DataScan0 = (int*)DataPointer;
                            byte* SourceA = (byte*)(Image.ScanA + Offset),
                                  SourceR = (byte*)(Image.ScanR + Offset),
                                  SourceG = (byte*)(Image.ScanG + Offset),
                                  SourceB = (byte*)(Image.ScanB + Offset);
                            for (int i = 0; i < ImageDatas.Length; i += 4)
                                *DataScan0++ = *SourceA++ << 24 |   // A
                                               *SourceR++ |         // R
                                               *SourceG++ << 8 |    // G
                                               *SourceB++ << 16;    // B

                            Compressor.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
            }

            Compressor.Close();
            WriteChunk(Stream,
                       new byte[] { 0x49, 0x44, 0x41, 0x54 },
                       ms.ToArray());

            // IEND
            byte[] IENDChunk =
            {
                0x00, 0x00, 0x00, 0x00,
                0x49, 0x45, 0x4E, 0x44,
                0xAE, 0x42, 0x60, 0x82
            };
            Stream.Write(IENDChunk, 0, IENDChunk.Length);
            Stream.Close();
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

            CRC32.Calculate(TypeCode, out uint NewRegister);
            CRC32.Calculate(Datas, out int CRCResult, NewRegister);
            Buffers = new byte[] { (byte)(CRCResult >> 24), (byte)(CRCResult >> 16), (byte)(CRCResult >> 8), (byte)CRCResult };
            Stream.Write(Buffers, 0, Buffers.Length);
        }

        public static bool Identify(byte[] Data)
        {
            if (Data.Length < IdentifyHeaderSize)
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
        public static void Parse(string FilePath)
        {
            FileStream FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            byte[] Datas = new byte[IdentifyHeaderSize];
            FS.Read(Datas, 0, Datas.Length);

            if (!Identify(Datas))
            {
                Debug.WriteLine("This is not Png file.");
                return;
            }

            string Result;
            do
            {
                Datas = new byte[sizeof(int)];
                FS.Read(Datas, 0, Datas.Length);
                int Length = Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3];
                FS.Read(Datas, 0, Datas.Length);
                string TypeCode = Encoding.Default.GetString(Datas, 0, Datas.Length);

                Result = "========================\r\n" +
                         $"          {TypeCode} ({string.Join(", ", Datas.Select(i => $"0x{i.ToString("X2")}"))})\r\n" +
                         "========================\r\n" +
                         $"Length : {Length}\r\n";
                switch (TypeCode)
                {
                    case "IHDR":
                        {
                            Datas = new byte[sizeof(int)];
                            FS.Read(Datas, 0, Datas.Length);
                            Result += $"Width       : {Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3]}\r\n";
                            FS.Read(Datas, 0, Datas.Length);
                            Result += $"Height      : {Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3]}\r\n";
                            Datas = new byte[5];
                            FS.Read(Datas, 0, Datas.Length);
                            Result += $"Depth       : {Datas[0]}\r\n";
                            Result += $"ColorType   : {Datas[1]}\r\n";
                            Result += $"Compression : {Datas[2]}\r\n";
                            Result += $"Filter      : {Datas[3]}\r\n";
                            Result += $"Interlace   : {Datas[4]}\r\n";
                        }
                        break;
                    case "IDAT":
                        {
                            Datas = new byte[Length];
                            FS.Read(Datas, 0, Datas.Length);

                            Result += $"Data   : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}\r\n";

                            MemoryStream DataStream = new MemoryStream();
                            bool IsRFC1950Header = IdentifyRFC1950(Datas);

                            if (IsRFC1950Header)
                                DataStream.Write(Datas, 2, Datas.Length - 2);
                            else
                                DataStream.Write(Datas, 0, Datas.Length);
                            DataStream.Position = 0;

                            List<byte> DecodeDatas = new List<byte>();
                            Datas = new byte[1024];
                            using (DeflateStream Decompressor = new DeflateStream(DataStream, CompressionMode.Decompress))
                            {
                                int ReadLength;
                                do
                                {
                                    ReadLength = Decompressor.Read(Datas, 0, Datas.Length);
                                    DecodeDatas.AddRange(Datas.Take(ReadLength));
                                } while (ReadLength.Equals(Datas.Length));
                            }
                            Result += $"Size   : {DecodeDatas.Count}\r\n";
                            Result += $"Decode : {string.Join(", ", DecodeDatas.Select(i => i.ToString("X2")))}\r\n";
                        }
                        break;
                    default:
                        if (Length > 0)
                        {
                            Datas = new byte[Length];
                            FS.Read(Datas, 0, Datas.Length);
                            Result += $"Data   : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}\r\n";
                        }
                        break;
                }

                Datas = new byte[sizeof(int)];
                FS.Read(Datas, 0, Datas.Length);
                Result += $"CRC    : {Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3]}";

                Debug.WriteLine(Result);
            } while (FS.Position < FS.Length);

            FS.Close();
        }

        private static bool IdentifyRFC1950(byte[] Datas)
        {
            return (Datas[0].Equals(0x08) && (Datas[1].Equals(0x1D) || Datas[1].Equals(0x5B) || Datas[1].Equals(0x99) || Datas[1].Equals(0xD7))) ||
                   (Datas[0].Equals(0x18) && (Datas[1].Equals(0x19) || Datas[1].Equals(0x57) || Datas[1].Equals(0x95) || Datas[1].Equals(0xD3))) ||
                   (Datas[0].Equals(0x28) && (Datas[1].Equals(0x15) || Datas[1].Equals(0x53) || Datas[1].Equals(0x91) || Datas[1].Equals(0xCF))) ||
                   (Datas[0].Equals(0x38) && (Datas[1].Equals(0x11) || Datas[1].Equals(0x4F) || Datas[1].Equals(0x8D) || Datas[1].Equals(0xCB))) ||
                   (Datas[0].Equals(0x48) && (Datas[1].Equals(0x0D) || Datas[1].Equals(0x4B) || Datas[1].Equals(0x89) || Datas[1].Equals(0xC7))) ||
                   (Datas[0].Equals(0x58) && (Datas[1].Equals(0x09) || Datas[1].Equals(0x47) || Datas[1].Equals(0x85) || Datas[1].Equals(0xC3))) ||
                   (Datas[0].Equals(0x68) && (Datas[1].Equals(0x05) || Datas[1].Equals(0x43) || Datas[1].Equals(0x81) || Datas[1].Equals(0xDE))) ||
                   (Datas[0].Equals(0x78) && (Datas[1].Equals(0x01) || Datas[1].Equals(0x5E) || Datas[1].Equals(0x9C) || Datas[1].Equals(0xDA)));
        }
    }
}
