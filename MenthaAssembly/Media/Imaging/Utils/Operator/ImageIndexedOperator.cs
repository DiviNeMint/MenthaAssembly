using MenthaAssembly.Media.Imaging.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageIndexedOperator<Pixel, Struct> : IImageOperator<Pixel>
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        public PixelOperator<Pixel> PixelOperator { get; }

        private ImageIndexedOperator()
        {
            PixelOperator = PixelOperator<Pixel>.GetOperator();
        }

        public Pixel ToPixel(byte A, byte R, byte G, byte B)
            => PixelOperator.ToPixel(A, R, G, B);

        public Pixel GetPixel(IImageContext Source, int X, int Y)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * (long)Y + (XBits >> 3);

            IPixelIndexed Indexed = *(Struct*)((byte*)Source.Scan0 + Offset) as IPixelIndexed;
            return (Pixel)Source.Palette[Indexed[XBits % Indexed.Length]];
        }

        public void SetPixel(IImageContext Source, int X, int Y, Pixel Pixel)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * (long)Y + (XBits >> 3);

            Struct* sScan = (Struct*)((byte*)Source.Scan0 + Offset);
            IPixelIndexed Indexed = *sScan as IPixelIndexed;

            int Index = Source.Palette.IndexOf(Pixel);
            if (Index == -1)
            {
                if ((1 << Indexed.BitsPerPixel) <= Source.Palette.Count)
                    throw new IndexOutOfRangeException("Palette is full.");

                Index = Source.Palette.Count;
                Source.Palette.Add(Pixel);
            }

            Indexed[XBits % Indexed.Length] = Index;
            *sScan = (Struct)Indexed;
        }

        public void ScanLineCopy(IImageContext Source, int X, int Y, int Length, byte* pDest)
            => ScanLineCopy(Source, X, Y, Length, pDest, PixelOperator);
        public void ScanLineCopy<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    Operator.Override(ref pDest, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDest++;
                }

                pStructs++;
            }
        }
        public void ScanLineCopy3(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;
                }

                pStructs++;
            }
        }
        public void ScanLineCopy4(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    *pDestA++ = Pixel.A;
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;
                }

                pStructs++;
            }
        }

        public void ScanLineOverlay(IImageContext Destination, int X, int Y, int Length, Pixel Color)
        {

        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
        }

        public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color, int OffsetX, int OffsetY)
        {

        }

        public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
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

            long XBits,
                 Offset;
            int X, Y, SaveX, XBitIndex, Rx, Lx;

            Struct* pSeed, pStructs;
            IPixelIndexed Indexed;
            IPixel Pixel;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                XBits = (long)X * Source.BitsPerPixel;
                Offset = Source.Stride * Y + (XBits >> 3);

                pSeed = (Struct*)((byte*)Source.Scan0 + Offset);
                pStructs = pSeed;

                Indexed = *pStructs as IPixelIndexed;
                XBitIndex = (int)(XBits % Indexed.Length);
                Pixel = Source.Palette[Indexed[XBitIndex]];
                while (X < Width && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
                {
                    X++;
                    XBitIndex++;

                    if (XBitIndex >= Indexed.Length)
                    {
                        XBitIndex = 0;
                        pStructs++;
                        Indexed = *pStructs as IPixelIndexed;
                    }

                    Pixel = Source.Palette[Indexed[XBitIndex]];
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                pStructs = pSeed;
                Indexed = *pStructs as IPixelIndexed;
                XBitIndex = (int)(XBits % Indexed.Length) - 1;
                if (XBitIndex < 0)
                {
                    XBitIndex = Indexed.Length - 1;
                    pStructs--;
                    Indexed = *pStructs as IPixelIndexed;
                }
                Pixel = Source.Palette[Indexed[XBitIndex]];

                pStructs = pSeed - 1;
                while (-1 < X && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
                {
                    X--;
                    XBitIndex--;

                    if (XBitIndex < 0)
                    {
                        XBitIndex = Indexed.Length - 1;
                        pStructs--;
                        Indexed = *pStructs as IPixelIndexed;
                    }
                    Pixel = Source.Palette[Indexed[XBitIndex]];
                }

                Lx = X + 1;

                // Log Region
                Contour[Y].Union(Lx, Rx);

                // Lower ScanLine's Seed
                bool NeedFill = false;
                X = Lx;
                Y++;

                XBits = (long)X * Source.BitsPerPixel;
                Offset = Source.Stride * Y + (XBits >> 3);

                pSeed = (Struct*)((byte*)Source.Scan0 + Offset);

                Indexed = *pSeed as IPixelIndexed;
                XBitIndex = (int)(XBits % Indexed.Length);
                Pixel = Source.Palette[Indexed[XBitIndex]];

                if (-1 < Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
                        {
                            NeedFill = true;
                            X++;
                            XBitIndex++;

                            if (XBitIndex >= Indexed.Length)
                            {
                                XBitIndex = 0;
                                pSeed++;
                                Indexed = *pSeed as IPixelIndexed;
                            }

                            Pixel = Source.Palette[Indexed[XBitIndex]];
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

                Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
                pSeed = (Struct*)((byte*)Source.Scan0 + Offset);
                if (0 <= Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
                        {
                            NeedFill = true;
                            X++;
                            XBitIndex++;

                            if (XBitIndex >= Indexed.Length)
                            {
                                XBitIndex = 0;
                                pSeed++;
                                Indexed = *pSeed as IPixelIndexed;
                            }

                            Pixel = Source.Palette[Indexed[XBitIndex]];
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
            string Key = $"{typeof(Pixel).Name}_{typeof(Struct).Name}";
            if (ImageOperators.TryGetValue(Key, out IImageOperator IOperator))
                return (IImageOperator<Pixel>)IOperator;

            IImageOperator<Pixel> Operator = new ImageIndexedOperator<Pixel, Struct>();
            ImageOperators.AddOrUpdate(Key, Operator, (k, o) => Operator);

            return Operator;
        }

    }
}
