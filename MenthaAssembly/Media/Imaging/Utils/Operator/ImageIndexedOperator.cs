using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageIndexedOperator<T, Struct> : IImageOperator<T, Struct>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public IImageContext<T, Struct> Context { get; }
        IImageContext IImageOperator.Context => this.Context;

        public T GetPixel(int X, int Y)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct Indexed = *(Struct*)((byte*)Context.Scan0 + Offset);
            return (T)Context.Palette[Indexed[XBits % Indexed.Length]];
        }
        IPixel IImageOperator.GetPixel(int X, int Y)
            => this.GetPixel(X, Y);

        public void SetPixel(int X, int Y, T Color)
        {
            if (!Context.Palette.TryGetOrAdd(Color, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* sScan = (Struct*)((byte*)Context.Scan0 + Offset);
            (*sScan)[XBits % sScan->Length] = Index;
        }
        void IImageOperator.SetPixel(int X, int Y, IPixel Pixel)
            => this.SetPixel(X, Y, Pixel.ToPixel<T>());

        public ImageIndexedOperator(IImageContext<T, Struct> Context)
        {
            this.Context = Context;
        }

        public void ScanLineOverride(int X, int Y, int Length, T Color)
        {
            if (!Context.Palette.TryGetOrAdd(Color, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);

            int IndexLength = pData->Length,
                BitIndex = XBits % IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; BitIndex < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    Data[BitIndex++] = Index;
                }

                pData++;
                BitIndex = 0;
            }
        }
        void IImageOperator.ScanLineOverride(int X, int Y, int Length, IPixel Color)
            => this.ScanLineOverride(X, Y, Length, Color.ToPixel<T>());
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDest)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            T* pDestPixel = (T*)pDest;
            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    *pDestPixel++ = Context.Palette[Data[XBits++]];
                }

                XBits = 0;
            }
        }
        public void ScanLineOverrideTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    pDest++->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                XBits = 0;
            }
        }
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;
                }

                XBits = 0;
            }
        }
        public void ScanLineOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    *pDestA++ = Pixel.A;
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;
                }

                XBits = 0;
            }
        }

        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDest)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            pDest += Length - 1;

            T* pDestPixel = (T*)pDest;
            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    *pDestPixel-- = Context.Palette[Data[XBits++]];
                }

                XBits = 0;
            }
        }
        public void ScanLineReverseOverrideTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            pDest += Length - 1;

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    pDest--->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                XBits = 0;
            }
        }
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    *pDestR-- = Pixel.R;
                    *pDestG-- = Pixel.G;
                    *pDestB-- = Pixel.B;
                }

                XBits = 0;
            }
        }
        public void ScanLineReverseOverrideTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            int OffsetToEnd = Length - 1;
            pDestA += OffsetToEnd;
            pDestR += OffsetToEnd;
            pDestG += OffsetToEnd;
            pDestB += OffsetToEnd;

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    *pDestA-- = Pixel.A;
                    *pDestR-- = Pixel.R;
                    *pDestG-- = Pixel.G;
                    *pDestB-- = Pixel.B;
                }

                XBits = 0;
            }
        }

        public void ScanLineOverlay(int X, int Y, int Length, T Color)
        {
            if (Color.A is byte.MinValue || Color.A is byte.MaxValue)
            {
                this.ScanLineOverlay(X, Y, Length, Color);
                return;
            }

            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);

            int IndexLength = pData->Length,
                BitIndex = XBits % IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; BitIndex < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Dest = Context.Palette[Data[BitIndex]].ToPixel<T>();
                    Dest.Overlay(Color.A, Color.R, Color.G, Color.B);

                    if (!Context.Palette.TryGetOrAdd(Dest, out int Index))
                        throw new IndexOutOfRangeException("Palette is full.");

                    Data[BitIndex++] = Index;
                }

                pData++;
                BitIndex = 0;
            }
        }
        void IImageOperator.ScanLineOverlay(int X, int Y, int Length, IPixel Color)
            => this.ScanLineOverlay(X, Y, Length, Color.ToPixel<T>());
        public void ScanLineOverlayTo<T2>(int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    pDest++->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                }

                XBits = 0;
            }
        }
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    PixelHelper.Overlay(ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                XBits = 0;
            }
        }
        public void ScanLineOverlayTo(int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;
            XBits %= IndexLength;

            for (int i = 0; i < Length;)
            {
                Struct Data = *pData++;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits++]];
                    PixelHelper.Overlay(ref pDestA, ref pDestR, ref pDestG, ref pDestB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                    pDestA++;
                    pDestR++;
                    pDestG++;
                    pDestB++;
                }

                XBits = 0;
            }
        }

        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDest)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            T* pDestPixel = (T*)pDest;
            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    *pDestPixel++ = Context.Palette[Data[XBits]];

                    FracX += Step;
                    while (FracX >= 1f)
                    {
                        FracX -= 1f;
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
        public void ScanLineNearestResizeTo<T2>(float FracX, float Step, int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits]];
                    pDest++->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);

                    FracX += Step;
                    while (FracX >= 1f)
                    {
                        FracX -= 1f;
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
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits]];
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;

                    FracX += Step;
                    while (FracX >= 1f)
                    {
                        FracX -= 1f;
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
        public void ScanLineNearestResizeTo(float FracX, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                Struct Data = *pData;
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    T Pixel = Context.Palette[Data[XBits]];
                    *pDestA++ = Pixel.A;
                    *pDestR++ = Pixel.R;
                    *pDestG++ = Pixel.G;
                    *pDestB++ = Pixel.B;

                    FracX += Step;
                    while (FracX >= 1f)
                    {
                        FracX -= 1f;
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

        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDest)
            => ScanLineBilinearResizeTo(FracX, FracY, Step, X, Y, Length, (T*)pDest);
        public void ScanLineBilinearResizeTo<T2>(float FracX, float FracY, float Step, int X, int Y, int Length, T2* pDest)
            where T2 : unmanaged, IPixel
        {
            long SourceStride = Context.Stride,
                 XBits = (long)X * Context.BitsPerPixel,
                 Offset = Context.Stride * Y + (XBits >> 3);

            byte* pData0 = (byte*)Context.Scan0 + Offset;
            Struct* pStructs0 = (Struct*)pData0,
                    pStructs1 = (Struct*)(pData0 + SourceStride);

            int SourceW = Context.Width,
                IndexLength = pStructs0->Length;
            float IFracY = 1f - FracY;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    int Index = (int)XBits;
                    T p00 = Context.Palette[(*pStructs0)[Index]],
                      p10 = Context.Palette[(*pStructs1)[Index]],
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
                            p01 = Context.Palette[(*pStructs0)[Index]];
                            p11 = Context.Palette[(*pStructs1)[Index]];
                        }
                        else
                        {
                            p01 = Context.Palette[(*(pStructs0 + 1))[Index]];
                            p11 = Context.Palette[(*(pStructs1 + 1))[Index]];
                        }
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
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Context.Stride,
                 XBits = (long)X * Context.BitsPerPixel,
                 Offset = Context.Stride * Y + (XBits >> 3);

            byte* pData0 = (byte*)Context.Scan0 + Offset;
            Struct* pStructs0 = (Struct*)pData0,
                    pStructs1 = (Struct*)(pData0 + SourceStride);

            int SourceW = Context.Width,
                IndexLength = pStructs0->Length;
            float IFracY = 1f - FracY;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    int Index = (int)XBits;
                    T p00 = Context.Palette[(*pStructs0)[Index]],
                      p10 = Context.Palette[(*pStructs1)[Index]],
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
                            p01 = Context.Palette[(*pStructs0)[Index]];
                            p11 = Context.Palette[(*pStructs1)[Index]];
                        }
                        else
                        {
                            p01 = Context.Palette[(*(pStructs0 + 1))[Index]];
                            p11 = Context.Palette[(*(pStructs1 + 1))[Index]];
                        }
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
        public void ScanLineBilinearResizeTo(float FracX, float FracY, float Step, int X, int Y, int Length, byte* pDestA, byte* pDestR, byte* pDestG, byte* pDestB)
        {
            long SourceStride = Context.Stride,
                 XBits = (long)X * Context.BitsPerPixel,
                 Offset = Context.Stride * Y + (XBits >> 3);

            byte* pData0 = (byte*)Context.Scan0 + Offset;
            Struct* pStructs0 = (Struct*)pData0,
                    pStructs1 = (Struct*)(pData0 + SourceStride);

            int SourceW = Context.Width,
                IndexLength = pStructs0->Length;

            float IFracY = 1f - FracY;

            XBits %= IndexLength;
            for (int i = 0; i < Length;)
            {
                for (; XBits < IndexLength; i++)
                {
                    if (i >= Length)
                        return;

                    int Index = (int)XBits;
                    T p00 = Context.Palette[(*pStructs0)[Index]],
                      p10 = Context.Palette[(*pStructs1)[Index]],
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
                            p01 = Context.Palette[(*pStructs0)[Index]];
                            p11 = Context.Palette[(*pStructs1)[Index]];
                        }
                        else
                        {
                            p01 = Context.Palette[(*(pStructs0 + 1))[Index]];
                            p11 = Context.Palette[(*(pStructs1 + 1))[Index]];
                        }
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

        public void ScanLineNearestResizeTo(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref byte* pDest)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            T* pDestPixel = (T*)pDest;
            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length,
                Skip = sizeof(T);

            XBits %= IndexLength;
            while (X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac)))
            {
                Struct Data = *pData;
                for (; XBits < IndexLength;)
                {
                    if (!(X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac))))
                        return;

                    *pDestPixel++ = Context.Palette[Data[XBits]];
                    pDest += Skip;

                    FracX += Step;
                    while (FracX >= 1f)
                    {
                        FracX -= 1f;
                        XBits++;
                        X++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pData++;
                }
            }
        }
        public void ScanLineNearestResizeTo<T2>(ref float FracX, float Step, ref int X, int MaxX, float MaxXFrac, int Y, ref T2* pDest)
            where T2 : unmanaged, IPixel
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            T* pDestPixel = (T*)pDest;
            Struct* pData = (Struct*)((byte*)Context.Scan0 + Offset);
            int IndexLength = pData->Length;

            XBits %= IndexLength;
            while (X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac)))
            {
                Struct Data = *pData;
                for (; XBits < IndexLength;)
                {
                    if (!(X < Context.Width && (X < MaxX || (X == MaxX && FracX < MaxXFrac))))
                        return;

                    T Pixel = Context.Palette[Data[XBits]];
                    pDest++->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);

                    FracX += Step;
                    while (FracX >= 1f)
                    {
                        FracX -= 1f;
                        XBits++;
                        X++;
                    }
                }

                while (XBits >= IndexLength)
                {
                    XBits -= IndexLength;
                    pData++;
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

            Action OverlayHandler;
            if (Color.A == 0 ||
                Color.A == byte.MaxValue)
            {
                if (!Context.Palette.TryGetOrAdd(Color, out int Index))
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

                        XBit += (Sx - CurrentX) * Context.BitsPerPixel;
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

                        XBit += (Sx - CurrentX) * Context.BitsPerPixel;
                        while (XBit >= IndexLength)
                        {
                            pTempDatas++;
                            XBit -= IndexLength;
                        }

                        Struct TempDatas = *pTempDatas;
                        for (int j = Sx; j <= Ex; j++)
                        {
                            T Pixel = Context.Palette[TempDatas[XBit]].ToPixel<T>();
                            Pixel.Overlay(Color.A, Color.R, Color.G, Color.B);

                            if (!Context.Palette.TryGetOrAdd(Pixel, out int Index))
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

                XBits = (long)X * Context.BitsPerPixel;
                Offset = Context.Stride * Y + (XBits >> 3);

                pSeed = (Struct*)((byte*)Context.Scan0 + Offset);
                pStructs = pSeed;

                Indexed = *pStructs;
                XBitIndex = (int)(XBits % Indexed.Length);
                Pixel = Context.Palette[Indexed[XBitIndex]];
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

                    Pixel = Context.Palette[Indexed[XBitIndex]];
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
                Pixel = Context.Palette[Indexed[XBitIndex]];

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
                    Pixel = Context.Palette[Indexed[XBitIndex]];
                }

                Lx = X + 1;

                // Log Region
                Contour[Y].Union(Lx, Rx);

                // Lower ScanLine's Seed
                bool NeedFill = false;
                X = Lx;
                Y++;

                XBits = (long)X * Context.BitsPerPixel;
                Offset = Context.Stride * Y + (XBits >> 3);

                pSeed = (Struct*)((byte*)Context.Scan0 + Offset);

                Indexed = *pSeed;
                XBitIndex = (int)(XBits % Indexed.Length);
                Pixel = Context.Palette[Indexed[XBitIndex]];

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

                            Pixel = Context.Palette[Indexed[XBitIndex]];
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

                Offset = Context.Stride * Y + (((long)X * Context.BitsPerPixel) >> 3);
                pSeed = (Struct*)((byte*)Context.Scan0 + Offset);
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

                            Pixel = Context.Palette[Indexed[XBitIndex]];
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
