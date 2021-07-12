﻿using System;
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
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest)
            => ScanLineOverrideTo<T, T>(Source, X, Y, Length, (T*)pDest);
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

        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest)
            => ScanLineReverseOverrideTo<T, T>(Source, X, Y, Length, (T*)pDest);
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

        public override void ScanLineReverseOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            pDest += Length - 1;

            for (int i = 0; i < Length; i++)
                pDest--->Overlay(byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
        }
        public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
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
                this.Overlay(ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                pDestR--;
                pDestG--;
                pDestB--;
            }
        }
        public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
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
                this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                pDestA--;
                pDestR--;
                pDestG--;
                pDestB--;
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

    //internal unsafe class ImageOperator3<Pixel> : IImageOperator<Pixel>
    //    where Pixel : unmanaged, IPixel
    //{
    //    public PixelOperator<Pixel> PixelOperator { get; }

    //    private ImageOperator3()
    //    {
    //        PixelOperator = PixelOperator<Pixel>.GetOperator();
    //    }

    //    public Pixel ToPixel(byte A, byte R, byte G, byte B)
    //        => PixelOperator.ToPixel(A, R, G, B);

    //    public Pixel GetPixel(IImageContext Source, int X, int Y)
    //    {
    //        Pixel Pixel = default;
    //        byte* pPixel = (byte*)&Pixel;
    //        long Offset = Source.Stride * Y + X;
    //        PixelOperator.Override(ref pPixel,
    //                               byte.MaxValue,
    //                               *((byte*)Source.ScanR + Offset),
    //                               *((byte*)Source.ScanG + Offset),
    //                               *((byte*)Source.ScanB + Offset));
    //        return Pixel;
    //    }

    //    public void SetPixel(IImageContext Source, int X, int Y, Pixel Pixel)
    //    {
    //        long Offset = Source.Stride * Y + X;
    //        *((byte*)Source.ScanR + Offset) = Pixel.R;
    //        *((byte*)Source.ScanG + Offset) = Pixel.G;
    //        *((byte*)Source.ScanB + Offset) = Pixel.B;
    //    }

    //    public void ScanLineOverride(IImageContext Destination, int X, int Y, int Length, Pixel Color)
    //    {
    //        long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
    //        byte* pPixelR = (byte*)Destination.ScanR + Offset,
    //              pPixelG = (byte*)Destination.ScanG + Offset,
    //              pPixelB = (byte*)Destination.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            *pPixelR++ = Color.R;
    //            *pPixelG++ = Color.G;
    //            *pPixelB++ = Color.B;
    //        }
    //    }
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDest)
    //        => ScanLineOverrideTo(Source, X, Y, Length, pDest, PixelOperator);
    //    public void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator)
    //    {
    //        long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //        byte* PixelR = (byte*)Source.ScanR + Offset,
    //              PixelG = (byte*)Source.ScanG + Offset,
    //              PixelB = (byte*)Source.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            Operator.Override(ref pDest, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
    //            pDest++;
    //        }
    //    }
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
    //    {
    //        long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //        byte* PixelR = (byte*)Source.ScanR + Offset,
    //              PixelG = (byte*)Source.ScanG + Offset,
    //              PixelB = (byte*)Source.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            *pDestR++ = *PixelR++;
    //            *pDestG++ = *PixelG++;
    //            *pDestB++ = *PixelB++;
    //        }
    //    }
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
    //    {
    //        long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //        byte* PixelR = (byte*)Source.ScanR + Offset,
    //              PixelG = (byte*)Source.ScanG + Offset,
    //              PixelB = (byte*)Source.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            *pDestA++ = byte.MaxValue;
    //            *pDestR++ = *PixelR++;
    //            *pDestG++ = *PixelG++;
    //            *pDestB++ = *PixelB++;
    //        }
    //    }

    //    public void ScanLineOverlay(IImageContext Destination, int X, int Y, int Length, Pixel Color)
    //    {
    //        if (Color.A is byte.MinValue)
    //            return;

    //        if (Color.A is byte.MaxValue)
    //        {
    //            ScanLineOverride(Destination, X, Y, Length, Color);
    //            return;
    //        }

    //        long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
    //        byte* pPixelR = (byte*)Destination.ScanR + Offset,
    //              pPixelG = (byte*)Destination.ScanG + Offset,
    //              pPixelB = (byte*)Destination.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            PixelOperator.Overlay(ref pPixelR, ref pPixelG, ref pPixelB, Color.A, Color.R, Color.G, Color.B);
    //            pPixelR++;
    //            pPixelG++;
    //            pPixelB++;
    //        }
    //    }
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator)
    //    {
    //        long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //        byte* PixelR = (byte*)Source.ScanR + Offset,
    //              PixelG = (byte*)Source.ScanG + Offset,
    //              PixelB = (byte*)Source.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            Operator.Overlay(ref pDest, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
    //            pDest++;
    //        }
    //    }
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator)
    //    {
    //        long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //        byte* PixelR = (byte*)Source.ScanR + Offset,
    //              PixelG = (byte*)Source.ScanG + Offset,
    //              PixelB = (byte*)Source.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            Operator.Overlay(ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
    //            pDestR++;
    //            pDestG++;
    //            pDestB++;
    //        }
    //    }
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator)
    //    {
    //        long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //        byte* PixelR = (byte*)Source.ScanR + Offset,
    //              PixelG = (byte*)Source.ScanG + Offset,
    //              PixelB = (byte*)Source.ScanB + Offset;

    //        for (int i = 0; i < Length; i++)
    //        {
    //            Operator.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
    //            pDestA++;
    //            pDestR++;
    //            pDestG++;
    //            pDestB++;
    //        }
    //    }

    //    public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color, int OffsetX, int OffsetY)
    //    {
    //        IEnumerator<KeyValuePair<int, ContourData>> Enumerator = Contour.GetEnumerator();
    //        if (!Enumerator.MoveNext())
    //            return;

    //        int MaxX = Destination.Width - 1,
    //            MaxY = Destination.Height - 1;
    //        KeyValuePair<int, ContourData> Current = Enumerator.Current;

    //        long Y = Current.Key + OffsetY;
    //        if (MaxY < Y)
    //            return;

    //        while (Y < 0)
    //        {
    //            if (!Enumerator.MoveNext())
    //                return;

    //            Current = Enumerator.Current;
    //            Y = Current.Key + OffsetY;

    //            if (MaxY < Y)
    //                return;
    //        }

    //        long Offset = Destination.Stride * Y;
    //        byte* pPixelR = (byte*)Destination.ScanR + Offset,
    //              pPixelG = (byte*)Destination.ScanG + Offset,
    //              pPixelB = (byte*)Destination.ScanB + Offset;

    //        ContourData Data = Current.Value;

    //        void OverlayHandler()
    //        {
    //            byte* pTempPixelR = pPixelR,
    //                  pTempPixelG = pPixelG,
    //                  pTempPixelB = pPixelB;

    //            int CurrentX = 0;
    //            for (int i = 0; i < Data.Count; i++)
    //            {
    //                int Sx = Math.Max(Data[i++] + OffsetX, 0),
    //                    Ex = Math.Min(Data[i] + OffsetX, MaxX);

    //                if (Ex <= Sx)
    //                    continue;

    //                if (MaxX < Sx)
    //                    return;

    //                Offset = ((Sx - CurrentX) * Destination.BitsPerPixel) >> 3;
    //                pTempPixelR += Offset;
    //                pTempPixelG += Offset;
    //                pTempPixelB += Offset;

    //                for (int j = Sx; j <= Ex; j++)
    //                {
    //                    PixelOperator.Overlay(ref pTempPixelR, ref pTempPixelG, ref pTempPixelB, Color.A, Color.R, Color.G, Color.B);
    //                    pTempPixelR++;
    //                    pTempPixelG++;
    //                    pTempPixelB++;
    //                }
    //                CurrentX = Ex + 1;
    //            }
    //        };

    //        OverlayHandler();
    //        while (Enumerator.MoveNext())
    //        {
    //            Current = Enumerator.Current;

    //            long TempY = Current.Key + OffsetY;
    //            if (MaxY < TempY)
    //                return;

    //            Offset = Destination.Stride * (TempY - Y);
    //            pPixelR += Offset;
    //            pPixelG += Offset;
    //            pPixelB += Offset;

    //            Y = TempY;
    //            Data = Current.Value;

    //            OverlayHandler();
    //        }
    //    }

    //    public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
    //    {
    //        long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
    //        byte* pPixelR = (byte*)Destination.ScanR + Offset,
    //              pPixelG = (byte*)Destination.ScanG + Offset,
    //              pPixelB = (byte*)Destination.ScanB + Offset;

    //        Source.BlockOverlayTo<Pixel>(OffsetX, OffsetY, Width, Height, pPixelR, pPixelG, pPixelB, Destination.Stride);
    //    }

    //    public ImageContour FindBound(IImageContext Source, int SeedX, int SeedY, ImagePredicate Predicate)
    //    {
    //        int Width = Source.Width,
    //            Height = Source.Height;

    //        if (SeedX < 0 || Width <= SeedX ||
    //            SeedY < 0 || Height <= SeedY)
    //            return null;

    //        ImageContour Contour = new ImageContour();
    //        Stack<int> StackX = new Stack<int>(),
    //                   StackY = new Stack<int>();
    //        StackX.Push(SeedX);
    //        StackY.Push(SeedY);

    //        int X, Y, SaveX, Rx, Lx;
    //        long Offset;
    //        byte* pSeedR, pSeedG, pSeedB,
    //              pPixelR, pPixelG, pPixelB;
    //        while (StackX.Count > 0)
    //        {
    //            X = StackX.Pop();
    //            Y = StackY.Pop();
    //            SaveX = X;

    //            Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);

    //            pSeedR = (byte*)Source.ScanR + Offset;
    //            pSeedG = (byte*)Source.ScanG + Offset;
    //            pSeedB = (byte*)Source.ScanB + Offset;

    //            // Find Right Bound
    //            pPixelR = pSeedR;
    //            pPixelG = pSeedG;
    //            pPixelB = pSeedB;
    //            while (X < Width && !Predicate(X, Y, byte.MaxValue, *pPixelR, *pPixelG, *pPixelB))
    //            {
    //                X++;
    //                pPixelR++;
    //                pPixelG++;
    //                pPixelB++;
    //            }

    //            // Find Left Bound
    //            Rx = X - 1;
    //            X = SaveX - 1;

    //            pPixelR = pSeedR - 1;
    //            pPixelG = pSeedG - 1;
    //            pPixelB = pSeedB - 1;
    //            while (-1 < X && !Predicate(X, Y, byte.MaxValue, *pPixelR, *pPixelG, *pPixelB))
    //            {
    //                X--;
    //                pPixelR--;
    //                pPixelG--;
    //                pPixelB--;
    //            }

    //            Lx = X + 1;

    //            // Log Region
    //            Contour[Y].Union(Lx, Rx);

    //            // Lower ScanLine's Seed
    //            bool NeedFill = false;
    //            X = Lx;
    //            Y++;

    //            Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //            pSeedR = (byte*)Source.ScanR + Offset;
    //            pSeedG = (byte*)Source.ScanG + Offset;
    //            pSeedB = (byte*)Source.ScanB + Offset;
    //            if (-1 < Y && Y < Height &&
    //                !Contour.Contain(X, Y))
    //                for (; X <= Rx; X++)
    //                {
    //                    while (X <= Rx && !Predicate(X, Y, byte.MaxValue, *pSeedR, *pSeedG, *pSeedB))
    //                    {
    //                        NeedFill = true;
    //                        X++;
    //                        pSeedR++;
    //                        pSeedG++;
    //                        pSeedB++;
    //                    }

    //                    if (NeedFill)
    //                    {
    //                        StackX.Push(X - 1);
    //                        StackY.Push(Y);
    //                        NeedFill = false;
    //                    }
    //                }

    //            // Upper ScanLine's Seed
    //            NeedFill = false;
    //            X = Lx;
    //            Y -= 2;

    //            Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
    //            pSeedR = (byte*)Source.ScanR + Offset;
    //            pSeedG = (byte*)Source.ScanG + Offset;
    //            pSeedB = (byte*)Source.ScanB + Offset;
    //            if (0 <= Y && Y < Height &&
    //                !Contour.Contain(X, Y))
    //                for (; X <= Rx; X++)
    //                {
    //                    while (X <= Rx && !Predicate(X, Y, byte.MaxValue, *pSeedR, *pSeedG, *pSeedB))
    //                    {
    //                        NeedFill = true;
    //                        X++;
    //                        pSeedR++;
    //                        pSeedG++;
    //                        pSeedB++;
    //                    }

    //                    if (NeedFill)
    //                    {
    //                        StackX.Push(X - 1);
    //                        StackY.Push(Y);
    //                        NeedFill = false;
    //                    }
    //                }
    //        }

    //        return Contour;
    //    }

    //    private static readonly ConcurrentDictionary<string, IImageOperator> ImageOperators = new ConcurrentDictionary<string, IImageOperator>();
    //    public static IImageOperator<Pixel> GetOperator()
    //    {
    //        string Key = $"{typeof(Pixel).Name}";
    //        if (ImageOperators.TryGetValue(Key, out IImageOperator IOperator))
    //            return (IImageOperator<Pixel>)IOperator;

    //        IImageOperator<Pixel> Operator = new ImageOperator3<Pixel>();
    //        ImageOperators.AddOrUpdate(Key, Operator, (k, o) => Operator);

    //        return Operator;
    //    }

    //}
}
