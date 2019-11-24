using MenthaAssembly.Interfaces;
using System;

namespace MenthaAssembly
{
    public class ImageContext : IImageContext
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

        public int PixelBytes { get; }

        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, int PixelBytes)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.PixelBytes = PixelBytes;
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
            this.Channel = 4;
        }

        protected byte[][] Datas { set; get; } = new byte[4][];
        public ImageContext(int Width, int Height, byte[] Datas, int Stride, int PixelBytes)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 1;
            this.PixelBytes = PixelBytes;
            this.Stride = Stride;

            this.Datas[0] = Datas;
            unsafe
            {
                fixed (byte* Buffer = &this.Datas[0][0])
                    Scan0 = (IntPtr)Buffer;
            }
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) : this(Width, Height, DataR, DataG, DataB, Width)
        {
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 3;
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
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) : this(Width, Height, DataA, DataR, DataG, DataB, Width)
        {
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 4;
            this.PixelBytes = 4;
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
                    ScanA = (IntPtr)R;
                    ScanR = (IntPtr)R;
                    ScanG = (IntPtr)G;
                    ScanB = (IntPtr)B;
                }
            }
        }

    }
}
