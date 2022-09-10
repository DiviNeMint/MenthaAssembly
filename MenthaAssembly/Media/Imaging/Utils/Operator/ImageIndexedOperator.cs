using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageIndexedOperator<T, Struct> : IImageOperator<T>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public IImageContext<T, Struct> Context { get; }

        public ImageIndexedOperator(IImageContext<T, Struct> Context)
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

        public void ScanLineNearestResizeTo(int X, int Y, int Length, float FracX, float Step, IPixelAdapter<T> Adapter)
            => Context.ScanLineNearestResizeTo(X, Y, Length, FracX, Step, Adapter, (s, d) => d.Override(s));

        public void ScanLineBilinearResizeTo(int X, int Y, int Length, float FracX, float FracY, float Step, IPixelAdapter<T> Adapter)
            => Context.ScanLineBilinearResizeTo(X, Y, Length, FracX, FracY, Step, Adapter, (Adapter, A, R, G, B) => Adapter.Override(A, R, G, B));

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, IPixelAdapter<T> Adapter)
        {
            byte* pScan0 = (byte*)Context.Scan0;

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
                    int XBits = a1 * BitsPerPixel;
                    byte* pData0 = pScan0 + Stride * b1 + (XBits >> 3);

                    Struct* pStruct = (Struct*)pData0;
                    Struct Indexed = *pStruct;
                    int IndexLength = pStruct->Length;
                    XBits %= IndexLength;

                    int a2 = (int)Math.Ceiling(FracX),
                        b3 = (int)Math.Ceiling(FracY);

                    double xa13 = FracX - a1,
                           xa24 = a2 - FracX,
                           yb12 = FracY - b1,
                           yb34 = b3 - FracY;
                    T p1 = Context.Palette[Indexed[XBits]];
                    if (xa13 != 0 & xa24 != 0 & yb12 != 0 & yb34 != 0)
                    {
                        T p2, p3, p4;

                        if (a2 > a1)
                        {
                            int Temp = XBits + 1;
                            if (Temp >= IndexLength)
                            {
                                Temp = 0;
                                Indexed = *++pStruct;
                            }

                            p2 = Context.Palette[Indexed[Temp]];
                            if (b3 > b1)
                            {
                                pStruct = (Struct*)(pData0 + Stride);
                                Indexed = *pStruct;
                                p3 = Context.Palette[Indexed[XBits++]];

                                if (XBits >= IndexLength)
                                {
                                    XBits = 0;
                                    Indexed = *++pStruct;
                                }

                                p4 = Context.Palette[Indexed[XBits]];
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
                                Indexed = *(Struct*)(pData0 + Context.Stride);
                                p3 = Context.Palette[Indexed[XBits]];
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
                        Adapter.Override(p1);
                    }
                }

                Adapter.MoveNext();

                FracX += Cos;
                FracY -= Sin;
            }
        }

        public void ScanLineFilterTo(int X, int Y, int Length, ImageFilter Filter, IPixelAdapter<T> Adapter)
        {
            byte* pScan0 = (byte*)Context.Scan0;
            long SourceStride = Context.Stride;
            int KernelW = Filter.PatchWidth,
                KernelH = Filter.PatchHeight,
                KernelHW = KernelW >> 1,
                KernelHH = KernelH >> 1,
                SourceWidthL = Context.Width - 1,
                SourceHeightL = Context.Height - 1,
                Index, Tx, LTx, XBits, Offset;

            Struct*[] pDatas = new Struct*[KernelH];

            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                pDatas[Index] = (Struct*)(pScan0 + MathHelper.Clamp(Y - Index - KernelHH, 0, SourceHeightL) * SourceStride);
                pDatas[KernelH - Index - 1] = (Struct*)(pScan0 + MathHelper.Clamp(Y - Index + KernelHH, 0, SourceHeightL) * SourceStride);
            }
            pDatas[Index] = (Struct*)(pScan0 + Y * SourceStride);

            int IndexLength = pDatas[0]->Length;

            List<T> Palette = Context.Palette.Datas;
            ImagePatch<T> Patch = new ImagePatch<T>(KernelW, KernelH);
            T[] Pixels = null;

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                Pixels = new T[KernelH];

                XBits = X * Context.BitsPerPixel;
                Offset = XBits >> 3;
                XBits %= IndexLength;

                for (int j = 0; j < KernelH; j++)
                    Pixels[j] = Palette[(*(pDatas[j] + Offset))[XBits]];
            };

            //Init Block
            Index = -KernelHW;
            LTx = int.MaxValue;
            for (; Index < KernelHW; Index++)
            {
                Tx = MathHelper.Clamp(X + Index, 0, SourceWidthL);
                if (LTx != Tx)
                {
                    FillPixelsByX(Tx);
                    LTx = Tx;
                }
                Patch.Enqueue(Pixels);
            }

            Tx = X + KernelHW;
            ImageFilterArgs Arg = new ImageFilterArgs();
            for (int i = 0; i < Length; i++, Tx++)
            {
                // Next & Enqueue
                FillPixelsByX(MathHelper.Clamp(Tx, 0, SourceWidthL));
                Patch.Enqueue(Pixels);

                // Filter
                Filter.Filter(Patch.Data0, Arg, out byte A, out byte R, out byte G, out byte B);

                // Override
                Adapter.Override(A, R, G, B);
                Adapter.MoveNext();
            }
        }

        public void ContourOverlay(IImageContour Contour, T Color, double OffsetX, double OffsetY)
            => Context.Contour<T>(Contour, OffsetX, OffsetY, a => a.Overlay(Color));

        public void BlockOverlay(int X, int Y, IImageContext Source, int SourceX, int SourceY, int Width, int Height)
            => Context.Block(X, Y, Source, SourceX, SourceY, Width, Height, (s, d) => d.Overlay(s));

        public ImageContour FindBound(int SeedX, int SeedY, ImagePredicate Predicate)
            => Context.FindBound(SeedX, SeedY, Predicate);

        public IPixelAdapter<U> GetAdapter<U>(int X, int Y)
            where U : unmanaged, IPixel
        {
            int XBits = X * Context.BitsPerPixel;
            long Stride = Context.Stride,
                 Offset = Stride * Y;
            Struct* pScan = (Struct*)Context.Scan0 + Offset;

            return Context.PixelType == typeof(U) ? new PixelIndexedAdapter<U, Struct>(pScan, Stride, XBits, Context.Palette.Handle) :
                                                    new PixelIndexedAdapter<T, U, Struct>(pScan, Stride, XBits, Context.Palette.Handle);
        }

    }
}
