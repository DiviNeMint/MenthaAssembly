using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public unsafe abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        internal IImageOperator<Pixel> Operator { get; }

        public int Width { get; }

        public int Height { get; }

        public long Stride { get; }

        public int BitsPerPixel { get; }

        public int Channels { get; }

        protected Type PixelType = typeof(Pixel);
        Type IImageContext.PixelType => this.PixelType;

        protected Type StructType = typeof(Struct);
        Type IImageContext.StructType => this.StructType;

        public Pixel this[int X, int Y]
        {
            get
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                return this.Operator.GetPixel(this, X, Y);
            }
            set
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                this.Operator.SetPixel(this, X, Y, value);
            }
        }
        IPixel IImageContext.this[int X, int Y]
        {
            get => this[X, Y];
            set => this[X, Y] = this.Operator.ToPixel(value.A, value.R, value.G, value.B);
        }

        private readonly byte[] Data0;
        private readonly IntPtr _Scan0;
        private readonly Func<IntPtr> GetScan0;
        public IntPtr Scan0 => GetScan0();

        private readonly byte[] DataA;
        private readonly IntPtr _ScanA;
        private readonly Func<IntPtr> GetScanA;
        protected IntPtr ScanA => GetScanA();
        IntPtr IImageContext.ScanA => this.ScanA;

        private readonly byte[] DataR;
        private readonly IntPtr _ScanR;
        private readonly Func<IntPtr> GetScanR;
        protected IntPtr ScanR => GetScanR();
        IntPtr IImageContext.ScanR => this.ScanR;

        private readonly byte[] DataG;
        private readonly IntPtr _ScanG;
        private readonly Func<IntPtr> GetScanG;
        protected IntPtr ScanG => GetScanG();
        IntPtr IImageContext.ScanG => this.ScanG;

        private readonly byte[] DataB;
        private readonly IntPtr _ScanB;
        private readonly Func<IntPtr> GetScanB;
        protected IntPtr ScanB => GetScanB();
        IntPtr IImageContext.ScanB => this.ScanB;

        public IList<Pixel> Palette { get; }

        IList<IPixel> IImageContext.Palette
            => this.Palette.Cast<IPixel>().ToList();

        internal ImageContextBase()
        {
            this.BitsPerPixel = default(Struct).BitsPerPixel;
        }

        private readonly HGlobalIntPtr UnmanagedScan0;
        internal ImageContextBase(int Width, int Height) : this()
        {
            this.Width = Width;
            this.Height = Height;

            this.Stride = Width * sizeof(Struct);
            this.Channels = 1;

            this.Palette = new List<Pixel>();

            long Size = this.Stride * Height;
            if (Size > int.MaxValue)
            {
                UnmanagedScan0 = new HGlobalIntPtr(Size);
                this._Scan0 = UnmanagedScan0.DangerousGetHandle();
                GetScan0 = () => this._Scan0;
            }
            else
            {
                this.Data0 = new byte[this.Stride * Height];
                GetScan0 = () =>
                {
                    fixed (byte* pScan0 = &this.Data0[0])
                        return (IntPtr)pScan0;
                };
            }

            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => throw new NotImplementedException();
            GetScanG = () => throw new NotImplementedException();
            GetScanB = () => throw new NotImplementedException();

            this.Operator = default(Struct) is IPixelIndexed ? ImageIndexedOperator<Pixel, Struct>.GetOperator() :
                                                               ImageOperator<Pixel>.GetOperator();
        }

        internal ImageContextBase(int Width, int Height, IntPtr Scan0, int Stride, IList<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 1;

            this.Palette = Palette ?? new List<Pixel>();

            this._Scan0 = Scan0;
            GetScan0 = () => this._Scan0;
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => throw new NotImplementedException();
            GetScanG = () => throw new NotImplementedException();
            GetScanB = () => throw new NotImplementedException();

            this.Operator = default(Struct) is IPixelIndexed ? ImageIndexedOperator<Pixel, Struct>.GetOperator() :
                                                               ImageOperator<Pixel>.GetOperator();
        }
        internal ImageContextBase(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 3;

            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => this._ScanR;
            GetScanG = () => this._ScanG;
            GetScanB = () => this._ScanB;

            this.Operator = ImageOperator3<Pixel>.GetOperator();
        }
        internal ImageContextBase(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 4;

            this._ScanA = ScanA;
            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () => this._ScanA;
            GetScanR = () => this._ScanR;
            GetScanG = () => this._ScanG;
            GetScanB = () => this._ScanB;

            this.Operator = ImageOperator4<Pixel>.GetOperator();
        }

        internal unsafe ImageContextBase(int Width, int Height, byte[] Data, IList<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;

            this.Stride = Data.Length / Height;
            this.Channels = 1;

            this.Palette = Palette ?? new List<Pixel>();

            this.Data0 = Data;
            GetScan0 = () =>
            {
                fixed (byte* pScan0 = &this.Data0[0])
                    return (IntPtr)pScan0;
            };
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => throw new NotImplementedException();
            GetScanG = () => throw new NotImplementedException();
            GetScanB = () => throw new NotImplementedException();

            this.Operator = default(Struct) is IPixelIndexed ? ImageIndexedOperator<Pixel, Struct>.GetOperator() :
                                                               ImageOperator<Pixel>.GetOperator();
        }
        internal unsafe ImageContextBase(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataR.Length / Height;
            this.Channels = 3;

            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () =>
            {
                fixed (byte* pScanR = &this.DataR[0])
                    return (IntPtr)pScanR;
            };
            GetScanG = () =>
            {
                fixed (byte* pScanG = &this.DataG[0])
                    return (IntPtr)pScanG;
            };
            GetScanB = () =>
            {
                fixed (byte* pScanB = &this.DataB[0])
                    return (IntPtr)pScanB;
            };

            this.Operator = ImageOperator3<Pixel>.GetOperator();
        }
        internal unsafe ImageContextBase(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataA.Length / Height;
            this.Channels = 4;

            this.DataA = DataA;
            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () =>
            {
                fixed (byte* pScanA = &this.DataA[0])
                    return (IntPtr)pScanA;
            };
            GetScanR = () =>
            {
                fixed (byte* pScanR = &this.DataR[0])
                    return (IntPtr)pScanR;
            };
            GetScanG = () =>
            {
                fixed (byte* pScanG = &this.DataG[0])
                    return (IntPtr)pScanG;
            };
            GetScanB = () =>
            {
                fixed (byte* pScanB = &this.DataB[0])
                    return (IntPtr)pScanB;
            };

            this.Operator = ImageOperator4<Pixel>.GetOperator();
        }

        protected abstract IImageContext FlipHandler(FlipMode Mode);
        IImageContext IImageContext.Flip(FlipMode Mode)
            => FlipHandler(Mode);

        protected abstract IImageContext CropHandler(int X, int Y, int Width, int Height);
        IImageContext IImageContext.Crop(int X, int Y, int Width, int Height)
            => CropHandler(X, Y, Width, Height);

        protected abstract IImageContext ConvoluteHandler(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum);
        IImageContext IImageContext.Convolute(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
            => ConvoluteHandler(Kernel, KernelFactorSum, KernelOffsetSum);
        IImageContext IImageContext.Convolute(ConvoluteKernel Kernel)
            => ConvoluteHandler(Kernel.Datas, Kernel.FactorSum, Kernel.Offset);

        protected abstract IImageContext CastHandler<T>()
            where T : unmanaged, IPixel;
        protected abstract IImageContext CastHandler<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed;
        IImageContext IImageContext.Cast<T>()
            => CastHandler<T>();
        IImageContext IImageContext.Cast<T, U>()
            => CastHandler<T, U>();

        protected abstract IImageContext CloneHandler();
        object ICloneable.Clone()
            => CloneHandler();

        public IntPtr CreateHBitmap()
            => Win32.Graphic.CreateBitmap(Width, Height, 1, BitsPerPixel, Scan0);

    }

}