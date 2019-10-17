using MenthaAssembly.Interfaces;
using System;

namespace MenthaAssembly
{
    public partial class ImageContext : IImageContext
    {
        public int Width { get; }

        public int Height { get; }

        public IntPtr Scan0 { get; }

        public IntPtr ScanA { get; }

        public IntPtr ScanR { get; }

        public IntPtr ScanG { get; }

        public IntPtr ScanB { get; }

        public int Stride { get; }

        public int PixelBytes { get; }

        public int Channel { get; }

        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, int PixelBytes)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 1;
            this.Scan0 = Scan0;
            this.Stride = Stride;
            this.PixelBytes = PixelBytes;
        }

        protected byte[] Datas { set; get; } = new byte[0];
        public ImageContext(int Width, int Height, byte[] Datas, int Stride, int PixelBytes)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 1;
            this.PixelBytes = PixelBytes;
            this.Stride = Stride;

            this.Datas = Datas;
            unsafe
            {
                fixed (byte* Buffer = &Datas[0])
                    Scan0 = (IntPtr)Buffer;
            }
        }

        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 3;
            this.ScanR = ScanR;
            this.ScanG = ScanG;
            this.ScanB = ScanB;
            this.Stride = Stride;
            this.PixelBytes = 3;
        }

        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Channel = 4;
            this.ScanA = ScanA;
            this.ScanR = ScanR;
            this.ScanG = ScanG;
            this.ScanB = ScanB;
            this.Stride = Stride;
            this.PixelBytes = 4;
        }

    }
}
