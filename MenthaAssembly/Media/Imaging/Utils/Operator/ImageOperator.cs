using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator<T> : IImageOperator<T>
        where T : unmanaged, IPixel
    {
        public IImageContext<T> Context { get; }
        IImageContext IImageOperator.Context => this.Context;

        public T GetPixel(int X, int Y)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            return *(T*)((byte*)Context.Scan0 + Offset);
        }
        IPixel IImageOperator.GetPixel(int X, int Y)
            => this.GetPixel(X, Y);

        public void SetPixel(int X, int Y, T Pixel)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            *(T*)((byte*)Context.Scan0 + Offset) = Pixel;
        }
        void IImageOperator.SetPixel(int X, int Y, IPixel Pixel)
            => this.SetPixel(X, Y, Pixel.ToPixel<T>());

        public ImageOperator(IImageContext<T> Context)
        {
            this.Context = Context;
        }

        public void ScanLineOverride(int X, int Y, int Length, T Color)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixels = (byte*)Context.Scan0 + Offset;

            T* pPixel0 = (T*)pPixels;
            for (int i = 0; i < Length; i++)
                *pPixel0++ = Color;
        }
        void IImageOperator.ScanLineOverride(int X, int Y, int Length, IPixel Color)
            => this.ScanLineOverride(X, Y, Length, Color.ToPixel<T>());
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDest)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset),
               pPixelDest = (T*)pDest;
            for (int i = 0; i < Length; i++)
                *pPixelDest++ = *pPixels++;
        }
        public void ScanLineOverrideTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                pDest++->Override(pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
            }
        }
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;
                pPixels++;
            }
        }
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = pPixels->A;
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;
                pPixels++;
            }
        }

        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDest)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset),
               pPixelDest = (T*)pDest;

            pPixelDest += Length - 1;
            for (int i = 0; i < Length; i++)
                *pPixelDest-- = *pPixels++;
        }
        public void ScanLineReverseOverrideTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

            pDest += Length - 1;
            for (int i = 0; i < Length; i++)
            {
                pDest--->Override(pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
            }
        }
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

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
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

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

        public void ScanLineOverlay(int X, int Y, int Length, T Color)
        {
            if (Color.A is byte.MinValue || Color.A is byte.MaxValue)
            {
                ScanLineOverride(X, Y, Length, Color);
                return;
            }

            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)Context.Scan0 + Offset;

            for (int i = 0; i < Length; i++)
                pPixels++->Overlay(Color.A, Color.R, Color.G, Color.B);
        }
        void IImageOperator.ScanLineOverlay(int X, int Y, int Length, IPixel Color)
            => this.ScanLineOverlay(X, Y, Length, Color.ToPixel<T>());
        public void ScanLineOverlayTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                pDest++->Overlay(pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
            }
        }
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                PixelHelper.Overlay(ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);

                pDestR++;
                pDestG++;
                pDestB++;
                pPixels++;
            }
        }
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                PixelHelper.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);

                pDestA++;
                pDestR++;
                pDestG++;
                pDestB++;
                pPixels++;
            }
        }

        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDest)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset),
               pPixelDest = (T*)pDest;

            for (int i = 0; i < Length; i++)
            {
                *pPixelDest++ = *pPixels;

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    pPixels++;
                }
            }
        }
        public void ScanLineNearestResizeTo<T2>(float FracX, float Step, int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

            for (int i = 0; i < Length; i++)
            {
                pDest++->Override(pPixels->A, pPixels->R, pPixels->G, pPixels->B);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    pPixels++;
                }
            }
        }
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    pPixels++;
                }
            }
        }
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = pPixels->A;
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    pPixels++;
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
            byte* pData0 = (byte*)Context.Scan0 + Offset;
            T* pPixels0 = (T*)pData0,
               pPixels1 = (T*)(pData0 + SourceStride);

            float IFracY = 1f - FracY;
            int SourceW = Context.Width;
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

                float IFracX = 1f - FracX,
                      IFxIFy = IFracX * IFracY,
                      IFxFy = IFracX * FracY,
                      FxIFy = FracX * IFracY,
                      FxFy = FracX * FracY;

                pDest++->Override((byte)(p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy),
                                  (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy),
                                  (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy),
                                  (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy));

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;

                    X++;
                    pPixels0++;
                    pPixels1++;
                }
            }
        }
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Context.Stride,
                 Offset = SourceStride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pData0 = (byte*)Context.Scan0 + Offset;
            T* pPixels0 = (T*)pData0,
               pPixels1 = (T*)(pData0 + SourceStride);

            float IFracY = 1f - FracY;
            int SourceW = Context.Width;
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

                float IFracX = 1f - FracX,
                      IFxIFy = IFracX * IFracY,
                      IFxFy = IFracX * FracY,
                      FxIFy = FracX * IFracY,
                      FxFy = FracX * FracY;

                *pDestR++ = (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy);
                *pDestG++ = (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy);
                *pDestB++ = (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;

                    X++;
                    pPixels0++;
                    pPixels1++;
                }
            }
        }
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Context.Stride,
                 Offset = SourceStride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pData0 = (byte*)Context.Scan0 + Offset;
            T* pPixels0 = (T*)pData0,
               pPixels1 = (T*)(pData0 + SourceStride);

            float IFracY = 1f - FracY;
            int SourceW = Context.Width;
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

                float IFracX = 1f - FracX,
                      IFxIFy = IFracX * IFracY,
                      IFxFy = IFracX * FracY,
                      FxIFy = FracX * IFracY,
                      FxFy = FracX * FracY;

                *pDestA++ = (byte)(p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy);
                *pDestR++ = (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy);
                *pDestG++ = (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy);
                *pDestB++ = (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;

                    X++;
                    pPixels0++;
                    pPixels1++;
                }
            }
        }

        public void ScanLineNearestResizeTo(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref byte* pDest)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset),
               pDestPixel = (T*)pDest;

            int Skip = sizeof(T);

            while (X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac)))
            {
                *pDestPixel++ = *pPixels;
                pDest += Skip;

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    pPixels++;
                    X++;
                }
            }
        }
        public void ScanLineNearestResizeTo<T2>(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref T2* pDest) 
            where T2 : unmanaged, IPixel
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

            while (X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac)))
            {
                pDest++->Override(pPixels->A, pPixels->R, pPixels->G, pPixels->B);

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    pPixels++;
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
            byte* pPixels = (byte*)Context.Scan0 + Offset;

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

                pPixels += Context.Stride * (TempY - Y);
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
            byte* pPixels = (byte*)Context.Scan0 + Offset;

            for (int j = 0; j < Height; j++)
            {
                Source.Operator.ScanLineOverlayTo(X, Y + j, Width, (T*)pPixels);
                pPixels += Stride;
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
            T* pSeed, pPixels;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
                pSeed = (T*)((byte*)Context.Scan0 + Offset);
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

                Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
                pSeed = (T*)((byte*)Context.Scan0 + Offset);
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

                Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
                pSeed = (T*)((byte*)Context.Scan0 + Offset);
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
