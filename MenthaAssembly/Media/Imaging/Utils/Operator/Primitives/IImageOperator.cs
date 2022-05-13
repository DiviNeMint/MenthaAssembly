using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, byte A, byte R, byte G, byte B);

    internal unsafe interface IImageOperator
    {
        public IPixelAdapter<T> GetAdapter<T>(int X, int Y) where T : unmanaged, IPixel;

        public void ScanLine<U>(int X, int Y, int Length, Action<IPixelAdapter<U>> Handler) where U : unmanaged, IPixel;
        public void ScanLine<U>(int X, int Y, int Length, Predicate<IPixelAdapter<U>> Predicate) where U : unmanaged, IPixel;

        public void BlockOverlay(int X, int Y, IImageContext Source, int SourceX, int SourceY, int Width, int Height);

        public ImageContour FindBound(int SeedX, int SeedY, ImagePredicate Predicate);

    }

    internal unsafe interface IImageOperator<T> : IImageOperator
        where T : unmanaged, IPixel
    {
        public T GetPixel(int X, int Y);

        public void SetPixel(int X, int Y, T Pixel);

        public void ScanLineNearestResizeTo(int X, int Y, int Length, float FracX, float Step, IPixelAdapter<T> Adapter);

        public void ScanLineBilinearResizeTo(int X, int Y, int Length, float FracX, float FracY, float Step, IPixelAdapter<T> Adapter);

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, IPixelAdapter<T> Adapter);

        public void ScanLineFilterTo(int X, int Y, int Length, ImageFilter Filter, IPixelAdapter<T> Adapter);

        public void ContourOverlay(ImageContour Contour, T Color, int OffsetX, int OffsetY);

    }

}