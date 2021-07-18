namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, byte A, byte R, byte G, byte B);

    internal unsafe abstract class IImageOperator
    {
        public abstract T GetPixel<T>(IImageContext Source, int X, int Y) where T : unmanaged, IPixel;
        public abstract void SetPixel<T>(IImageContext Source, int X, int Y, T Pixel) where T : unmanaged, IPixel;

        public static T ToPixel<T>(IPixel Color)
            where T : unmanaged, IPixel
            => ToPixel<T>(Color.A, Color.R, Color.G, Color.B);
        public static T ToPixel<T>(byte A, byte R, byte G, byte B)
            where T : unmanaged, IPixel
        {
            T Pixel = default;
            Pixel.Override(A, R, G, B);
            return Pixel;
        }

        public abstract void ScanLineOverride<T>(IImageContext Destination, int X, int Y, int Length, T Color) where T : unmanaged, IPixel;
        public virtual void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest) where T : unmanaged, IPixel
            => ScanLineOverrideTo<T, T>(Source, X, Y, Length, (T*)pDest);
        public abstract void ScanLineOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest) where T : unmanaged, IPixel where T2 : unmanaged, IPixel;
        public abstract void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;
        public abstract void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;

        public virtual void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest) where T : unmanaged, IPixel
            => ScanLineReverseOverrideTo<T, T>(Source, X, Y, Length, (T*)pDest);
        public abstract void ScanLineReverseOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest) where T : unmanaged, IPixel where T2 : unmanaged, IPixel;
        public abstract void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;
        public abstract void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;

        public abstract void ScanLineOverlay<T>(IImageContext Destination, int X, int Y, int Length, T Color) where T : unmanaged, IPixel;
        public abstract void ScanLineOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest) where T : unmanaged, IPixel where T2 : unmanaged, IPixel;
        public abstract void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;
        public abstract void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;

        //public abstract void ScanLineReverseOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest) where T : unmanaged, IPixel where T2 : unmanaged, IPixel;
        //public abstract void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;
        //public abstract void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;

        public virtual void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDest) where T : unmanaged, IPixel
            => ScanLineNearestResizeTo<T, T>(Source, Step, Max, X, Y, Length, (T*)pDest);
        public abstract void ScanLineNearestResizeTo<T, T2>(IImageContext Source, int Step, int Max, int X, int Y, int Length, T2* pDest) where T : unmanaged, IPixel where T2 : unmanaged, IPixel;
        public abstract void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;
        public abstract void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;

        public virtual void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDest) where T : unmanaged, IPixel
            => ScanLineBilinearResizeTo<T, T>(Source, StepX, FracY, Max, X, Y, Length, (T*)pDest);
        public abstract void ScanLineBilinearResizeTo<T, T2>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, T2* pDest) where T : unmanaged, IPixel where T2 : unmanaged, IPixel;
        public abstract void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;
        public abstract void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB) where T : unmanaged, IPixel;

        protected void Overlay(ref byte* pDestR, ref byte* pDestG, ref byte* pDestB, byte A, byte R, byte G, byte B)
        {
            if (A == 0)
                return;

            if (A == byte.MaxValue)
            {
                *pDestR = R;
                *pDestG = G;
                *pDestB = B;
                return;
            }

            int rA = 255 - A;

            *pDestR = (byte)((R * A + *pDestR * rA) / 255);
            *pDestG = (byte)((G * A + *pDestG * rA) / 255);
            *pDestB = (byte)((B * A + *pDestB * rA) / 255);
        }
        protected void Overlay(ref byte* pDestA, ref byte* pDestR, ref byte* pDestG, ref byte* pDestB, byte A, byte R, byte G, byte B)
        {
            if (A == 0)
                return;

            if (A == byte.MaxValue)
            {
                *pDestA = A;
                *pDestR = R;
                *pDestG = G;
                *pDestB = B;
                return;
            }

            int A1 = *pDestA,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            *pDestA = (byte)(Alpha / 255);
            *pDestR = (byte)((R * A * 255 + *pDestR * A1 * rA) / Alpha);
            *pDestG = (byte)((G * A * 255 + *pDestG * A1 * rA) / Alpha);
            *pDestB = (byte)((B * A * 255 + *pDestB * A1 * rA) / Alpha);
        }

        public abstract void ContourOverlay<T>(IImageContext Destination, ImageContour Contour, T Color, int OffsetX, int OffsetY) where T : unmanaged, IPixel;

        public abstract void BlockOverlay<T>(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height) where T : unmanaged, IPixel;

        public abstract ImageContour FindBound<T>(IImageContext Source, int SeedX, int SeedY, ImagePredicate Predicate) where T : unmanaged, IPixel;

    }


    //internal interface IImageOperator
    //{
    //}
    //internal interface IImageOperator<Pixel> : IImageOperator
    //    where Pixel : unmanaged, IPixel
    //{
    //    public PixelOperator<Pixel> PixelOperator { get; }

    //    public Pixel GetPixel(IImageContext Source, int X, int Y);

    //    public void SetPixel(IImageContext Source, int X, int Y, Pixel Pixel);

    //    public Pixel ToPixel(byte A, byte R, byte G, byte B);

    //    public void ScanLineOverride(IImageContext Destination, int X, int Y, int Length, Pixel Color);
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDest);
    //    public void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel;
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB);
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB);

    //    public void ScanLineOverlay(IImageContext Destination, int X, int Y, int Length, Pixel Color);
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel;
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel;
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel;

    //    public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color, int OffsetX, int OffsetY);

    //    public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height);

    //    public ImageContour FindBound(IImageContext Source, int SeedX, int SeedY, ImagePredicate Predicate);

    //}
}