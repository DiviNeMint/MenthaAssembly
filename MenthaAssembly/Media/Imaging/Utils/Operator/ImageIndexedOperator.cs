using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageIndexedOperator<Struct> : IImageOperator
        where Struct : unmanaged, IPixelIndexed
    {
        public override T GetPixel<T>(IImageContext Source, int X, int Y)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct Indexed = *(Struct*)((byte*)Source.Scan0 + Offset);
            return (T)Source.Palette[Indexed[XBits % Indexed.Length]];
        }
        public override void SetPixel<T>(IImageContext Source, int X, int Y, T Color)
        {
            if (!Source.Palette.TryGetOrAdd(Color, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* sScan = (Struct*)((byte*)Source.Scan0 + Offset);
            (*sScan)[XBits % sScan->Length] = Index;
        }

        public override void ScanLineOverride<T>(IImageContext Destination, int X, int Y, int Length, T Color)
        {
            if (!Destination.Palette.TryGetOrAdd(Color, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            int XBits = X * Destination.BitsPerPixel;
            long Offset = Destination.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Destination.Scan0 + Offset);

            int IndexLength = pData->Length,
                BitIndex = XBits % IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; i < Length && BitIndex < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    Data[BitIndex++] = Index;
                }

                pData++;
                BitIndex = 0;
            }
        }
        public override void ScanLineOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    pDest++->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                XBits = 0;
            }
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;
                }

                XBits = 0;
            }
        }
        public override void ScanLineOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    *pDestA++ = Pixel.A;
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;
                }

                XBits = 0;
            }
        }

        public override void ScanLineReverseOverrideTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            pDest += Length - 1;

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    pDest--->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                XBits = 0;
            }
        }
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    *pDestR-- = Pixel.R;
                    *pDestG-- = Pixel.G;
                    *pDestB-- = Pixel.B;
                }

                XBits = 0;
            }
        }
        public override void ScanLineReverseOverrideTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestA += OffsetToEnd;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    *pDestA-- = Pixel.A;
                    *pDestR-- = Pixel.R;
                    *pDestG-- = Pixel.G;
                    *pDestB-- = Pixel.B;
                }

                XBits = 0;
            }
        }

        public override void ScanLineOverlay<T>(IImageContext Destination, int X, int Y, int Length, T Color)
        {
            if (Color.A is byte.MinValue || Color.A is byte.MaxValue)
            {
                this.ScanLineOverlay(Destination, X, Y, Length, Color);
                return;
            }

            int XBits = X * Destination.BitsPerPixel;
            long Offset = Destination.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Destination.Scan0 + Offset);

            int IndexLength = pData->Length,
                BitIndex = XBits % IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; BitIndex < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Dest = ToPixel<T>(Destination.Palette[Data[BitIndex]]);
                    Dest.Overlay(Color.A, Color.R, Color.G, Color.B);

                    if (!Destination.Palette.TryGetOrAdd(Dest, out int Index))
                        throw new IndexOutOfRangeException("Palette is full.");

                    Data[BitIndex++] = Index;
                }

                pData++;
                BitIndex = 0;
            }
        }
        public override void ScanLineOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    pDest++->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                XBits = 0;
            }
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    this.Overlay(ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                XBits = 0;
            }
        }
        public override void ScanLineOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits++]];
                    this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestA++;
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                XBits = 0;
            }
        }

        //public override void ScanLineReverseOverlayTo<T, T2>(IImageContext Source, int X, int Y, int Length, T2* pDest)
        //{
        //    long XBits = (long)X * Source.BitsPerPixel,
        //         Offset = Source.Stride * Y + (XBits >> 3);

        //    pDest += Length - 1;

        //    Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
        //    for (int i = 0; i < Length;)
        //    {
        //        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
        //        XBits %= Indexed.Length;

        //        for (; i < Length && XBits < Indexed.Length; i++)
        //        {
        //            IPixel Pixel = Source.Palette[Indexed[XBits++]];
        //            pDest--->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        //        }

        //        pStructs++;
        //    }
        //}
        //public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        //{
        //    long XBits = (long)X * Source.BitsPerPixel,
        //         Offset = Source.Stride * Y + (XBits >> 3);

        //    int OffsetToEnd = Length - 1;
        //    pDestR += OffsetToEnd;
        //    pDestG += OffsetToEnd;
        //    pDestB += OffsetToEnd;

        //    Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
        //    for (int i = 0; i < Length;)
        //    {
        //        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
        //        XBits %= Indexed.Length;

        //        for (; i < Length && XBits < Indexed.Length; i++)
        //        {
        //            IPixel Pixel = Source.Palette[Indexed[XBits++]];
        //            this.Overlay(ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        //            pDestR--;
        //            pDestG--;
        //            pDestB--;
        //        }

        //        pStructs++;
        //    }
        //}
        //public override void ScanLineReverseOverlayTo<T>(IImageContext Source, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        //{
        //    long XBits = (long)X * Source.BitsPerPixel,
        //         Offset = Source.Stride * Y + (XBits >> 3);

        //    int OffsetToEnd = Length - 1;
        //    pDestA += OffsetToEnd;
        //    pDestR += OffsetToEnd;
        //    pDestG += OffsetToEnd;
        //    pDestB += OffsetToEnd;

        //    Struct* pStructs = (Struct*)((byte*)Source.Scan0 + Offset);
        //    for (int i = 0; i < Length;)
        //    {
        //        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
        //        XBits %= Indexed.Length;

        //        for (; i < Length && XBits < Indexed.Length; i++)
        //        {
        //            IPixel Pixel = Source.Palette[Indexed[XBits++]];
        //            this.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        //            pDestA--;
        //            pDestR--;
        //            pDestG--;
        //            pDestB--;
        //        }

        //        pStructs++;
        //    }
        //}

        public override void ScanLineNearestResizeTo<T, T2>(IImageContext Source, int Step, int Max, int X, int Y, int Length, T2* pDest)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int Count = 0,
                IndexLength = pData->Length;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits]];
                    pDest++->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);

                    Count += Step;
                    while (Count >= Max)
                    {
                        Count -= Max;
                        XBits++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pData++;
                }
            }
        }
        public override void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int Count = 0,
                IndexLength = pData->Length;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits]];
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;

                    Count += Step;
                    while (Count >= Max)
                    {
                        Count -= Max;
                        XBits++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pData++;
                }
            }
        }
        public override void ScanLineNearestResizeTo<T>(IImageContext Source, int Step, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Source.BitsPerPixel;
            long Offset = Source.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Source.Scan0 + Offset);
            int Count = 0,
                IndexLength = pData->Length;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    IPixel Pixel = Source.Palette[Data[XBits]];
                    *pDestA++ = Pixel.A;
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;

                    Count += Step;
                    while (Count >= Max)
                    {
                        Count -= Max;
                        XBits++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pData++;
                }
            }
        }

        public override void ScanLineBilinearResizeTo<T, T2>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, T2* pDest)
        {
            long SourceStride = Source.Stride,
                 XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            byte* pData0 = (byte*)Source.Scan0 + Offset;
            Struct* pStructs0 = (Struct*)pData0,
                    pStructs1 = (Struct*)(pData0 + SourceStride);

            int SourceW = Source.Width,
                IndexLength = pStructs0->Length,
                FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    int Index = (int)XBits;
                    IPixel p00 = Source.Palette[(*pStructs0)[Index]],
                           p10 = Source.Palette[(*pStructs1)[Index]],
                           p01, p11;

                    if (X < SourceW)
                    {
                        p01 = p00;
                        p11 = p10;
                    }
                    else
                    {
                        Index++;

                        if (Index < IndexLength)
                        {
                            p01 = Source.Palette[(*pStructs0)[Index]];
                            p11 = Source.Palette[(*pStructs1)[Index]];
                        }
                        else
                        {
                            p01 = Source.Palette[(*(pStructs0 + 1))[Index]];
                            p11 = Source.Palette[(*(pStructs1 + 1))[Index]];
                        }
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
                        XBits++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pStructs0++;
                    pStructs1++;
                }
            }
        }
        public override void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Source.Stride,
                 XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            byte* pData0 = (byte*)Source.Scan0 + Offset;
            Struct* pStructs0 = (Struct*)pData0,
                    pStructs1 = (Struct*)(pData0 + SourceStride);

            int SourceW = Source.Width,
                IndexLength = pStructs0->Length,
                FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    int Index = (int)XBits;
                    IPixel p00 = Source.Palette[(*pStructs0)[Index]],
                           p10 = Source.Palette[(*pStructs1)[Index]],
                           p01, p11;

                    if (X < SourceW)
                    {
                        p01 = p00;
                        p11 = p10;
                    }
                    else
                    {
                        Index++;

                        if (Index < IndexLength)
                        {
                            p01 = Source.Palette[(*pStructs0)[Index]];
                            p11 = Source.Palette[(*pStructs1)[Index]];
                        }
                        else
                        {
                            p01 = Source.Palette[(*(pStructs0 + 1))[Index]];
                            p11 = Source.Palette[(*(pStructs1 + 1))[Index]];
                        }
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
                        XBits++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pStructs0++;
                    pStructs1++;
                }
            }
        }
        public override void ScanLineBilinearResizeTo<T>(IImageContext Source, int StepX, int FracY, int Max, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Source.Stride,
                 XBits = (long)X * Source.BitsPerPixel,
                 Offset = Source.Stride * Y + (XBits >> 3);

            byte* pData0 = (byte*)Source.Scan0 + Offset;
            Struct* pStructs0 = (Struct*)pData0,
                    pStructs1 = (Struct*)(pData0 + SourceStride);

            int SourceW = Source.Width,
                IndexLength = pStructs0->Length,
                FracX = 0,
                IFracY = Max - FracY,
                SqrMax = Max * Max;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    int Index = (int)XBits;
                    IPixel p00 = Source.Palette[(*pStructs0)[Index]],
                           p10 = Source.Palette[(*pStructs1)[Index]],
                           p01, p11;

                    if (X < SourceW)
                    {
                        p01 = p00;
                        p11 = p10;
                    }
                    else
                    {
                        Index++;

                        if (Index < IndexLength)
                        {
                            p01 = Source.Palette[(*pStructs0)[Index]];
                            p11 = Source.Palette[(*pStructs1)[Index]];
                        }
                        else
                        {
                            p01 = Source.Palette[(*(pStructs0 + 1))[Index]];
                            p11 = Source.Palette[(*(pStructs1 + 1))[Index]];
                        }
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
                        XBits++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pStructs0++;
                    pStructs1++;
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

            Action OverlayHandler;
            if (Color.A == 0 ||
                Color.A == byte.MaxValue)
            {
                if (!Destination.Palette.TryGetOrAdd(Color, out int Index))
                    throw new IndexOutOfRangeException("Palette is full.");

                OverlayHandler = () =>
                {
                    Struct* pTempDatas = (Struct*)pPixels;
                    int CurrentX = 0,
                        XBit = 0,
                        IndexLength = pTempDatas->Length;
                    for (int i = 0; i < Data.Count; i++)
                    {
                        int Sx = Math.Max(Data[i++] + OffsetX, 0),
                            Ex = Math.Min(Data[i] + OffsetX, MaxX);

                        if (Ex <= Sx)
                            continue;

                        if (MaxX < Sx)
                            return;

                        XBit += (Sx - CurrentX) * Destination.BitsPerPixel;
                        while (XBit >= IndexLength)
                        {
                            pTempDatas++;
                            XBit -= IndexLength;
                        }

                        Struct TempDatas = *pTempDatas;
                        for (int j = Sx; j <= Ex; j++)
                        {
                            TempDatas[XBit++] = Index;

                            if (XBit >= IndexLength)
                            {
                                pTempDatas++;
                                XBit -= IndexLength;
                            }
                        }

                        CurrentX = Ex + 1;
                    }
                };
            }
            else
            {
                OverlayHandler = () =>
                {
                    Struct* pTempDatas = (Struct*)pPixels;
                    int CurrentX = 0,
                        XBit = 0,
                        IndexLength = pTempDatas->Length;
                    for (int i = 0; i < Data.Count; i++)
                    {
                        int Sx = Math.Max(Data[i++] + OffsetX, 0),
                            Ex = Math.Min(Data[i] + OffsetX, MaxX);

                        if (Ex <= Sx)
                            continue;

                        if (MaxX < Sx)
                            return;

                        XBit += (Sx - CurrentX) * Destination.BitsPerPixel;
                        while (XBit >= IndexLength)
                        {
                            pTempDatas++;
                            XBit -= IndexLength;
                        }

                        Struct TempDatas = *pTempDatas;
                        for (int j = Sx; j <= Ex; j++)
                        {
                            T Pixel = ToPixel<T>(Destination.Palette[TempDatas[XBit]]);
                            Pixel.Overlay(Color.A, Color.R, Color.G, Color.B);

                            if (!Destination.Palette.TryGetOrAdd(Pixel, out int Index))
                                throw new IndexOutOfRangeException("Palette is full.");

                            TempDatas[XBit++] = Index;

                            if (XBit >= IndexLength)
                            {
                                pTempDatas++;
                                XBit -= IndexLength;
                            }
                        }

                        CurrentX = Ex + 1;
                    }
                };
            }

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
            Struct Indexed;
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

                Indexed = *pStructs;
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
                        Indexed = *pStructs;
                    }

                    Pixel = Source.Palette[Indexed[XBitIndex]];
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                pStructs = pSeed;
                Indexed = *pStructs;
                XBitIndex = (int)(XBits % Indexed.Length) - 1;
                if (XBitIndex < 0)
                {
                    XBitIndex = Indexed.Length - 1;
                    pStructs--;
                    Indexed = *pStructs;
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
                        Indexed = *pStructs;
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

                Indexed = *pSeed;
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
                                Indexed = *pSeed;
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
                                Indexed = *pSeed;
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
}
