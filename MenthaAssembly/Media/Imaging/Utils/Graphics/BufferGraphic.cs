using System;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        private unsafe delegate void ScanLineCopyAction1(int OffsetX, int Y, int Length, byte* Dest0, SetPixelValue Handler);
        private unsafe delegate void ScanLineCopyAction3(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB);
        private unsafe delegate void ScanLineCopyAction4(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB);
        private readonly ScanLineCopyAction1 ScanLineCopy1;
        private readonly ScanLineCopyAction3 ScanLineCopy3;
        private readonly ScanLineCopyAction4 ScanLineCopy4;

        public unsafe void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, int DestStride)
            where T : unmanaged, IPixel
        {
            SetPixelValue Handler = CreateSetPixelValue<T>();
            Parallel.For(0, Height, (j) =>
            {
                byte* Dest = Dest0 + DestStride * j;
                ScanLineCopy1(X, Y + j, Width, Dest, Handler);
            });
        }
        public unsafe void BlockCopy(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
        {
            Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLineCopy3(X, Y + j, Width, DestR + Offset, DestG + Offset, DestB + Offset);
            });
        }
        public unsafe void BlockCopy(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
        {
            Parallel.For(0, Height, (j) =>
            {
                long Offset = (long)Width * j;
                ScanLineCopy4(X, Y + j, Width, DestA + Offset, DestR + Offset, DestG + Offset, DestB + Offset);
            });
        }

        public unsafe void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte* Dest0) where T : unmanaged, IPixel
            => ScanLineCopy(OffsetX, Y, Length, Dest0, CreateSetPixelValue<T>());
        public unsafe void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
            => ScanLineCopy3(OffsetX, Y, Length, DestR, DestG, DestB);
        public unsafe void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => ScanLineCopy4(OffsetX, Y, Length, DestA, DestR, DestG, DestB);
        protected unsafe void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0, SetPixelValue Handler)
            => ScanLineCopy1(OffsetX, Y, Length, Dest0, Handler);

        protected unsafe delegate void SetPixelValue(ref byte* Pixel, byte A, byte R, byte G, byte B);
        protected unsafe SetPixelValue CreateSetPixelValue<T>()
            where T : unmanaged, IPixel
        {
            SetPixelValue Result;
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
                };
            }

            return Result;
        }

    }
}
