using MenthaAssembly.Media.Imaging.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageIndexedOperator<Struct> : IImageOperator
        where Struct : unmanaged, IPixelBase
    {
        public override T GetPixel<T>(IImageContext Source, int X, int Y)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * (long)Y + (XBits >> 3);

            IPixelIndexed Indexed = *(Struct*)((byte*)Source.Scan0 + Offset) as IPixelIndexed;
            return (T)Source.Palette[Indexed[XBits % Indexed.Length]];
        }
        public override void SetPixel<T>(IImageContext Source, int X, int Y, T Color)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* sScan = (Struct*)((byte*)Source.Scan0 + Offset);
            IPixelIndexed Indexed = *sScan as IPixelIndexed;

            if (!Source.Palette.TryGetOrAdd(Color, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            Indexed[XBits % Indexed.Length] = Index;
            *sScan = (Struct)Indexed;
        }

        public override void ScanLineOverride<T>(IImageContext Destination, int X, int Y, int Length, T Color)
        {
            int XBits = X * Destination.BitsPerPixel;
            long Offset = Destination.Stride * Y + (XBits >> 3);

            Struct* sScan = (Struct*)((byte*)Destination.Scan0 + Offset);
            IPixelIndexed Indexed = *sScan as IPixelIndexed;

            if (!Destination.Palette.TryGetOrAdd(Color, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            int BitIndex = XBits % Indexed.Length;
            for (int i = 0; i < Length;)
            {
                for (; i < Length && BitIndex < Indexed.Length; i++)
                    Indexed[BitIndex++] = Index;

                *sScan++ = (Struct)Indexed;
                BitIndex = 0;
            }
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest)
            => ScanLineOverrideTo<T, T>(Source, X, Y, Length, (T*)pDest);
        public override void ScanLineOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
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
                    pDest++->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                pStructs++;
            }
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
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
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
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

        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest)
            => ScanLineReverseOverrideTo<T, T>(Source, X, Y, Length, (T*)pDest);
        public override void ScanLineReverseOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            pDest += Length - 1;

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    pDest--->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                pStructs++;
            }
        }
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    *pDestR-- = Pixel.R;
                    *pDestG-- = Pixel.G;
                    *pDestB-- = Pixel.B;
                }

                pStructs++;
            }
        }
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestA += OffsetToEnd;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    *pDestA-- = Pixel.A;
                    *pDestR-- = Pixel.R;
                    *pDestG-- = Pixel.G;
                    *pDestB-- = Pixel.B;
                }

                pStructs++;
            }
        }

        public override void ScanLineOverlay<T>(IImageContext Destination, int X, int Y, int Length, T Color)
        {
            if (Color.A is byte.MinValue || Color.A is byte.MaxValue)
            {
                this.ScanLineOverlay(Destination, X, Y, Length, Color);
                return;
            }

            //int XBits = X * Destination.BitsPerPixel;
            //long Offset = Destination.Stride * Y + (XBits >> 3);

            //Struct* sScan = (Struct*)((byte*)Destination.Scan0 + Offset);
            //IPixelIndexed Indexed = *sScan as IPixelIndexed;

            //int BitIndex = XBits % Indexed.Length;
            //for (int i = 0; i < Length;)
            //{
            //    for (; i < Length && BitIndex < Indexed.Length; i++)
            //    {
            //        int Index = Indexed[BitIndex];
            //        IPixel Destination.Palette[Index];

            //        int Index = Destination.Palette.IndexOf(Color);
            //        if (Index == -1)
            //        {
            //            if ((1 << Indexed.BitsPerPixel) <= Destination.Palette.Count)
            //                throw new IndexOutOfRangeException("Palette is full.");

            //            Index = Destination.Palette.Count;
            //            Destination.Palette.Add(Color);
            //        }

            //        Indexed[BitIndex++] = Index;
            //    }

            //    *sScan++ = (Struct)Indexed;
            //    BitIndex = 0;
            //}
        }
        public override void ScanLineOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
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
                    pDest++->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                pStructs++;
            }
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
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
                    this.Overlay(ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                pStructs++;
            }
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
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
                    this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestA++;
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                pStructs++;
            }
        }

        public override void ScanLineReverseOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            pDest += Length - 1;

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    pDest--->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                pStructs++;
            }
        }
        public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    this.Overlay(ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestR--;
                    pDestG--;
                    pDestB--;
                }

                pStructs++;
            }
        }
        public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestA += OffsetToEnd;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
            for (int i = 0; i < Length;)
            {
                IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                XBits %= Indexed.Length;

                for (; i < Length && XBits < Indexed.Length; i++)
                {
                    IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
                    this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestA--;
                    pDestR--;
                    pDestG--;
                    pDestB--;
                }

                pStructs++;
            }
        }

        public override void ContourOverlay<T>(IImageContext Destination, ImageContour Contour, T Color, int OffsetX, int OffsetY)
        {
        }

        public override void BlockOverlay<T>(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
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
        public static IImageOperator GetOperator()
        {
            string Key = $"{typeof(Struct).Name}";
            if (ImageOperators.TryGetValue(Key, out IImageOperator IOperator))
                return IOperator;

            IImageOperator Operator = new ImageIndexedOperator<Struct>();
            ImageOperators.AddOrUpdate(Key, Operator, (k, o) => Operator);

            return Operator;
        }

    }

    //internal unsafe class ImageIndexedOperator<Pixel, Struct> : IImageOperator<Pixel>
    //    where Pixel : unmanaged, IPixel
    //    where Struct : unmanaged, IPixelBase
    //{
    //    public PixelOperator<Pixel> PixelOperator { get; }

    //    private ImageIndexedOperator()
    //    {
    //        PixelOperator = PixelOperator<Pixel>.GetOperator();
    //    }

    //    public Pixel ToPixel(byte A, byte R, byte G, byte B)
    //        => PixelOperator.ToPixel(A, R, G, B);

    //    public Pixel GetPixel(IImageContext Source, int X, int Y)
    //    {
    //        int XBits = X * Source.BitsPerPixel;
    //        long Offset = Source.Stride * (long)Y + (XBits >> 3);

    //        IPixelIndexed Indexed = *(Struct*)((byte*)Source.Scan0 + Offset) as IPixelIndexed;
    //        return (Pixel)Source.Palette[Indexed[XBits % Indexed.Length]];
    //    }

    //    public void SetPixel(IImageContext Source, int X, int Y, Pixel Pixel)
    //    {
    //        int XBits = X * Source.BitsPerPixel;
    //        long Offset = Source.Stride * (long)Y + (XBits >> 3);

    //        Struct* sScan = (Struct*)((byte*)Source.Scan0 + Offset);
    //        IPixelIndexed Indexed = *sScan as IPixelIndexed;

    //        int Index = Source.Palette.IndexOf(Pixel);
    //        if (Index == -1)
    //        {
    //            if ((1 << Indexed.BitsPerPixel) <= Source.Palette.Count)
    //                throw new IndexOutOfRangeException("Palette is full.");

    //            Index = Source.Palette.Count;
    //            Source.Palette.Add(Pixel);
    //        }

    //        Indexed[XBits % Indexed.Length] = Index;
    //        *sScan = (Struct)Indexed;
    //    }

    //    public void ScanLineOverride(IImageContext Destination, int X, int Y, int Length, Pixel Color)
    //    {

    //    }
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDest)
    //        => ScanLineOverrideTo(Source, X, Y, Length, pDest, PixelOperator);
    //    public void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
    //    {
    //        long XBits = (long)X * Source.BitsPerPixel,
    //             Offset = Source.Stride * Y + (XBits >> 3);

    //        Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
    //        for (int i = 0; i < Length;)
    //        {
    //            IPixelIndexed Indexed = *pStructs as IPixelIndexed;
    //            XBits %= Indexed.Length;

    //            for (; i < Length && XBits < Indexed.Length; i++)
    //            {
    //                IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
    //                Operator.Override(ref pDest, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
    //                pDest++;
    //            }

    //            pStructs++;
    //        }
    //    }
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
    //    {
    //        long XBits = (long)X * Source.BitsPerPixel,
    //             Offset = Source.Stride * Y + (XBits >> 3);

    //        Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
    //        for (int i = 0; i < Length;)
    //        {
    //            IPixelIndexed Indexed = *pStructs as IPixelIndexed;
    //            XBits %= Indexed.Length;

    //            for (; i < Length && XBits < Indexed.Length; i++)
    //            {
    //                IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
    //                *pDestR++ = Pixel.R;
    //                *pDestG++ = Pixel.G;
    //                *pDestB++ = Pixel.B;
    //            }

    //            pStructs++;
    //        }
    //    }
    //    public void ScanLineOverrideTo(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
    //    {
    //        long XBits = (long)X * Source.BitsPerPixel,
    //             Offset = Source.Stride * Y + (XBits >> 3);

    //        Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
    //        for (int i = 0; i < Length;)
    //        {
    //            IPixelIndexed Indexed = *pStructs as IPixelIndexed;
    //            XBits %= Indexed.Length;

    //            for (; i < Length && XBits < Indexed.Length; i++)
    //            {
    //                IPixel Pixel = Source.Palette[Indexed[(int)XBits++]];
    //                *pDestA++ = Pixel.A;
    //                *pDestR++ = Pixel.R;
    //                *pDestG++ = Pixel.G;
    //                *pDestB++ = Pixel.B;
    //            }

    //            pStructs++;
    //        }
    //    }

    //    public void ScanLineOverlay(IImageContext Destination, int X, int Y, int Length, Pixel Color)
    //    {

    //    }
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
    //    {
    //    }
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
    //    {
    //    }
    //    public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
    //    {
    //    }

    //    public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color, int OffsetX, int OffsetY)
    //    {

    //    }

    //    public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
    //    {
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

    //        long XBits,
    //             Offset;
    //        int X, Y, SaveX, XBitIndex, Rx, Lx;

    //        Struct* pSeed, pStructs;
    //        IPixelIndexed Indexed;
    //        IPixel Pixel;
    //        while (StackX.Count > 0)
    //        {
    //            X = StackX.Pop();
    //            Y = StackY.Pop();
    //            SaveX = X;

    //            XBits = (long)X * Source.BitsPerPixel;
    //            Offset = Source.Stride * Y + (XBits >> 3);

    //            pSeed = (Struct*)((byte*)Source.Scan0 + Offset);
    //            pStructs = pSeed;

    //            Indexed = *pStructs as IPixelIndexed;
    //            XBitIndex = (int)(XBits % Indexed.Length);
    //            Pixel = Source.Palette[Indexed[XBitIndex]];
    //            while (X < Width && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
    //            {
    //                X++;
    //                XBitIndex++;

    //                if (XBitIndex >= Indexed.Length)
    //                {
    //                    XBitIndex = 0;
    //                    pStructs++;
    //                    Indexed = *pStructs as IPixelIndexed;
    //                }

    //                Pixel = Source.Palette[Indexed[XBitIndex]];
    //            }

    //            // Find Left Bound
    //            Rx = X - 1;
    //            X = SaveX - 1;

    //            pStructs = pSeed;
    //            Indexed = *pStructs as IPixelIndexed;
    //            XBitIndex = (int)(XBits % Indexed.Length) - 1;
    //            if (XBitIndex < 0)
    //            {
    //                XBitIndex = Indexed.Length - 1;
    //                pStructs--;
    //                Indexed = *pStructs as IPixelIndexed;
    //            }
    //            Pixel = Source.Palette[Indexed[XBitIndex]];

    //            pStructs = pSeed - 1;
    //            while (-1 < X && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
    //            {
    //                X--;
    //                XBitIndex--;

    //                if (XBitIndex < 0)
    //                {
    //                    XBitIndex = Indexed.Length - 1;
    //                    pStructs--;
    //                    Indexed = *pStructs as IPixelIndexed;
    //                }
    //                Pixel = Source.Palette[Indexed[XBitIndex]];
    //            }

    //            Lx = X + 1;

    //            // Log Region
    //            Contour[Y].Union(Lx, Rx);

    //            // Lower ScanLine's Seed
    //            bool NeedFill = false;
    //            X = Lx;
    //            Y++;

    //            XBits = (long)X * Source.BitsPerPixel;
    //            Offset = Source.Stride * Y + (XBits >> 3);

    //            pSeed = (Struct*)((byte*)Source.Scan0 + Offset);

    //            Indexed = *pSeed as IPixelIndexed;
    //            XBitIndex = (int)(XBits % Indexed.Length);
    //            Pixel = Source.Palette[Indexed[XBitIndex]];

    //            if (-1 < Y && Y < Height &&
    //                !Contour.Contain(X, Y))
    //                for (; X <= Rx; X++)
    //                {
    //                    while (X <= Rx && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
    //                    {
    //                        NeedFill = true;
    //                        X++;
    //                        XBitIndex++;

    //                        if (XBitIndex >= Indexed.Length)
    //                        {
    //                            XBitIndex = 0;
    //                            pSeed++;
    //                            Indexed = *pSeed as IPixelIndexed;
    //                        }

    //                        Pixel = Source.Palette[Indexed[XBitIndex]];
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

    //            Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
    //            pSeed = (Struct*)((byte*)Source.Scan0 + Offset);
    //            if (0 <= Y && Y < Height &&
    //                !Contour.Contain(X, Y))
    //                for (; X <= Rx; X++)
    //                {
    //                    while (X <= Rx && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
    //                    {
    //                        NeedFill = true;
    //                        X++;
    //                        XBitIndex++;

    //                        if (XBitIndex >= Indexed.Length)
    //                        {
    //                            XBitIndex = 0;
    //                            pSeed++;
    //                            Indexed = *pSeed as IPixelIndexed;
    //                        }

    //                        Pixel = Source.Palette[Indexed[XBitIndex]];
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
    //        string Key = $"{typeof(Pixel).Name}_{typeof(Struct).Name}";
    //        if (ImageOperators.TryGetValue(Key, out IImageOperator IOperator))
    //            return (IImageOperator<Pixel>)IOperator;

    //        IImageOperator<Pixel> Operator = new ImageIndexedOperator<Pixel, Struct>();
    //        ImageOperators.AddOrUpdate(Key, Operator, (k, o) => Operator);

    //        return Operator;
    //    }

    //}
}
