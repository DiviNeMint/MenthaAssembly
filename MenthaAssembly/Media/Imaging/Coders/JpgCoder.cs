using MenthaAssembly.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    // https://github.com/corkami/formats/blob/master/image/JPEGRGB_dissected.png
    // https://blog.csdn.net/u010192735/article/details/120860826
    // https://blog.csdn.net/menglongbor/article/details/89742771
    // https://blog.csdn.net/u010192735/article/details/120869528
    // https://blog.csdn.net/weixin_58208902/article/details/125560863
    // https://github.com/MROS/jpeg_tutorial/blob/master/doc/
    public static unsafe partial class JpgCoder
    {
        /// <summary>
        /// The length in bytes of the jpg file format identifier.
        /// </summary>
        public const int IdentifierSize = 2;

        private static readonly byte[] Zigzag =
        {
             0,  1,  5,  6, 14, 15, 27, 28,
             2,  4,  7, 13, 16, 26, 29, 42,
             3,  8, 12, 17, 25, 30, 41, 43,
             9, 11, 18, 24, 31, 40, 44, 53,
            10, 19, 23, 32, 39, 45, 52, 54,
            20, 22, 33, 38, 46, 51, 55, 60,
            21, 34, 37, 47, 50, 56, 59, 61,
            35, 36, 48, 49, 57, 58, 62, 63,
        };
        private static readonly byte[] ReverseZigzag =
        {
             0,  1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63,
        };

        static JpgCoder()
        {
            const int BlockSize = 8;
            const int MatrixSize = BlockSize * BlockSize;

            // DCT
            DctMatrix = new double[MatrixSize];
            int i = 0;

            double Factor = Math.Sqrt(0.125d);
            for (int x = 0; x < BlockSize; x++)
                DctMatrix[i++] = Factor;

            Factor = Math.PI * 0.0625d;
            for (int y = 1; y < BlockSize; y++)
                for (int x = 0; x < BlockSize; x++)
                    DctMatrix[i++] = 0.5d * Math.Cos(((x << 1) + 1) * y * Factor);

            // DCTT
            DctTMatrix = new double[MatrixSize];
            for (int y = 0; y < BlockSize; y++)
            {
                int u = y * BlockSize + y,
                    v = u + BlockSize;

                DctTMatrix[u] = DctMatrix[u++];
                for (int x = y + 1; x < BlockSize; x++, u++, v += BlockSize)
                {
                    DctTMatrix[u] = DctMatrix[v];
                    DctTMatrix[v] = DctMatrix[u];
                }
            }
        }

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

            List<byte> DecodeOrder = new();
            byte[] DCQTable = null;
            Dictionary<byte, int> CHSFactors = new(),                   // Component ID, Factor
                                  CVSFactors = new(),                   // Component ID, Factor
                                  CACTableMaxBits = new(),              // Component ID, Max Bits
                                  CDCTableMaxBits = new();              // Component ID, Max Bits
            Dictionary<byte, byte[]> CQTables = new();                  // Component ID, Table Content
            Dictionary<byte, HuffmanDecodeTable> CACTables = new(),     // Component ID, Table Content
                                                 CDCTables = new();     // Component ID, Table Content

            // Headers
            byte[] Identifier = ArrayPool<byte>.Shared.Rent(IdentifierSize);
            try
            {
                Dictionary<byte, int> CQTSelectors = new();             // Component ID, Quantization Table ID
                Dictionary<int, byte[]> QTables = new();                // Table ID, Table Content
                Dictionary<int, HuffmanDecodeTable> ACTables = new(),   // Table ID, Table Content
                                                    DCTables = new();   // Table ID, Table Content

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

                            QTables.Add(ID, TableDatas);
                        }
                    }
                    #endregion

                    #region SOF (Start of Frame)
                    else if (Identifier[1] is 0xC0 or
                             0xC1 or
                             0xC2 or
                             0xC3)
                    {
                        if (!Stream.TrySeek(1, SeekOrigin.Current) ||               // !Stream.TryRead(out byte BitDepth) ||
                            !Stream.TryReverseRead(out ushort Height) ||
                            !Stream.TryReverseRead(out ushort Width) ||
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
                            CHSFactors.Add(ID, Info >> 4);
                            CVSFactors.Add(ID, Info & 0x0F);
                            CQTSelectors.Add(ID, TableID);
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
                        // Image Size
                        if (Iw < 0 || Ih < 0)
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        // Component Count
                        int DataLength = Length - 3;                    // Length 2 Bytes & Components 1 Byte
                        if (!Stream.TryRead(out byte Components))
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        // Component Infos
                        for (int i = 0; i < Components; i++)
                        {
                            DataLength -= 2;
                            if (!Stream.TryRead(out byte ID) ||
                                !Stream.TryRead(out byte Info) ||
                                !ACTables.TryGetValue(Info & 0x0F, out HuffmanDecodeTable ACTable) ||
                                !DCTables.TryGetValue(Info >> 4, out HuffmanDecodeTable DCTable) ||
                                !CQTSelectors.TryGetValue(ID, out int TableID) ||
                                !QTables.TryGetValue(TableID, out byte[] QuantizationTable))
                            {
                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            DecodeOrder.Add(ID);
                            CACTables.Add(ID, ACTable);
                            CDCTables.Add(ID, DCTable);
                            CACTableMaxBits.Add(ID, ACTable.Bits.Max());
                            CDCTableMaxBits.Add(ID, DCTable.Bits.Max());
                            CQTables.Add(ID, QuantizationTable);
                        }

                        if (!QTables.TryGetValue(0, out DCQTable))
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        // SpectralSelectStart
                        // SpectralSelectEnd
                        // SuccessiveApprox
                        DataLength -= 3;
                        if (!Stream.TrySeek(3, SeekOrigin.Current) ||
                            DataLength < 0)
                        {
                            Stream.TrySeek(Begin, SeekOrigin.Begin);
                            return false;
                        }

                        break;
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

            // Image Datas
            const int MCUBlockSize = 8,
                      ReadBufferSize = 8192;
            byte[] ImageReadBuffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
            try
            {
                Dictionary<byte, int> LastDC = DecodeOrder.ToDictionary(k => k, i => 0);    // Component ID, Last Absolute DC Value

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
                                Value = 0;
                                return false;
                            }

                            else if (Mark is not 0x00)
                            {
                                Value = 0;
                                return false;
                            }
                        }
                    }

                    Value = (ReadValue >> (7 - ReadBitIndex)) & 1;
                    ReadBitIndex++;
                    return true;
                }
                bool TryRead(HuffmanDecodeTable Table, int MaxBits, out byte[] Value, out bool Reset)
                {
                    int Code = 0;
                    for (int Bit = 1; Bit <= MaxBits; Bit++, Code <<= 1)
                    {
                        if (!TryReadBit(out int BitCode, out Reset))
                        {
                            Value = null;
                            return false;
                        }

                        Code |= BitCode;
                        if (Table[Bit, Code] is byte[] Data)
                        {
                            Value = Data;
                            return true;
                        }
                    }

                    Reset = false;
                    Value = null;
                    return false;
                }
                bool TryDecodeBlocks(byte ComponentID, out List<double[]> Blocks)
                {
                    const int BlockLength = 64;
                    if (!CDCTables.TryGetValue(ComponentID, out HuffmanDecodeTable DCTable) ||
                        !CACTables.TryGetValue(ComponentID, out HuffmanDecodeTable ACTable) ||
                        !CDCTableMaxBits.TryGetValue(ComponentID, out int DCMaxBits) ||
                        !CACTableMaxBits.TryGetValue(ComponentID, out int ACMaxBits) ||
                        !CQTables.TryGetValue(ComponentID, out byte[] QuantizationTable) ||
                        !CHSFactors.TryGetValue(ComponentID, out int Bw) ||
                        !CVSFactors.TryGetValue(ComponentID, out int Bh))
                    {
                        Blocks = null;
                        return false;
                    }

                    Blocks = new List<double[]>();
                    for (int j = 0; j < Bh; j++)
                    {
                        for (int i = 0; i < Bw; i++)
                        {
                            // DC Info
                            if (!TryRead(DCTable, DCMaxBits, out byte[] DCInfo, out bool Reset))
                            {
                                ReturnBlocks(Blocks);
                                Blocks.Clear();
                                Blocks = null;

                                // RSTn
                                if (Reset)
                                {
                                    // Reset Last DC
                                    foreach (byte Key in LastDC.Keys.ToArray())
                                        LastDC[Key] = 0;

                                    return TryDecodeBlocks(ComponentID, out Blocks);
                                }

                                return false;
                            }

                            // Last DC
                            if (!LastDC.TryGetValue(ComponentID, out int DC))
                            {
                                ReturnBlocks(Blocks);
                                Blocks.Clear();
                                Blocks = null;
                                return false;
                            }

                            // Read DC
                            int DCLength = DCInfo[0] & 0x0F;
                            if (DCLength > 0)
                            {
                                if (!TryReadBit(out int Code, out Reset))
                                {
                                    ReturnBlocks(Blocks);
                                    Blocks.Clear();
                                    Blocks = null;

                                    // RSTn
                                    if (Reset)
                                    {
                                        // Reset Last DC
                                        foreach (byte Key in LastDC.Keys.ToArray())
                                            LastDC[Key] = 0;

                                        return TryDecodeBlocks(ComponentID, out Blocks);
                                    }

                                    return false;
                                }

                                bool Negative = Code == 0;
                                for (int z = 1; z < DCLength; z++)
                                {
                                    if (!TryReadBit(out int NewCode, out Reset) || Reset)
                                    {
                                        ReturnBlocks(Blocks);
                                        Blocks.Clear();
                                        Blocks = null;

                                        // RSTn
                                        if (Reset)
                                        {
                                            // Reset Last DC
                                            foreach (byte Key in LastDC.Keys.ToArray())
                                                LastDC[Key] = 0;

                                            return TryDecodeBlocks(ComponentID, out Blocks);
                                        }

                                        return false;
                                    }

                                    Code = (Code << 1) | NewCode;
                                }

                                if (Negative)
                                    Code -= (1 << DCLength) - 1;

                                // Dequantization
                                Code *= DCQTable[0];

                                // Set DC
                                DC += Code;
                                LastDC[ComponentID] = DC;
                            }

                            double[] Block = ArrayPool<double>.Shared.Rent(BlockLength);
                            Block[0] = DC;
                            for (int z = 1; z < BlockLength;)
                            {
                                // AC Info
                                if (!TryRead(ACTable, ACMaxBits, out byte[] ACInfo, out Reset))
                                {
                                    ReturnBlocks(Blocks);
                                    Blocks.Clear();
                                    Blocks = null;

                                    // RSTn
                                    if (Reset)
                                    {
                                        // Reset Last DC
                                        foreach (byte Key in LastDC.Keys.ToArray())
                                            LastDC[Key] = 0;

                                        return TryDecodeBlocks(ComponentID, out Blocks);
                                    }

                                    return false;
                                }

                                int Info = ACInfo[0];

                                // EOB (End of Block)
                                if (Info == 0x00)
                                {
                                    for (; z < BlockLength; z++)
                                    {
                                        // Reverse Zigzag
                                        int Index = ReverseZigzag[z];

                                        // Set AC
                                        Block[Index] = 0;
                                    }
                                    break;
                                }

                                // ZRL (Zero Run Length)
                                else if (Info == 0xF0)
                                {
                                    for (int v = 0; v < 16; v++, z++)
                                    {
                                        // Reverse Zigzag
                                        int Index = ReverseZigzag[z];

                                        // Set AC
                                        Block[Index] = 0;
                                    }
                                }

                                else
                                {
                                    int ZeroLength = Info >> 4;
                                    for (int v = 0; v < ZeroLength; v++, z++)
                                    {
                                        // Reverse Zigzag
                                        int Index = ReverseZigzag[z];

                                        // Set AC
                                        Block[Index] = 0;
                                    }

                                    // Read AC
                                    int ACLength = Info & 0x0F;
                                    if (ACLength > 0)
                                    {
                                        if (!TryReadBit(out int AC, out Reset))
                                        {
                                            ReturnBlocks(Blocks);
                                            Blocks.Clear();
                                            Blocks = null;

                                            // RSTn
                                            if (Reset)
                                            {
                                                // Reset Last DC
                                                foreach (byte Key in LastDC.Keys.ToArray())
                                                    LastDC[Key] = 0;

                                                return TryDecodeBlocks(ComponentID, out Blocks);
                                            }

                                            return false;
                                        }

                                        bool Negative = AC == 0;
                                        for (int u = 1; u < ACLength; u++)
                                        {
                                            if (!TryReadBit(out int NewCode, out Reset))
                                            {
                                                ReturnBlocks(Blocks);
                                                Blocks.Clear();
                                                Blocks = null;

                                                // RSTn
                                                if (Reset)
                                                {
                                                    // Reset Last DC
                                                    foreach (byte Key in LastDC.Keys.ToArray())
                                                        LastDC[Key] = 0;

                                                    return TryDecodeBlocks(ComponentID, out Blocks);
                                                }

                                                return false;
                                            }

                                            AC = (AC << 1) | NewCode;
                                        }

                                        if (Negative)
                                            AC -= (1 << ACLength) - 1;

                                        // Dequantization
                                        AC *= QuantizationTable[z];

                                        // Reverse Zigzag
                                        int Index = ReverseZigzag[z++];

                                        // Set AC
                                        Block[Index] = AC;
                                    }
                                }
                            }

                            // IDCT
                            double[] IDCTBlocks = IDCT(Block);
                            ArrayPool<double>.Shared.Return(Block);

                            Blocks.Add(IDCTBlocks);
                        }
                    }

                    return true;
                }
                void ReturnBlocks(List<double[]> Blocks)
                {
                    foreach (double[] Block in Blocks)
                        ArrayPool<double>.Shared.Return(Block);
                }

                // MCU Infos
                int MaxHSFactor = CHSFactors.Max(i => i.Value),
                    MaxVSFactor = CVSFactors.Max(i => i.Value),
                    MCUWidth = MaxHSFactor << 3,
                    MCUHeight = MaxVSFactor << 3,
                    MCUXBlockCount = (Iw + MCUWidth - 1) / MCUWidth,
                    MCUYBlockCount = (Ih + MCUHeight - 1) / MCUHeight;

                // YCbCr to RGB parameters
                if (!CHSFactors.TryGetValue(0x01, out int YHSFactor) ||
                    !CHSFactors.TryGetValue(0x02, out int CbHSFactor) ||
                    !CHSFactors.TryGetValue(0x03, out int CrHSFactor) ||
                    !CVSFactors.TryGetValue(0x01, out int YVSFactor) ||
                    !CVSFactors.TryGetValue(0x02, out int CbVSFactor) ||
                    !CVSFactors.TryGetValue(0x03, out int CrVSFactor))
                {
                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                    return false;
                }

                int HSFactorLCM = MathHelper.LCM(CHSFactors.Values.ToArray()),
                    VSFactorLCM = MathHelper.LCM(CVSFactors.Values.ToArray()),
                    YMaxHSFactor = HSFactorLCM / YHSFactor,
                    CbMaxHSFactor = HSFactorLCM / CbHSFactor,
                    CrMaxHSFactor = HSFactorLCM / CrHSFactor,
                    YMaxVSFactor = VSFactorLCM / YVSFactor,
                    CbMaxVSFactor = VSFactorLCM / CbVSFactor,
                    CrMaxVSFactor = VSFactorLCM / CrVSFactor,
                    HStep = HSFactorLCM / MaxHSFactor,
                    VStep = VSFactorLCM / MaxVSFactor;

                // Image Datas
                int Stride = Iw * 3;
                byte[] ImageDatas = new byte[Stride * Ih];

                // Decode
                for (int MCUBj = 0, Iy0 = 0; MCUBj < MCUYBlockCount; MCUBj++, Iy0 += MCUHeight)
                {
                    for (int MCUBi = 0, Ix0 = 0; MCUBi < MCUXBlockCount; MCUBi++, Ix0 += MCUWidth)
                    {
                        List<double[]> YBlocks = null,
                                       CbBlocks = null,
                                       CrBlocks = null;

                        // Decodes Blocks
                        foreach (byte ComponentID in DecodeOrder)
                        {
                            if (!TryDecodeBlocks(ComponentID, out List<double[]> Blocks))
                            {
                                if (YBlocks != null)
                                    ReturnBlocks(YBlocks);

                                if (CbBlocks != null)
                                    ReturnBlocks(CbBlocks);

                                if (CrBlocks != null)
                                    ReturnBlocks(CrBlocks);

                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }

                            // Y
                            if (ComponentID == 1)
                                YBlocks = Blocks;

                            // Cb
                            else if (ComponentID == 2)
                                CbBlocks = Blocks;

                            // Cr
                            else if (ComponentID == 3)
                                CrBlocks = Blocks;

                            // Unknown
                            else
                            {
                                if (YBlocks != null)
                                    ReturnBlocks(YBlocks);

                                if (CbBlocks != null)
                                    ReturnBlocks(CbBlocks);

                                if (CrBlocks != null)
                                    ReturnBlocks(CrBlocks);

                                Stream.TrySeek(Begin, SeekOrigin.Begin);
                                return false;
                            }
                        }

                        //YCbCr to RGB
                        int YBi0 = 0,           // Index of Y blocks.
                            YBy = 0,            // Y-coordinate in current Y block.
                            YVFactor = 0,       // Counter to calculate YBy & YBj.
                            CbBi0 = 0,          // Index of Cb blocks.
                            CbBy = 0,           // Y-coordinate in current Cb block.
                            CbVFactor = 0,      // Counter to calculate CbBy & CbBj.
                            CrBi0 = 0,          // Index of Cr blocks.
                            CrBy = 0,           // Y-coordinate in current Cr block.
                            CrVFactor = 0;      // Counter to calculate CrBy & CrBj.
                        for (int MCUy = 0, Iy = Iy0; MCUy < MCUHeight && Iy < Ih; MCUy++, Iy++, YVFactor += VStep, CbVFactor += VStep, CrVFactor += VStep)
                        {
                            // Checks Y Vertical Block Index
                            while (YVFactor >= YMaxVSFactor)
                            {
                                YVFactor -= YMaxVSFactor;
                                YBy++;
                            }
                            while (YBy >= MCUBlockSize)
                            {
                                YBy -= MCUBlockSize;
                                YBi0 += YHSFactor;
                            }

                            // Checks Cb Vertical Block Index
                            while (CbVFactor >= CbMaxVSFactor)
                            {
                                CbVFactor -= CbMaxVSFactor;
                                CbBy++;
                            }
                            while (CbBy >= MCUBlockSize)
                            {
                                CbBy -= MCUBlockSize;
                                CbBi0 += CbHSFactor;
                            }

                            // Checks Cr Vertical Block Index
                            while (CrVFactor >= CrMaxVSFactor)
                            {
                                CrVFactor -= CrMaxVSFactor;
                                CrBy++;

                            }
                            while (CrBy >= MCUBlockSize)
                            {
                                CrBy -= MCUBlockSize;
                                CrBi0 += CrHSFactor;
                            }

                            int YBi = YBi0,
                                YBOffset = YBy * MCUBlockSize,      // The Y offset from the original to the current y-coordinate.
                                YBx = 0,                            // X-coordinate in current Y block.
                                YHFactor = 0,                       // Counter to calculate YBy & YBx.
                                CbBi = CbBi0,
                                CbBOffset = CbBy * MCUBlockSize,    // The Cb offset from the original to the current y-coordinate.
                                CbBx = 0,                           // X-coordinate in current Cb block.
                                CbHFactor = 0,                      // Counter to calculate CbBy & CbBx.
                                CrBi = CrBi0,
                                CrBOffset = CrBy * MCUBlockSize,    // The Cr offset from the original to the current y-coordinate.
                                CrBx = 0,                           // X-coordinate in current Cr block.
                                CrHFactor = 0,                      // Counter to calculate CrBx & CrBx.
                                Offset = Iy * Stride + Ix0 * 3;
                            for (int MCUx = 0, Ix = Ix0; MCUx < MCUWidth && Ix < Iw; MCUx++, Ix++, YHFactor += HStep, CbHFactor += HStep, CrHFactor += HStep)
                            {
                                // Checks Y Horizontal Block Index
                                while (YHFactor >= YMaxHSFactor)
                                {
                                    YHFactor -= YMaxHSFactor;
                                    YBx++;
                                }
                                while (YBx >= MCUBlockSize)
                                {
                                    YBx -= MCUBlockSize;
                                    YBi++;
                                }

                                // Checks Cb Horizontal Block Index
                                while (CbHFactor >= CbMaxHSFactor)
                                {
                                    CbHFactor -= CbMaxHSFactor;
                                    CbBx++;
                                }
                                while (CbBx >= MCUBlockSize)
                                {
                                    CbBx -= MCUBlockSize;
                                    CbBi++;
                                }

                                // Checks Cr Horizontal Block Index
                                while (CrHFactor >= CrMaxHSFactor)
                                {
                                    CrHFactor -= CrMaxHSFactor;
                                    CrBx++;

                                }
                                while (CrBx >= MCUBlockSize)
                                {
                                    CrBx -= MCUBlockSize;
                                    CrBi++;
                                }

                                double Y = YBlocks[YBi][YBOffset + YBx] + 128d,
                                       Cb = CbBlocks[CbBi][CbBOffset + CbBx],
                                       Cr = CrBlocks[CrBi][CrBOffset + CrBx];

                                ImageDatas[Offset++] = (byte)MathHelper.Clamp(Y + 1.402d * Cr, 0d, 255d);
                                ImageDatas[Offset++] = (byte)MathHelper.Clamp(Y - 0.34414d * Cb - 0.71414d * Cr, 0d, 255d);
                                ImageDatas[Offset++] = (byte)MathHelper.Clamp(Y + 1.772d * Cb, 0d, 255d);
                            }
                        }
                    }
                }

                Image = new ImageContext<RGB>(Iw, Ih, ImageDatas);
                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(ImageReadBuffer);
            }
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

        private static readonly double[] DctMatrix, DctTMatrix;
        private static double[] DCT(double[] Block)
            => MatrixMultiply(DctMatrix, Block, DctTMatrix);
        private static double[] IDCT(double[] Block)
            => MatrixMultiply(DctTMatrix, Block, DctMatrix);
        private static double[] MatrixMultiply(double[] m1, double[] m2, double[] m3)
        {
            double[] Matrix = MatrixMultiply(m1, m2);
            try
            {
                return MatrixMultiply(Matrix, m3);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(Matrix);
            }
        }
        private static double[] MatrixMultiply(double[] m1, double[] m2)
        {
            const int BlockSize = 8;
            const int MatrixSize = BlockSize * BlockSize;

            double[] m = ArrayPool<double>.Shared.Rent(MatrixSize);
            for (int Oy = 0; Oy < MatrixSize; Oy += BlockSize)
            {
                for (int x = 0; x < BlockSize; x++)
                {
                    double sum = 0d;
                    for (int k1 = 0, k2 = x; k1 < BlockSize; k1++, k2 += BlockSize)
                        sum += m1[Oy + k1] * m2[k2];

                    m[Oy + x] = sum;
                }
            }

            return m;
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
                                    !Stream.TryReverseRead(out ushort Ih) ||
                                    !Stream.TryReverseRead(out ushort Iw) ||
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
                                                        ImageBuffer.Write(ImageReadBuffer, LastIndex, Next - LastIndex);
                                                        LastIndex = Next + 1;
                                                        i = Next;               // For-Loop will + 1
                                                    }

                                                    // RSTn Mark
                                                    else if (Mark is 0xD0 or 0xD1 or 0xD2 or 0xD3 or 0xD4 or 0xD5 or 0xD6 or 0xD7)
                                                    {
                                                        // Remove
                                                        //End = true;
                                                        //break;
                                                    }

                                                    else if (IdentifyMark(Mark))
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

                                                    // RSTn Mark
                                                    else if (Mark is 0xD0 or 0xD1 or 0xD2 or 0xD3 or 0xD4 or 0xD5 or 0xD6 or 0xD7)
                                                    {
                                                        // Display
                                                        ImageBuffer.WriteByte(0xFF);
                                                        LastIndex = 0;

                                                        //// Remove
                                                        //End = true;
                                                        //break;
                                                    }

                                                    else if (IdentifyMark(Mark))
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

    }
}