namespace MenthaAssembly.Media.Imaging.Utils
{
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

        public unsafe void ScanLineCopy(IImageContext Source, int X, int Y, int Length, byte* pDest);
        public unsafe void ScanLineCopy<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel;
        public unsafe void ScanLineCopy3(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
        public unsafe void ScanLineCopy4(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

        public void ScanLineOverlay(IImageContext Destination, int X, int Y, int Length, Pixel Color);
        public unsafe void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel;
        public unsafe void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel;
        public unsafe void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel;

        public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color);

        public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height);

    }
}