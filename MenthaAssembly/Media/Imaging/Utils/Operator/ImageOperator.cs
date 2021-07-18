using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator : IImageOperator
    {
        public static ImageOperator Instance { get; } = new ImageOperator();

        public override T GetPixel<T>(IImageContext Source, int X, int Y)

        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            return *(T*)((byte*)Source.Scan0 + Offset);
        }
        public override void SetPixel<T>(IImageContext Source, int X, int Y, T Pixel)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            *(T*)((byte*)Source.Scan0 + Offset) = Pixel;
        }

        public override void ScanLineOverride<T>(IImageContext Destination, int X, int Y, int Length, T Color)
        {
            long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
            byte* pPixels = (byte*)Destination.Scan0 + Offset;

            T* pPixel0 = (T*)pPixels;
            for (int i = 0; i < Length; i++)
                *pPixel0++ = Color;
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset),
               pPixelDest = (T*)pDest;
            for (int i = 0; i < Length; i++)
                *pPixelDest++ = *pPixels++;
        }
        public override void ScanLineOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                pDest++->Override(pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
            }
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;
                pPixels++;
            }
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = pPixels->A;
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;
                pPixels++;
            }
        }

        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset),
               pPixelDest = (T*)pDest;

            pPixelDest += Length - 1;
            for (int i = 0; i < Length; i++)
                *pPixelDest-- = *pPixels++;
        }
        public override void ScanLineReverseOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

            pDest += Length - 1;
            for (int i = 0; i < Length; i++)
            {
                pDest--->Override(pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
            }
        }
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

            int OffsetToEnd = Length - 1;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            for (int i = 0; i < Length; i++)
            {
                *pDestR-- = pPixels->R;
                *pDestG-- = pPixels->G;
                *pDestB-- = pPixels->B;
                pPixels++;
            }
        }
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

            int OffsetToEnd = Length - 1;
            pDestA += OffsetToEnd;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            for (int i = 0; i < Length; i++)
            {
                *pDestA-- = pPixels->A;
                *pDestR-- = pPixels->R;
                *pDestG-- = pPixels->G;
                *pDestB-- = pPixels->B;
                pPixels++;
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
            T* pPixels = (T*)Destination.Scan0 + Offset;

            for (int i = 0; i < Length; i++)
                pPixels++->Overlay(Color.A, Color.R, Color.G, Color.B);
        }
        public override void ScanLineOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                pDest++->Overlay(pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
            }
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                this.Overlay(ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);

                pDestR++;
                pDestG++;
                pDestB++;
                pPixels++;
            }
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);

                pDestA++;
                pDestR++;
                pDestG++;
                pDestB++;
                pPixels++;
            }
        }

        //public override void ScanLineReverseOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        //{
        //    long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
        //    T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

        //    pDest += Length - 1;
        //    for (int i = 0; i < Length; i++)
        //    {
        //        pDest--->Overlay(pPixels->A, pPixels->R, pPixels->G, pPixels->B);
        //        pPixels++;
        //    }
        //}
        //public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        //{
        //    long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
        //    T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

        //    int OffsetToEnd = Length - 1;
        //    pDestR += OffsetToEnd;
        //    pDestG += OffsetToEnd;
        //    pDestB += OffsetToEnd;

        //    for (int i = 0; i < Length; i++)
        //    {
        //        this.Overlay(ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);

        //        pDestR--;
        //        pDestG--;
        //        pDestB--;
        //        pPixels++;
        //    }
        //}
        //public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        //{
        //    long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
        //    T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

        //    int OffsetToEnd = Length - 1;
        //    pDestA += OffsetToEnd;
        //    pDestR += OffsetToEnd;
        //    pDestG += OffsetToEnd;
        //    pDestB += OffsetToEnd;

        //    for (int i = 0; i < Length; i++)
        //    {
        //        this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);

        //        pDestA--;
        //        pDestR--;
        //        pDestG--;
        //        pDestB--;
        //        pPixels++;
        //    }
        //}

        public override void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset),
               pPixelDest = (T*)pDest;

            int Count = 0;
            for (int i = 0; i < Length; i++)
            {
                *pPixelDest++ = *pPixels;

                Count += Step;
                while (Count >= Max)
                {
                    Count -= Max;
                    pPixels++;
                }
            }
        }
        public override void ScanLineNearestResizeTo<T, T2>(IImageContext Source, int Step, int Max, int X, int Y, int Length, T2* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

            int Count = 0;
            for (int i = 0; i < Length; i++)
            {
                pDest++->Override(pPixels->A, pPixels->R, pPixels->G, pPixels->B);

                Count += Step;
                while (Count >= Max)
                {
                    Count -= Max;
                    pPixels++;
                }
            }
        }
        public override void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

            int Count = 0;
            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;

                Count += Step;
                while (Count >= Max)
                {
                    Count -= Max;
                    pPixels++;
                }
            }
        }
        public override void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Source.Scan0 + Offset);

            int Count = 0;
            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = pPixels->A;
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;

                Count += Step;
                while (Count >= Max)
                {
                    Count -= Max;
                    pPixels++;
                }
            }
        }

        public override void ScanLineBilinearResizeTo<T, T2>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, T2* pDest)
        {
            long SourceStride = Source.Stride,
                 Offset = SourceStride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* pData0 = (byte*)Source.Scan0 + Offset;
            T* pPixels0 = (T*)pData0,
               pPixels1 = (T*)(pData0 + SourceStride);

            int FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max,
                SourceW = Source.Width;
            for (int i = 0; i < Length; i++)
            {
                T p00 = *pPixels0,
                  p10 = *pPixels1,
                  p01, p11;

                if (X < SourceW)
                {
                    p01 = p00;
                    p11 = p10;
                }
                else
                {
                    p01 = *(pPixels0 + 1);
                    p11 = *(pPixels1 + 1);
                }

                int IFracX = Max - FracX,
                    IFxIFy = IFracX * IFracY,
                    IFxFy = IFracX * FracY,
                    FxIFy = FracX * IFracY,
                    FxFy = FracX * FracY;

                pDest++->Override((byte)((p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy) / SqrMax),
                                  (byte)((p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy) / SqrMax),
                                  (byte)((p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy) / SqrMax),
                                  (byte)((p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy) / SqrMax));

                FracX += StepX;
                while (FracX >= Max)
                {
                    FracX -= Max;

                    X++;
                    pPixels0++;
                    pPixels1++;
                }
            }
        }
        public override void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Source.Stride,
                 Offset = SourceStride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* pData0 = (byte*)Source.Scan0 + Offset;
            T* pPixels0 = (T*)pData0,
               pPixels1 = (T*)(pData0 + SourceStride);

            int FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max,
                SourceW = Source.Width;
            for (int i = 0; i < Length; i++)
            {
                T p00 = *pPixels0,
                  p10 = *pPixels1,
                  p01, p11;

                if (X < SourceW)
                {
                    p01 = p00;
                    p11 = p10;
                }
                else
                {
                    p01 = *(pPixels0 + 1);
                    p11 = *(pPixels1 + 1);
                }

                int IFracX = Max - FracX,
                    IFxIFy = IFracX * IFracY,
                    IFxFy = IFracX * FracY,
                    FxIFy = FracX * IFracY,
                    FxFy = FracX * FracY;

                *pDestR++ = (byte)((p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy) / SqrMax);
                *pDestG++ = (byte)((p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy) / SqrMax);
                *pDestB++ = (byte)((p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy) / SqrMax);

                FracX += StepX;
                while (FracX >= Max)
                {
                    FracX -= Max;

                    X++;
                    pPixels0++;
                    pPixels1++;
                }
            }
        }
        public override void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Source.Stride,
                 Offset = SourceStride * Y + ((X * Source.BitsPerPixel) >> 3);
            byte* pData0 = (byte*)Source.Scan0 + Offset;
            T* pPixels0 = (T*)pData0,
               pPixels1 = (T*)(pData0 + SourceStride);

            int FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max,
                SourceW = Source.Width;
            for (int i = 0; i < Length; i++)
            {
                T p00 = *pPixels0,
                  p10 = *pPixels1,
                  p01, p11;

                if (X < SourceW)
                {
                    p01 = p00;
                    p11 = p10;
                }
                else
                {
                    p01 = *(pPixels0 + 1);
                    p11 = *(pPixels1 + 1);
                }

                int IFracX = Max - FracX,
                    IFxIFy = IFracX * IFracY,
                    IFxFy = IFracX * FracY,
                    FxIFy = FracX * IFracY,
                    FxFy = FracX * FracY;

                *pDestA++ = (byte)((p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy) / SqrMax);
                *pDestR++ = (byte)((p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy) / SqrMax);
                *pDestG++ = (byte)((p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy) / SqrMax);
                *pDestB++ = (byte)((p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy) / SqrMax);

                FracX += StepX;
                while (FracX >= Max)
                {
                    FracX -= Max;

                    X++;
                    pPixels0++;
                    pPixels1++;
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
            byte* pPixels = (byte*)Destination.Scan0 + Offset;

            ContourData Data = Current.Value;

            Action OverlayHandler = Color.A == 0 || Color.A == byte.MaxValue ?
                new Action(() =>
                {
                    T* pTempPixels = (T*)pPixels;
                    int CurrentX = 0;
                    for (int i = 0; i < Data.Count; i++)
                    {
                        int Sx = Math.Max(Data[i++] + OffsetX, 0),
                            Ex = Math.Min(Data[i] + OffsetX, MaxX);

                        if (Ex <= Sx)
                            continue;

                        if (MaxX < Sx)
                            return;

                        pTempPixels += Sx - CurrentX;
                        for (int j = Sx; j <= Ex; j++)
                            *pTempPixels++ = Color;

                        CurrentX = Ex + 1;
                    }
                }) :
                () =>
                {
                    T* pTempPixels = (T*)pPixels;
                    int CurrentX = 0;
                    for (int i = 0; i < Data.Count; i++)
                    {
                        int Sx = Math.Max(Data[i++] + OffsetX, 0),
                            Ex = Math.Min(Data[i] + OffsetX, MaxX);

                        if (Ex <= Sx)
                            continue;

                        if (MaxX < Sx)
                            return;

                        pTempPixels += Sx - CurrentX;
                        for (int j = Sx; j <= Ex; j++)
                            pTempPixels++->Overlay(Color.A, Color.R, Color.G, Color.B);

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

                pPixels += Destination.Stride * (TempY - Y);
                Y = TempY;
                Data = Current.Value;

                OverlayHandler();
            }
        }

        public override void BlockOverlay<T>(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
            long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
            byte* pPixels = (byte*)Destination.Scan0 + Offset;
            Source.BlockOverlayTo<T>(OffsetX, OffsetY, Width, Height, pPixels, Destination.Stride);
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
            T* pSeed, pPixels;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
                pSeed = (T*)((byte*)Source.Scan0 + Offset);
                pPixels = pSeed;

                // Find Right Bound
                while (X < Width && !Predicate(X, Y, pPixels->A, pPixels->R, pPixels->G, pPixels->B))
                {
                    X++;
                    pPixels++;
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                pPixels = pSeed - 1;
                while (-1 < X && !Predicate(X, Y, pPixels->A, pPixels->R, pPixels->G, pPixels->B))
                {
                    X--;
                    pPixels--;
                }

                Lx = X + 1;

                // Log Region
                Contour[Y].Union(Lx, Rx);

                // Lower ScanLine's Seed
                bool NeedFill = false;
                X = Lx;
                Y++;

                Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
                pSeed = (T*)((byte*)Source.Scan0 + Offset);
                if (-1 < Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, pSeed->A, pSeed->R, pSeed->G, pSeed->B))
                        {
                            NeedFill = true;
                            X++;
                            pSeed++;
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
                pSeed = (T*)((byte*)Source.Scan0 + Offset);
                if (0 <= Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, pSeed->A, pSeed->R, pSeed->G, pSeed->B))
                        {
                            NeedFill = true;
                            X++;
                            pSeed++;
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
