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
        protected delegate void CopyPixelAction(ref byte* Pixel, byte A, byte R, byte G, byte B);
        protected CopyPixelAction CreateCopyPixelHandler<T>()
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
        protected CopyPixelAction CopyPixelHandler;

        private delegate void ScanLineCopyAction1(int OffsetX, int Y, int Length, byte* Ptr0, CopyPixelAction Handler);
        private delegate void ScanLineBaseAction3(int OffsetX, int Y, int Length, byte* PtrR, byte* PtrG, byte* PtrB);
        private delegate void ScanLineBaseAction4(int OffsetX, int Y, int Length, byte* PtrA, byte* PtrR, byte* PtrG, byte* PtrB);
        private readonly ScanLineCopyAction1 ScanLineCopy1;
        private readonly ScanLineBaseAction3 ScanLineCopy3;
        private readonly ScanLineBaseAction4 ScanLineCopy4;


        internal void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, Func<int, int, bool> Condition)
            where T : unmanaged, IPixel
        {
            if (StructType == typeof(T) &&
                (_Scan0.HasValue || Data0 != null))
            {
                byte* sScan0 = (byte*)this.Scan0;
                Parallel.For(0, Height, (j) =>
                {
                    int Ry = Y + j,
                        Rx = X;
                    long Offset = this.Stride * Ry + (((long)X * BitsPerPixel) >> 3);
                    T* sScanT = (T*)(sScan0 + Offset),
                       dScanT = Dest0 + j * Height;

                    for (int i = 0; i < Width; i++)
                    {
                        if (Condition(Rx++,Ry))
                            *dScanT++ = *sScanT;

                        sScanT++;
                    }
                });
            }
            else
            {
                BlockCopy(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), CreateCopyPixelHandler<T>());
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0)
            where T : unmanaged, IPixel
        {
            if (StructType == typeof(T) &&
                (_Scan0.HasValue || Data0 != null))
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
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
        {
            if (_Scan0.HasValue || Data0 != null)
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
        public void BlockCopy(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLineCopy4(X, Y + j, Width, DestA + Offset, DestR + Offset, DestG + Offset, DestB + Offset);
            });

        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0)
            where T : unmanaged, IPixel
        {
            if (StructType == typeof(T) &&
                (_Scan0.HasValue || Data0 != null))
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
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0)
        {
            if (_Scan0.HasValue || Data0 != null)
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
        protected void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0, CopyPixelAction Handler)
            => ScanLineCopy1(OffsetX, Y, Length, Dest0, Handler);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
            => ScanLineCopy3(OffsetX, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => ScanLineCopy4(OffsetX, Y, Length, DestA, DestR, DestG, DestB);

        private readonly ScanLineBaseAction3 ScanLinePaste3;
        private readonly ScanLineBaseAction4 ScanLinePaste4;

        public void BlockPaste<T>(int X, int Y, int Width, int Height, T* Source)
            where T : unmanaged, IPixel
        {
            byte* pSource = (byte*)Source;
            int sStride = sizeof(T) * Width;
            if (_Scan0.HasValue || Data0 != null)
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
            else if (_ScanA.HasValue || DataA != null)
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
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceR, byte* SourceG, byte* SourceB)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLinePaste3(X, Y + j, Width, SourceR + Offset, SourceG + Offset, SourceB + Offset);
            });
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLinePaste4(X, Y + j, Width, SourceA + Offset, SourceR + Offset, SourceG + Offset, SourceB + Offset);
            });

        public void ScanLinePaste<T>(int OffsetX, int Y, int Length, T* Source)
            where T : unmanaged, IPixel
        {
            long Offset = (long)this.Stride * Y + OffsetX;
            if (_Scan0.HasValue || Data0 != null)
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
            else if (_ScanA.HasValue || DataA != null)
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
            if (_Scan0.HasValue || Data0 != null)
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
            else if (_ScanA.HasValue || DataA != null)
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
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceR, byte* SourceG, byte* SourceB)
            => ScanLinePaste3(OffsetX, Y, Length, SourceR, SourceG, SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
            => ScanLinePaste4(OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB);

    }
}
