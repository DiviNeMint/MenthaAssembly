using System;
using System.IO;

namespace MenthaAssembly
{
    public class ImageContext
    {
        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public int Channel { get; }

        public IntPtr Scan0 { get; }

        public IntPtr ScanA { get; }

        public IntPtr ScanR { get; }

        public IntPtr ScanG { get; }

        public IntPtr ScanB { get; }

        public int BitsPerPixel { get; }

        public int PixelBytes { get; }

        public ImageContext(int Width, int Height, IntPtr Scan0) : this(Width, Height, Scan0, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.BitsPerPixel = (Stride << 3) / Width;
            this.PixelBytes = (BitsPerPixel + 7) >> 3;
            this.Scan0 = Scan0;
            this.Channel = 1;
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, int BitsPerPixel)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.BitsPerPixel = BitsPerPixel;
            this.PixelBytes = (BitsPerPixel + 7) >> 3;
            this.Scan0 = Scan0;
            this.Channel = 1;
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) : this(Width, Height, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.ScanR = ScanR;
            this.ScanG = ScanG;
            this.ScanB = ScanB;
            this.PixelBytes = 3;
            this.BitsPerPixel = 24;
            this.Channel = 3;
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) : this(Width, Height, ScanA, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.ScanA = ScanA;
            this.ScanR = ScanR;
            this.ScanG = ScanG;
            this.ScanB = ScanB;
            this.PixelBytes = 4;
            this.BitsPerPixel = 32;
            this.Channel = 4;
        }

        protected byte[][] Datas { set; get; } = new byte[4][];
        public ImageContext(int Width, int Height, byte[] Datas) : this(Width, Height, Datas, Datas.Length / Height)
        {
        }
        public ImageContext(int Width, int Height, byte[] Datas, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 1;
            this.BitsPerPixel = (Stride << 3) / Width;
            this.PixelBytes = (BitsPerPixel + 7) >> 3;
            this.Stride = Stride;

            this.Datas[0] = Datas;
            unsafe
            {
                fixed (byte* Buffer = &this.Datas[0][0])
                    Scan0 = (IntPtr)Buffer;
            }
        }
        public ImageContext(int Width, int Height, byte[] Datas, int Stride, int BitsPerPixel)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 1;
            this.PixelBytes = (BitsPerPixel + 7) >> 3;
            this.BitsPerPixel = BitsPerPixel;
            this.Stride = Stride;

