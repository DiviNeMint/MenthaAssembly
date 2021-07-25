using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator3<T> : IImageOperator<T>
        where T : unmanaged, IPixel
    {
        public IImageContext<T> Context { get; }
        IImageContext IImageOperator.Context => this.Context;

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
        IPixel IImageOperator.GetPixel(int X, int Y)
            => this.GetPixel(X, Y);

        public void SetPixel(int X, int Y, T Pixel)
        {
            long Offset = Context.Stride * Y + X;
            *((byte*)Context.ScanR + Offset) = Pixel.R;
            *((byte*)Context.ScanG + Offset) = Pixel.G;
            *((byte*)Context.ScanB + Offset) = Pixel.B;
        }
        void IImageOperator.SetPixel(int X, int Y, IPixel Pixel)
            => this.SetPixel(X, Y, Pixel.ToPixel<T>());

        public ImageOperator3(IImageContext<T> Context)
        {
            this.Context = Context;
        }

        public void ScanLineOverride(int X, int Y, int Length, T Color)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pPixelR++ = Color.R;
                *pPixelG++ = Color.G;
                *pPixelB++ = Color.B;
            }
        }
        void IImageOperator.ScanLineOverride(int X, int Y, int Length, IPixel Color)
            => this.ScanLineOverride(X, Y, Length, Color.ToPixel<T>());
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDest)
            => this.ScanLineOverrideTo(X, Y, Length, (T*)pDest);
        public void ScanLineOverrideTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
                pDest++->Override(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        }
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = *PixelR++;
                *pDestG++ = *PixelG++;
                *pDestB++ = *PixelB++;
            }
        }
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = byte.MaxValue;
                *pDestR++ = *PixelR++;
                *pDestG++ = *PixelG++;
                *pDestB++ = *PixelB++;
            }
        }

        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDest)
            => this.ScanLineReverseOverrideTo(X, Y, Length, (T*)pDest);
        public void ScanLineReverseOverrideTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            pDest += Length - 1;

            for (int i = 0; i < Length; i++)
                pDest--->Override(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        }
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            int OffsetToEnd = Length - 1;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            for (int i = 0; i < Length; i++)
            {
                *pDestR-- = *PixelR++;
                *pDestG-- = *PixelG++;
                *pDestB-- = *PixelB++;
            }
        }
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            int OffsetToEnd = Length - 1;
            pDestA += OffsetToEnd;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            for (int i = 0; i < Length; i++)
            {
                *pDestA-- = byte.MaxValue;
                *pDestR-- = *PixelR++;
                *pDestG-- = *PixelG++;
                *pDestB-- = *PixelB++;
            }
        }

        public void ScanLineOverlay(int X, int Y, int Length, T Color)
        {
            if (Color.A is byte.MinValue || Color.A is byte.MaxValue)
            {
                ScanLineOverride(X, Y, Length, Color);
                return;
            }

            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                PixelHelper.Overlay(ref pPixelR, ref pPixelG, ref pPixelB, Color.A, Color.R, Color.G, Color.B);
                pPixelR++;
                pPixelG++;
                pPixelB++;
            }
        }
        void IImageOperator.ScanLineOverlay(int X, int Y, int Length, IPixel Color)
            => this.ScanLineOverlay(X, Y, Length, Color.ToPixel<T>());
        public void ScanLineOverlayTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
                pDest++->Overlay(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        }
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                PixelHelper.Overlay(ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                pDestR++;
                pDestG++;
                pDestB++;
            }
        }
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                PixelHelper.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                pDestA++;
                pDestR++;
                pDestG++;
                pDestB++;
            }
        }

        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDest)
            => ScanLineNearestResizeTo(FracX, Step, X, Y, Length, (T*)pDest);
        public void ScanLineNearestResizeTo<T2>(float FracX, float Step, int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                pDest++->Override(byte.MaxValue, *PixelR, *PixelG, *PixelB);

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
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = *PixelR;
                *pDestG++ = *PixelG;
                *pDestB++ = *PixelB;

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
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = byte.MaxValue;
                *pDestR++ = *PixelR;
                *pDestG++ = *PixelG;
                *pDestB++ = *PixelB;

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
        public void ScanLineNearestResizeTo(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref byte* pDest)
        {
            T* pTemp = (T*)pDest;
            ScanLineNearestResizeTo(ref FracX, Step, ref X, MaxX, MaxXFrac, Y, ref pTemp);
            pDest = (byte*)pTemp;
        }
        public void ScanLineNearestResizeTo<T2>(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            while (X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac)))
            {
                pDest++->Override(byte.MaxValue, *PixelR, *PixelG, *PixelB);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    PixelR++;
                    PixelG++;
                    PixelB++;
                    X++;
                }
            }
        }

        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDest)
            => ScanLineBilinearResizeTo(FracX, FracY, Step, X, Y, Length, (T*)pDest);
        public void ScanLineBilinearResizeTo<T2>(float FracX, float FracY, float Step, int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
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

                pDest++->Override(byte.MaxValue,
                                  (byte)(R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy),
                                  (byte)(G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy),
                                  (byte)(B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy));

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
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
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

                *pDestR++ = (byte)(R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy);
                *pDestG++ = (byte)(G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy);
                *pDestB++ = (byte)(B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy);

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
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
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

                *pDestA++ = byte.MaxValue;
                *pDestR++ = (byte)(R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy);
                *pDestG++ = (byte)(G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy);
                *pDestB++ = (byte)(B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy);

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

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, byte* pDest)
            => ScanLineRotateTo(X, Y, Length, FracX, FracY, Sin, Cos, (T*)pDest);
        public void ScanLineRotateTo<T2>(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, T2* pDest) where T2 : unmanaged, IPixel
        {
            byte* pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

            FracX += X;
            FracY += Y;

            long Stride = Context.Stride;
            int Wo = Context.Width,
                Lo = Context.Height,
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

                        *pDest++ = PixelHelper.ToPixel<T2>(byte.MaxValue, R, G, B);
                    }
                    else
                    {
                        pDest++->Override(byte.MaxValue, *pDataR, *pDataG, *pDataB);
                    }
                }
                else
                {
                    pDest++;
                }

                FracX += Cos;
                FracY -= Sin;
            }
        }
        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            byte* pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

            FracX += X;
            FracY += Y;

            long Stride = Context.Stride;
            int Wo = Context.Width,
                Lo = Context.Height,
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

                        *pDestR++ = (byte)((p1R * xa24 + p2R * xa13) * yb34 + (p3R * xa24 + p4R * xa13) * yb12);
                        *pDestG++ = (byte)((p1G * xa24 + p2G * xa13) * yb34 + (p3G * xa24 + p4G * xa13) * yb12);
                        *pDestB++ = (byte)((p1B * xa24 + p2B * xa13) * yb34 + (p3B * xa24 + p4B * xa13) * yb12);
                    }
                    else
                    {
                        *pDestR++ = *pDataR;
                        *pDestG++ = *pDataG;
                        *pDestB++ = *pDataB;
                    }
                }
                else
                {
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                FracX += Cos;
                FracY -= Sin;
            }
        }
        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            byte* pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

            FracX += X;
            FracY += Y;

            long Stride = Context.Stride;
            int Wo = Context.Width,
                Lo = Context.Height,
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

                        *pDestA++ = byte.MaxValue;
                        *pDestR++ = (byte)((p1R * xa24 + p2R * xa13) * yb34 + (p3R * xa24 + p4R * xa13) * yb12);
                        *pDestG++ = (byte)((p1G * xa24 + p2G * xa13) * yb34 + (p3G * xa24 + p4G * xa13) * yb12);
                        *pDestB++ = (byte)((p1B * xa24 + p2B * xa13) * yb34 + (p3B * xa24 + p4B * xa13) * yb12);
                    }
                    else
                    {
                        *pDestA++ = byte.MaxValue;
                        *pDestR++ = *pDataR;
                        *pDestG++ = *pDataG;
                        *pDestB++ = *pDataB;
                    }
                }
                else
                {
                    pDestA++;
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                FracX += Cos;
                FracY -= Sin;
            }
        }

        public void ScanLineConvolute(int X, int Y, int Length, ConvoluteKernel Kernel, byte* pDest)
            => ScanLineConvolute(X, Y, Length, Kernel, (T*)pDest);
        public void ScanLineConvolute<T2>(int X, int Y, int Length, ConvoluteKernel Kernel, T2* pDest) where T2 : unmanaged, IPixel
        {
            byte* pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

            int[,] Datas = Kernel.Datas;
            int KernelW = Kernel.Width,
                KernelH = Kernel.Height,
                KernelHW = KernelW >> 1,
                KernelHH = KernelH >> 1,
                KernelSum = Kernel.FactorSum,
                KernelOffset = Kernel.Offset,
                SourceWidthL = Context.Width - 1,
                SourceHeightL = Context.Width - 1,
                Index,
                LTx;

            byte*[,] pDatas = new byte*[4, KernelH];
            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                long Offset = Math.Max(Y - Index, 0) * Context.Stride;

                pDatas[0, Index] = pScanR + Offset;
                pDatas[1, Index] = pScanG + Offset;
                pDatas[2, Index] = pScanB + Offset;

                LTx = KernelH - Index - 1;
                Offset = Math.Min(Y - Index, SourceHeightL) * Context.Stride;
                pDatas[0, LTx] = pScanR + Offset;
                pDatas[1, LTx] = pScanG + Offset;
                pDatas[2, LTx] = pScanB + Offset;
            }

            Queue<byte[,]> PixelBlock = new Queue<byte[,]>();
            byte[,] Pixels = new byte[4, KernelH];

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                for (int j = 0; j < KernelH; j++)
                {
                    Pixels[0, j] = *(pDatas[0, j] + Xt);
                    Pixels[1, j] = *(pDatas[1, j] + Xt);
                    Pixels[2, j] = *(pDatas[2, j] + Xt);
                }
            };

            //Init Block
            Index = -KernelHW;
            LTx = int.MaxValue;
            for (; Index <= KernelHW; Index++)
            {
                int Tx = MathHelper.Clamp(X + Index, 0, SourceWidthL);
                if (LTx != Tx)
                {
                    FillPixelsByX(Tx);
                    LTx = Tx;
                }
                PixelBlock.Enqueue(Pixels);
            }

            for (int i = 0; i < Length; i++)
            {
                int R = 0,
                    G = 0,
                    B = 0;

                // Left Bound and not enqueue.
                Index = 0;
                Pixels = PixelBlock.Dequeue();
                for (int j = 0; j < KernelH; j++)
                {
                    int k = Datas[j, Index];
                    if (k == 0)
                        continue;

                    R += Pixels[0, j] * k;
                    G += Pixels[1, j] * k;
                    B += Pixels[2, j] * k;
                }

                for (Index = 1; Index < KernelW - 1; Index++)
                {
                    Pixels = PixelBlock.Dequeue();
                    for (int j = 0; j < KernelH; j++)
                    {
                        int k = Datas[j, Index];
                        if (k == 0)
                            continue;

                        R += Pixels[0, j] * k;
                        G += Pixels[1, j] * k;
                        B += Pixels[2, j] * k;
                    }

                    PixelBlock.Enqueue(Pixels);
                }

                // Right Bound and enqueue
                LTx = MathHelper.Clamp(X + i + KernelHW, 0, SourceWidthL);
                for (int j = 0; j < KernelH; j++)
                {
                    Pixels[0, j] = *(pDatas[0, j] + LTx);
                    Pixels[1, j] = *(pDatas[1, j] + LTx);
                    Pixels[2, j] = *(pDatas[2, j] + LTx);

                    int k = Datas[j, Index];
                    if (k == 0)
                        continue;

                    R += Pixels[0, j] * k;
                    G += Pixels[1, j] * k;
                    B += Pixels[2, j] * k;
                }

                PixelBlock.Enqueue(Pixels);

                pDest++->Override(byte.MaxValue,
                                  (byte)MathHelper.Clamp((R / KernelSum) + KernelOffset, 0, 255),
                                  (byte)MathHelper.Clamp((G / KernelSum) + KernelOffset, 0, 255),
                                  (byte)MathHelper.Clamp((B / KernelSum) + KernelOffset, 0, 255));
            }
        }
        public void ScanLineConvolute(int X, int Y, int Length, ConvoluteKernel Kernel, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            byte* pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

            int[,] Datas = Kernel.Datas;
            int KernelW = Kernel.Width,
                KernelH = Kernel.Height,
                KernelHW = KernelW >> 1,
                KernelHH = KernelH >> 1,
                KernelSum = Kernel.FactorSum,
                KernelOffset = Kernel.Offset,
                SourceWidthL = Context.Width - 1,
                SourceHeightL = Context.Width - 1,
                Index,
                LTx;

            byte*[,] pDatas = new byte*[4, KernelH];
            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                long Offset = Math.Max(Y - Index, 0) * Context.Stride;

                pDatas[0, Index] = pScanR + Offset;
                pDatas[1, Index] = pScanG + Offset;
                pDatas[2, Index] = pScanB + Offset;

                LTx = KernelH - Index - 1;
                Offset = Math.Min(Y - Index, SourceHeightL) * Context.Stride;
                pDatas[0, LTx] = pScanR + Offset;
                pDatas[1, LTx] = pScanG + Offset;
                pDatas[2, LTx] = pScanB + Offset;
            }

            Queue<byte[,]> PixelBlock = new Queue<byte[,]>();
            byte[,] Pixels = new byte[4, KernelH];

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                for (int j = 0; j < KernelH; j++)
                {
                    Pixels[0, j] = *(pDatas[0, j] + Xt);
                    Pixels[1, j] = *(pDatas[1, j] + Xt);
                    Pixels[2, j] = *(pDatas[2, j] + Xt);
                }
            };

            //Init Block
            Index = -KernelHW;
            LTx = int.MaxValue;
            for (; Index <= KernelHW; Index++)
            {
                int Tx = MathHelper.Clamp(X + Index, 0, SourceWidthL);
                if (LTx != Tx)
                {
                    FillPixelsByX(Tx);
                    LTx = Tx;
                }
                PixelBlock.Enqueue(Pixels);
            }

            for (int i = 0; i < Length; i++)
            {
                int R = 0,
                    G = 0,
                    B = 0;

                // Left Bound and not enqueue.
                Index = 0;
                Pixels = PixelBlock.Dequeue();
                for (int j = 0; j < KernelH; j++)
                {
                    int k = Datas[j, Index];
                    if (k == 0)
                        continue;

                    R += Pixels[0, j] * k;
                    G += Pixels[1, j] * k;
                    B += Pixels[2, j] * k;
                }

                for (Index = 1; Index < KernelW - 1; Index++)
                {
                    Pixels = PixelBlock.Dequeue();
                    for (int j = 0; j < KernelH; j++)
                    {
                        int k = Datas[j, Index];
                        if (k == 0)
                            continue;

                        R += Pixels[0, j] * k;
                        G += Pixels[1, j] * k;
                        B += Pixels[2, j] * k;
                    }

                    PixelBlock.Enqueue(Pixels);
                }

                // Right Bound and enqueue
                LTx = MathHelper.Clamp(X + i + KernelHW, 0, SourceWidthL);
                for (int j = 0; j < KernelH; j++)
                {
                    Pixels[0, j] = *(pDatas[0, j] + LTx);
                    Pixels[1, j] = *(pDatas[1, j] + LTx);
                    Pixels[2, j] = *(pDatas[2, j] + LTx);

                    int k = Datas[j, Index];
                    if (k == 0)
                        continue;

                    R += Pixels[0, j] * k;
                    G += Pixels[1, j] * k;
                    B += Pixels[2, j] * k;
                }

                PixelBlock.Enqueue(Pixels);

                *pDestR++ = (byte)MathHelper.Clamp((R / KernelSum) + KernelOffset, 0, 255);
                *pDestG++ = (byte)MathHelper.Clamp((G / KernelSum) + KernelOffset, 0, 255);
                *pDestB++ = (byte)MathHelper.Clamp((B / KernelSum) + KernelOffset, 0, 255);
            }
        }
        public void ScanLineConvolute(int X, int Y, int Length, ConvoluteKernel Kernel, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            byte* pScanR = (byte*)Context.ScanR,
                  pScanG = (byte*)Context.ScanG,
                  pScanB = (byte*)Context.ScanB;

            int[,] Datas = Kernel.Datas;
            int KernelW = Kernel.Width,
                KernelH = Kernel.Height,
                KernelHW = KernelW >> 1,
                KernelHH = KernelH >> 1,
                KernelSum = Kernel.FactorSum,
                KernelOffset = Kernel.Offset,
                SourceWidthL = Context.Width - 1,
                SourceHeightL = Context.Width - 1,
                Index,
                LTx;

            byte*[,] pDatas = new byte*[4, KernelH];
            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                long Offset = Math.Max(Y - Index, 0) * Context.Stride;

                pDatas[0, Index] = pScanR + Offset;
                pDatas[1, Index] = pScanG + Offset;
                pDatas[2, Index] = pScanB + Offset;

                LTx = KernelH - Index - 1;
                Offset = Math.Min(Y - Index, SourceHeightL) * Context.Stride;
                pDatas[0, LTx] = pScanR + Offset;
                pDatas[1, LTx] = pScanG + Offset;
                pDatas[2, LTx] = pScanB + Offset;
            }

            Queue<byte[,]> PixelBlock = new Queue<byte[,]>();
            byte[,] Pixels = new byte[4, KernelH];

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                for (int j = 0; j < KernelH; j++)
                {
                    Pixels[0, j] = *(pDatas[0, j] + Xt);
                    Pixels[1, j] = *(pDatas[1, j] + Xt);
                    Pixels[2, j] = *(pDatas[2, j] + Xt);
                }
            };

            //Init Block
            Index = -KernelHW;
            LTx = int.MaxValue;
            for (; Index <= KernelHW; Index++)
            {
                int Tx = MathHelper.Clamp(X + Index, 0, SourceWidthL);
                if (LTx != Tx)
                {
                    FillPixelsByX(Tx);
                    LTx = Tx;
                }
                PixelBlock.Enqueue(Pixels);
            }

            for (int i = 0; i < Length; i++)
            {
                int R = 0,
                    G = 0,
                    B = 0;

                // Left Bound and not enqueue.
                Index = 0;
                Pixels = PixelBlock.Dequeue();
                for (int j = 0; j < KernelH; j++)
                {
                    int k = Datas[j, Index];
                    if (k == 0)
                        continue;

                    R += Pixels[0, j] * k;
                    G += Pixels[1, j] * k;
                    B += Pixels[2, j] * k;
                }

                for (Index = 1; Index < KernelW - 1; Index++)
                {
                    Pixels = PixelBlock.Dequeue();
                    for (int j = 0; j < KernelH; j++)
                    {
                        int k = Datas[j, Index];
                        if (k == 0)
                            continue;

                        R += Pixels[0, j] * k;
                        G += Pixels[1, j] * k;
                        B += Pixels[2, j] * k;
                    }

                    PixelBlock.Enqueue(Pixels);
                }

                // Right Bound and enqueue
                LTx = MathHelper.Clamp(X + i + KernelHW, 0, SourceWidthL);
                for (int j = 0; j < KernelH; j++)
                {
                    Pixels[0, j] = *(pDatas[0, j] + LTx);
                    Pixels[1, j] = *(pDatas[1, j] + LTx);
                    Pixels[2, j] = *(pDatas[2, j] + LTx);

                    int k = Datas[j, Index];
                    if (k == 0)
                        continue;

                    R += Pixels[0, j] * k;
                    G += Pixels[1, j] * k;
                    B += Pixels[2, j] * k;
                }

                PixelBlock.Enqueue(Pixels);

                *pDestA++ = byte.MaxValue;
                *pDestR++ = (byte)MathHelper.Clamp((R / KernelSum) + KernelOffset, 0, 255);
                *pDestG++ = (byte)MathHelper.Clamp((G / KernelSum) + KernelOffset, 0, 255);
                *pDestB++ = (byte)MathHelper.Clamp((B / KernelSum) + KernelOffset, 0, 255);
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

                    if (Ex <= Sx)
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
        void IImageOperator.ContourOverlay(ImageContour Contour, IPixel Color, int OffsetX, int OffsetY)
            => this.ContourOverlay(Contour, Color.ToPixel<T>(), OffsetX, OffsetY);

        public void BlockOverlay(int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
            long Stride = Context.Stride,
                 Offset = Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            for (int j = 0; j < Height; j++)
            {
                Source.Operator.ScanLineOverlayTo(X, Y + j, Width, pPixelR, pPixelG, pPixelB);
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

    }
}
