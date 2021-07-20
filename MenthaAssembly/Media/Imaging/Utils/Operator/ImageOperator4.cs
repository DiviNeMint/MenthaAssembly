using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator4<T> : IImageOperator<T>
        where T : unmanaged, IPixel
    {
        public IImageContext<T> Context { get; }
        IImageContext IImageOperator.Context => this.Context;

        public T GetPixel(int X, int Y)
        {
            T Pixel = default;
            long Offset = Context.Stride * Y + X;
            Pixel.Override(*((byte*)Context.ScanA + Offset),
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
            *((byte*)Context.ScanA + Offset) = Pixel.A;
            *((byte*)Context.ScanR + Offset) = Pixel.R;
            *((byte*)Context.ScanG + Offset) = Pixel.G;
            *((byte*)Context.ScanB + Offset) = Pixel.B;
        }
        void IImageOperator.SetPixel(int X, int Y, IPixel Pixel)
            => this.SetPixel(X, Y, Pixel.ToPixel<T>());

        public ImageOperator4(IImageContext<T> Context)
        {
            this.Context = Context;
        }

        public void ScanLineOverride(int X, int Y, int Length, T Color)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixelA = (byte*)Context.ScanA + Offset,
                  pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pPixelA++ = Color.A;
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
                pDest++->Override(*PixelA++, *PixelR++, *PixelG++, *PixelB++);
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = *PixelA++;
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            pDest += Length - 1;

            for (int i = 0; i < Length; i++)
                pDest--->Override(*PixelA++, *PixelR++, *PixelG++, *PixelB++);
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            int OffsetToEnd = Length - 1;
            pDestA += OffsetToEnd;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            for (int i = 0; i < Length; i++)
            {
                *pDestA-- = *PixelA++;
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
            byte* pDestA = (byte*)Context.ScanA + Offset,
                  pDestR = (byte*)Context.ScanR + Offset,
                  pDestG = (byte*)Context.ScanG + Offset,
                  pDestB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                PixelHelper.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, Color.A, Color.R, Color.G, Color.B);
                pDestA++;
                pDestR++;
                pDestG++;
                pDestB++;
            }
        }
        void IImageOperator.ScanLineOverlay(int X, int Y, int Length, IPixel Color)
            => this.ScanLineOverlay(X, Y, Length, Color.ToPixel<T>());
        public void ScanLineOverlayTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
                pDest++->Overlay(*PixelA++, *PixelR++, *PixelG++, *PixelB++);
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                PixelHelper.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, *PixelA++, *PixelR++, *PixelG++, *PixelB++);
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                pDest++->Override(*PixelA, *PixelR, *PixelG, *PixelB);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    PixelA++;
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = *PixelA;
                *pDestR++ = *PixelR;
                *pDestG++ = *PixelG;
                *pDestB++ = *PixelB;

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    PixelA++;
                    PixelR++;
                    PixelG++;
                    PixelB++;
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
            byte* pPixelA0 = (byte*)Context.ScanA + Offset,
                  pPixelR0 = (byte*)Context.ScanR + Offset,
                  pPixelG0 = (byte*)Context.ScanG + Offset,
                  pPixelB0 = (byte*)Context.ScanB + Offset,
                  pPixelA1 = pPixelA0 + SourceStride,
                  pPixelR1 = pPixelR0 + SourceStride,
                  pPixelG1 = pPixelG0 + SourceStride,
                  pPixelB1 = pPixelB0 + SourceStride;

            float IFracY = 1f - FracY;
            int SourceW = Context.Width;
            for (int i = 0; i < Length; i++)
            {
                byte A00 = *pPixelA0,
                     R00 = *pPixelR0,
                     G00 = *pPixelG0,
                     B00 = *pPixelB0,
                     A10 = *pPixelA1,
                     R10 = *pPixelR1,
                     G10 = *pPixelG1,
                     B10 = *pPixelB1,
                     A01, R01, G01, B01, A11, R11, G11, B11;

                if (X < SourceW)
                {
                    A01 = A00;
                    R01 = R00;
                    G01 = G00;
                    B01 = B00;

                    A11 = A10;
                    R11 = R10;
                    G11 = G10;
                    B11 = B10;
                }
                else
                {
                    A01 = *(pPixelA0 + 1);
                    R01 = *(pPixelR0 + 1);
                    G01 = *(pPixelG0 + 1);
                    B01 = *(pPixelB0 + 1);

                    A11 = *(pPixelA1 + 1);
                    R11 = *(pPixelR1 + 1);
                    G11 = *(pPixelG1 + 1);
                    B11 = *(pPixelB1 + 1);
                }

                float IFracX = 1f - FracX,
                      IFxIFy = IFracX * IFracY,
                      IFxFy = IFracX * FracY,
                      FxIFy = FracX * IFracY,
                      FxFy = FracX * FracY;

                pDest++->Override((byte)(A00 * IFxIFy + A01 * FxIFy + A10 * IFxFy + A11 * FxFy),
                                  (byte)(R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy),
                                  (byte)(G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy),
                                  (byte)(B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy));

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;

                    X++;

                    pPixelA0++;
                    pPixelR0++;
                    pPixelG0++;
                    pPixelB0++;

                    pPixelA1++;
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
                  pPixelR1 = pPixelR0 + SourceStride,
                  pPixelG1 = pPixelG0 + SourceStride,
                  pPixelB1 = pPixelB0 + SourceStride;

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
            byte* pPixelA0 = (byte*)Context.ScanA + Offset,
                  pPixelR0 = (byte*)Context.ScanR + Offset,
                  pPixelG0 = (byte*)Context.ScanG + Offset,
                  pPixelB0 = (byte*)Context.ScanB + Offset,
                  pPixelA1 = pPixelA0 + SourceStride,
                  pPixelR1 = pPixelR0 + SourceStride,
                  pPixelG1 = pPixelG0 + SourceStride,
                  pPixelB1 = pPixelB0 + SourceStride;

            float IFracY = 1f - FracY;
            int SourceW = Context.Width;
            for (int i = 0; i < Length; i++)
            {
                byte A00 = *pPixelA0,
                     R00 = *pPixelR0,
                     G00 = *pPixelG0,
                     B00 = *pPixelB0,
                     A10 = *pPixelA1,
                     R10 = *pPixelR1,
                     G10 = *pPixelG1,
                     B10 = *pPixelB1,
                     A01, R01, G01, B01, A11, R11, G11, B11;

                if (X < SourceW)
                {
                    A01 = A00;
                    R01 = R00;
                    G01 = G00;
                    B01 = B00;

                    A11 = A10;
                    R11 = R10;
                    G11 = G10;
                    B11 = B10;
                }
                else
                {
                    A01 = *(pPixelA0 + 1);
                    R01 = *(pPixelR0 + 1);
                    G01 = *(pPixelG0 + 1);
                    B01 = *(pPixelB0 + 1);

                    A11 = *(pPixelA1 + 1);
                    R11 = *(pPixelR1 + 1);
                    G11 = *(pPixelG1 + 1);
                    B11 = *(pPixelB1 + 1);
                }

                float IFracX = 1f - FracX,
                      IFxIFy = IFracX * IFracY,
                      IFxFy = IFracX * FracY,
                      FxIFy = FracX * IFracY,
                      FxFy = FracX * FracY;

                *pDestA++ = (byte)(A00 * IFxIFy + A01 * FxIFy + A10 * IFxFy + A11 * FxFy);
                *pDestR++ = (byte)(R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy);
                *pDestG++ = (byte)(G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy);
                *pDestB++ = (byte)(B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;

                    X++;

                    pPixelA0++;
                    pPixelR0++;
                    pPixelG0++;
                    pPixelB0++;

                    pPixelA1++;
                    pPixelR1++;
                    pPixelG1++;
                    pPixelB1++;
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
            byte* PixelA = (byte*)Context.ScanA + Offset,
                  PixelR = (byte*)Context.ScanR + Offset,
                  PixelG = (byte*)Context.ScanG + Offset,
                  PixelB = (byte*)Context.ScanB + Offset;

            while (X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac)))
            {
                pDest++->Override(*PixelA, *PixelR, *PixelG, *PixelB);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    PixelA++;
                    PixelR++;
                    PixelG++;
                    PixelB++;
                    X++;
                }
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
            byte* pPixelA = (byte*)Context.ScanA + Offset,
                  pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            ContourData Data = Current.Value;

            void OverlayHandler()
            {
                byte* pTempPixelA = pPixelA,
                      pTempPixelR = pPixelR,
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
                    pTempPixelA += Offset;
                    pTempPixelR += Offset;
                    pTempPixelG += Offset;
                    pTempPixelB += Offset;

                    for (int j = Sx; j <= Ex; j++)
                    {
                        PixelHelper.Overlay(ref pTempPixelA, ref pTempPixelR, ref pTempPixelG, ref pTempPixelB, Color.A, Color.R, Color.G, Color.B);
                        pTempPixelA++;
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
                pPixelA += Offset;
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
            byte* pPixelA = (byte*)Context.ScanA + Offset,
                  pPixelR = (byte*)Context.ScanR + Offset,
                  pPixelG = (byte*)Context.ScanG + Offset,
                  pPixelB = (byte*)Context.ScanB + Offset;

            for (int j = 0; j < Height; j++)
            {
                Source.Operator.ScanLineOverlayTo(X, Y + j, Width, pPixelA, pPixelR, pPixelG, pPixelB);
                pPixelA += Stride;
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
            byte* pSeedA, pSeedR, pSeedG, pSeedB,
                  pPixelA, pPixelR, pPixelG, pPixelB;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);

                pSeedA = (byte*)Context.ScanA + Offset;
                pSeedR = (byte*)Context.ScanR + Offset;
                pSeedG = (byte*)Context.ScanG + Offset;
                pSeedB = (byte*)Context.ScanB + Offset;

                // Find Right Bound
                pPixelA = pSeedA;
                pPixelR = pSeedR;
                pPixelG = pSeedG;
                pPixelB = pSeedB;
                while (X < Width && !Predicate(X, Y, *pPixelA, *pPixelR, *pPixelG, *pPixelB))
                {
                    X++;
                    pPixelA++;
                    pPixelR++;
                    pPixelG++;
                    pPixelB++;
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                pPixelA = pSeedA - 1;
                pPixelR = pSeedR - 1;
                pPixelG = pSeedG - 1;
                pPixelB = pSeedB - 1;
                while (-1 < X && !Predicate(X, Y, *pPixelA, *pPixelR, *pPixelG, *pPixelB))
                {
                    X--;
                    pPixelA--;
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
                pSeedA = (byte*)Context.ScanA + Offset;
                pSeedR = (byte*)Context.ScanR + Offset;
                pSeedG = (byte*)Context.ScanG + Offset;
                pSeedB = (byte*)Context.ScanB + Offset;
                if (-1 < Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, *pSeedA, *pSeedR, *pSeedG, *pSeedB))
                        {
                            NeedFill = true;
                            X++;
                            pSeedA++;
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
                pSeedA = (byte*)Context.ScanA + Offset;
                pSeedR = (byte*)Context.ScanR + Offset;
                pSeedG = (byte*)Context.ScanG + Offset;
                pSeedB = (byte*)Context.ScanB + Offset;
                if (0 <= Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, *pSeedA, *pSeedR, *pSeedG, *pSeedB))
                        {
                            NeedFill = true;
                            X++;
                            pSeedA++;
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
