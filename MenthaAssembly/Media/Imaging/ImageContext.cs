using System;
using System.Collections.Generic;

namespace MenthaAssembly
{
    public class ImageContext
    {
        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public int Channels { get; }

        public IntPtr Scan0 { get; }

        public IntPtr ScanA { get; }

        public IntPtr ScanR { get; }

        public IntPtr ScanG { get; }

        public IntPtr ScanB { get; }

        public int BitsPerPixel { get; }

        public IList<int> Palette { get; }

        public ImageContext(int Width, int Height, IntPtr Scan0, IList<int> Palette = null) : this(Width, Height, Scan0, Width, Palette)
        {
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.BitsPerPixel = (Stride << 3) / Width;
            this.Palette = Palette;
            this.Scan0 = Scan0;
            this.Channels = 1;
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, int BitsPerPixel, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.BitsPerPixel = BitsPerPixel;
            this.Palette = Palette;
            this.Scan0 = Scan0;
            this.Channels = 1;
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
            this.BitsPerPixel = 24;
            this.Channels = 3;
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
            this.BitsPerPixel = 32;
            this.Channels = 4;
        }

        internal protected byte[][] Datas { set; get; } = new byte[4][];
        public ImageContext(int Width, int Height, byte[] Data, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Data.Length / Height;
            this.BitsPerPixel = (this.Stride << 3) / Width;
            this.Palette = Palette;
            this.Channels = 1;

            this.Datas[0] = Data;
            unsafe
            {
                fixed (byte* Buffer = &this.Datas[0][0])
                    Scan0 = (IntPtr)Buffer;
            }
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataR.Length / Height;
            this.BitsPerPixel = 24;
            this.Channels = 3;

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
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataA.Length / Height;
            this.Channels = 4;
            this.BitsPerPixel = 32;

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
