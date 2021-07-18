using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator3 : IImageOperator
    {
        public static ImageOperator3 Instance { get; } = new ImageOperator3();

        public override T GetPixel<T>(IImageContext Source, int X, int Y)
        {
            T Pixel = default;
            long Offset = Source.Stride * Y + X;
            Pixel.Override(byte.MaxValue,
                           *((byte*)Source.ScanR + Offset),
                           *((byte*)Source.ScanG + Offset),
                           *((byte*)Source.ScanB + Offset));
            return Pixel;
        }
        public override void SetPixel<T>(IImageContext Source, int X, int Y, T Pixel)
        {
            long Offset = Source.Stride * Y + X;
            *((byte*)Source.ScanR + Offset) = Pixel.R;
            *((byte*)Source.ScanG + Offset) = Pixel.G;
            *((byte*)Source.ScanB + Offset) = Pixel.B;
        }

        public override void ScanLineOverride<T>(IImageContext Destination, int X, int Y, int Length, T Color)
        {
            long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
            byte* pPixelR = (byte*)Destination.ScanR + Offset,
                  pPixelG = (byte*)Destination.ScanG + Offset,
                  pPixelB = (byte*)Destination.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pPixelR++ = Color.R;
                *pPixelG++ = Color.G;
                *pPixelB++ = Color.B;
            }
        }
        public override void ScanLineOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
                pDest++->Override(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = *PixelR++;
                *pDestG++ = *PixelG++;
                *pDestB++ = *PixelB++;
            }
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = byte.MaxValue;
                *pDestR++ = *PixelR++;
                *pDestG++ = *PixelG++;
                *pDestB++ = *PixelB++;
            }
        }

        public override void ScanLineReverseOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            pDest += Length - 1;

            for (int i = 0; i < Length; i++)
                pDest--->Override(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        }
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

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
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

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

        public override void ScanLineOverlay<T>(IImageContext Destination, int X, int Y, int Length, T Color)
        {
            if (Color.A is byte.MinValue || Color.A is byte.MaxValue)
            {
                ScanLineOverride(Destination, X, Y, Length, Color);
                return;
            }

            long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
            byte* pPixelR = (byte*)Destination.ScanR + Offset,
                  pPixelG = (byte*)Destination.ScanG + Offset,
                  pPixelB = (byte*)Destination.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                this.Overlay(ref pPixelR, ref pPixelG, ref pPixelB, Color.A, Color.R, Color.G, Color.B);
                pPixelR++;
                pPixelG++;
                pPixelB++;
            }
        }
        public override void ScanLineOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
                pDest++->Overlay(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                this.Overlay(ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                pDestR++;
                pDestG++;
                pDestB++;
            }
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                pDestA++;
                pDestR++;
                pDestG++;
                pDestB++;
            }
        }

        //public override void ScanLineReverseOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        //{
        //    long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
        //    byte* PixelR = (byte*)Source.ScanR + Offset,
        //          PixelG = (byte*)Source.ScanG + Offset,
        //          PixelB = (byte*)Source.ScanB + Offset;

        //    pDest += Length - 1;

        //    for (int i = 0; i < Length; i++)
        //        pDest--->Overlay(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        //}
        //public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        //{
        //    long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
        //    byte* PixelR = (byte*)Source.ScanR + Offset,
        //          PixelG = (byte*)Source.ScanG + Offset,
        //          PixelB = (byte*)Source.ScanB + Offset;

        //    int OffsetToEnd = Length - 1;
        //    pDestR += OffsetToEnd;
        //    pDestG += OffsetToEnd;
        //    pDestB += OffsetToEnd;

        //    for (int i = 0; i < Length; i++)
        //    {
        //        this.Overlay(ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        //        pDestR--;
        //        pDestG--;
        //        pDestB--;
        //    }
        //}
        //public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        //{
        //    long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
        //    byte* PixelR = (byte*)Source.ScanR + Offset,
        //          PixelG = (byte*)Source.ScanG + Offset,
        //          PixelB = (byte*)Source.ScanB + Offset;

        //    int OffsetToEnd = Length - 1;
        //    pDestA += OffsetToEnd;
        //    pDestR += OffsetToEnd;
        //    pDestG += OffsetToEnd;
        //    pDestB += OffsetToEnd;

        //    for (int i = 0; i < Length; i++)
        //    {
        //        this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        //        pDestA--;
        //        pDestR--;
        //        pDestG--;
        //        pDestB--;
        //    }
        //}

        public override void ScanLineNearestResizeTo<T, T2>(IImageContext Source, int Step, int Max, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            int Count = 0;
            for (int i = 0; i < Length; i++)
            {
                pDest++->Override(byte.MaxValue, *PixelR, *PixelG, *PixelB);

                Count += Step;
                while (Count >= Max)
                {
                    Count -= Max;
                    PixelR++;
                    PixelG++;
                    PixelB++;
                }
            }
        }
        public override void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            int Count = 0;
            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = *PixelR;
                *pDestG++ = *PixelG;
                *pDestB++ = *PixelB;

                Count += Step;
                while (Count >= Max)
                {
                    Count -= Max;
                    PixelR++;
                    PixelG++;
                    PixelB++;
                }
            }
        }
        public override void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            int Count = 0;
            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = byte.MaxValue;
                *pDestR++ = *PixelR;
                *pDestG++ = *PixelG;
                *pDestB++ = *PixelB;

                Count += Step;
                while (Count >= Max)
                {
                    Count -= Max;
                    PixelR++;
                    PixelG++;
                    PixelB++;
                }
            }
        }

        public override void ScanLineBilinearResizeTo<T, T2>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, T2* pDest)
        {
            long SourceStride = Source.Stride,
                 Offset = SourceStride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* pPixelR0 = (byte*)Source.ScanR + Offset,
                  pPixelG0 = (byte*)Source.ScanG + Offset,
                  pPixelB0 = (byte*)Source.ScanB + Offset,
                  pPixelR1 = pPixelR0 + SourceStride,
                  pPixelG1 = pPixelG0 + SourceStride,
                  pPixelB1 = pPixelB0 + SourceStride;

            int FracX = 0,
                 IFracY = Max - FracY,
                 SqrMax = Max * Max,
                 SourceW = Source.Width;
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

                int IFracX = Max - FracX,
                    IFxIFy = IFracX * IFracY,
                    IFxFy = IFracX * FracY,
                    FxIFy = FracX * IFracY,
                    FxFy = FracX * FracY;

                pDest++->Override(byte.MaxValue,
                                  (byte)((R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy) / SqrMax),
                                  (byte)((G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy) / SqrMax),
                                  (byte)((B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy) / SqrMax));

                FracX += StepX;
                while (FracX >= Max)
                {
                    FracX -= Max;

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
        public override void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Source.Stride,
                 Offset = SourceStride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* pPixelR0 = (byte*)Source.ScanR + Offset,
                  pPixelG0 = (byte*)Source.ScanG + Offset,
                  pPixelB0 = (byte*)Source.ScanB + Offset,
                  pPixelR1 = pPixelR0 + SourceStride,
                  pPixelG1 = pPixelG0 + SourceStride,
                  pPixelB1 = pPixelB0 + SourceStride;

            int FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max,
                SourceW = Source.Width;
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

                int IFracX = Max - FracX,
                    IFxIFy = IFracX * IFracY,
                    IFxFy = IFracX * FracY,
                    FxIFy = FracX * IFracY,
                    FxFy = FracX * FracY;

                *pDestR++ = (byte)((R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy) / SqrMax);
                *pDestG++ = (byte)((G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy) / SqrMax);
                *pDestB++ = (byte)((B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy) / SqrMax);

                FracX += StepX;
                while (FracX >= Max)
                {
                    FracX -= Max;

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
        public override void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Source.Stride,
                 Offset = SourceStride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* pPixelR0 = (byte*)Source.ScanR + Offset,
                  pPixelG0 = (byte*)Source.ScanG + Offset,
                  pPixelB0 = (byte*)Source.ScanB + Offset,
                  pPixelR1 = pPixelR0 + SourceStride,
                  pPixelG1 = pPixelG0 + SourceStride,
                  pPixelB1 = pPixelB0 + SourceStride;

            int FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max,
                SourceW = Source.Width;
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

                int IFracX = Max - FracX,
                    IFxIFy = IFracX * IFracY,
                    IFxFy = IFracX * FracY,
                    FxIFy = FracX * IFracY,
                    FxFy = FracX * FracY;

                *pDestA++ = byte.MaxValue;
                *pDestR++ = (byte)((R00 * IFxIFy + R01 * FxIFy + R10 * IFxFy + R11 * FxFy) / SqrMax);
                *pDestG++ = (byte)((G00 * IFxIFy + G01 * FxIFy + G10 * IFxFy + G11 * FxFy) / SqrMax);
                *pDestB++ = (byte)((B00 * IFxIFy + B01 * FxIFy + B10 * IFxFy + B11 * FxFy) / SqrMax);

                FracX += StepX;
                while (FracX >= Max)
                {
                    FracX -= Max;

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

        public override void ContourOverlay<T>(IImageContext Destination, ImageContour Contour, T Color, int OffsetX, int OffsetY)
        {
            IEnumerator<KeyValuePair<int, ContourData>> Enumerator = Contour.GetEnumerator();
            if (!Enumerator.MoveNext())
                return;

            int MaxX = Destination.Width - 1,
                MaxY = Destination.Height - 1;
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

            long Offset = Destination.Stride * Y;
            byte* pPixelR = (byte*)Destination.ScanR + Offset,
                  pPixelG = (byte*)Destination.ScanG + Offset,
                  pPixelB = (byte*)Destination.ScanB + Offset;

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

                    Offset = ((Sx - CurrentX) * Destination.BitsPerPixel) >> 3;
                    pTempPixelR += Offset;
                    pTempPixelG += Offset;
                    pTempPixelB += Offset;

                    for (int j = Sx; j <= Ex; j++)
                    {
                        this.Overlay(ref pTempPixelR, ref pTempPixelG, ref pTempPixelB, Color.A, Color.R, Color.G, Color.B);
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

                Offset = Destination.Stride * (TempY - Y);
                pPixelR += Offset;
                pPixelG += Offset;
                pPixelB += Offset;

                Y = TempY;
                Data = Current.Value;

                OverlayHandler();
            }
        }

        public override void BlockOverlay<T>(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
            long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
            byte* pPixelR = (byte*)Destination.ScanR + Offset,
                  pPixelG = (byte*)Destination.ScanG + Offset,
                  pPixelB = (byte*)Destination.ScanB + Offset;

            Source.BlockOverlayTo<T>(OffsetX, OffsetY, Width, Height, pPixelR, pPixelG, pPixelB, Destination.Stride);
        }

        public override ImageContour FindBound<T>(IImageContext Source, int SeedX, int SeedY, ImagePredicate Predicate)
        {
            int Width = Source.Width,
                Height = Source.Height;

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

                Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);

                pSeedR = (byte*)Source.ScanR + Offset;
                pSeedG = (byte*)Source.ScanG + Offset;
                pSeedB = (byte*)Source.ScanB + Offset;

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

                Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
                pSeedR = (byte*)Source.ScanR + Offset;
                pSeedG = (byte*)Source.ScanG + Offset;
                pSeedB = (byte*)Source.ScanB + Offset;
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

                Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
                pSeedR = (byte*)Source.ScanR + Offset;
                pSeedG = (byte*)Source.ScanG + Offset;
                pSeedB = (byte*)Source.ScanB + Offset;
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
