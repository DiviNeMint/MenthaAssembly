using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        protected abstract IImageContext FlipHandler(FlipMode Mode);
        IImageContext IImageContext.Flip(FlipMode Mode)
            => FlipHandler(Mode);

        #region Crop
        protected abstract IImageContext CropHandler(int X, int Y, int Width, int Height);
        IImageContext IImageContext.Crop(int X, int Y, int Width, int Height)
            => CropHandler(X, Y, Width, Height);

        protected abstract IImageContext ParallelCropHandler(int X, int Y, int Width, int Height);
        IImageContext IImageContext.ParallelCrop(int X, int Y, int Width, int Height)
            => ParallelCropHandler(X, Y, Width, Height);

        protected abstract IImageContext ParallelCropHandler(int X, int Y, int Width, int Height, ParallelOptions Options);
        IImageContext IImageContext.ParallelCrop(int X, int Y, int Width, int Height, ParallelOptions Options)
            => ParallelCropHandler(X, Y, Width, Height, Options);

        #endregion

        protected abstract IImageContext ConvoluteHandler(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum);
        IImageContext IImageContext.Convolute(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
            => ConvoluteHandler(Kernel, KernelFactorSum, KernelOffsetSum);
        IImageContext IImageContext.Convolute(ConvoluteKernel Kernel)
            => ConvoluteHandler(Kernel.Datas, Kernel.FactorSum, Kernel.Offset);

        #region Cast
        protected abstract IImageContext CastHandler<T>() where T : unmanaged, IPixel;
        IImageContext IImageContext.Cast<T>()
            => CastHandler<T>();

        protected abstract IImageContext CastHandler<T, U>() where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;
        IImageContext IImageContext.Cast<T, U>()
            => CastHandler<T, U>();

        protected abstract IImageContext ParallelCastHandler<T>() where T : unmanaged, IPixel;
        IImageContext IImageContext.ParallelCast<T>()
            => ParallelCastHandler<T>();

        protected abstract IImageContext ParallelCastHandler<T>(ParallelOptions Options) where T : unmanaged, IPixel;
        IImageContext IImageContext.ParallelCast<T>(ParallelOptions Options)
            => ParallelCastHandler<T>(Options);

        protected abstract IImageContext ParallelCastHandler<T, U>() where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;
        IImageContext IImageContext.ParallelCast<T, U>()
            => ParallelCastHandler<T, U>();

        protected abstract IImageContext ParallelCastHandler<T, U>(ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;
        IImageContext IImageContext.ParallelCast<T, U>(ParallelOptions Options)
            => ParallelCastHandler<T, U>(Options);

        #endregion

        #region Clear
        public void Clear(Pixel Color)
        {
            for (int j = 0; j < Height; j++)
                Operator.ScanLineOverlay(this, 0, j, Width, Color);
        }
        void IImageContext.Clear(IPixel Color)
            => this.Clear(Operator.ToPixel(Color.A, Color.R, Color.G, Color.B));

        public void ParallelClear(Pixel Color)
            => Parallel.For(0, Height, j => Operator.ScanLineOverlay(this, 0, j, Width, Color));
        void IImageContext.ParallelClear(IPixel Color)
            => this.ParallelClear(Operator.ToPixel(Color.A, Color.R, Color.G, Color.B));

        #endregion

        protected abstract IImageContext CloneHandler();
        object ICloneable.Clone()
            => CloneHandler();

        public IntPtr CreateHBitmap()
            => Win32.Graphic.CreateBitmap(Width, Height, 1, BitsPerPixel, Scan0);

    }
}
