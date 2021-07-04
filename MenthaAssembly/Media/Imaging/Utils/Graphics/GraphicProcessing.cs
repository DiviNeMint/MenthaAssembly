using System;
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
        public abstract ImageContext<T> Cast<T>() where T : unmanaged, IPixel;

        public abstract ImageContext<T, U> Cast<T, U>() where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;

        public abstract ImageContext<T> ParallelCast<T>() where T : unmanaged, IPixel;

        public abstract ImageContext<T> ParallelCast<T>(ParallelOptions Options) where T : unmanaged, IPixel;

        public abstract ImageContext<T, U> ParallelCast<T, U>() where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;

        public abstract ImageContext<T, U> ParallelCast<T, U>(ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;

        #endregion

        #region Clear
        public void Clear(Pixel Color)
        {
            for (int j = 0; j < Height; j++)
                Operator.ScanLineOverride(this, 0, j, Width, Color);
        }
        void IImageContext.Clear(IPixel Color)
            => this.Clear(Operator.ToPixel(Color.A, Color.R, Color.G, Color.B));

        public void ParallelClear(Pixel Color)
            => Parallel.For(0, Height, j => Operator.ScanLineOverride(this, 0, j, Width, Color));
        void IImageContext.ParallelClear(IPixel Color)
            => this.ParallelClear(Operator.ToPixel(Color.A, Color.R, Color.G, Color.B));

        #endregion

        protected abstract IImageContext CloneHandler();
        object ICloneable.Clone()
            => CloneHandler();

    }
}
