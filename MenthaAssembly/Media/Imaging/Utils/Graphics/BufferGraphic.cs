using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public unsafe abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        internal readonly Func<byte, byte, byte, byte, Pixel> ToPixel;
        internal readonly Func<int, int, Pixel> GetPixel;
        internal readonly Action<int, int, Pixel> SetPixel;

        internal protected delegate void CopyPixelAction(ref byte* Pixel, byte A, byte R, byte G, byte B);
        internal protected static CopyPixelAction CreateCopyPixelHandler<T>()
            where T : unmanaged, IPixel
        {
            CopyPixelAction Result;
            Type TType = typeof(T);
            if (TType == typeof(BGRA))
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel++ = R;
                    *Pixel = A;
                };
            }
            else if (TType == typeof(RGBA))
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel++ = B;
                    *Pixel = A;
                };
            }
            else if (TType == typeof(ARGB))
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = A;
                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel = B;
                };
            }
            else if (TType == typeof(ABGR))
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = A;
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel = R;
                };
            }
            else if (TType == typeof(BGR))
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel = R;
                };
            }
            else if (TType == typeof(RGB))
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel = B;
                };
            }
            else if (TType == typeof(Gray8))
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                    *Pixel = (byte)((R * 30 +
                                     G * 59 +
                                     B * 11 + 50) / 100);
            }
            else
            {
                Result = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    dynamic Result = new BGRA(B, G, R, A);
                    *(T*)Pixel = (T)Result;
                    Pixel += sizeof(T) - 1;
                };
            }

            return Result;
        }
        internal protected readonly CopyPixelAction CopyPixelHandler;

        private delegate void ScanLineCopyAction1(int OffsetX, int Y, int Length, byte* Ptr0, CopyPixelAction Handler);
        private delegate void ScanLineBaseAction3(int OffsetX, int Y, int Length, byte* PtrR, byte* PtrG, byte* PtrB);
        private delegate void ScanLineBaseAction4(int OffsetX, int Y, int Length, byte* PtrA, byte* PtrR, byte* PtrG, byte* PtrB);
        private readonly ScanLineCopyAction1 ScanLineCopy1;
        private readonly ScanLineBaseAction3 ScanLineCopy3;
        private readonly ScanLineBaseAction4 ScanLineCopy4;

        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0)
            where T : unmanaged, IPixel
        {
            if (this.Channels == 1 &&
                StructType == typeof(T))
            {
                byte* sScan0 = (byte*)this.Scan0;
                Parallel.For(0, Height, (j) =>
                {
                    long Offset = this.Stride * (Y + j) + (((long)X * BitsPerPixel) >> 3);
                    T* sScanT = (T*)(sScan0 + Offset),
                       dScanT = Dest0 + j * Height;

                    for (int i = 0; i < Width; i++)
                        *dScanT++ = *sScanT++;
                });
            }
            else
            {
                BlockCopy(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), CreateCopyPixelHandler<T>());
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                BlockCopy(X, Y, Width, Height, pDest);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
        {
            if (this.Channels == 1)
            {
                byte* sScan0 = (byte*)this.Scan0;
                Parallel.For(0, Height, (j) =>
                {
                    long Offset = this.Stride * (Y + j) + (((long)X * BitsPerPixel) >> 3);
                    Pixel* sScanT = (Pixel*)(sScan0 + Offset),
                           dScanT = (Pixel*)(Dest0 + j * Height);

                    for (int i = 0; i < Width; i++)
                        *dScanT++ = *sScanT++;
                });
            }
            else
            {
                BlockCopy(X, Y, Width, Height, Dest0, this.Stride, CopyPixelHandler);
            }
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset)
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                BlockCopy(X, Y, Width, Height, pDest);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                BlockCopy(X, Y, Width, Height, (T*)pDest);
        }
        protected void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, int DestStride, CopyPixelAction Handler)
            => Parallel.For(0, Height, (j) =>
            {
                byte* Dest = Dest0 + DestStride * j;
                ScanLineCopy1(X, Y + j, Width, Dest, Handler);
            });
        public void BlockCopy(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLineCopy3(X, Y + j, Width, DestR + Offset, DestG + Offset, DestB + Offset);
            });
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy(X, Y, Width, Height, pDestR, pDestG, pDestB);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLineCopy4(X, Y + j, Width, DestA + Offset, DestR + Offset, DestG + Offset, DestB + Offset);
            });
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB);
        }

        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0)
            where T : unmanaged, IPixel
        {
            if (this.Channels == 1 &&
                StructType == typeof(T))
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                T* sScanT = (T*)((byte*)this.Scan0 + Offset);

                for (int i = 0; i < Length; i++)
                    *Dest0++ = *sScanT++;
            }
            else
            {
                ScanLineCopy(OffsetX, Y, Length, (byte*)Dest0, CreateCopyPixelHandler<T>());
            }
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                ScanLineCopy(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0)
        {
            if (this.Channels == 1)
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                Pixel* sScanT = (Pixel*)((byte*)this.Scan0 + Offset),
                       dScanT = (Pixel*)Dest0;

                for (int i = 0; i < Length; i++)
                    *dScanT++ = *sScanT++;
            }
            else
            {
                ScanLineCopy(OffsetX, Y, Length, Dest0, CopyPixelHandler);
            }
        }
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ScanLineCopy(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ScanLineCopy(OffsetX, Y, Length, (T*)pDest);
        }
        protected void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0, CopyPixelAction Handler)
            => ScanLineCopy1(OffsetX, Y, Length, Dest0, Handler);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
            => ScanLineCopy3(OffsetX, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ScanLineCopy(OffsetX, Y, Length, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => ScanLineCopy4(OffsetX, Y, Length, DestA, DestR, DestG, DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ScanLineCopy(OffsetX, Y, Length, pDestA, pDestR, pDestG, pDestB);
        }

        private readonly ScanLineBaseAction3 ScanLinePaste3;
        private readonly ScanLineBaseAction4 ScanLinePaste4;

        public void BlockPaste<T>(int X, int Y, int Width, int Height, T* Source)
            where T : unmanaged, IPixel
        {
            byte* pSource = (byte*)Source;
            int sStride = sizeof(T) * Width;
            if (this.Channels == 1)
            {
                byte* dScan0 = (byte*)this.Scan0;
                if (StructType == typeof(T))
                {
                    Parallel.For(0, Height, (j) =>
                    {
                        long dOffset = (long)this.Stride * (Y + j) + X,
                             sOffset = (long)sStride * j;
                        T* dScanT = (T*)(dScan0 + dOffset),
                           sScanT = (T*)(pSource + sOffset);

                        for (int i = 0; i < Width; i++)
                            *dScanT++ = *sScanT++;
                    });
                }
                else
                {
                    Parallel.For(0, Height, (j) =>
                    {
                        long dOffset = (long)this.Stride * (Y + j) + X,
                             sOffset = (long)sStride * j;
                        byte* dScan = dScan0 + dOffset;
                        T* sScanT = (T*)(pSource + sOffset);

                        for (int i = 0; i < Width; i++)
                        {
                            CopyPixelHandler(ref dScan, sScanT->A, sScanT->R, sScanT->G, sScanT->B);
                            sScanT++;
                            dScan++;
                        }
                    });
                }
            }
            else if (this.Channels == 4)
            {
                byte* dScanA = (byte*)ScanA;
                byte* dScanR = (byte*)ScanR;
                byte* dScanG = (byte*)ScanG;
                byte* dScanB = (byte*)ScanB;
                Parallel.For(0, Height, (j) =>
                {
                    long dOffset = (long)this.Stride * (Y + j) + X,
                         sOffset = (long)sStride * j;

                    byte* dScanA0 = dScanA + dOffset,
                          dScanR0 = dScanR + dOffset,
                          dScanG0 = dScanG + dOffset,
                          dScanB0 = dScanB + dOffset;
                    T* sScanT = (T*)(pSource + sOffset);

                    *dScanA0++ = sScanT->A;
                    *dScanR0++ = sScanT->R;
                    *dScanG0++ = sScanT->G;
                    *dScanB0++ = sScanT->B;
                });
            }
            else
            {
                byte* dScanR = (byte*)ScanR;
                byte* dScanG = (byte*)ScanG;
                byte* dScanB = (byte*)ScanB;
                Parallel.For(0, Height, (j) =>
                {
                    long dOffset = (long)this.Stride * (Y + j) + X,
                         sOffset = (long)sStride * j;

                    byte* dScanR0 = dScanR + dOffset,
                          dScanG0 = dScanG + dOffset,
                          dScanB0 = dScanB + dOffset;
                    T* sScanT = (T*)(pSource + sOffset);

                    *dScanR0++ = sScanT->R;
                    *dScanG0++ = sScanT->G;
                    *dScanB0++ = sScanT->B;
                });
            }
        }
        public void BlockPaste<T>(int X, int Y, int Width, int Height, T[] Source0)
            where T : unmanaged, IPixel
        {
            fixed (T* pSource = &Source0[0])
                BlockPaste(X, Y, Width, Height, pSource);
        }
        public void BlockPaste<T>(int X, int Y, int Width, int Height, byte[] Source0, int SourceOffset)
            where T : unmanaged, IPixel
        {
            fixed (byte* pSource = &Source0[SourceOffset])
                BlockPaste(X, Y, Width, Height, (T*)pSource);
        }
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceR, byte* SourceG, byte* SourceB)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLinePaste3(X, Y + j, Width, SourceR + Offset, SourceG + Offset, SourceB + Offset);
            });
        public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        {
            fixed (byte* pSourceR = &SourceR[0],
                         pSourceG = &SourceG[0],
                         pSourceB = &SourceB[0])
                BlockPaste(X, Y, Width, Height, pSourceR, pSourceG, pSourceB);
        }
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLinePaste4(X, Y + j, Width, SourceA + Offset, SourceR + Offset, SourceG + Offset, SourceB + Offset);
            });
        public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        {
            fixed (byte* pSourceA = &SourceA[0],
                         pSourceR = &SourceR[0],
                         pSourceG = &SourceG[0],
                         pSourceB = &SourceB[0])
                BlockPaste(X, Y, Width, Height, pSourceA, pSourceR, pSourceG, pSourceB);
        }

        public void ScanLinePaste<T>(int OffsetX, int Y, int Length, T* Source)
            where T : unmanaged, IPixel
        {
            long Offset = (long)this.Stride * Y + OffsetX;
            if (this.Channels == 1)
            {
                byte* dScan0 = (byte*)Scan0 + Offset;
                if (StructType == typeof(T))
                {
                    T* dScanT = (T*)dScan0;

                    for (int i = 0; i < Length; i++)
                        *dScanT++ = *Source++;
                }
                else
                {
                    for (int i = 0; i < Length; i++)
                    {
                        CopyPixelHandler(ref dScan0, Source->A, Source->R, Source->G, Source->B);
                        Source++;
                        dScan0++;
                    }
                }
            }
            else if (this.Channels == 4)
            {
                byte* dScanA = (byte*)ScanA + Offset;
                byte* dScanR = (byte*)ScanR + Offset;
                byte* dScanG = (byte*)ScanG + Offset;
                byte* dScanB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *dScanA++ = Source->A;
                    *dScanR++ = Source->R;
                    *dScanG++ = Source->G;
                    *dScanB++ = Source->B;
                    Source++;
                }
            }
            else
            {
                byte* dScanR = (byte*)ScanR + Offset;
                byte* dScanG = (byte*)ScanG + Offset;
                byte* dScanB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *dScanR++ = Source->R;
                    *dScanG++ = Source->G;
                    *dScanB++ = Source->B;
                    Source++;
                }
            }
        }
        public void ScanLinePaste<T>(int OffsetX, int Y, IEnumerable<T> Source)
            where T : unmanaged, IPixel
        {
            IEnumerator<T> Enumerator = Source.GetEnumerator();

            long Offset = (long)this.Stride * Y + OffsetX;
            if (this.Channels == 1)
            {
                byte* dScan0 = (byte*)Scan0 + Offset;
                if (StructType == typeof(T))
                {
                    T* dScanT = (T*)dScan0;

                    for (; Enumerator.MoveNext(); OffsetX++)
                        *dScanT++ = Enumerator.Current;
                }
                else
                {
                    for (; Enumerator.MoveNext(); OffsetX++)
                    {
                        T Pixel = Enumerator.Current;
                        CopyPixelHandler(ref dScan0, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                        dScan0++;
                    }
                }
            }
            else if (this.Channels == 4)
            {
                byte* dScanA = (byte*)ScanA + Offset;
                byte* dScanR = (byte*)ScanR + Offset;
                byte* dScanG = (byte*)ScanG + Offset;
                byte* dScanB = (byte*)ScanB + Offset;

                for (; Enumerator.MoveNext(); OffsetX++)
                {
                    T Pixel = Enumerator.Current;
                    *dScanA++ = Pixel.A;
                    *dScanR++ = Pixel.R;
                    *dScanG++ = Pixel.G;
                    *dScanB++ = Pixel.B;
                }
            }
            else
            {
                byte* dScanR = (byte*)ScanR + Offset;
                byte* dScanG = (byte*)ScanG + Offset;
                byte* dScanB = (byte*)ScanB + Offset;

                for (; Enumerator.MoveNext(); OffsetX++)
                {
                    T Pixel = Enumerator.Current;
                    *dScanR++ = Pixel.R;
                    *dScanG++ = Pixel.G;
                    *dScanB++ = Pixel.B;
                }
            }
        }
        public void ScanLinePaste<T>(int OffsetX, int Y, int Length, byte[] Source, int SourceOffset)
            where T : unmanaged, IPixel
        {
            fixed (byte* pSource = &Source[SourceOffset])
                ScanLinePaste(OffsetX, Y, Length, (T*)pSource);
        }
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceR, byte* SourceG, byte* SourceB)
            => ScanLinePaste3(OffsetX, Y, Length, SourceR, SourceG, SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        {
            fixed (byte* pSourceR = &SourceR[0],
                         pSourceG = &SourceG[0],
                         pSourceB = &SourceB[0])
                ScanLinePaste(OffsetX, Y, Length, pSourceR, pSourceG, pSourceB);
        }
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
            => ScanLinePaste4(OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        {
            fixed (byte* pSourceA = &SourceA[0],
                         pSourceR = &SourceR[0],
                         pSourceG = &SourceG[0],
                         pSourceB = &SourceB[0])
                ScanLinePaste(OffsetX, Y, Length, pSourceA, pSourceR, pSourceG, pSourceB);
        }

    }
}