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

        public void ScanLineOverlay(IImageContext Source, int X, int Y, int Length, Pixel Color)
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

        public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color)
        {
        }

        public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
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
