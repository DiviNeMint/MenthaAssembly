using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class ImageOperator<T> : IImageOperator<T>
        where T : unmanaged, IPixel
    {
        public IImageContext<T> Context { get; }

        public ImageOperator(IImageContext<T> Context)
        {
            this.Context = Context;
        }

        public T GetPixel(int X, int Y)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            return *(T*)((byte*)Context.Scan0 + Offset);
        }

        public void SetPixel(int X, int Y, T Pixel)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            *(T*)((byte*)Context.Scan0 + Offset) = Pixel;
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
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            T* pPixels = (T*)((byte*)Context.Scan0 + Offset);

            for (int i = 0; i < Length; i++)
            {
                Adapter.Override(*pPixels);
                Adapter.MoveNext();

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    pPixels++;
                }
            }
        }

        public void ScanLineBilinearResizeTo(int X, int Y, int Length, float FracX, float FracY, float Step, IPixelAdapter<T> Adapter)
        {
            long SourceStride = Context.Stride,
                 Offset = SourceStride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pData0 = (byte*)Context.Scan0 + Offset;
            T* pPixels0 = (T*)pData0,
               pPixels1 = Y + 1 < Context.Height ? (T*)(pData0 + SourceStride) : pPixels0;

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
                    pPixels0++;
                    pPixels1++;
                }
            }
        }

        public void ScanLineRotateTo(int X, int Y, int Length, double FracX, double FracY, double Sin, double Cos, IPixelAdapter<T> Adapter)
        {
            long Offset = Context.Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pScan0 = (byte*)Context.Scan0 + Offset;

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
                    byte* pData0 = pScan0 + Stride * b1 + ((a1 * BitsPerPixel) >> 3);

                    int a2 = (int)Math.Ceiling(FracX),
                        b3 = (int)Math.Ceiling(FracY);

                    double xa13 = FracX - a1,
                           xa24 = a2 - FracX,
                           yb12 = FracY - b1,
                           yb34 = b3 - FracY;
                    T* pData = (T*)pData0;
                    if (xa13 != 0 & xa24 != 0 & yb12 != 0 & yb34 != 0)
                    {
                        T p1 = *pData,
                          p2, p3, p4;

                        if (a2 > a1)
                        {
                            p2 = *++pData;
                            if (b3 > b1)
                            {
                                pData = (T*)(pData0 + Stride);
                                p3 = *pData++;
                                p4 = *pData;
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
                                pData = (T*)(pData0 + Stride);
                                p3 = *pData;
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
                        Adapter.Override(*pData);
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
                Index,
                Tx, LTx;

            T*[] pDatas = new T*[KernelH];

            Index = 0;
            for (; Index < KernelHH; Index++)
            {
                pDatas[Index] = (T*)(pScan0 + MathHelper.Clamp(Y - Index - KernelHH, 0, SourceHeightL) * SourceStride);
                pDatas[KernelH - Index - 1] = (T*)(pScan0 + MathHelper.Clamp(Y - Index + KernelHH, 0, SourceHeightL) * SourceStride);
            }
            pDatas[Index] = (T*)(pScan0 + Y * SourceStride);

            ImagePatch<T> Patch = new ImagePatch<T>(KernelW, KernelH);
            T[] Pixels = null;

            // Init Common Function
            void FillPixelsByX(int Xt)
            {
                Pixels = new T[KernelH];
                for (int j = 0; j < KernelH; j++)
                    Pixels[j] = *(pDatas[j] + Xt);
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

        private delegate void PixelAction(T* pData);
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

            PixelAction Handler = Color.A == 0 || Color.A == byte.MaxValue ?
                pData => *pData = Color :
                pData => pData->Overlay(Color.A, Color.R, Color.G, Color.B);

            void DoOverlay()
            {
                T* pTempPixels = (T*)pPixels;
                int CurrentX = 0;
                for (int i = 0; i < Data.Count; i++)
                {
                    int Sx = Math.Max(Data[i++] + OffsetX, 0),
                        Ex = Math.Min(Data[i] + OffsetX, MaxX);

                    if (Ex < Sx)
                        continue;

                    if (MaxX < Sx)
                        return;

                    pTempPixels += Sx - CurrentX;
                    for (int j = Sx; j <= Ex; j++)
                        Handler(pTempPixels++);

                    CurrentX = Ex + 1;
                }
            }

            DoOverlay();
            while (Enumerator.MoveNext())
            {
                Current = Enumerator.Current;

                long TempY = Current.Key + OffsetY;
                if (MaxY < TempY)
                    return;

                pPixels += Context.Stride * (TempY - Y);
                Y = TempY;
                Data = Current.Value;

                DoOverlay();
            }
        }

        public void BlockOverlay(int X, int Y, IImageContext Source, int OffsetX, int OffsetY, int Width, int Height)
        {
            long Stride = Context.Stride,
                 Offset = Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pPixels = (byte*)Context.Scan0 + Offset;

            for (int j = 0; j < Height; j++)
            {
                T* pScan = (T*)pPixels;
                Source.Operator.ScanLine<T>(X, Y + j, Width, Adapter => Adapter.OverlayTo(pScan++));
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

        public IPixelAdapter<U> GetAdapter<U>(int X, int Y)
            where U : unmanaged, IPixel
        {
            long Stride = Context.Stride,
                 Offset = Stride * Y + ((X * Context.BitsPerPixel) >> 3);
            byte* pScan = (byte*)Context.Scan0 + Offset;

            return Context.PixelType == typeof(U) ? new PixelAdapter<U>(pScan, Stride) :
                                                    new PixelAdapter<T, U>(pScan, Stride);
        }

    }
}
