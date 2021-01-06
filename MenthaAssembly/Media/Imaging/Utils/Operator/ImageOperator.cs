using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator<Pixel> : IImageOperator<Pixel>
        where Pixel : unmanaged, IPixel
    {
        public PixelOperator<Pixel> PixelOperator { get; }

        private ImageOperator()
        {
            PixelOperator = PixelOperator<Pixel>.GetOperator();
        }

        public Pixel ToPixel(byte A, byte R, byte G, byte B)
            => PixelOperator.ToPixel(A, R, G, B);

        public Pixel GetPixel(IImageContext Source, int X, int Y)
        {
            long Offset = Source.Stride * (long)Y + ((X * Source.BitsPerPixel) >> 3);
            return *(Pixel*)((byte*)Source.Scan0 + Offset);
        }

        public void SetPixel(IImageContext Source, int X, int Y, Pixel Pixel)
        {
            long Offset = Source.Stride * (long)Y + ((X * Source.BitsPerPixel) >> 3);
            *(Pixel*)((byte*)Source.Scan0 + Offset) = Pixel;
        }

        public void ScanLineCopy(IImageContext Source, int X, int Y, int Length, byte* pDest)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            Pixel* pPixels = (Pixel*)((byte*)Source.Scan0 + Offset),
                   pPixelDest = (Pixel*)pDest;
            for (int i = 0; i < Length; i++)
                *pPixelDest++ = *pPixels++;
        }
        public void ScanLineCopy<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            Pixel* pPixels = (Pixel*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                Operator.Override(ref pDest, pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
                pDest++;
            }
        }
        public void ScanLineCopy3(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            Pixel* pPixels = (Pixel*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;
                pPixels++;
            }
        }
        public void ScanLineCopy4(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            Pixel* pPixels = (Pixel*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = pPixels->A;
                *pDestR++ = pPixels->R;
                *pDestG++ = pPixels->G;
                *pDestB++ = pPixels->B;
                pPixels++;
            }
        }

        public void ScanLineOverlay(IImageContext Destination, int X, int Y, int Length, Pixel Color)
        {
            long Offset = Destination.Stride * Y + ((X * Destination.BitsPerPixel) >> 3);
            byte* pPixels = (byte*)Destination.Scan0 + Offset;

            if (Color.A == byte.MinValue || Color.A == byte.MaxValue)
            {
                Pixel* pPixel0 = (Pixel*)pPixels;
                for (int i = 0; i < Length; i++)
                    *pPixel0++ = Color;
            }
            else
            {
                for (int i = 0; i < Length; i++)
                {
                    PixelOperator.Overlay(ref pPixels, Color.A, Color.R, Color.G, Color.B);
                    pPixels++;
                }
            }
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            Pixel* pPixels = (Pixel*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                Operator.Overlay(ref pDest, pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pPixels++;
                pDest++;
            }
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            Pixel* pPixels = (Pixel*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                Operator.Overlay(ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pDestR++;
                pDestG++;
                pDestB++;
                pPixels++;
            }
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
            Pixel* pPixels = (Pixel*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length; i++)
            {
                Operator.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                pDestA++;
                pDestR++;
                pDestG++;
                pDestB++;
                pPixels++;
            }
        }

        public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color, int OffsetX, int OffsetY)
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

            Action OverlayHandler = Color.A == byte.MinValue || Color.A == byte.MaxValue ?
                new Action(() =>
                {
                    Pixel* pTempPixels = (Pixel*)pPixels;
                    int CurrentX = 0;
                    for (int i = 0; i < Data.Count; i++)
                    {
                        int Sx = Math.Max(Data[i++] + OffsetX, 0),
                            Ex = Math.Min(Data[i] + OffsetX, MaxX);

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
                    byte* pTempPixels = pPixels;
                    int CurrentX = 0;
                    for (int i = 0; i < Data.Count; i++)
                    {
                        int Sx = Math.Max(Data[i++] + OffsetX, 0),
                            Ex = Math.Min(Data[i] + OffsetX, MaxX);

                        if (MaxX < Sx)
                            return;

                        pTempPixels += ((Sx - CurrentX) * Destination.BitsPerPixel) >> 3;
                        for (int j = Sx; j <= Ex; j++)
                        {
                            PixelOperator.Overlay(ref pTempPixels, Color.A, Color.R, Color.G, Color.B);
                            pTempPixels++;
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

                pPixels += Destination.Stride * (TempY - Y);
                Y = TempY;
                Data = Current.Value;

                OverlayHandler();
            }
        }

        public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
            long Offset = Destination.Stride * Y + (((long)X * Destination.BitsPerPixel) >> 3);
            byte* pPixels = (byte*)Destination.Scan0 + Offset;
            Source.BlockOverlayTo<Pixel>(OffsetX, OffsetY, Width, Height, pPixels, Destination.Stride);
        }

        public ImageContour FindBound(IImageContext Source, int SeedX, int SeedY, ImagePredicate Predicate)
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
            Pixel* pSeed, pPixels;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Offset = Source.Stride * Y + ((X * Source.BitsPerPixel) >> 3);
                pSeed = (Pixel*)((byte*)Source.Scan0 + Offset);
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
                pSeed = (Pixel*)((byte*)Source.Scan0 + Offset);
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
                pSeed = (Pixel*)((byte*)Source.Scan0 + Offset);
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

        private static readonly ConcurrentDictionary<string, IImageOperator> ImageOperators = new ConcurrentDictionary<string, IImageOperator>();
        public static IImageOperator<Pixel> GetOperator()
        {
            string Key = $"{typeof(Pixel).Name}";
            if (ImageOperators.TryGetValue(Key, out IImageOperator IOperator))
                return (IImageOperator<Pixel>)IOperator;

            IImageOperator<Pixel> Operator = new ImageOperator<Pixel>();
            ImageOperators.AddOrUpdate(Key, Operator, (k, o) => Operator);

            return Operator;
        }

    }
}
