using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator3<T> : IImageOperator<T>
        where T : unmanaged, IPixel
    {
        public IImageContext<T> Context { get; }

        public ImageOperator3(IImageContext<T> Context)
        {
            this.Context = Context;
        }

        public T GetPixel(int X, int Y)
        {
            T Pixel = default;
            long Offset = Context.Stride * Y + X;
            Pixel.Override(byte.MaxValue,
                           *((byte*)Context.ScanR + Offset),
                           *((byte*)Context.ScanG + Offset),
                           *((byte*)Context.ScanB + Offset));
            return Pixel;
        }

        public void SetPixel(int X, int Y, T Pixel)
        {
            long Offset = Context.Stride * Y + X;
            *((byte*)Context.ScanR + Offset) = Pixel.R;
            *((byte*)Context.ScanG + Offset) = Pixel.G;
            *((byte*)Context.ScanB + Offset) = Pixel.B;
        }

        public void ScanLine<U>(int X, int Y, int Length, Action<IPixelAdapter<U>> Handler)
            where U : unmanaged, IPixel
        {
            IPixelAdapter<U> Adapter = GetAdapter<U>(X, Y);
            for (int i = 0; i < Length; i++, Adapter.MoveNext())
                Handler(Adapter);
        }
        public void ScanLine<U>(int X, int Y, int Length, Predicate<IPixelAdapter<U>> Predicate)
            where U : unmanaged, IPixel
        {
            IPixelAdapter<U> Adapter = GetAdapter<U>(X, Y);
            for (int i = 0; i < Length; i++)
                if (Predicate(Adapter))
                    Adapter.MoveNext();
        }

        public void ScanLineNearestResizeTo(int X, int Y, int Length, float FracX, float Step, IPixelAdapter<T> Adapter)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                Adapter.Override(byte.MaxValue, *PixelR, *PixelG, *PixelB);
                Adapter.MoveNext();

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    PixelR++;
                    PixelG++;
                    PixelB++;
                }
            }
        }

        public void ScanLineBilinearResizeTo(int X, int Y, int Length, float FracX, float FracY, float Step, IPixelAdapter<T> Adapter)
        {
            long SourceStride = Context.Stride,
                 Offset = SourceStride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixelR0 = (byte*)Context.ScanR + Offset,
                  pPixelG0 = (byte*)Context.ScanG + Offset,
                  pPixelB0 = (byte*)Context.ScanB + Offset,
                  pPixelR1, pPixelG1, pPixelB1;

            if (Y + 1 < Context.Height)
            {
                pPixelR1 = pPixelR0 + SourceStride;
                pPixelG1 = pPixelG0 + SourceStride;
                pPixelB1 = pPixelB0 + SourceStride;
            }
            else
            {
                pPixelR1 = pPixelR0;
                pPixelG1 = pPixelG0;
                pPixelB1 = pPixelB0;
            }


            float IFracY = 1f - FracY;
            int SourceW = Context.Width;
            for (int i = 0; i < Length; i++)
            {
                byte R00 = *pPixelR0,
                     G00 = *pPixelG0,
                     B00 = *pPixelB0,
                     R10 = *pPixelR1,
                     G10 = *pPixelG1,
                     B10 = *pPixelB1,
                     R01, G01, B01, R11, G11, B11;

                if (X < SourceW)
                {
                    R01 = R00;
                    G01 = G00;
                    B01 = B00;

                    R11 = R10;
                    G11 = G10;
                    B11 = B10;
                }
                else
                {
                    R01 = *(pPixelR0 + 1);
                    G01 = *(pPixelG0 + 1);
                    B01 = *(pPixelB0 + 1);

                    R11 = *(pPixelR1 + 1);
                    G11 = *(pPixelG1 + 1);
                    B11 = *(pPixelB1 + 1);
                }

                float IFracX = 1f - FracX,
                      IFxIFy = IFracX * IFracY,
                      IFxFy = IFracX * FracY,
                      FxIFy = FracX * IFracY,
                      FxFy = FracX * FracY;

                Adapter.Override(byte.MaxValue,
                                 (byte)(R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy),
                                 (byte)(G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy),
                                 (byte)(B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy));
                Adapter.MoveNext();

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;

                    X++;

                    pPixelR0++;
                    pPixelG0++;
                    pPixelB0++;

                    pPixelR1++;
                    pPixelG1++;
                    pPixelB1++;
                }
            }
        }

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, IPixelAdapter<T> Adapter)
        {
            byte* pScanR = (byte*)Context.ScanR,
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
                    byte* pDataR = pScanR + Offset,
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
                        byte p1R = *pDataR,
                             p1G = *pDataG,
                             p1B = *pDataB,
                             p2R, p2G, p2B, p3R, p3G, p3B, p4R, p4G, p4B;

                        if (a2 > a1)
                        {
                            p2R = *++pDataR;
                            p2G = *++pDataG;
                            p2B = *++pDataB;

                            if (b3 > b1)
                            {
                                pDataR += Stride;
                                pDataG += Stride;
                                pDataB += Stride;

                                p3R = *pDataR++;
                                p3G = *pDataG++;
                                p3B = *pDataB++;

                                p4R = *pDataR;
                                p4G = *pDataG;
                                p4B = *pDataB;
                            }
                            else
                            {
                                p3R = p1R;
                                p3G = p1G;
                                p3B = p1B;

                                p4R = p2R;
                                p4G = p2G;
                                p4B = p2B;
                            }
                        }
                        else
                        {
                            p2R = p1R;
                            p2G = p1G;
                            p2B = p1B;

                            if (b3 > b1)
                            {
                                pDataR += Stride;
                                pDataG += Stride;
                                pDataB += Stride;

                                p3R = *pDataR;
                                p3G = *pDataG;
                                p3B = *pDataB;

                                p4R = p3R;
                                p4G = p3G;
                                p4B = p3B;
                            }
                            else
                            {
                                p3R = p1R;
                                p3G = p1G;
                                p3B = p1B;

                                p4R = p2R;
                                p4G = p2G;
                                p4B = p2B;
                            }
                        }

                        byte R = (byte)((p1R * xa24 + p2R * xa13) * yb34 + (p3R * xa24 + p4R * xa13) * yb12),
                             G = (byte)((p1G * xa24 + p2G * xa13) * yb34 + (p3G * xa24 + p4G * xa13) * yb12),
                             B = (byte)((p1B * xa24 + p2B * xa13) * yb34 + (p3B * xa24 + p4B * xa13) * yb12);

                        Adapter.Override(byte.MaxValue, R, G, B);
                    }
                    else
                    {
                        Adapter.Override(byte.MaxValue, *pDataR, *pDataG, *pDataB);
                    }
                }

                Adapter.MoveNext();

                FracX += Cos;
                FracY -= Sin;
            }
        }

        public void ScanLineFilterTo(int X, int Y, int Length, ImageFilter Filter, IPixelAdapter<T> Adapter)
        {
            byte* pScanR = (byte*)Context.ScanR,
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

            byte*[,] pDatas = new byte*[3, KernelH];
            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                long Offset = MathHelper.Clamp(Y - Index - KernelHH, 0, SourceHeightL) * SourceStride;

                pDatas[1, Index] = pScanR + Offset;
                pDatas[2, Index] = pScanG + Offset;
                pDatas[3, Index] = pScanB + Offset;

                LTx = KernelH - Index - 1;
                Offset = MathHelper.Clamp(Y - Index + KernelHH, 0, SourceHeightL) * SourceStride;
                pDatas[1, LTx] = pScanR + Offset;
                pDatas[2, LTx] = pScanG + Offset;
                pDatas[3, LTx] = pScanB + Offset;
            }

            ImagePatch<T> Patch = new ImagePatch<T>(KernelW, KernelH);
            byte[] PixelsR = null,
                   PixelsG = null,
                   PixelsB = null;

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                PixelsR = new byte[KernelH];
                PixelsG = new byte[KernelH];
                PixelsB = new byte[KernelH];

                for (int j = 0; j < KernelH; j++)
                {
                    PixelsR[j] = *(pDatas[0, j] + Xt);
                    PixelsG[j] = *(pDatas[1, j] + Xt);
                    PixelsB[j] = *(pDatas[2, j] + Xt);
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
                Patch.Enqueue(PixelsR, PixelsG, PixelsB);
            }

            Tx = X + KernelHW;
            ImageFilterArgs Arg = new ImageFilterArgs();
            for (int i = 0; i < Length; i++, Tx++)
            {
                // Next & Enqueue
                FillPixelsByX(MathHelper.Clamp(Tx, 0, SourceWidthL));
                Patch.Enqueue(PixelsR, PixelsG, PixelsB);

                // Filter
                Filter.Filter3(Patch, Arg, out byte A, out byte R, out byte G, out byte B);

                // Override
                Adapter.Override(A, R, G, B);
                Adapter.MoveNext();
            }
        }

        public void ContourOverlay(ImageContour Contour, T Color, int OffsetX, int OffsetY)
        {
            IEnumerator<KeyValuePair<int, ContourData>> Enumerator = Contour.GetEnumerator();
            if (!Enumerator.MoveNext())
                return;

            int MaxX = Context.Width - 1,
                MaxY = Context.Height - 1;
            KeyValuePair<int, ContourData> Current = Enumerator.Current;

            long Y = Current.Key + OffsetY;
            if (MaxY < Y)
                return;

            while (Y < 0)
            {
                if (!Enumerator.MoveNext())
                    return;

                Current = Enumerator.Current;
                Y = Current.Key + OffsetY;

                if (MaxY < Y)
                    return;
            }

            long Offset = Context.Stride * Y;
            byte* pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            ContourData Data = Current.Value;

            void OverlayHandler()
            {
                byte* pTempPixelR = pPixelR,
                      pTempPixelG = pPixelG,
                      pTempPixelB = pPixelB;

                int CurrentX = 0;
                for (int i = 0; i < Data.Count; i++)
                {
                    int Sx = Math.Max(Data[i++] + OffsetX, 0),
                        Ex = Math.Min(Data[i] + OffsetX, MaxX);

                    if (Ex < Sx)
                        continue;

                    if (MaxX < Sx)
                        return;

                    Offset = ((Sx - CurrentX) * Context.BitsPerPixel) >> 3;
                    pTempPixelR += Offset;
                    pTempPixelG += Offset;
                    pTempPixelB += Offset;

                    for (int j = Sx; j <= Ex; j++)
                    {
                        PixelHelper.Overlay(ref pTempPixelR, ref pTempPixelG, ref pTempPixelB, Color.A, Color.R, Color.G, Color.B);
                        pTempPixelR++;
                        pTempPixelG++;
                        pTempPixelB++;
                    }
                    CurrentX = Ex + 1;
                }
            };

            OverlayHandler();
            while (Enumerator.MoveNext())
            {
                Current = Enumerator.Current;

                long TempY = Current.Key + OffsetY;
                if (MaxY < TempY)
                    return;

                Offset = Context.Stride * (TempY - Y);
                pPixelR += Offset;
                pPixelG += Offset;
                pPixelB += Offset;

                Y = TempY;
                Data = Current.Value;

                OverlayHandler();
            }
        }

        public void BlockOverlay(int X, int Y, IImageContext Source, int SourceX, int SourceY, int Width, int Height)
        {
            long Stride = Context.Stride,
                 Offset = Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            for (int j = 0; j < Height; j++)
            {
                byte* pDestR = pPixelR,
                      pDestG = pPixelG,
                      pDestB = pPixelB;
                Source.Operator.ScanLine<T>(SourceX, SourceY + j, Width, Adapter => Adapter.OverlayTo(pDestR++, pDestG++, pDestB++));

                pPixelR += Stride;
                pPixelG += Stride;
                pPixelB += Stride;
            }
        }

        public ImageContour FindBound(int SeedX, int SeedY, ImagePredicate Predicate)
        {
            int Width = Context.Width,
                Height = Context.Height;

            if (SeedX < 0 || Width <= SeedX ||
                SeedY < 0 || Height <= SeedY)
                return null;

            ImageContour Contour = new ImageContour();
            Stack<int> StackX = new Stack<int>(),
                       StackY = new Stack<int>();
            StackX.Push(SeedX);
            StackY.Push(SeedY);

            int X, Y, SaveX, Rx, Lx;
            long Offset;
            byte* pSeedR, pSeedG, pSeedB,
                  pPixelR, pPixelG, pPixelB;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);

                pSeedR = (byte*)Context.ScanR + Offset;
                pSeedG = (byte*)Context.ScanG + Offset;
                pSeedB = (byte*)Context.ScanB + Offset;

                // Find Right Bound
                pPixelR = pSeedR;
                pPixelG = pSeedG;
                pPixelB = pSeedB;
                while (X < Width && !Predicate(X, Y, byte.MaxValue, *pPixelR, *pPixelG, *pPixelB))
                {
                    X++;
                    pPixelR++;
                    pPixelG++;
                    pPixelB++;
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                pPixelR = pSeedR - 1;
                pPixelG = pSeedG - 1;
                pPixelB = pSeedB - 1;
                while (-1 < X && !Predicate(X, Y, byte.MaxValue, *pPixelR, *pPixelG, *pPixelB))
                {
                    X--;
                    pPixelR--;
                    pPixelG--;
                    pPixelB--;
                }

                Lx = X + 1;

                // Log Region
                Contour[Y].Union(Lx, Rx);

                // Lower ScanLine's Seed
                bool NeedFill = false;
                X = Lx;
                Y++;

                Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
                pSeedR = (byte*)Context.ScanR + Offset;
                pSeedG = (byte*)Context.ScanG + Offset;
                pSeedB = (byte*)Context.ScanB + Offset;
                if (-1 < Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, byte.MaxValue, *pSeedR, *pSeedG, *pSeedB))
                        {
                            NeedFill = true;
                            X++;
                            pSeedR++;
                            pSeedG++;
                            pSeedB++;
                        }

                        if (NeedFill)
                        {
                            StackX.Push(X - 1);
                            StackY.Push(Y);
                            NeedFill = false;
                        }
                    }

                // Upper ScanLine's Seed
                NeedFill = false;
                X = Lx;
                Y -= 2;

                Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
                pSeedR = (byte*)Context.ScanR + Offset;
                pSeedG = (byte*)Context.ScanG + Offset;
                pSeedB = (byte*)Context.ScanB + Offset;
                if (0 <= Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, byte.MaxValue, *pSeedR, *pSeedG, *pSeedB))
                        {
                            NeedFill = true;
                            X++;
                            pSeedR++;
                            pSeedG++;
                            pSeedB++;
                        }

                        if (NeedFill)
                        {
                            StackX.Push(X - 1);
                            StackY.Push(Y);
                            NeedFill = false;
                        }
                    }
            }

            return Contour;
        }

        public IPixelAdapter<U> GetAdapter<U>(int X, int Y)
            where U : unmanaged, IPixel
        {
            long Stride = Context.Stride,
                 Offset = Stride * Y + X;
            byte* pScanR = (byte*)Context.ScanR + Offset,
                  pScanG = (byte*)Context.ScanG + Offset,
                  pScanB = (byte*)Context.ScanB + Offset;

            return new PixelAdapter3<U>(pScanR, pScanG, pScanB, Stride);
        }

    }
}