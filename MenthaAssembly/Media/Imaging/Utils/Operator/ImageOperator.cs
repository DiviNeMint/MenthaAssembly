using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator<T> : IImageOperator<T>
        where T : unmanaged, IPixel
    {
        public IImageContext<T> Context { get; }

        public ImageOperator(IImageContext<T> Context)
        {
            this.Context = Context;
        }

        public T GetPixel(int X, int Y)
            => Context.GetPixel<T>(X, Y);

        public void SetPixel(int X, int Y, T Pixel)
            => Context.SetPixel(X, Y, Pixel);

        public void ScanLine<U>(int X, int Y, int Length, PixelAdapterAction<U> Handler)
            where U : unmanaged, IPixel
            => Context.ScanLine(X, Y, Length, Handler);
        public void ScanLine<U>(int X, int Y, int Length, PixelAdapterFunc<U, bool> Predicate)
            where U : unmanaged, IPixel
            => Context.ScanLine(X, Y, Length, Predicate);
        public void ScanLine<U>(int X, int Y, ImageContourScanLine Range, PixelAdapterAction<U> Handler)
            where U : unmanaged, IPixel
            => Context.ScanLine(X, Y, Range, Handler);

        public void ScanLineNearestResizeTo(int X, int Y, int Length, float FracX, float Step, PixelAdapter<T> Adapter)
            => Context.ScanLineNearestResizeTo(X, Y, Length, FracX, Step, Adapter, (s, d) => d.Override(s));

        public void ScanLineBilinearResizeTo(int X, int Y, int Length, float FracX, float FracY, float Step, PixelAdapter<T> Adapter)
            => Context.ScanLineBilinearResizeTo(X, Y, Length, FracX, FracY, Step, Adapter, (Adapter, A, R, G, B) => Adapter.Override(A, R, G, B));

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, PixelAdapter<T> Adapter)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pScan0 = (byte*)Context.Scan0 + Offset;

            FracX += X;
            FracY += Y;

            long Stride = Context.Stride;
            int Wo = Context.Width - 1,
                Lo = Context.Height - 1,
                BitsPerPixel = Context.BitsPerPixel;
            for (int i = 0; i < Length; i++)
            {
                int a1 = (int)FracX,
                    b1 = (int)FracY;
                if (0 <= a1 & a1 < Wo & 0 <= b1 & b1 < Lo)
                {
                    byte* pData0 = pScan0 + Stride * b1 + ((a1 * BitsPerPixel) >> 3);

                    int a2 = (int)Math.Ceiling(FracX),
                        b3 = (int)Math.Ceiling(FracY);

                    double xa13 = FracX - a1,
                           xa24 = a2 - FracX,
                           yb12 = FracY - b1,
                           yb34 = b3 - FracY;
                    T* pData = (T*)pData0;
                    if (xa13 != 0 & xa24 != 0 & yb12 != 0 & yb34 != 0)
                    {
                        T p1 = *pData,
                          p2, p3, p4;

                        if (a2 > a1)
                        {
                            p2 = *++pData;
                            if (b3 > b1)
                            {
                                pData = (T*)(pData0 + Stride);
                                p3 = *pData++;
                                p4 = *pData;
                            }
                            else
                            {
                                p3 = p1;
                                p4 = p2;
                            }
                        }
                        else
                        {
                            p2 = p1;
                            if (b3 > b1)
                            {
                                pData = (T*)(pData0 + Stride);
                                p3 = *pData;
                                p4 = p3;
                            }
                            else
                            {
                                p3 = p1;
                                p4 = p2;
                            }
                        }

                        byte A = (byte)((p1.A * xa24 + p2.A * xa13) * yb34 + (p3.A * xa24 + p4.A * xa13) * yb12),
                             R = (byte)((p1.R * xa24 + p2.R * xa13) * yb34 + (p3.R * xa24 + p4.R * xa13) * yb12),
                             G = (byte)((p1.G * xa24 + p2.G * xa13) * yb34 + (p3.G * xa24 + p4.G * xa13) * yb12),
                             B = (byte)((p1.B * xa24 + p2.B * xa13) * yb34 + (p3.B * xa24 + p4.B * xa13) * yb12);

                        Adapter.Override(A, R, G, B);
                    }
                    else
                    {
                        Adapter.Override(*pData);
                    }
                }

                Adapter.MoveNext();

                FracX += Cos;
                FracY -= Sin;
            }
        }

        public void ContourOverlay(IImageContour Contour, T Color, double OffsetX, double OffsetY)
            => Context.Contour<T>(Contour, OffsetX, OffsetY, a => a.Overlay(Color));

        public void BlockOverlay(int X, int Y, IImageContext Source, int SourceX, int SourceY, int Width, int Height)
            => Context.Block(X, Y, Source, SourceX, SourceY, Width, Height, (s, d) => d.Overlay(s));

        public ImageContour FindBound(int SeedX, int SeedY, ImagePredicate Predicate)
            => Context.FindBound(SeedX, SeedY, Predicate);

        public PixelAdapter<U> GetAdapter<U>(int X, int Y)
            where U : unmanaged, IPixel
            => Context.PixelType == typeof(U) ? new PixelAdapter1<U>(Context, X, Y) :
                                                new PixelAdapter1<T, U>(Context, X, Y);

    }
}
