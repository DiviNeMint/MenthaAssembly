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

        internal protected IntPtr? _Scan0;
        public IntPtr Scan0
        {
            get
            {
                if (_Scan0 is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* S0 = &this.Datas[0][0])
                        return (IntPtr)S0;
                }
            }
        }

        internal protected IntPtr? _ScanA;
        public IntPtr ScanA 
        {
            get 
            {
                if (_ScanA is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* A = &this.Datas[1][0])
                        return (IntPtr)A;
                }
            } 
        }

        internal protected IntPtr? _ScanR;
        public IntPtr ScanR
        {
            get
            {
                if (_ScanR is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* R = &this.Datas[2][0])
                        return (IntPtr)R;
                }
            }
        }

        internal protected IntPtr? _ScanG;
        public IntPtr ScanG
        {
            get
            {
                if (_ScanG is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* G = &this.Datas[3][0])
                        return (IntPtr)G;
                }
            }
        }

        internal protected IntPtr? _ScanB;
        public IntPtr ScanB
        {
            get
            {
                if (_ScanB is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* B = &this.Datas[4][0])
                        return (IntPtr)B;
                }
            }
        }

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
            this._Scan0 = Scan0;
            this.Channels = 1;
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, int BitsPerPixel, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.BitsPerPixel = BitsPerPixel;
            this.Palette = Palette;
            this._Scan0 = Scan0;
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
            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;
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
            this._ScanA = ScanA;
            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;
            this.BitsPerPixel = 32;
            this.Channels = 4;
        }

        /// <summary>
        /// Index :
        /// Scan0 = 0, 
        /// ScanA = 1, 
        /// ScanR = 2, 
        /// ScanG = 3, 
        /// ScanB = 4
        /// </summary>
        internal protected byte[][] Datas { set; get; } = new byte[5][];
        public ImageContext(int Width, int Height, byte[] Data, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Data.Length / Height;
            this.BitsPerPixel = (this.Stride << 3) / Width;
            this.Palette = Palette;
            this.Channels = 1;

            this.Datas[0] = Data;
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataR.Length / Height;
            this.BitsPerPixel = 24;
            this.Channels = 3;

            this.Datas[2] = DataR;
            this.Datas[3] = DataG;
            this.Datas[4] = DataB;
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataA.Length / Height;
            this.Channels = 4;
            this.BitsPerPixel = 32;

            this.Datas[1] = DataA;
            this.Datas[2] = DataR;
            this.Datas[3] = DataG;
            this.Datas[4] = DataB;
        }

    }
}
