using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageIndexedOperator<T, Struct> : IImageOperator<T>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public IImageContext<T, Struct> Context { get; }

        public ImageIndexedOperator(IImageContext<T, Struct> Context)
        {
            this.Context = Context;
        }

        public T GetPixel(int X, int Y)
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct Indexed = *(Struct*)((byte*)Context.Scan0 + Offset);
            return Context.Palette[Indexed[XBits % Indexed.Length]];
        }

        public void SetPixel(int X, int Y, T Color)
        {
            if (!Context.Palette.TryGetOrAdd(Color, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y + (XBits >> 3);

            Struct* sScan = (Struct*)((byte*)Context.Scan0 + Offset);
            (*sScan)[XBits % sScan->Length] = Index;
        }

        public void ScanLine<U>(int X, int Y, int Length, Action<IPixelAdapter<U>> Handler)
            where U : unmanaged, IPixel
        {
            IPixelAdapter<U> Adapter = GetAdapter<U>(X, Y);
            for (int i = 0; i < Length; i++, Adapter.MoveNext())
                Handler(Adapter);
        }
        public void ScanLine<U>(int X, int Y, int Length, Predicate<IPixelAdapter<U>> Predicate)
            where U : unmanaged, IPixel
        {
            IPixelAdapter<U> Adapter = GetAdapter<U>(X, Y);
            for (int i = 0; i < Length; i++)
                if (Predicate(Adapter))
                    Adapter.MoveNext();
        }

        public void ScanLineNearestResizeTo(int X, int Y, int Length, float FracX, float Step, IPixelAdapter<T> Adapter)
        {
            IPixelAdapter<T> SourceAdapter = GetAdapter<T>(X, Y);

            T Pixel;
            SourceAdapter.OverrideTo(&Pixel);

            for (int i = 0; i < Length; i++)
            {
                Adapter.Override(Pixel);
                Adapter.MoveNext();

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    SourceAdapter.MoveNext();
                    SourceAdapter.OverrideTo(&Pixel);
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

        public void ScanLineBilinearResizeTo(int X, int Y, int Length, float FracX, float FracY, float Step, IPixelAdapter<T> Adapter)
        {
            long SourceStride = Context.Stride,
                 XBits = (long)X * Context.BitsPerPixel,
                 Offset = SourceStride * Y + (XBits >> 3);

            byte* pData0 = (byte*)Context.Scan0 + Offset;
            Struct* pStructs0 = (Struct*)pData0,
                    pStructs1 = Y + 1 < Context.Height ? (Struct*)(pData0 + SourceStride) : pStructs0;

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

                    Adapter.Override((byte)(p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy),
                                     (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy),
                                     (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy),
                                     (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy));
                    Adapter.MoveNext();

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

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, IPixelAdapter<T> Adapter)
        {
            byte* pScan0 = (byte*)Context.Scan0;

            FracX += X;
            FracY += Y;

            long Stride = Context.Stride;
            int Wo = Context.Width - 1,
                Lo = Context.Height - 1,
                BitsPerPixel = Context.BitsPerPixel;
            for (int i = 0; i < Length; i++)
            {
                int a1 = (int)FracX,
                    b1 = (int)FracY;
                if (0 <= a1 & a1 < Wo & 0 <= b1 & b1 < Lo)
                {
                    int XBits = a1 * BitsPerPixel;
                    byte* pData0 = pScan0 + Stride * b1 + (XBits >> 3);

                    Struct* pStruct = (Struct*)pData0;
                    Struct Indexed = *pStruct;
                    int IndexLength = pStruct->Length;
                    XBits %= IndexLength;

                    int a2 = (int)Math.Ceiling(FracX),
                        b3 = (int)Math.Ceiling(FracY);

                    double xa13 = FracX - a1,
                           xa24 = a2 - FracX,
                           yb12 = FracY - b1,
                           yb34 = b3 - FracY;
                    T p1 = Context.Palette[Indexed[XBits]];
                    if (xa13 != 0 & xa24 != 0 & yb12 != 0 & yb34 != 0)
                    {
                        T p2, p3, p4;

                        if (a2 > a1)
                        {
                            int Temp = XBits + 1;
                            if (Temp >= IndexLength)
                            {
                                Temp = 0;
                                Indexed = *++pStruct;
                            }

                            p2 = Context.Palette[Indexed[Temp]];
                            if (b3 > b1)
                            {
                                pStruct = (Struct*)(pData0 + Stride);
                                Indexed = *pStruct;
                                p3 = Context.Palette[Indexed[XBits++]];

                                if (XBits >= IndexLength)
                                {
                                    XBits = 0;
                                    Indexed = *++pStruct;
                                }

                                p4 = Context.Palette[Indexed[XBits]];
                            }
                            else
                            {
                                p3 = p1;
                                p4 = p2;
                            }
                        }
                        else
                        {
                            p2 = p1;
                            if (b3 > b1)
                            {
                                Indexed = *(Struct*)(pData0 + Context.Stride);
                                p3 = Context.Palette[Indexed[XBits]];
                                p4 = p3;
                            }
                            else
                            {
                                p3 = p1;
                                p4 = p2;
                            }
                        }

                        byte A = (byte)((p1.A * xa24 + p2.A * xa13) * yb34 + (p3.A * xa24 + p4.A * xa13) * yb12),
                             R = (byte)((p1.R * xa24 + p2.R * xa13) * yb34 + (p3.R * xa24 + p4.R * xa13) * yb12),
                             G = (byte)((p1.G * xa24 + p2.G * xa13) * yb34 + (p3.G * xa24 + p4.G * xa13) * yb12),
                             B = (byte)((p1.B * xa24 + p2.B * xa13) * yb34 + (p3.B * xa24 + p4.B * xa13) * yb12);

                        Adapter.Override(A, R, G, B);
                    }
                    else
                    {
                        Adapter.Override(p1);
                    }
                }

                Adapter.MoveNext();

                FracX += Cos;
                FracY -= Sin;
            }
        }

        public void ScanLineFilterTo(int X, int Y, int Length, ImageFilter Filter, IPixelAdapter<T> Adapter)
        {
            byte* pScan0 = (byte*)Context.Scan0;
            long SourceStride = Context.Stride;
            int KernelW = Filter.PatchWidth,
                KernelH = Filter.PatchHeight,
                KernelHW = KernelW >> 1,
                KernelHH = KernelH >> 1,
                SourceWidthL = Context.Width - 1,
                SourceHeightL = Context.Height - 1,
                Index, Tx, LTx, XBits, Offset;

            Struct*[] pDatas = new Struct*[KernelH];

            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                pDatas[Index] = (Struct*)(pScan0 + MathHelper.Clamp(Y - Index - KernelHH, 0, SourceHeightL) * SourceStride);
                pDatas[KernelH - Index - 1] = (Struct*)(pScan0 + MathHelper.Clamp(Y - Index + KernelHH, 0, SourceHeightL) * SourceStride);
            }
            pDatas[Index] = (Struct*)(pScan0 + Y * SourceStride);

            int IndexLength = pDatas[0]->Length;

            List<T> Palette = Context.Palette.Datas;
            ImagePatch<T> Patch = new ImagePatch<T>(KernelW, KernelH);
            T[] Pixels = null;

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                Pixels = new T[KernelH];

                XBits = X * Context.BitsPerPixel;
                Offset = XBits >> 3;
                XBits %= IndexLength;

                for (int j = 0; j < KernelH; j++)
                    Pixels[j] = Palette[(*(pDatas[j] + Offset))[XBits]];
            };

            //Init Block
            Index = -KernelHW;
            LTx = int.MaxValue;
            for (; Index < KernelHW; Index++)
            {
                Tx = MathHelper.Clamp(X + Index, 0, SourceWidthL);
                if (LTx != Tx)
                {
                    FillPixelsByX(Tx);
                    LTx = Tx;
                }
                Patch.Enqueue(Pixels);
            }

            Tx = X + KernelHW;
            ImageFilterArgs Arg = new ImageFilterArgs();
            for (int i = 0; i < Length; i++, Tx++)
            {
                // Next & Enqueue
                FillPixelsByX(MathHelper.Clamp(Tx, 0, SourceWidthL));
                Patch.Enqueue(Pixels);

                // Filter
                Filter.Filter(Patch.Data0, Arg, out byte A, out byte R, out byte G, out byte B);

                // Override
                Adapter.Override(A, R, G, B);
                Adapter.MoveNext();
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

        public IPixelAdapter<U> GetAdapter<U>(int X, int Y)
            where U : unmanaged, IPixel
        {
            int XBits = X * Context.BitsPerPixel;
            long Offset = Context.Stride * Y;
            Struct* pScan = (Struct*)Context.Scan0 + Offset;

            return Context.PixelType == typeof(U) ? new PixelIndexedAdapter<U, Struct>(pScan, XBits, Context.Palette.Handle) :
                                                    new PixelIndexedAdapter<T, U, Struct>(pScan, XBits, Context.Palette.Handle);
        }

    }
}
