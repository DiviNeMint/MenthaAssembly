namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, byte A, byte R, byte G, byte B);

    internal unsafe interface IImageOperator
    {
        public IImageContext Context { get; }

        public IPixel GetPixel(int X, int Y);
        public void SetPixel(int X, int Y, IPixel Pixel);

        public void ScanLineOverride(int X, int Y, int Length, IPixel Color);
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDest);
        public void ScanLineOverrideTo<T>(int X, int Y, int Length, T* pDest) where T : unmanaged, IPixel;
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDest);
        public void ScanLineReverseOverrideTo<T>(int X, int Y, int Length, T* pDest) where T : unmanaged, IPixel;
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ScanLineOverlay(int X, int Y, int Length, IPixel Color);
        public void ScanLineOverlayTo<T>(int X, int Y, int Length, T* pDest) where T : unmanaged, IPixel;
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDest);
        public void ScanLineNearestResizeTo<T>(float FracX, float Step, int X, int Y, int Length, T* pDest) where T : unmanaged, IPixel;
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineNearestResizeTo(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref byte* pDest);
        public void ScanLineNearestResizeTo<T>(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref T* pDest) where T : unmanaged, IPixel;

        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDest);
        public void ScanLineBilinearResizeTo<T>(float FracX, float FracY, float Step, int X, int Y, int Length, T* pDest) where T : unmanaged, IPixel;
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, byte* pDest);
        public void ScanLineRotateTo<T>(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, T* pDest) where T : unmanaged, IPixel;
        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ScanLineConvolute(int X, int Y, int Length, ConvoluteKernel Kernel, byte* pDest);
        public void ScanLineConvolute<T>(int X, int Y, int Length, ConvoluteKernel Kernel, T* pDest) where T : unmanaged, IPixel;
        public void ScanLineConvolute(int X, int Y, int Length, ConvoluteKernel Kernel, byte* pDestR, byte* pDestG, byte* pDestB);
        public void ScanLineConvolute(int X, int Y, int Length, ConvoluteKernel Kernel, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ContourOverlay(ImageContour Contour, IPixel Color, int OffsetX, int OffsetY);

        public void BlockOverlay(int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height);

        public ImageContour FindBound(int SeedX, int SeedY, ImagePredicate Predicate);

    }

    internal unsafe interface IImageOperator<T> : IImageOperator
        where T : unmanaged, IPixel
    {
        public new IImageContext<T> Context { get; }

        public new T GetPixel(int X, int Y);
        public void SetPixel(int X, int Y, T Pixel);

        public void ScanLineOverride(int X, int Y, int Length, T Color);
        public void ScanLineOverlay(int X, int Y, int Length, T Color);

        public void ContourOverlay(ImageContour Contour, T Color, int OffsetX, int OffsetY);

    }

    internal unsafe interface IImageOperator<T, Struct> : IImageOperator
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public new IImageContext<T, Struct> Context { get; }

        public new T GetPixel(int X, int Y);
        public void SetPixel(int X, int Y, T Pixel);

        public void ScanLineOverride(int X, int Y, int Length, T Color);
        public void ScanLineOverlay(int X, int Y, int Length, T Color);

        public void ContourOverlay(ImageContour Contour, T Color, int OffsetX, int OffsetY);

    }
}