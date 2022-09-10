using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, byte A, byte R, byte G, byte B);

    public delegate void PixelAdapterAction<T>(PixelAdapter<T> Source) where T : unmanaged, IPixel;
    public delegate U PixelAdapterFunc<T, U>(PixelAdapter<T> Source) where T : unmanaged, IPixel;
    public delegate void PixelAdapterAction2<T>(PixelAdapter<T> Source, PixelAdapter<T> Destination) where T : unmanaged, IPixel;
    public delegate void PixelAdapterAction3<T>(PixelAdapter<T> Destination, byte A, byte R, byte G, byte B) where T : unmanaged, IPixel;

    internal unsafe interface IImageOperator
    {
        public PixelAdapter<T> GetAdapter<T>(int X, int Y) where T : unmanaged, IPixel;

        public void ScanLine<U>(int X, int Y, int Length, PixelAdapterAction<U> Handler) where U : unmanaged, IPixel;
        public void ScanLine<U>(int X, int Y, int Length, PixelAdapterFunc<U, bool> Predicate) where U : unmanaged, IPixel;
        public void ScanLine<U>(int X, int Y, ImageContourScanLine Range, PixelAdapterAction<U> Handler) where U : unmanaged, IPixel;

        public void BlockOverlay(int X, int Y, IImageContext Source, int SourceX, int SourceY, int Width, int Height);

        public ImageContour FindBound(int SeedX, int SeedY, ImagePredicate Predicate);

    }

    internal unsafe interface IImageOperator<T> : IImageOperator
        where T : unmanaged, IPixel
    {
        public T GetPixel(int X, int Y);

        public void SetPixel(int X, int Y, T Pixel);

        public void ScanLineNearestResizeTo(int X, int Y, int Length, float FracX, float Step, PixelAdapter<T> Adapter);

        public void ScanLineBilinearResizeTo(int X, int Y, int Length, float FracX, float FracY, float Step, PixelAdapter<T> Adapter);

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, PixelAdapter<T> Adapter);

        public void ScanLineFilterTo(int X, int Y, int Length, ImageFilter Filter, PixelAdapter<T> Adapter);

        public void ContourOverlay(IImageContour Contour, T Color, double OffsetX, double OffsetY);

    }

}