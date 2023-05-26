﻿using MenthaAssembly.Utils;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    // https://github.com/corkami/formats/blob/master/image/JPEGRGB_dissected.png
    // https://blog.csdn.net/u010192735/article/details/120860826
    // https://blog.csdn.net/menglongbor/article/details/89742771
    // https://blog.csdn.net/u010192735/article/details/120869528
    // https://blog.csdn.net/weixin_58208902/article/details/125560863
    public static unsafe class JpgCoder
    {
        /// <summary>
        /// The length in bytes of the jpg file format identifier.
        /// </summary>
        public const int IdentifierSize = 2;

        /// <summary>
        /// Decodes a jpg file from the specified path.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(string Path, out IImageContext Image)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);
            return TryDecode(Stream, out Image);
        }
        /// <summary>
        /// Decodes a jpg file from the specified stream.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Image">The decoded image.</param>
        public static bool TryDecode(Stream Stream, out IImageContext Image)
        {
            Image = null;
            long Begin = Stream.CanSeek ? Stream.Position : 0L;

            int Iw = -1,
                Ih = -1;

            Dictionary<byte, int> HorizontalSamplingFactor = new(),     // Component ID, Factor
                                  VerticalSamplingFactor = new(),       // Component ID, Factor
                                  QuantizationTableSelectors = new();   // Component ID, Quantization Table ID
            Dictionary<int, byte[]> QuantizationTables = new();         // Table ID, Table Content
            Dictionary<int, HuffmanDecodeTable> ACTables = new(),       // Table ID, Table Content
                                                DCTables = new();       // Table ID, Table Content

            byte[] Identifier = ArrayPool<byte>.Shared.Rent(IdentifierSize);
            try
            {
                // SOI (Start of Image)
                if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||
                    !(Identifier[0] != 0xFF || Identifier[0] != 0xD8))
                {
                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                    return false;
                }

                do
                {
                    // Tags
                    if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||
                        Identifier[0] != 0xFF)
                    {
                        Stream.TrySeek(Begin, SeekOrigin.Begin);
                        return false;
                    }

                    // EOI (End of Image)
                    if (Identifier[1] == 0xD9)
                        break;

                    // Length
                    if (!Stream.TryReverseRead(out ushort Length))
                    {
                        Stream.TrySeek(Begin, SeekOrigin.Begin);
                        return false;
                    }

                    #region APP0 (Application Marker 0)
                    if (Identifier[1] == 0xE0)
                    {
                        if (!Stream.TryReadString(5, Encoding.ASCII, out string JFIFIdentifier) ||
                            JFIFIdentifier != "JFIF\0" ||
                            !Stream.TrySeek(Length - 7, SeekOrigin.Current))
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }
                    }
                    #endregion

                    #region DQT (Define Quantization Table)
                    else if (Identifier[1] == 0xDB)
                    {
                        int DataLength = Length - 2;
                        while (DataLength > 0)
                        {
                            // Info
                            if (!Stream.TryRead(out byte Info))
                            {
                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            // Table
                            int ID = Info & 0x0F,
                                TableLength = ((Info >> 4) + 1) << 6;
                            byte[] TableDatas = new byte[TableLength];

                            DataLength -= TableLength + 1;
                            if (DataLength < 0 ||
                                !Stream.ReadBuffer(TableDatas, 0, TableLength))
                            {
                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            QuantizationTables.Add(ID, TableDatas);
                        }
                    }
                    #endregion

                    #region SOF (Start of Frame)
                    else if (Identifier[1] == 0xC0 ||
                             Identifier[1] == 0xC1 ||
                             Identifier[1] == 0xC2 ||
                             Identifier[1] == 0xC3)
                    {
                        if (!Stream.TrySeek(1, SeekOrigin.Current) ||               // !Stream.TryRead(out byte BitDepth) ||
                            !Stream.TryReverseRead(out ushort Width) ||
                            !Stream.TryReverseRead(out ushort Height) ||
                            !Stream.TryRead(out byte Components))
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        Iw = Width;
                        Ih = Height;

                        Length -= 8;
                        for (int i = 0; i < Components; i++)
                        {
                            // Component Info
                            if (!Stream.TryRead(out byte ID) ||
                                !Stream.TryRead(out byte Info) ||
                                !Stream.TryRead(out byte TableID))
                            {
                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            Length -= 3;
                            HorizontalSamplingFactor.Add(ID, Info >> 4);
                            VerticalSamplingFactor.Add(ID, Info & 0x0F);
                            QuantizationTableSelectors.Add(ID, TableID);
                        }

                        if (Length != 0)
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }
                    }
                    #endregion

                    #region DHT (Define Huffman Table)
                    else if (Identifier[1] == 0xC4)
                    {
                        const int HuffmanCodeBits = 16;

                        int DataLength = Length - 2;
                        byte[] Datas = ArrayPool<byte>.Shared.Rent(HuffmanCodeBits);
                        try
                        {
                            do
                            {
                                DataLength -= 17;
                                if (!Stream.TryRead(out byte Info) ||
                                    !Stream.ReadBuffer(Datas, 0, HuffmanCodeBits))
                                {
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                                HuffmanDecodeTable Table = new();
                                switch (Info >> 4)
                                {
                                    case 0:
                                        {
                                            DCTables.Add(Info & 0x0F, Table);
                                            break;
                                        }
                                    case 1:
                                        {
                                            ACTables.Add(Info & 0x0F, Table);
                                            break;
                                        }
                                    default:
                                        {
                                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                                            return false;
                                        }
                                }

                                int LastCode = -1,
                                    LastBits = 0;
                                for (int i = 0; i < 16; i++)
                                {
                                    int CodeLength = Datas[i];
                                    if (CodeLength > 0)
                                    {
                                        int Bits = i + 1;

                                        DataLength -= CodeLength;
                                        byte[] CodeDatas = Stream.Read(CodeLength);

                                        for (int j = 0; j < CodeLength; j++)
                                        {
                                            LastCode++;

                                            // Check LastBits
                                            if (((LastCode >> LastBits) & 1) > 0)
                                                LastBits++;

                                            // Check Bits
                                            if (LastBits < Bits)
                                                LastCode <<= Bits - LastBits;

                                            LastBits = Bits;
                                            Table.Add(Bits, LastCode, CodeDatas[j]);
                                        }
                                    }
                                }

                                if (DataLength < 0)
                                {
                                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                                    return false;
                                }

                            } while (DataLength > 0);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(Datas);
                        }
                    }
                    #endregion

                    #region SOS (Start of Scan)
                    else if (Identifier[1] == 0xDA)
                    {
                        // Components;
                        int DataLength = Length - 3;                    // Length 2 Bytes & Components 1 Byte
                        if (!Stream.TryRead(out byte Components))
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        List<byte> ComponentsOrder = new();
                        Dictionary<byte, int> ACTableSelectors = new(),     // Component ID, AC Table ID
                                              DCTableSelectors = new();     // Component ID, DC Table ID

                        // Components
                        for (int i = 0; i < Components; i++)
                        {
                            DataLength -= 2;
                            if (!Stream.TryRead(out byte ID) ||
                                !Stream.TryRead(out byte Info))
                            {
                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            ComponentsOrder.Add(ID);
                            ACTableSelectors.Add(ID, Info & 0x0F);
                            DCTableSelectors.Add(ID, Info >> 4);
                        }

                        DataLength -= 3;
                        if (!Stream.TrySeek(3, SeekOrigin.Current) ||       // Skips SpectralSelectStart、SpectralSelectEnd、SuccessiveApprox
                            DataLength < 0)
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        #region ImageDatas
                        const int ReadBufferSize = 8192;
                        byte[] ImageReadBuffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
                        try
                        {
                            int BufferLength = 0,
                                BufferIndex = 0,
                                ReadValue = 0,
                                ReadBitIndex = 8;
                            bool TryReadBit(out int Value, out bool Reset)
                            {
                                Reset = false;
                                if (ReadBitIndex == 8)
                                {
                                    if (BufferIndex < BufferLength)
                                    {
                                        ReadValue = ImageReadBuffer[BufferIndex++];
                                        ReadBitIndex = 0;
                                    }
                                    else
                                    {
                                        BufferLength = Stream.Read(ImageReadBuffer, 0, ReadBufferSize);
                                        if (BufferLength == 0)
                                        {
                                            Value = 0;
                                            return false;
                                        }

                                        ReadBitIndex = 0;
                                        BufferIndex = 1;
                                        ReadValue = ImageReadBuffer[0];
                                    }

                                    if (ReadValue == 0xFF)
                                    {
                                        byte Mark;
                                        if (BufferIndex < BufferLength)
                                            Mark = ImageReadBuffer[BufferIndex++];

                                        else
                                        {
                                            BufferLength = Stream.Read(ImageReadBuffer, 0, ReadBufferSize);
                                            if (BufferLength == 0)
                                            {
                                                Value = 0;
                                                return false;
                                            }

                                            BufferIndex = 1;
                                            Mark = ImageReadBuffer[0];
                                        }

                                        if (Mark is 0xD0 or 0xD1 or 0xD2 or 0xD3 or 0xD4 or 0xD5 or 0xD6 or 0xD7)
                                        {
                                            Reset = true;
                                            ReadBitIndex = 8;
                                            return TryReadBit(out Value, out _);
                                        }

                                        else if (Mark is not 0x00)
                                        {
                                            Value = 0;
                                            return false;
                                        }
                                    }

                                    Debug.WriteLine($"{ReadValue:X2}");
                                }

                                Value = (ReadValue >> (7 - ReadBitIndex)) & 1;
                                ReadBitIndex++;
                                return true;
                            }

                            if (Iw < 0 ||
                                Ih < 0)
                            {
                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            int XCount = (Iw + 7) >> 3,
                                YCount = (Ih + 7) >> 3;

                            for (int j = 0; j < YCount; j++)
                            {
                                for (int i = 0; i < XCount; i++)
                                {
                                    foreach (byte ComponentID in ComponentsOrder)
                                    {
                                        if (!DCTableSelectors.TryGetValue(ComponentID, out int TableID) ||
                                            !DCTables.TryGetValue(TableID, out HuffmanDecodeTable DCTable) ||
                                            !ACTableSelectors.TryGetValue(ComponentID, out TableID) ||
                                            !ACTables.TryGetValue(TableID, out HuffmanDecodeTable ACTable) ||
                                            !QuantizationTableSelectors.TryGetValue(ComponentID, out TableID) ||
                                            !QuantizationTables.TryGetValue(TableID, out byte[] QuantizationTable))
                                        {
                                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                                            return false;
                                        }

                                        int DCMaxBits = DCTable.Bits.Max(),
                                            ACMaxBits = ACTable.Bits.Max(),
                                            Code, Bit, DCLength;

                                        Code = 0;
                                        Bit = 0;
                                        DCLength = 0;
                                        while (TryReadBit(out int BitCode, out bool Reset))
                                        {
                                            Bit++;
                                            if (DCMaxBits < Bit)
                                            {
                                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                                return false;
                                            }

                                            Code |= BitCode;
                                            if (DCTable[Bit, Code] is byte[] Data)
                                            {
                                                DCLength = Data[0];
                                                break;
                                            }

                                            Code <<= 1;
                                        }






                                    }
                                }
                            }

                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(ImageReadBuffer);
                        }
                        #endregion

                    }
                    #endregion

                    // Skips others
                    else if (!Stream.TrySeek(Length - 2, SeekOrigin.Current))
                    {
                        Stream.TrySeek(Begin, SeekOrigin.Begin);
                        return false;
                    }

                } while (!Stream.CanSeek || Stream.Position < Stream.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Identifier);
            }

            return true;
        }

        /// <summary>
        /// Only decode the image size of the jpg file at the specified path.
        /// </summary>
        /// <param name="Path">The specified path.</param>
        /// <param name="Width">The width of image.</param>
        /// <param name="Height">The height of image.</param>
        public static bool TryGetImageSize(string Path, out int Width, out int Height)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read);
            return TryGetImageSize(Stream, out Width, out Height);
        }
        /// <summary>
        /// Only decode the image size of the jpg file at the specified stream.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Width">The width of image.</param>
        /// <param name="Height">The height of image.</param>
        public static bool TryGetImageSize(Stream Stream, out int Width, out int Height)
        {
            Width = 0;
            Height = 0;
            long Begin = Stream.CanSeek ? Stream.Position : 0L;

            byte[] Identifier = ArrayPool<byte>.Shared.Rent(IdentifierSize);
            try
            {
                // Identifier
                if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||    // SOI 
                    !Identify(Identifier))
                {
                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                    return false;
                }

                //// Header
                //if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||            // APP0
                //    !(Identifier[0] != 0xFF || Identifier[0] != 0xE0) ||            // 0xFF 0xE0
                //    !Stream.TryReverseRead(out ushort Length) ||                    // Length
                //    !Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string JFIFIdentifier) ||
                //    JFIFIdentifier != "JFIF\0")
                //{
                //    Stream.TrySeek(Begin, SeekOrigin.Begin);
                //    return false;
                //}

                //if (!Stream.TrySeek(Length - 2 - IdentifierSize, SeekOrigin.Current))
                //    return false;

                while (Stream.Position < Stream.Length)
                {
                    if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||    // Identifier
                        Identifier[0] != 0xFF ||                                // 0xFF _
                        !Stream.TryReverseRead(out ushort Length))              // Length

                    {
                        Stream.TrySeek(Begin, SeekOrigin.Begin);
                        return false;
                    }

                    // Check Start of Frame
                    if (Identifier[1] != 0xC0)
                    {
                        if (!Stream.TrySeek(Length - 2, SeekOrigin.Current))
                            return false;

                        continue;
                    }


                    if (!Stream.TrySeek(1, SeekOrigin.Current) ||
                        !Stream.TryReverseRead(out ushort Iw) ||
                        !Stream.TryReverseRead(out ushort Ih))
                        return false;

                    Height = Iw;
                    Width = Ih;
                    return true;
                }

                return false;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Identifier);
            }

        }

        /// <summary>
        /// Indicates whether the specified Identifier is jpg Identifier.
        /// </summary>
        /// <param name="Identifier">The specified Identifier.</param>
        public static bool Identify(byte[] Identifier)
            => Identifier.Length >= IdentifierSize &&
               Identifier[0] == 0xFF &&
               Identifier[1] == 0xD8;

        private static bool IdentifyMark(byte Mark)
            => Mark is 0x01 or  // TEM
                       0x0A or  // JXL
                       0x51 or  // SIZ
                       0x52 or  // COD
                       0x53 or  // COC
                       0x55 or  // TLM
                       0x57 or  // PLM
                       0x58 or  // PLT
                       0x5C or  // QCD
                       0x5D or  // QCC
                       0x5E or  // RGN
                       0x5F or  // POC
                       0x60 or  // PPM
                       0x61 or  // PPT
                       0x63 or  // CRG
                       0x64 or  // COM
                       0x65 or  // SEC
                       0x66 or  // EPB
                       0x67 or  // ESD
                       0x68 or  // EPC
                       0x69 or  // RED
                       0x90 or  // SOT
                       0x91 or  // SOP
                       0x92 or  // EPH
                       0x93 or  // SOD
                       0x94 or  // INSEC
                       0xC0 or  // SOF0
                       0xC1 or  // SOF1
                       0xC2 or  // SOF2
                       0xC3 or  // SOF3
                       0xC4 or  // DHT
                       0xC5 or  // SOF5
                       0xC6 or  // SOF6
                       0xC7 or  // SOF7
                       0xC8 or  // JPG
                       0xC9 or  // SOF9
                       0xCA or  // SOF10
                       0xCB or  // SOF11
                       0xCC or  // DAC
                       0xCD or  // SOF13
                       0xCE or  // SOF14
                       0xCF or  // SOF15
                       0xD0 or  // RST0
                       0xD1 or  // RST1
                       0xD2 or  // RST2
                       0xD3 or  // RST3
                       0xD4 or  // RST4
                       0xD5 or  // RST5
                       0xD6 or  // RST6
                       0xD7 or  // RST7
                       0xD8 or  // SOI
                       0xD9 or  // EOI/EOC
                       0xDA or  // SOS
                       0xDB or  // DQT
                       0xDC or  // DNL
                       0xDD or  // DRI
                       0xDE or  // DHP
                       0xDF or  // EXP
                       0xE0 or  // APP0
                       0xE1 or  // APP1
                       0xE2 or  // APP2
                       0xE3 or  // APP3
                       0xE4 or  // APP4
                       0xE5 or  // APP5
                       0xE6 or  // APP6
                       0xE7 or  // APP7
                       0xE8 or  // APP8
                       0xE9 or  // APP9
                       0xEA or  // APP10
                       0xEB or  // APP11
                       0xEC or  // APP12
                       0xED or  // APP13
                       0xEE or  // APP14
                       0xEF or  // APP15
                       0xF0 or  // JPG0
                       0xF1 or  // JPG1
                       0xF2 or  // JPG2
                       0xF3 or  // JPG3
                       0xF4 or  // JPG4
                       0xF5 or  // JPG5
                       0xF6 or  // JPG6
                       0xF7 or  // SOF48
                       0xF8 or  // LSE
                       0xF9 or  // JPG9
                       0xFA or  // JPG10
                       0xFB or  // JPG11
                       0xFC or  // JPG12
                       0xFD or  // JPG13
                       0xFE;    // COM

        [Conditional("DEBUG")]
        public static void Parse(Stream Stream)
        {
            MemoryStream ImageBuffer = null;
            byte[] Identifier = ArrayPool<byte>.Shared.Rent(IdentifierSize);
            try
            {
                // SOI
                if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||
                    !(Identifier[0] != 0xFF || Identifier[0] != 0xD8))
                {
                    Debug.WriteLine("This is not Ico file.");
                    return;
                }

                Debug.WriteLine($"==========================================");
                Debug.WriteLine($"                   SOI                    ");
                Debug.WriteLine($"==========================================");

                #region APP0
                if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||
                    !(Identifier[0] != 0xFF || Identifier[0] != 0xE0))
                {
                    Debug.WriteLine("This is not jpg file.");
                    return;
                }

                Debug.WriteLine($"================== APP0 ==================");

                if (!Stream.TryReverseRead(out ushort Length) ||
                    !Stream.TryReadString(5, Encoding.ASCII, out string JFIFIdentifier) ||
                    JFIFIdentifier != "JFIF\0")
                {
                    Debug.WriteLine("This is not jpg file.");
                    return;
                }

                {
                    if (!Stream.TryRead(out byte Version1) ||
                        !Stream.TryRead(out byte Version2) ||
                        !Stream.TryRead(out byte Units) ||
                        !Stream.TryReverseRead(out ushort DensityX) ||
                        !Stream.TryReverseRead(out ushort DensityY) ||
                        !Stream.TryRead(out byte ThumbnailWidth) ||
                        !Stream.TryRead(out byte ThumbnailHeight))
                        return;

                    JFIFIdentifier = new(JFIFIdentifier.Where(c => !char.IsControl(c)).ToArray());
                    string Version = $"{Version1}.{Version2}",
                           Unit = Units switch
                           {
                               0 => "None",
                               1 => "Dpi",
                               2 => "Dpcm",
                               _ => "Unknown",
                           };

                    Debug.WriteLine($"Length            : {Length}");           // 2 Bytes
                    Debug.WriteLine($"Identifier        : {JFIFIdentifier}");   // 5 Bytes
                    Debug.WriteLine($"Verstion          : {Version}");          // 2 Bytes
                    Debug.WriteLine($"Unit              : {Unit}");             // 1 Bytes
                    Debug.WriteLine($"DensityX          : {DensityX}");         // 2 Bytes
                    Debug.WriteLine($"DensityY          : {DensityY}");         // 2 Bytes
                    Debug.WriteLine($"Thumbnail Width   : {ThumbnailWidth}");   // 1 Bytes
                    Debug.WriteLine($"Thumbnail Height  : {ThumbnailHeight}");  // 1 Bytes

                    int ThumbnailLength = Length - 16;
                    if (ThumbnailLength > 0)
                    {
                        byte[] ThumbnailDatas = Stream.Read(ThumbnailLength);
                        Debug.WriteLine($"Thumbnail Data    : {string.Join(", ", ThumbnailDatas.Select(i => i.ToString("X2")))}");
                    }
                    else
                    {
                        Debug.WriteLine($"Thumbnail Data    :");
                    }
                }
                #endregion

                while (Stream.Position < Stream.Length)
                {
                    if (!Stream.ReadBuffer(Identifier, 0, IdentifierSize) ||
                        Identifier[0] != 0xFF ||
                        (Identifier[1] != 0xD9 && !Stream.TryReverseRead(out Length)))
                        return;

                    switch (Identifier[1])
                    {
                        #region APPn (Application)
                        case 0xE1:
                        case 0xE2:
                        case 0xE3:
                        case 0xE4:
                        case 0xE5:
                        case 0xE6:
                        case 0xE7:
                        case 0xE8:
                        case 0xE9:
                        case 0xEA:
                        case 0xEB:
                        case 0xEC:
                        case 0xED:
                        case 0xEE:
                        case 0xEF:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   APP{Identifier[1] - 0xE0}                   ");
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes

                                int DataLength = Length - 2;
                                if (DataLength > 0)
                                {
                                    byte[] Datas = Stream.Read(DataLength);
                                    Debug.WriteLine($"Data              : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");
                                }
                                else
                                {
                                    Debug.WriteLine($"Data              :");
                                }
                                break;
                            }
                        #endregion

                        #region DQT (Define Quantization Table)
                        case 0xDB:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   DQT                    ");
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes
                                int DataLength = Length - 2;
                                while (DataLength > 0)
                                {
                                    // Info
                                    DataLength--;
                                    if (!Stream.TryRead(out byte Info))
                                        return;

                                    // Table
                                    int Precision = Info >> 4,
                                        ID = Info & 0x0F,
                                        TableLength = (Precision + 1) << 6;

                                    Debug.WriteLine($"Precision         : {Precision}");

                                    byte[] Datas = Stream.Read(TableLength);
                                    Debug.WriteLine($"------------- Quantization{ID} --------------");
                                    for (int i = 0; i < TableLength; i += 8)
                                        Debug.WriteLine(string.Join(", ", Datas.Skip(i).Take(8).Select(i => i.ToString("X2"))));

                                    DataLength -= TableLength;
                                    if (DataLength < 0)
                                        return;
                                }
                                break;
                            }
                        #endregion

                        #region SOF (Start of Frame)
                        case 0xC0:
                        case 0xC1:
                        case 0xC2:
                        case 0xC3:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   SOF{Identifier[1] - 0xC0}                   ");
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes
                                Length -= 2;


                                Length -= 6;
                                if (!Stream.TryRead(out byte Precision) ||
                                    !Stream.TryReverseRead(out ushort Iw) ||
                                    !Stream.TryReverseRead(out ushort Ih) ||
                                    !Stream.TryRead(out byte Components))
                                    return;

                                Debug.WriteLine($"Precision         : {Precision}");        // 1 Bytes
                                Debug.WriteLine($"Width             : {Iw}");               // 2 Bytes
                                Debug.WriteLine($"Height            : {Ih}");               // 2 Bytes
                                Debug.WriteLine($"Components        : {Components}");       // 1 Bytes

                                for (int i = 0; i < Components; i++)
                                {
                                    // Component Info
                                    Length -= 3;
                                    if (!Stream.TryRead(out byte ID) ||
                                        !Stream.TryRead(out byte Info) ||
                                        !Stream.TryRead(out byte TableID))
                                        return;

                                    int HorizontalFactor = Info >> 4,
                                        VerticalFactor = Info & 0x0F;
                                    Debug.WriteLine($"--------------- Component{ID} ---------------");
                                    Debug.WriteLine($"Horizontal Factor : {HorizontalFactor}");
                                    Debug.WriteLine($"Vertical Factor   : {VerticalFactor}");
                                    Debug.WriteLine($"Quantization ID   : {TableID}");
                                }

                                if (Length != 0)
                                    return;

                                break;
                            }
                        #endregion

                        #region DHT (Define Huffman Table)
                        case 0xC4:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   DHT                    ");
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes

                                int DataLength = Length - 2;
                                byte[] Datas = ArrayPool<byte>.Shared.Rent(16);
                                try
                                {
                                    do
                                    {
                                        DataLength -= 17;
                                        if (!Stream.TryRead(out byte Info) ||
                                            !Stream.ReadBuffer(Datas, 0, 16))
                                            return;


                                        int TableIndex = Info & 0x0F;
                                        string Type = (Info >> 4) switch
                                        {
                                            0 => "DC",
                                            1 => "AC",
                                            _ => "Unknown"
                                        };

                                        Debug.WriteLine($"Index             : {TableIndex}");
                                        Debug.WriteLine($"Class             : {Type}");
                                        Debug.WriteLine($"------------- Huffman Table --------------");
                                        HuffmanDecodeTable Table = new();

                                        int LastCode = -1,
                                            LastBits = 0;

                                        for (int i = 0; i < 16; i++)
                                        {
                                            int CodeLength = Datas[i];
                                            if (CodeLength > 0)
                                            {
                                                int Bits = i + 1;

                                                DataLength -= CodeLength;
                                                byte[] CodeDatas = Stream.Read(CodeLength);

                                                for (int j = 0; j < CodeLength; j++)
                                                {
                                                    LastCode++;
                                                    if (((LastCode >> LastBits) & 1) > 0)
                                                        LastBits++;

                                                    //string Value = Convert.ToString(LastCode, 2);
                                                    //Value = Value.PadLeft(LastBits, '0');
                                                    //if (Value.Length < Bits)

                                                    if (LastBits < Bits)
                                                        LastCode <<= Bits - LastBits;

                                                    LastBits = Bits;
                                                    Table.Add(Bits, LastCode, CodeDatas[j]);
                                                }
                                            }
                                        }

                                        if (DataLength != 0)
                                            return;

                                        Debug.Write(Table);

                                    } while (DataLength > 0);
                                }
                                finally
                                {
                                    ArrayPool<byte>.Shared.Return(Datas);
                                }

                                break;
                            }
                        #endregion

                        #region DRI (Define Restart Interval) 
                        case 0xDD:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   DRI                    ");
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes

                                if (!Stream.TryRead(out ushort RestartInterval))
                                    return;

                                // 每 n 個 MCU 塊就有一個 RSTn 標記。
                                // 第一個標記是 RST0，第二個是 RST1 ……，RST7 後再從 RST0 重複。
                                Debug.WriteLine($"Restart Interval  : {RestartInterval}");  // 2 Bytes

                                break;
                            }
                        #endregion

                        #region COM (Comment)
                        case 0xFE:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   COM                    ");
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes

                                int DatasLength = Length - 2;
                                byte[] Datas = ArrayPool<byte>.Shared.Rent(DatasLength);
                                try
                                {
                                    if (!Stream.ReadBuffer(Datas, 0, DatasLength))
                                        return;

                                    Debug.WriteLine($"Content           : {Encoding.ASCII.GetString(Datas)}");
                                }
                                finally
                                {
                                    ArrayPool<byte>.Shared.Return(Datas);
                                }
                                break;
                            }
                        #endregion

                        #region SOS (Start of Scan)
                        case 0xDA:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   SOS                    ");
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes

                                int DataLength = Length - 2;

                                // Components;
                                Length--;
                                if (!Stream.TryRead(out byte Components))
                                    return;

                                Debug.WriteLine($"Component         : {Components}");
                                for (int i = 0; i < Components; i++)
                                {
                                    // Component Info
                                    Length -= 2;
                                    if (!Stream.TryRead(out byte ID) ||
                                        !Stream.TryRead(out byte Info))
                                        return;

                                    int DC = Info >> 4,
                                        AC = Info & 0x0F;

                                    Debug.WriteLine($"--------------- Component{ID} ---------------");
                                    Debug.WriteLine($"DC Table          : {DC}");
                                    Debug.WriteLine($"AC Table          : {AC}");
                                }

                                //Spectral
                                Length -= 3;
                                if (!Stream.TryRead(out byte SpectralSelectStart) ||
                                    !Stream.TryRead(out byte SpectralSelectEnd) ||
                                    !Stream.TryRead(out byte SuccessiveApprox) ||
                                    DataLength < 0)
                                    return;

                                Debug.WriteLine($"------------------------------------------");
                                Debug.WriteLine($"SpectralStart     : {SpectralSelectStart}");
                                Debug.WriteLine($"SpectralEnd       : {SpectralSelectEnd}");
                                Debug.WriteLine($"Successive Approx : {SuccessiveApprox}");

                                #region ImageDatas
                                const int ReadBufferSize = 8192;
                                ImageBuffer = new(ReadBufferSize);
                                byte[] ImageReadBuffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
                                try
                                {
                                    do
                                    {
                                        bool End = false;
                                        int ReadLength = Stream.Read(ImageReadBuffer, 0, ReadBufferSize),
                                            LastIndex = 0,
                                            i = 0;

                                        for (; i < ReadLength; i++)
                                        {
                                            if (ImageReadBuffer[i] == 0xFF)
                                            {
                                                int Next = i + 1;
                                                if (Next < ReadLength)
                                                {
                                                    byte Mark = ImageReadBuffer[Next];
                                                    if (Mark == 0x00)
                                                    {
                                                        ImageBuffer.Write(ImageReadBuffer, LastIndex, Next);
                                                        LastIndex = Next + 1;
                                                        i = Next;               // For-Loop will + 1
                                                    }

                                                    else if (Mark != 0xD0 &&  // RST0
                                                             Mark != 0xD1 &&  // RST1
                                                             Mark != 0xD2 &&  // RST2
                                                             Mark != 0xD3 &&  // RST3
                                                             Mark != 0xD4 &&  // RST4
                                                             Mark != 0xD5 &&  // RST5
                                                             Mark != 0xD6 &&  // RST6
                                                             Mark != 0xD7)    // RST7
                                                    {
                                                        End = true;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    ImageBuffer.Write(ImageReadBuffer, LastIndex, i - LastIndex);
                                                    ReadLength = Stream.Read(ImageReadBuffer, 0, ReadLength);
                                                    i = 0;

                                                    byte Mark = ImageReadBuffer[Next];
                                                    if (Mark == 0x00)
                                                    {
                                                        ImageBuffer.WriteByte(0xFF);
                                                        LastIndex = 1;
                                                        // For-Loop will set i = 1;
                                                    }

                                                    else if (IdentifyMark(Mark) &&
                                                             Mark != 0xD0 &&  // RST0
                                                             Mark != 0xD1 &&  // RST1
                                                             Mark != 0xD2 &&  // RST2
                                                             Mark != 0xD3 &&  // RST3
                                                             Mark != 0xD4 &&  // RST4
                                                             Mark != 0xD5 &&  // RST5
                                                             Mark != 0xD6 &&  // RST6
                                                             Mark != 0xD7)    // RST7
                                                    {
                                                        End = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        int L = i - LastIndex;
                                        if (L > 0)
                                            ImageBuffer.Write(ImageReadBuffer, LastIndex, L);

                                        if (End)
                                        {
                                            Stream = new ConcatStream(ImageReadBuffer, i, ReadLength - i, Stream);

                                            Debug.WriteLine($"==========================================");
                                            Debug.WriteLine($"                Image Data                ");
                                            Debug.WriteLine($"==========================================");
                                            Debug.WriteLine(string.Join(", ", ImageBuffer.ToArray().Select(i => $"{i:X2}")));
                                            break;
                                        }

                                    } while (Stream.Position < Stream.Length);
                                }
                                finally
                                {
                                    ArrayPool<byte>.Shared.Return(ImageReadBuffer);
                                }
                                #endregion

                                break;
                            }
                        #endregion

                        #region EOI (End of Image)
                        case 0xD9:
                            {
                                Debug.WriteLine($"==========================================");
                                Debug.WriteLine($"                   EOI                    ");
                                return;
                            }
                        #endregion

                        default:
                            {
                                Debug.WriteLine($"================== 0x{Identifier[1]:X2} ==================");
                                Debug.WriteLine($"Length            : {Length}");           // 2 Bytes

                                if (!Stream.TrySeek(Length - 2, SeekOrigin.Current))
                                    return;

                                break;
                            }
                    }
                }
            }
            finally
            {
                ImageBuffer?.Dispose();
                ArrayPool<byte>.Shared.Return(Identifier);
                Debug.WriteLine($"==========================================");
            }
        }

        public class HuffmanStream : Stream
        {
            private const int BufferSize = 8192;

            public override bool CanRead { get; }

            public override bool CanWrite { get; }

            public override bool CanSeek
                => false;

            public override long Length
                => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            private byte[] CoderBuffer;
            private readonly Stream Stream;
            private readonly bool LeaveOpen;
            public HuffmanStream(Stream Stream, HuffmanDecodeTable Table) : this(Stream, Table, false)
            {

            }
            public HuffmanStream(Stream Stream, HuffmanDecodeTable Table, bool LeaveOpen)
            {
                CanRead = true;

                DecodeTable = Table;

                ReadBitIndex = 0;
                ReadValue = 0;
                MaxBits = Table.Bits.Max();
                CoderBufferLength = CoderBufferIndex = BufferSize;
                CoderBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);

                this.Stream = Stream;
                this.LeaveOpen = LeaveOpen;
            }

            private byte[] LastData;
            private int LastIndex;
            private readonly int MaxBits;
            private readonly HuffmanDecodeTable DecodeTable;
            public override int Read(byte[] Buffer, int Offset, int Count)
            {
                CheckDispose();

                if (!CanRead)
                    throw new NotImplementedException();

                int Read = 0,
                    Index = Offset,
                    Bit = 0,
                    Code = 0;

                if (LastData != null)
                {
                    int DataLength = LastData.Length - LastIndex;
                    if (DataLength < Count)
                    {
                        Array.Copy(LastData, LastIndex, Buffer, Index, DataLength);
                        Index += DataLength;
                        Read = DataLength;
                        LastData = null;
                    }
                    else
                    {
                        LastIndex += Count;
                        Array.Copy(LastData, LastIndex, Buffer, Index, Count);
                        return Read;
                    }
                }

                while (TryReadBit(out int BitCode))
                {
                    Bit++;
                    if (MaxBits < Bit)
                        throw new IOException("Invalid decodetable.");

                    Code |= BitCode;

                    if (DecodeTable[Bit, Code] is byte[] Data)
                    {
                        Bit = 0;
                        Code = 0;

                        int DataLength = Data.Length,
                            NewRead = Read + DataLength;
                        if (NewRead < Count)
                        {
                            Array.Copy(Data, 0, Buffer, Index, DataLength);
                            Index += DataLength;
                            Read = NewRead;
                            continue;
                        }
                        else
                        {
                            LastIndex = Count - Read;
                            LastData = Data;
                            Array.Copy(Data, 0, Buffer, Index, LastIndex);
                            return Count;
                        }
                    }

                    Code <<= 1;
                }

                return Read;
            }

            private int CoderBufferLength, CoderBufferIndex, ReadValue, ReadBitIndex = 8;
            private bool TryReadBit(out int Bit)
            {
                if (ReadBitIndex == 8)
                {
                    if (CoderBufferIndex < CoderBufferLength)
                    {
                        ReadValue = CoderBuffer[CoderBufferIndex++];
                        ReadBitIndex = 0;
                    }
                    else
                    {
                        CoderBufferLength = Stream.Read(CoderBuffer, 0, BufferSize);
                        if (CoderBufferLength == 0)
                        {
                            Bit = 0;
                            return false;
                        }

                        ReadBitIndex = 0;
                        CoderBufferIndex = 1;
                        ReadValue = CoderBuffer[0];
                    }
                }

                Bit = (ReadValue >> (7 - ReadBitIndex)) & 1;
                ReadBitIndex++;
                return true;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                CheckDispose();

                if (!CanWrite)
                    throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
                => throw new NotSupportedException();

            public override void SetLength(long value)
                => throw new NotSupportedException();

            public override void Flush()
            {
                if (!CanWrite)
                    throw new NotSupportedException();

            }

            public override void Close()
            {
                if (IsDisposed)
                    return;

                base.Close();
            }

            private void CheckDispose()
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(nameof(ConcatStream));
            }

            private bool IsDisposed = false;
            protected override void Dispose(bool disposing)
            {
                if (IsDisposed)
                    return;

                try
                {
                    ArrayPool<byte>.Shared.Return(CoderBuffer);

                    if (!LeaveOpen)
                        Stream.Dispose();

                    base.Dispose(disposing);
                }
                finally
                {
                    IsDisposed = true;
                }
            }

        }

        public class HuffmanDecodeTable : IEnumerable<KeyValuePair<int, Dictionary<int, byte[]>>>
        {
            private readonly Dictionary<int, Dictionary<int, byte[]>> Context = new();

            public IEnumerable<int> Bits
                => Context.Keys;

            public IEnumerable<int> Codes
                => Context.SelectMany(i => i.Value.Keys);

            public Dictionary<int, byte[]> this[int Bit]
            {
                get => Bit <= 32 ? Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content) ? Content : null :
                       throw new NotSupportedException();
                set => Context[Bit] = value;
            }

            public byte[] this[int Bit, int Code]
            {
                get => Bit <= 32 ? (Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content) &&
                                    Content.TryGetValue(Code, out byte[] Values) ? Values : null) :
                       throw new NotSupportedException();
                set
                {
                    if (!Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content))
                    {
                        Content = new Dictionary<int, byte[]>();
                        Context.Add(Bit, Content);
                    }

                    Content.Add(Code, value);
                }
            }

            public void Add(int Bit, int Code, params byte[] Values)
            {
                if (!Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content))
                {
                    Content = new Dictionary<int, byte[]>();
                    Context.Add(Bit, Content);
                }

                Content.Add(Code, Values);
            }

            public void Remove(int Bit)
                => Context.Remove(Bit);
            public void Remove(int Bit, int Code)
            {
                if (Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content))
                    Content.Remove(Code);
            }

            public void Clear()
                => Context.Clear();

            public IEnumerator<KeyValuePair<int, Dictionary<int, byte[]>>> GetEnumerator()
                => Context.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator()
                => Context.GetEnumerator();

            public override string ToString()
            {
                StringBuilder Builder = new();
                try
                {
                    foreach (KeyValuePair<int, Dictionary<int, byte[]>> Content in Context)
                    {
                        int Bits = Content.Key;
                        foreach (KeyValuePair<int, byte[]> Data in Content.Value)
                            Builder.AppendLine($"{string.Join(", ", Data.Value.Select(i => $"{i:X2}"))} : {Convert.ToString(Data.Key, 2).PadLeft(Bits, '0')}");
                    }

                    return Builder.ToString();
                }
                finally
                {
                    Builder.Clear();
                }
            }

        }

    }
}