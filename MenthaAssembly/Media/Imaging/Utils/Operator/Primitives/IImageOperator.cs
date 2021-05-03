namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, byte A, byte R, byte G, byte B);

    internal interface IImageOperator
    {
    }
    internal interface IImageOperator<Pixel> : IImageOperator
        where Pixel : unmanaged, IPixel
    {
        public PixelOperator<Pixel> PixelOperator { get; }

        public Pixel GetPixel(IImageContext Source, int X, int Y);

        public void SetPixel(IImageContext Source, int X, int Y, Pixel Pixel);

        public Pixel ToPixel(byte A, byte R, byte G, byte B);

        public void ScanLineOverride(IImageContext Destination, int X, int Y, int Length, Pixel Color);
        public unsafe void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDest);
        public unsafe void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel;
        public unsafe void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
        public unsafe void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ScanLineOverlay(IImageContext Destination, int X, int Y, int Length, Pixel Color);
        public unsafe void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel;
        public unsafe void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel;
        public unsafe void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel;

        public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color, int OffsetX, int OffsetY);

        public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height);

        public ImageContour FindBound(IImageContext Source, int SeedX, int SeedY, ImagePredicate Predicate);

    }
}