using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator4<Pixel> : IImageOperator<Pixel>
        where Pixel : unmanaged, IPixel
    {
        public PixelOperator<Pixel> PixelOperator { get; }

        private ImageOperator4()
        {
            PixelOperator = PixelOperator<Pixel>.GetOperator();
        }

        public Pixel ToPixel(byte A, byte R, byte G, byte B)
            => PixelOperator.ToPixel(A, R, G, B);

        public Pixel GetPixel(IImageContext Source, int X, int Y)
        {
            Pixel Pixel = default;
            byte* pPixel = (byte*)&Pixel;
            long Offset = Source.Stride * (long)Y + X;
            PixelOperator.Override(ref pPixel,
                                   *((byte*)Source.ScanA + Offset),
                                   *((byte*)Source.ScanR + Offset),
                                   *((byte*)Source.ScanG + Offset),
                                   *((byte*)Source.ScanB + Offset));
            return Pixel;
        }

        public void SetPixel(IImageContext Source, int X, int Y, Pixel Pixel)
        {
            long Offset = Source.Stride * (long)Y + X;
            *((byte*)Source.ScanR + Offset) = Pixel.R;
            *((byte*)Source.ScanG + Offset) = Pixel.G;
            *((byte*)Source.ScanB + Offset) = Pixel.B;
            *((byte*)Source.ScanA + Offset) = Pixel.A;
        }

        public void ScanLineCopy(IImageContext Source, int X, int Y, int Length, byte* pDest)
            => ScanLineCopy(Source, X, Y, Length, pDest, PixelOperator);
        public void ScanLineCopy<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
            byte* PixelA = (byte*)Source.ScanA + Offset,
                  PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                Operator.Override(ref pDest, *PixelA++, *PixelR++, *PixelG++, *PixelB++);
                pDest++;
            }
        }
        public void ScanLineCopy3(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
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
        public void ScanLineCopy4(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
            byte* PixelA = (byte*)Source.ScanA + Offset,
                  PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                *pDestA++ = *PixelA++;
                *pDestR++ = *PixelR++;
                *pDestG++ = *PixelG++;
                *pDestB++ = *PixelB++;
            }
        }

        public void ScanLineOverlay(IImageContext Source, int X, int Y, int Length, Pixel Color)
        {
            if (Color.A == 0)
                return;

            long Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
            byte* pPixelA = (byte*)Source.ScanA + Offset,
                  pPixelR = (byte*)Source.ScanR + Offset,
                  pPixelG = (byte*)Source.ScanG + Offset,
                  pPixelB = (byte*)Source.ScanB + Offset;

            if (Color.A == 255)
            {
                for (int i = 0; i < Length; i++)
                {
                    *pPixelA++ = Color.A;
                    *pPixelR++ = Color.R;
                    *pPixelG++ = Color.G;
                    *pPixelB++ = Color.B;
                }
            }
            else
            {
                for (int i = 0; i < Length; i++)
                {
                    PixelOperator.Overlay(ref pPixelA, ref pPixelR, ref pPixelG, ref pPixelB, Color.A, Color.R, Color.G, Color.B);
                    pPixelA++;
                    pPixelR++;
                    pPixelG++;
                    pPixelB++;
                }
            }
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDest, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
            byte* PixelA = (byte*)Source.ScanA + Offset,
                  PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                Operator.Overlay(ref pDest, *PixelA++, *PixelR++, *PixelG++, *PixelB++);
                pDest++;
            }
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
            byte* PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                Operator.Overlay(ref pDestR, ref pDestG, ref pDestB, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                pDestR++;
                pDestG++;
                pDestB++;
            }
        }
        public void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB, PixelOperator<T> Operator) where T : unmanaged, IPixel
        {
            long Offset = Source.Stride * Y + (((long)X * Source.BitsPerPixel) >> 3);
            byte* PixelA = (byte*)Source.ScanA + Offset,
                  PixelR = (byte*)Source.ScanR + Offset,
                  PixelG = (byte*)Source.ScanG + Offset,
                  PixelB = (byte*)Source.ScanB + Offset;

            for (int i = 0; i < Length; i++)
            {
                Operator.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, *PixelA++, *PixelR++, *PixelG++, *PixelB++);
                pDestA++;
                pDestR++;
                pDestG++;
                pDestB++;
            }
        }

        public void ContourOverlay(IImageContext Destination, ImageContour Contour, Pixel Color)
        {
            if (Color.A == 0)
                return;

            IEnumerator<KeyValuePair<int, ContourData>> Enumerator = Contour.GetEnumerator();
            if (!Enumerator.MoveNext())
                return;

            KeyValuePair<int, ContourData> Current = Enumerator.Current;

            long Y = Current.Key;

            long Offset = Destination.Stride * Y;
            byte* pPixelA = (byte*)Destination.ScanA + Offset,
                  pPixelR = (byte*)Destination.ScanR + Offset,
                  pPixelG = (byte*)Destination.ScanG + Offset,
                  pPixelB = (byte*)Destination.ScanB + Offset;

            ContourData Data = Current.Value;

            void OverlayHandler()
            {
                byte* pTempPixelA = pPixelA,
                      pTempPixelR = pPixelR,
                      pTempPixelG = pPixelG,
                      pTempPixelB = pPixelB;

                int CurrentX = 0;
                for (int i = 0; i < Data.Count;)
                {
                    Offset = ((Data[i] - CurrentX) * Destination.BitsPerPixel) >> 3;
                    pTempPixelA += Offset;
                    pTempPixelR += Offset;
                    pTempPixelG += Offset;
                    pTempPixelB += Offset;

                    for (int j = Data[i++]; j <= Data[i]; j++)
                    {
                        PixelOperator.Overlay(ref pTempPixelA, ref pTempPixelR, ref pTempPixelG, ref pTempPixelB, Color.A, Color.R, Color.G, Color.B);
                        pTempPixelA++;
                        pTempPixelR++;
                        pTempPixelG++;
                        pTempPixelB++;
                    }
                    CurrentX = Data[i++] + 1;
                }
            };

            OverlayHandler();
            while (Enumerator.MoveNext())
            {
                Current = Enumerator.Current;
                Offset = Destination.Stride * (Current.Key - Y);
                pPixelA += Offset;
                pPixelR += Offset;
                pPixelG += Offset;
                pPixelB += Offset;

                Y = Current.Key;
                Data = Current.Value;

                OverlayHandler();
            }
        }

        public void BlockOverlay(IImageContext Destination, int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
            long Offset = Destination.Stride * Y + (((long)X * Destination.BitsPerPixel) >> 3);
            byte* pPixelA = (byte*)Destination.ScanA + Offset,
                  pPixelR = (byte*)Destination.ScanR + Offset,
                  pPixelG = (byte*)Destination.ScanG + Offset,
                  pPixelB = (byte*)Destination.ScanB + Offset;

            Source.BlockOverlayTo<Pixel>(OffsetX, OffsetY, Width, Height, pPixelA, pPixelR, pPixelG, pPixelB, Destination.Stride);
        }

        private static readonly ConcurrentDictionary<string, IImageOperator> ImageOperators = new ConcurrentDictionary<string, IImageOperator>();
        public static IImageOperator<Pixel> GetOperator()
        {
            string Key = $"{typeof(Pixel).Name}";
            if (ImageOperators.TryGetValue(Key, out IImageOperator IOperator))
                return (IImageOperator<Pixel>)IOperator;

            IImageOperator<Pixel> Operator = new ImageOperator4<Pixel>();
            ImageOperators.AddOrUpdate(Key, Operator, (k, o) => Operator);

            return Operator;
        }

    }
}
