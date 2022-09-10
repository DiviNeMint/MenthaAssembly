using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator4<T> : IImageOperator<T>
        where T : unmanaged, IPixel
    {
        public IImageContext<T> Context { get; }

        public ImageOperator4(IImageContext<T> Context)
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
            byte* pScanA = (byte*)Context.ScanA,
                  pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

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
                    long Offset = Stride * b1 + ((a1 * Context.BitsPerPixel) >> 3);
                    byte* pDataA = pScanA + Offset,
                          pDataR = pScanR + Offset,
                          pDataG = pScanG + Offset,
                          pDataB = pScanB + Offset;

                    int a2 = (int)Math.Ceiling(FracX),
                        b3 = (int)Math.Ceiling(FracY);

                    double xa13 = FracX - a1,
                           xa24 = a2 - FracX,
                           yb12 = FracY - b1,
                           yb34 = b3 - FracY;

                    if (xa13 != 0 & xa24 != 0 & yb12 != 0 & yb34 != 0)
                    {
                        byte p1A = *pDataA,
                             p1R = *pDataR,
                             p1G = *pDataG,
                             p1B = *pDataB,
                             p2A, p2R, p2G, p2B, p3A, p3R, p3G, p3B, p4A, p4R, p4G, p4B;

                        if (a2 > a1)
                        {
                            p2A = *++pDataA;
                            p2R = *++pDataR;
                            p2G = *++pDataG;
                            p2B = *++pDataB;

                            if (b3 > b1)
                            {
                                pDataA += Stride;
                                pDataR += Stride;
                                pDataG += Stride;
                                pDataB += Stride;

                                p3A = *pDataA++;
                                p3R = *pDataR++;
                                p3G = *pDataG++;
                                p3B = *pDataB++;

                                p4A = *pDataA;
                                p4R = *pDataR;
                                p4G = *pDataG;
                                p4B = *pDataB;
                            }
                            else
                            {
                                p3A = p1A;
                                p3R = p1R;
                                p3G = p1G;
                                p3B = p1B;

                                p4A = p2A;
                                p4R = p2R;
                                p4G = p2G;
                                p4B = p2B;
                            }
                        }
                        else
                        {
                            p2A = p1A;
                            p2R = p1R;
                            p2G = p1G;
                            p2B = p1B;

                            if (b3 > b1)
                            {
                                pDataA += Stride;
                                pDataR += Stride;
                                pDataG += Stride;
                                pDataB += Stride;

                                p3A = *pDataA;
                                p3R = *pDataR;
                                p3G = *pDataG;
                                p3B = *pDataB;

                                p4A = p3A;
                                p4R = p3R;
                                p4G = p3G;
                                p4B = p3B;
                            }
                            else
                            {
                                p3A = p1A;
                                p3R = p1R;
                                p3G = p1G;
                                p3B = p1B;

                                p4A = p2A;
                                p4R = p2R;
                                p4G = p2G;
                                p4B = p2B;
                            }
                        }

                        byte A = (byte)((p1A * xa24 + p2A * xa13) * yb34 + (p3A * xa24 + p4A * xa13) * yb12),
                             R = (byte)((p1R * xa24 + p2R * xa13) * yb34 + (p3R * xa24 + p4R * xa13) * yb12),
                             G = (byte)((p1G * xa24 + p2G * xa13) * yb34 + (p3G * xa24 + p4G * xa13) * yb12),
                             B = (byte)((p1B * xa24 + p2B * xa13) * yb34 + (p3B * xa24 + p4B * xa13) * yb12);

                        Adapter.Override(A, R, G, B);
                    }
                    else
                    {
                        Adapter.Override(*pDataA, *pDataR, *pDataG, *pDataB);
                    }
                }

                Adapter.MoveNext();

                FracX += Cos;
                FracY -= Sin;
            }
        }

        public void ScanLineFilterTo(int X, int Y, int Length, ImageFilter Filter, IPixelAdapter<T> Adapter)
        {
            byte* pScanA = (byte*)Context.ScanA,
                  pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

            long SourceStride = Context.Stride;
            int KernelW = Filter.PatchWidth,
                KernelH = Filter.PatchHeight,
                KernelHW = KernelW >> 1,
                KernelHH = KernelH >> 1,
                SourceWidthL = Context.Width - 1,
                SourceHeightL = Context.Height - 1,
                Index,
                Tx, LTx;

            byte*[,] pDatas = new byte*[4, KernelH];
            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                long Offset = MathHelper.Clamp(Y - Index - KernelHH, 0, SourceHeightL) * SourceStride;

                pDatas[0, Index] = pScanA + Offset;
                pDatas[1, Index] = pScanR + Offset;
                pDatas[2, Index] = pScanG + Offset;
                pDatas[3, Index] = pScanB + Offset;

                LTx = KernelH - Index - 1;
                Offset = MathHelper.Clamp(Y - Index + KernelHH, 0, SourceHeightL) * SourceStride;
                pDatas[0, LTx] = pScanA + Offset;
                pDatas[1, LTx] = pScanR + Offset;
                pDatas[2, LTx] = pScanG + Offset;
                pDatas[3, LTx] = pScanB + Offset;
            }

            ImagePatch<T> Patch = new ImagePatch<T>(KernelW, KernelH);
            byte[] PixelsA = null,
                   PixelsR = null,
                   PixelsG = null,
                   PixelsB = null;

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                PixelsA = new byte[KernelH];
                PixelsR = new byte[KernelH];
                PixelsG = new byte[KernelH];
                PixelsB = new byte[KernelH];
                for (int j = 0; j < KernelH; j++)
                {
                    PixelsA[j] = *(pDatas[0, j] + Xt);
                    PixelsR[j] = *(pDatas[1, j] + Xt);
                    PixelsG[j] = *(pDatas[2, j] + Xt);
                    PixelsB[j] = *(pDatas[3, j] + Xt);
                }
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
                Patch.Enqueue(PixelsA, PixelsR, PixelsG, PixelsB);
            }

            Tx = X + KernelHW;
            ImageFilterArgs Arg = new ImageFilterArgs();
            for (int i = 0; i < Length; i++, Tx++)
            {
                // Next & Enqueue
                FillPixelsByX(MathHelper.Clamp(Tx, 0, SourceWidthL));
                Patch.Enqueue(PixelsA, PixelsR, PixelsG, PixelsB);

                // Filter
                Filter.Filter4(Patch, Arg, out byte A, out byte R, out byte G, out byte B);

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
            => new PixelAdapter4<U>(Context, X, Y);

    }
}