            this.Datas[0] = Datas;
            unsafe
            {
                fixed (byte* Buffer = &this.Datas[0][0])
                    Scan0 = (IntPtr)Buffer;
            }
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) : this(Width, Height, DataR, DataG, DataB, DataR.Length / Height)
        {
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 3;
            this.BitsPerPixel = 24;
            this.PixelBytes = 3;
            this.Stride = Stride;

            this.Datas[0] = DataR;
            this.Datas[1] = DataG;
            this.Datas[2] = DataB;
            unsafe
            {
                fixed (byte* R = &this.Datas[0][0],
                             G = &this.Datas[1][0],
                             B = &this.Datas[2][0])
                {
                    ScanR = (IntPtr)R;
                    ScanG = (IntPtr)G;
                    ScanB = (IntPtr)B;
                }
            }
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) : this(Width, Height, DataA, DataR, DataG, DataB, DataA.Length / Height)
        {
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 4;
            this.PixelBytes = 4;
            this.BitsPerPixel = 32;
            this.Stride = Stride;

            this.Datas[0] = DataA;
            this.Datas[1] = DataR;
            this.Datas[2] = DataG;
            this.Datas[3] = DataB;
            unsafe
            {
                fixed (byte* A = &this.Datas[0][0],
                             R = &this.Datas[1][0],
                             G = &this.Datas[2][0],
                             B = &this.Datas[3][0])
                {
                    ScanA = (IntPtr)A;
                    ScanR = (IntPtr)R;
                    ScanG = (IntPtr)G;
                    ScanB = (IntPtr)B;
                }
            }
        }

        public static ImageContext LoadBmp(string FilePath)
        {
            FileStream FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read);



            byte[] Datas = new byte[sizeof(int)];

            // Width
            FS.Seek(18, SeekOrigin.Begin);
            FS.Read(Datas, 0, Datas.Length);
            int Width = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // Height
            FS.Read(Datas, 0, Datas.Length);
            int Height = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // BitsPerPixel
            FS.Seek(28, SeekOrigin.Begin);
            FS.Read(Datas, 0, sizeof(short));
            int Bits = Datas[0] | Datas[1] << 8;

            // ImageSize
            FS.Seek(34, SeekOrigin.Begin);
            FS.Read(Datas, 0, Datas.Length);
            int ImageSize = Datas[0] | Datas[1] << 8 | Datas[2] << 16 | Datas[3] << 24;

            // ImageDatas
            Datas = new byte[ImageSize];
            FS.Seek(54, SeekOrigin.Begin);
            FS.Read(Datas, 0, Datas.Length);

            FS.Close();
            return new ImageContext(Width, Height, Datas, ImageSize / Height, Bits);

            //Debug.WriteLine($"File Header\r\n" +                                                                            // 14 Bytes (Total)
            //                $"Format : {Datas[0]}, {Datas[1]}\r\n" +                                                        //  2 Bytes
            //                $"Size   : {Datas[2] | Datas[3] << 8 | Datas[4] << 16 | Datas[5] << 24}\r\n" +                  //  4 Bytes (int)
            //                // Reserved 4 byte                                                                              //  4 Bytes
            //                $"Offset : {Datas[10] | Datas[11] << 8 | Datas[12] << 16 | Datas[13] << 24}\r\n" +              //  4 Bytes (int)

            //                $"Info Header\r\n" +                                                                            // 40 Bytes (Total)
            //                $"Size   : {Datas[14] | Datas[15] << 8 | Datas[16] << 16 | Datas[17] << 24}\r\n" +              //  4 Bytes (int)
            //                $"Width  : {Datas[18] | Datas[19] << 8 | Datas[20] << 16 | Datas[21] << 24}\r\n" +              //  4 Bytes (int)
            //                $"Height : {Datas[22] | Datas[23] << 8 | Datas[24] << 16 | Datas[25] << 24}\r\n" +              //  4 Bytes (int)
            //                $"Planes : {Datas[26] | Datas[27] << 8}\r\n" +                                                  //  2 Bytes (short)
            //                $"Bits   : {Datas[28] | Datas[29] << 8}\r\n" +                                                  //  2 Bytes (short)
            //                $"Compression : {Datas[30] | Datas[31] << 8 | Datas[32] << 16 | Datas[33] << 24}\r\n" +         //  4 Bytes (int)
            //                $"ImageSize   : {Datas[34] | Datas[35] << 8 | Datas[36] << 16 | Datas[37] << 24}\r\n" +         //  4 Bytes (int)
            //                $"XResolution : {Datas[38] | Datas[39] << 8 | Datas[40] << 16 | Datas[41] << 24}\r\n" +         //  4 Bytes (int)
            //                $"YResolution : {Datas[42] | Datas[43] << 8 | Datas[44] << 16 | Datas[45] << 24}\r\n" +         //  4 Bytes (int)
            //                $"NColours    : {Datas[46] | Datas[47] << 8 | Datas[48] << 16 | Datas[49] << 24}\r\n" +         //  4 Bytes (int)
            //                $"ImportantColours : {Datas[50] | Datas[51] << 8 | Datas[52] << 16 | Datas[53] << 24}\r\n");    //  4 Bytes (int)
        }

        public void SaveBmp(string FilePath)
        {
            // Bitmap File Struct
            // https://crazycat1130.pixnet.net/blog/post/1345538#mark-4

            FileStream FS = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            int RealStride = Channel == 1 ? Stride : (Width * PixelBytes + 7) >> 3 << 3;
            int ImageSize = RealStride * Height;
            int FileSize = ImageSize + 54;  // 54 Bytes for Header Struct.
            byte[] InfoDatas =
            {
                66, 77,                                                                                     // Format       , 2 Bytes
                (byte)FileSize, (byte)(FileSize >> 8), (byte)(FileSize >> 16), (byte)(FileSize >> 24),      // FileSize     , 4 Bytes
                0, 0, 0, 0,                                                                                 // Reserved     , 4 Bytes
                54, 0, 0, 0,                                                                                // Offset       , 4 Bytes (54 Bytes for Header Struct)
                40, 0, 0, 0,                                                                                // InfoSize     , 4 Bytes (40 Bytes for Info Struce)
                (byte)Width, (byte)(Width >> 8), (byte)(Width >> 16), (byte)(Width >> 24),                  // Width        , 4 Bytes
                (byte)Height, (byte)(Height >> 8), (byte)(Height >> 16), (byte)(Height >> 24),              // Height       , 4 Bytes
                1, 0,                                                                                       // Planes       , 2 Bytes (forever be set 1.)
                (byte)BitsPerPixel, (byte)(BitsPerPixel >> 8),                                              // BitsPerPixel , 2 Bytes
                0, 0, 0, 0,                                                                                 // Compression  , 4 Bytes
                (byte)ImageSize, (byte)(ImageSize >> 8), (byte)(ImageSize >> 16), (byte)(ImageSize >> 24),  // ImageSize    , 4 Bytes
                0, 0, 0, 0,                                                                                 // XResolution  , 4 Bytes (Dpi * 39.37)
                0, 0, 0, 0,                                                                                 // YResolution  , 4 Bytes
                0, 0, 0, 0,                                                                                 // NColours     , 4 Bytes
                0, 0, 0, 0,                                                                                 // ImportantColours  , 4 Bytes
            };
            FS.Write(InfoDatas, 0, InfoDatas.Length);

            switch (Channel)
            {
                case 1:
                    if (Datas[0] is null)
                    {
                        unsafe
                        {
                            byte[] ImageDatas = new byte[RealStride];
                            byte* Source = (byte*)Scan0;
                            for (int j = 0; j < Height; j++)
                            {
                                for (int i = 0; i < Stride; i++)
                                    ImageDatas[i] = *Source++;

                                FS.Write(ImageDatas, 0, ImageDatas.Length);
                            }
                        }
                    }
                    else
                    {
                        FS.Write(Datas[0], 0, ImageSize);
                    }
                    break;
                case 3:
                    unsafe
                    {
                        byte[] ImageDatas = new byte[RealStride];
                        for (int j = 0; j < Height; j++)
                        {
                            int Offset = Stride * j;
                            byte* SourceR = (byte*)(ScanR + Offset),
                                  SourceG = (byte*)(ScanG + Offset),
                                  SourceB = (byte*)(ScanB + Offset);
                            for (int i = 0; i < ImageDatas.Length - 2; i += 3)
                            {
                                ImageDatas[i] = *SourceB++;         // B
                                ImageDatas[i + 1] = *SourceG++;     // G
                                ImageDatas[i + 2] = *SourceR++;     // R
                            }

                            FS.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
                case 4:
                    unsafe
                    {
                        byte[] ImageDatas = new byte[RealStride];
                        IntPtr DataPointer;
                        fixed (byte* DataScan = &ImageDatas[0])
                            DataPointer = (IntPtr)DataScan;

                        for (int j = 0; j < Height; j++)
                        {
                            int Offset = Stride * j;
                            int* DataScan0 = (int*)DataPointer;
                            byte* SourceA = (byte*)(ScanA + Offset),
                                  SourceR = (byte*)(ScanR + Offset),
                                  SourceG = (byte*)(ScanG + Offset),
                                  SourceB = (byte*)(ScanB + Offset);
                            for (int i = 0; i < ImageDatas.Length; i += 4)
                            {
                                *DataScan0++ = *SourceA++ << 24 |  // A
                                               *SourceR++ << 16 |  // R
                                               *SourceG++ << 8 |   // G
                                               *SourceB++;         // B
                            }
                            FS.Write(ImageDatas, 0, ImageDatas.Length);
                        }
                    }
                    break;
            }


            FS.Close();
        }

        ~ImageContext()
        {
            Datas[0] = null;
            Datas[1] = null;
            Datas[2] = null;
            Datas[3] = null;
            Datas = null;
            GC.Collect();
        }
    }
}
