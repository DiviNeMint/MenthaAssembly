using MenthaAssembly.Utils;
using System;
using System.Buffers;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    // https://github.com/corkami/formats/blob/master/image/JPEGRGB_dissected.png
    public static unsafe class JpgCoder
    {
        /// <summary>
        /// The length in bytes of the jpg file format identifier.
        /// </summary>
        public const int IdentifierSize = 5;
        private const int TagSize = 2;

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

            byte[] TagBuffer = ArrayPool<byte>.Shared.Rent(TagSize);
            try
            {
                // Header
                if (!Stream.ReadBuffer(TagBuffer, 0, TagSize) ||            // SOI 
                    !(TagBuffer[0] != 0xFF || TagBuffer[0] != 0xD8) ||      // 0xFF 0xD8
                    !Stream.ReadBuffer(TagBuffer, 0, TagSize) ||            // APP0
                    !(TagBuffer[0] != 0xFF || TagBuffer[0] != 0xE0) ||      // 0xFF 0xE0
                    !Stream.TryReverseRead(out ushort Length) ||            // Length
                    !Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string Identifier) ||
                    !Identify(Identifier))
                {
                    Stream.TrySeek(Begin, SeekOrigin.Begin);
                    return false;
                }

                if (!Stream.TrySeek(Length - 2 - IdentifierSize, SeekOrigin.Current))
                    return false;

                while (Stream.Position < Stream.Length)
                {
                    if (!Stream.ReadBuffer(TagBuffer, 0, TagSize) ||    // Tag
                        TagBuffer[0] != 0xFF ||                         // 0xFF _
                        !Stream.TryReverseRead(out Length))             // Length

                    {
                        Stream.TrySeek(Begin, SeekOrigin.Begin);
                        return false;
                    }

                    // Check Start of Frame
                    if (TagBuffer[1] != 0xC0)
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
                ArrayPool<byte>.Shared.Return(TagBuffer);
            }

        }

        /// <summary>
        /// Indicates whether the specified Identifier is jpg Identifier.
        /// </summary>
        /// <param name="Identifier">The specified Identifier.</param>
        public static bool Identify(string Identifier)
            => Identifier.Length == IdentifierSize &&
               Identifier == "JFIF\0";

        [Conditional("DEBUG")]
        public static void Parse(Stream Stream)
        {
            const int ReadBufferSize = 8192;
            byte[] TagBuffer = ArrayPool<byte>.Shared.Rent(TagSize),
                   ImageReadBuffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
            MemoryStream ImageBuffer = new(ReadBufferSize);
            try
            {
                // SOI
                if (!Stream.ReadBuffer(TagBuffer, 0, TagSize) ||
                    !(TagBuffer[0] != 0xFF || TagBuffer[0] != 0xD8))
                {
                    Debug.WriteLine("This is not Ico file.");
                    return;
                }

                Debug.WriteLine($"==========================================");
                Debug.WriteLine($"                   SOI                    ");
                Debug.WriteLine($"==========================================");

                #region APP0
                if (!Stream.ReadBuffer(TagBuffer, 0, TagSize) ||
                    !(TagBuffer[0] != 0xFF || TagBuffer[0] != 0xE0))
                {
                    Debug.WriteLine("This is not Ico file.");
                    return;
                }

                Debug.WriteLine($"================== APP0 ==================");

                if (!Stream.TryReverseRead(out ushort Length) ||
                    !Stream.TryReadString(IdentifierSize, Encoding.ASCII, out string Identifier) ||
                    !Identify(Identifier))
                {
                    Debug.WriteLine("This is not Ico file.");
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

                    Identifier = new(Identifier.Where(c => !char.IsControl(c)).ToArray());
                    string Version = $"{Version1}.{Version2}",
                           Unit = Units switch
                           {
                               0 => "None",
                               1 => "Dpi",
                               2 => "Dpcm",
                               _ => "Unknown",
                           };

                    Debug.WriteLine($"Length            : {Length}");           // 2 Bytes
                    Debug.WriteLine($"Identifier        : {Identifier}");       // 5 Bytes
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

                bool StartImageDatas = false;
                while (Stream.Position < Stream.Length)
                {
                    #region ImageDatas
                    if (StartImageDatas)
                    {
                        do
                        {
                            int ReadLength = Stream.Read(ImageReadBuffer, 0, ReadBufferSize),
                                i = 0;

                            for (; i < ReadLength; i++)
                                if (ImageReadBuffer[i] == 0xFF)
                                    break;

                            ImageBuffer.Write(ImageReadBuffer, 0, i);
                            if (i < ReadLength)
                            {
                                Stream = new ConcatStream(ImageReadBuffer, i, ReadLength - i, Stream);
                                StartImageDatas = false;

                                Debug.WriteLine($"=============== Image Data ===============");
                                Debug.WriteLine(string.Join(", ", ImageReadBuffer.Select(i => i.ToString("X2"))));
                                break;
                            }

                        } while (Stream.Position < Stream.Length);
                    }
                    #endregion

                    if (!Stream.ReadBuffer(TagBuffer, 0, TagSize) ||
                        TagBuffer[0] != 0xFF ||
                        (TagBuffer[1] != 0xD9 && !Stream.TryReverseRead(out Length)))
                        return;

                    switch (TagBuffer[1])
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
                                Debug.WriteLine($"================== APP{TagBuffer[1] - 0xE0} ==================");
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
                                Debug.WriteLine($"================== DQT ===================");
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
                                Debug.WriteLine($"================== SOF{TagBuffer[1] - 0xC0} ==================");
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
                                Debug.WriteLine($"================== DHT ===================");
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
                                        Debug.WriteLine($"CodeLength Table  : {string.Join(", ", Datas.Select(i => i.ToString("X2")))}");

                                        for (int i = 0; i < 16; i++)
                                        {
                                            int CodeLength = Datas[i];
                                            if (CodeLength > 0)
                                            {
                                                byte[] CodeDatas = Stream.Read(CodeLength);
                                                DataLength -= CodeLength;
                                                Debug.WriteLine($"{CodeLength} code of {i + 1} bits  : {string.Join(", ", CodeDatas.Select(i => i.ToString("X2")))}");
                                            }
                                        }

                                        if (DataLength != 0)
                                            return;

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
                                Debug.WriteLine($"================== DRI ===================");
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
                                Debug.WriteLine($"================== COM ===================");
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
                                Debug.WriteLine($"================== SOS ===================");
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

                                StartImageDatas = true;
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
                                Debug.WriteLine($"================== 0x{TagBuffer[1]:X2} ==================");
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
                ArrayPool<byte>.Shared.Return(TagBuffer);
                ArrayPool<byte>.Shared.Return(ImageReadBuffer);
                ImageBuffer.Dispose();
                Debug.WriteLine($"==========================================");
            }
        }

    }
}