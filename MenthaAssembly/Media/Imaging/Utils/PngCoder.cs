using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging
{
    // PNG Specification
    // https://www.w3.org/TR/PNG/
    public static class PngCoder
    {
        public static int IdentifyHeaderSize => 8;

        public static bool TryDecode(string FilePath, out ImageContext Image)
            => TryDecode(new FileStream(FilePath, FileMode.Open, FileAccess.Read), out Image);
        public static bool TryDecode(Stream Stream, out ImageContext Image)
        {
            bool CheckCRC32 = true;
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

            int Width = 0,
                Height = 0,
                Bits = 0,
                Channels = 0,
                Stride = 0;
            byte[][] ImageDatas = null;
            IList<int> Palette = null;
            //byte Compression,
            //Filter,
            //Interlace;

            MemoryStream DataStream = new MemoryStream();
            DeflateStream Decompressor = new DeflateStream(DataStream, CompressionMode.Decompress);
            int IDAT_Offset = 0,
                IDAT_DecodeLength = 0,
                IDAT_DecodeHeight = 0;
            byte[] DecodeDatas = null;

            byte[] TypeCode = new byte[sizeof(int)];
            do
            {
                Datas = new byte[sizeof(int)];

                Stream.Read(Datas, 0, Datas.Length);
                int Length = Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3];

                Stream.Read(TypeCode, 0, TypeCode.Length);  // TypeCode

                #region IEND
                // IEND
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
                else
                #endregion
                // sBIT
                //if (TypeCode[0].Equals(0x73) && TypeCode[1].Equals(0x42) && TypeCode[2].Equals(0x49) && TypeCode[3].Equals(0x54)) { }
                #region PLTE
                if (TypeCode[0].Equals(0x50) && TypeCode[1].Equals(0x4C) && TypeCode[2].Equals(0x54) && TypeCode[3].Equals(0x45))
                {
                    Datas = new byte[Length];
                    Stream.Read(Datas, 0, Datas.Length);

                    Palette = new List<int>();
                    for (int i = 0; i < Length; i += 3)
                        Palette.Add(Datas[i] << 16 |
                                    Datas[i + 1] << 8 |
                                    Datas[i + 2] |
                                    -16777216); // 0xFF << 24 = -16777216
                }
                else
                #endregion
                // pHYs
                //if (TypeCode[0].Equals(0x70) && TypeCode[1].Equals(0x48) && TypeCode[2].Equals(0x59) && TypeCode[3].Equals(0x73)) { }
                //else
                #region tEXt
                if (TypeCode[0].Equals(0x74) && TypeCode[1].Equals(0x45) && TypeCode[2].Equals(0x58) && TypeCode[3].Equals(0x74))
                {
                    Datas = new byte[Length];
                    Stream.Read(Datas, 0, Datas.Length);

                    int SeparatorIndex = Datas.FindIndex(i => i.Equals(0x00));
                    if (SeparatorIndex > 0)
                    {
                        string Property = Encoding.Default.GetString(Datas, 0, SeparatorIndex),
                               Content = Encoding.Default.GetString(Datas.AsSpan(SeparatorIndex + 1));
                        Debug.WriteLine($"{Property} : {Content}");
                    }
                }
                else
                #endregion

                #region IDAT
                if (TypeCode[0].Equals(0x49) && TypeCode[1].Equals(0x44) && TypeCode[2].Equals(0x41) && TypeCode[3].Equals(0x54))
                {
                    Datas = new byte[Length];
                    Stream.Read(Datas, 0, Datas.Length);

                    DataStream.Position = 0;
                    DataStream.Write(Datas.AsSpan(IdentifyRFC1950(Datas) ? 2 : 0));
                    DataStream.Position = 0;

                    //if (IdentifyRFC1950(Datas))
                    //    DataStream.Write(Datas, 2, Datas.Length - 2);
                    //else
                    //    DataStream.Write(Datas, 0, Datas.Length);
                    //DataStream.Position = 0;


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
                else
                #endregion

                #region Other
                if (Length > 0)
                {
                    Stream.Seek(Length + sizeof(int), SeekOrigin.Current);  // Skip Datas & CRC32.
                    continue;
                }
                #endregion

                #region CRC32
                if (CheckCRC32)
                {
                    CRC32.Calculate(Datas: TypeCode, out uint NextRegister);
                    CRC32.Calculate(Datas: Datas, out int CRCResult, NextRegister);
                    Datas = new byte[sizeof(int)];
                    Stream.Read(Datas, 0, Datas.Length);
                    if (!CRCResult.Equals(Datas[0] << 24 |
                                          Datas[1] << 16 |
                                          Datas[2] << 8 |
                                          Datas[3]))
                    {
                        Image = null;
                        return false;
                    }
                }
                else
                {
                    Stream.Seek(sizeof(int), SeekOrigin.Current);   // Skip CRC32.
                }

                #endregion
            } while (Stream.Position < Stream.Length);
            Decompressor.Dispose();
            DataStream.Dispose();
            Stream.Dispose();

            switch (ImageDatas.Length)
            {
                case 1:
                    Image = new ImageContext(Width, Height, ImageDatas[0], Palette);
                    return true;
                case 3:
                    Image = new ImageContext(Width, Height, ImageDatas[0], ImageDatas[1], ImageDatas[2]);
                    return true;
                case 4:
                    Image = new ImageContext(Width, Height, ImageDatas[3], ImageDatas[0], ImageDatas[1], ImageDatas[2]);
                    return true;
            }
            Image = null;
            return false;
        }

        public static void Encode(ImageContext Image, string FilePath)
            => Encode(Image, new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write));
        public static void Encode(ImageContext Image, Stream Stream)
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
                if (Image.Palette is null)
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
                else
                {
                    Datas = new byte[Image.Palette.Count * 3];
                    for (int i = 0; i < Image.Palette.Count; i++)
                    {
                        int Value = Image.Palette[i];
                        Datas[i * 3] = (byte)(Value >> 16);
                        Datas[i * 3 + 1] = (byte)(Value >> 8);
                        Datas[i * 3 + 2] = (byte)Value;
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
            // Mark LZ77 Compress ()
            DataStream.Write(new byte[] { 0x78, 0xDA }, 0, 2);

            Stream Compressor = new DeflateStream(DataStream, CompressionLevel.Optimal, true);
            switch (Image.Channels)
            {
                case 1:
                    unsafe
                    {
                        for (int j = 0; j < Image.Height; j++)
                        {
                            Marshal.Copy(Image.Scan0 + Image.Stride * j, ImageDatas, 1, Stride);
                            Compressor.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
                case 3:
                    unsafe
                    {
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

            CRC32.Calculate(Datas: TypeCode, out uint NewRegister);
            CRC32.Calculate(Datas: Datas, out int CRCResult, NewRegister);
            Buffers = new byte[] { (byte)(CRCResult >> 24), (byte)(CRCResult >> 16), (byte)(CRCResult >> 8), (byte)CRCResult };
            Stream.Write(Buffers, 0, Buffers.Length);
        }
        private static void WriteChunk(Stream Stream, byte[] TypeCode, ReadOnlySpan<byte> Datas)
        {
            // Length
            byte[] Buffers = { (byte)(Datas.Length >> 24), (byte)(Datas.Length >> 16), (byte)(Datas.Length >> 8), (byte)Datas.Length };
            Stream.Write(Buffers, 0, Buffers.Length);
            // TypeCode
            Stream.Write(TypeCode, 0, TypeCode.Length);
            // Datas
            Stream.Write(Datas);

            CRC32.Calculate(Datas: TypeCode, out uint NewRegister);
            CRC32.Calculate(Datas, out int CRCResult, NewRegister);
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
            int pa = Math.Abs(b - c);
            int pb = Math.Abs(a - c);
            int pc = Math.Abs(a + b - 2 * c);

            if (pa <= pb && pa <= pc)
                return a;

            if (pb <= pc)
                return b;

            return c;
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
        public static void Parse(string FilePath, bool ShowIDAT = true)
        {
            FileStream FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            byte[] Datas = new byte[IdentifyHeaderSize];
            FS.Read(Datas, 0, Datas.Length);

            if (!Identify(Datas))
            {
                Debug.WriteLine("This is not Png file.");
                return;
            }

            MemoryStream DataStream = new MemoryStream();
            DeflateStream Decompressor = new DeflateStream(DataStream, CompressionMode.Decompress);
            string Result;
            do
            {
                Datas = new byte[sizeof(int)];
                FS.Read(Datas, 0, Datas.Length);
                int Length = Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3];
                FS.Read(Datas, 0, Datas.Length);
                string TypeCode = Encoding.Default.GetString(Datas);

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
                            break;
                        }
                    case "IDAT":
                        {
                            Datas = new byte[Length];
                            FS.Read(Datas, 0, Datas.Length);

                            if (!ShowIDAT)
                                break;

                            Result += $"Data   : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}\r\n";

                            bool IsRFC1950Header = IdentifyRFC1950(Datas);

                            DataStream.Position = 0;
                            if (IsRFC1950Header)
                                DataStream.Write(Datas, 2, Datas.Length - 2);
                            else
                                DataStream.Write(Datas, 0, Datas.Length);
                            DataStream.Position = 0;

                            List<byte> DecodeDatas = new List<byte>();
                            Datas = new byte[1024];

                            int ReadLength;
                            do
                            {
                                ReadLength = Decompressor.Read(Datas, 0, Datas.Length);
                                DecodeDatas.AddRange(Datas.Take(ReadLength));
                            } while (ReadLength.Equals(Datas.Length));

                            Result += $"Size   : {DecodeDatas.Count}\r\n";
                            Result += $"Decode : {string.Join(", ", DecodeDatas.Select(i => i.ToString("X2")))}\r\n";
                            break;
                        }
                    case "tEXt":
                        {
                            Datas = new byte[Length];
                            FS.Read(Datas, 0, Datas.Length);

                            int SeparatorIndex = Datas.FindIndex(i => i.Equals(0x00));
                            if (SeparatorIndex > 0)
                                Result += $"{Encoding.Default.GetString(Datas, 0, SeparatorIndex)} : {Encoding.Default.GetString(Datas, SeparatorIndex + 1, Datas.Length - SeparatorIndex - 1)}\r\n";
                            break;
                        }
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

            Decompressor.Dispose();
            DataStream.Dispose();
            FS.Close();
        }

        private static bool IdentifyRFC1950(byte[] Datas)
            => (Datas[0].Equals(0x08) && (Datas[1].Equals(0x1D) || Datas[1].Equals(0x5B) || Datas[1].Equals(0x99) || Datas[1].Equals(0xD7))) ||
               (Datas[0].Equals(0x18) && (Datas[1].Equals(0x19) || Datas[1].Equals(0x57) || Datas[1].Equals(0x95) || Datas[1].Equals(0xD3))) ||
               (Datas[0].Equals(0x28) && (Datas[1].Equals(0x15) || Datas[1].Equals(0x53) || Datas[1].Equals(0x91) || Datas[1].Equals(0xCF))) ||
               (Datas[0].Equals(0x38) && (Datas[1].Equals(0x11) || Datas[1].Equals(0x4F) || Datas[1].Equals(0x8D) || Datas[1].Equals(0xCB))) ||
               (Datas[0].Equals(0x48) && (Datas[1].Equals(0x0D) || Datas[1].Equals(0x4B) || Datas[1].Equals(0x89) || Datas[1].Equals(0xC7))) ||
               (Datas[0].Equals(0x58) && (Datas[1].Equals(0x09) || Datas[1].Equals(0x47) || Datas[1].Equals(0x85) || Datas[1].Equals(0xC3))) ||
               (Datas[0].Equals(0x68) && (Datas[1].Equals(0x05) || Datas[1].Equals(0x43) || Datas[1].Equals(0x81) || Datas[1].Equals(0xDE))) ||
               (Datas[0].Equals(0x78) && (Datas[1].Equals(0x01) || Datas[1].Equals(0x5E) || Datas[1].Equals(0x9C) || Datas[1].Equals(0xDA)));
    }
}
