using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public unsafe abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        #region BlockCopy
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0)
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy(X, Y, Width, Height, pDest, sizeof(Pixel) * Width);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy(X, Y, Width, Height, pDest, DestStride);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                BlockCopy(X, Y, Width, Height, pDest, DestStride);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0)
            => BlockCopy(X, Y, Width, Height, (byte*)Dest0, sizeof(Pixel) * Width);
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
            => BlockCopy(X, Y, Width, Height, (byte*)Dest0, DestStride);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
            => BlockCopy(X, Y, Width, Height, Dest0, sizeof(Pixel) * Width);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
        {
            for (int j = 0; j < Height; j++)
            {
                byte* Dest = Dest0 + DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, Dest);
            }
        }

        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, Width * sizeof(T));
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0)
            where T : unmanaged, IPixel
        {
            BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, pDest, Width * sizeof(T));
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0)
            where T : unmanaged, IPixel
        {
            BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0)
            where T : unmanaged, IPixel
        {
            BlockCopy<T>(X, Y, Width, Height, Dest0, Width * sizeof(Pixel));
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            PixelOperator<T> PixelOperator = PixelOperator<T>.GetOperator();
            for (int j = 0; j < Height; j++)
            {
                byte* Dest = Dest0 + DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, Dest, PixelOperator);
            }
        }

        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, Width);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => BlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => BlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
            => BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Width);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            for (int j = 0; j < Height; j++)
            {
                long Offset = DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, DestR + Offset, DestG + Offset, DestB + Offset);
            }
        }

        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, Width);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => BlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => BlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Width);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            for (int j = 0; j < Height; j++)
            {
                long Offset = DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, DestA + Offset, DestR + Offset, DestG + Offset, DestB + Offset);
            }
        }

        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0)
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy(X, Y, Width, Height, pDest, sizeof(Pixel) * Width);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy(X, Y, Width, Height, pDest, DestStride);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ParallelBlockCopy(X, Y, Width, Height, pDest, DestStride);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0)
            => ParallelBlockCopy(X, Y, Width, Height, (byte*)Dest0, sizeof(Pixel) * Width);
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
            => ParallelBlockCopy(X, Y, Width, Height, (byte*)Dest0, DestStride);
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
            => ParallelBlockCopy(X, Y, Width, Height, Dest0, sizeof(Pixel) * Width);
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
            => Parallel.For(0, Height, (j) =>
            {
                byte* Dest = Dest0 + DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, Dest);
            });

        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options)
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy(X, Y, Width, Height, pDest, sizeof(Pixel) * Width, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy(X, Y, Width, Height, pDest, DestStride, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options)
            => ParallelBlockCopy(X, Y, Width, Height, (byte*)Dest0, sizeof(Pixel) * Width, Options);
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options)
            => ParallelBlockCopy(X, Y, Width, Height, (byte*)Dest0, DestStride, Options);
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ParallelBlockCopy(X, Y, Width, Height, pDest, DestStride, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options)
            => ParallelBlockCopy(X, Y, Width, Height, Dest0, sizeof(Pixel) * Width, Options);
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options)
            => Parallel.For(0, Height, Options, (j) =>
             {
                 byte* Dest = Dest0 + DestStride * j;
                 Operator.ScanLineOverrideTo(this, X, Y + j, Width, Dest);
             });

        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)pDest, Width * sizeof(T));
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
                ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, pDest, Width * sizeof(T));
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ParallelBlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, Width * sizeof(Pixel));
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            PixelOperator<T> PixelOperator = PixelOperator<T>.GetOperator();

            Parallel.For(0, Height, (j) =>
            {
                byte* Dest = Dest0 + DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, Dest, PixelOperator);
            });
        }

        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)pDest, Width * sizeof(T), Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
                ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, pDest, Width * sizeof(T), Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                ParallelBlockCopy<T>(X, Y, Width, Height, pDest, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ParallelBlockCopy<T>(X, Y, Width, Height, pDest, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, Width * sizeof(Pixel), Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            PixelOperator<T> PixelOperator = PixelOperator<T>.GetOperator();

            Parallel.For(0, Height, Options, (j) =>
            {
                byte* Dest = Dest0 + DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, Dest, PixelOperator);
            });
        }

        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, Width);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ParallelBlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => ParallelBlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width);
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => ParallelBlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride);
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
            => ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Width);
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, DestR + Offset, DestG + Offset, DestB + Offset);
            });

        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, Width, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ParallelBlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
            => ParallelBlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width, Options);
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
            => ParallelBlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride, Options);
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
            => ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Width, Options);
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
            => Parallel.For(0, Height, Options, (j) =>
            {
                long Offset = DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, DestR + Offset, DestG + Offset, DestB + Offset);
            });

        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, Width);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ParallelBlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => ParallelBlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width);
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => ParallelBlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride);
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Width);
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Parallel.For(0, Height, (j) =>
            {
                long Offset = DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, DestA + Offset, DestR + Offset, DestG + Offset, DestB + Offset);
            });

        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, Width, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ParallelBlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ParallelBlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
            => ParallelBlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width, Options);
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
            => ParallelBlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride, Options);
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
            => ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Width, Options);
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
            => Parallel.For(0, Height, Options, (j) =>
            {
                long Offset = DestStride * j;
                Operator.ScanLineOverrideTo(this, X, Y + j, Width, DestA + Offset, DestR + Offset, DestG + Offset, DestB + Offset);
            });

        #endregion

        #region ScanLineCopy
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] Dest0)
        {
            fixed (byte* pDest = &Dest0[0])
                ScanLineCopy(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ScanLineCopy(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy(int OffsetX, int Y, int Length, IntPtr Dest0)
            => ScanLineCopy(OffsetX, Y, Length, (byte*)Dest0);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0)
            => Operator.ScanLineOverrideTo(this, OffsetX, Y, Length, Dest0);

        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0)
            where T : unmanaged, IPixel
        {
            ScanLineCopy<T>(OffsetX, Y, Length, (byte*)Dest0);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                ScanLineCopy<T>(OffsetX, Y, Length, (byte*)pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
                ScanLineCopy<T>(OffsetX, Y, Length, (byte*)pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                ScanLineCopy<T>(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ScanLineCopy<T>(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, IntPtr Dest0)
            where T : unmanaged, IPixel
        {
            ScanLineCopy<T>(OffsetX, Y, Length, (byte*)Dest0);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte* Dest0)
            where T : unmanaged, IPixel
        {
            Operator.ScanLineOverrideTo(this, OffsetX, Y, Length, Dest0, PixelOperator<T>.GetOperator());
        }

        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ScanLineCopy3(OffsetX, Y, Length, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ScanLineCopy3(OffsetX, Y, Length, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy3(int OffsetX, int Y, int Length, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => ScanLineCopy3(OffsetX, Y, Length, (byte*)DestR, (byte*)DestG, (byte*)DestB);
        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
            => Operator.ScanLineOverrideTo(this, OffsetX, Y, Length, DestR, DestG, DestB);

        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ScanLineCopy4(OffsetX, Y, Length, pDestA, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ScanLineCopy4(OffsetX, Y, Length, pDestA, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy4(int OffsetX, int Y, int Length, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => ScanLineCopy4(OffsetX, Y, Length, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB);
        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Operator.ScanLineOverrideTo(this, OffsetX, Y, Length, DestA, DestR, DestG, DestB);

        #endregion

        #region BlockOverlayTo
        void IImageContext.BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
        {
            PixelOperator<T> PixelOperator = PixelOperator<T>.GetOperator();
            for (int j = 0; j < Height; j++)
            {
                Operator.ScanLineOverlayTo(this, X, Y + j, Width, Dest0, PixelOperator);
                Dest0 += DestStride;
            }
        }
        void IImageContext.BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            PixelOperator<T> PixelOperator = PixelOperator<T>.GetOperator();
            for (int j = 0; j < Height; j++)
            {
                Operator.ScanLineOverlayTo(this, X, Y + j, Width, DestR, DestG, DestB, PixelOperator);
                DestR += DestStride;
                DestG += DestStride;
                DestB += DestStride;
            }
        }
        void IImageContext.BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            PixelOperator<T> PixelOperator = PixelOperator<T>.GetOperator();
            for (int j = 0; j < Height; j++)
            {
                Operator.ScanLineOverlayTo(this, X, Y + j, Width, DestA, DestR, DestG, DestB, PixelOperator);
                DestA += DestStride;
                DestR += DestStride;
                DestG += DestStride;
                DestB += DestStride;
            }
        }

        #endregion

        //public void BlockPaste<T>(int X, int Y, int Width, int Height, T* Source)
        //    where T : unmanaged, IPixel
        //{
        //    byte* pSource = (byte*)Source;
        //    int sStride = sizeof(T) * Width;
        //    if (this.Channels == 1)
        //    {
        //        byte* dScan0 = (byte*)this.Scan0;
        //        if (StructType == typeof(T))
        //        {
        //            Parallel.For(0, Height, (j) =>
        //            {
        //                long dOffset = (long)this.Stride * (Y + j) + X,
        //                     sOffset = (long)sStride * j;
        //                T* dScanT = (T*)(dScan0 + dOffset),
        //                   sScanT = (T*)(pSource + sOffset);

        //                for (int i = 0; i < Width; i++)
        //                    *dScanT++ = *sScanT++;
        //            });
        //        }
        //        else
        //        {
        //            Parallel.For(0, Height, (j) =>
        //            {
        //                long dOffset = (long)this.Stride * (Y + j) + X,
        //                     sOffset = (long)sStride * j;
        //                byte* dScan = dScan0 + dOffset;
        //                T* sScanT = (T*)(pSource + sOffset);

        //                for (int i = 0; i < Width; i++)
        //                {
        //                    PixelCopyHandler(ref dScan, sScanT->A, sScanT->R, sScanT->G, sScanT->B);
        //                    sScanT++;
        //                    dScan++;
        //                }
        //            });
        //        }
        //    }
        //    else if (this.Channels == 4)
        //    {
        //        byte* dScanA = (byte*)ScanA;
        //        byte* dScanR = (byte*)ScanR;
        //        byte* dScanG = (byte*)ScanG;
        //        byte* dScanB = (byte*)ScanB;
        //        Parallel.For(0, Height, (j) =>
        //        {
        //            long dOffset = (long)this.Stride * (Y + j) + X,
        //                 sOffset = (long)sStride * j;

        //            byte* dScanA0 = dScanA + dOffset,
        //                  dScanR0 = dScanR + dOffset,
        //                  dScanG0 = dScanG + dOffset,
        //                  dScanB0 = dScanB + dOffset;
        //            T* sScanT = (T*)(pSource + sOffset);

        //            *dScanA0++ = sScanT->A;
        //            *dScanR0++ = sScanT->R;
        //            *dScanG0++ = sScanT->G;
        //            *dScanB0++ = sScanT->B;
        //        });
        //    }
        //    else
        //    {
        //        byte* dScanR = (byte*)ScanR;
        //        byte* dScanG = (byte*)ScanG;
        //        byte* dScanB = (byte*)ScanB;
        //        Parallel.For(0, Height, (j) =>
        //        {
        //            long dOffset = (long)this.Stride * (Y + j) + X,
        //                 sOffset = (long)sStride * j;

        //            byte* dScanR0 = dScanR + dOffset,
        //                  dScanG0 = dScanG + dOffset,
        //                  dScanB0 = dScanB + dOffset;
        //            T* sScanT = (T*)(pSource + sOffset);

        //            *dScanR0++ = sScanT->R;
        //            *dScanG0++ = sScanT->G;
        //            *dScanB0++ = sScanT->B;
        //        });
        //    }
        //}
        //public void BlockPaste<T>(int X, int Y, int Width, int Height, T[] Source0)
        //    where T : unmanaged, IPixel
        //{
        //    fixed (T* pSource = &Source0[0])
        //        BlockPaste(X, Y, Width, Height, pSource);
        //}
        //public void BlockPaste<T>(int X, int Y, int Width, int Height, byte[] Source0, int SourceOffset)
        //    where T : unmanaged, IPixel
        //{
        //    fixed (byte* pSource = &Source0[SourceOffset])
        //        BlockPaste(X, Y, Width, Height, (T*)pSource);
        //}
        //public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceR, byte* SourceG, byte* SourceB)
        //    => Parallel.For(0, Height, (j) =>
        //    {
        //        long Offset = (long)Width * j;
        //        ScanLinePaste3(X, Y + j, Width, SourceR + Offset, SourceG + Offset, SourceB + Offset);
        //    });
        //public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        //{
        //    fixed (byte* pSourceR = &SourceR[0],
        //                 pSourceG = &SourceG[0],
        //                 pSourceB = &SourceB[0])
        //        BlockPaste(X, Y, Width, Height, pSourceR, pSourceG, pSourceB);
        //}
        //public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
        //    => Parallel.For(0, Height, (j) =>
        //    {
        //        long Offset = (long)Width * j;
        //        ScanLinePaste4(X, Y + j, Width, SourceA + Offset, SourceR + Offset, SourceG + Offset, SourceB + Offset);
        //    });
        //public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        //{
        //    fixed (byte* pSourceA = &SourceA[0],
        //                 pSourceR = &SourceR[0],
        //                 pSourceG = &SourceG[0],
        //                 pSourceB = &SourceB[0])
        //        BlockPaste(X, Y, Width, Height, pSourceA, pSourceR, pSourceG, pSourceB);
        //}

        //public void ScanLinePaste<T>(int OffsetX, int Y, int Length, T* Source)
        //    where T : unmanaged, IPixel
        //{
        //    long Offset = (long)this.Stride * Y + OffsetX;
        //    if (this.Channels == 1)
        //    {
        //        byte* dScan0 = (byte*)Scan0 + Offset;
        //        if (StructType == typeof(T))
        //        {
        //            T* dScanT = (T*)dScan0;

        //            for (int i = 0; i < Length; i++)
        //                *dScanT++ = *Source++;
        //        }
        //        else
        //        {
        //            for (int i = 0; i < Length; i++)
        //            {
        //                PixelCopyHandler(ref dScan0, Source->A, Source->R, Source->G, Source->B);
        //                Source++;
        //                dScan0++;
        //            }
        //        }
        //    }
        //    else if (this.Channels == 4)
        //    {
        //        byte* dScanA = (byte*)ScanA + Offset;
        //        byte* dScanR = (byte*)ScanR + Offset;
        //        byte* dScanG = (byte*)ScanG + Offset;
        //        byte* dScanB = (byte*)ScanB + Offset;

        //        for (int i = 0; i < Length; i++)
        //        {
        //            *dScanA++ = Source->A;
        //            *dScanR++ = Source->R;
        //            *dScanG++ = Source->G;
        //            *dScanB++ = Source->B;
        //            Source++;
        //        }
        //    }
        //    else
        //    {
        //        byte* dScanR = (byte*)ScanR + Offset;
        //        byte* dScanG = (byte*)ScanG + Offset;
        //        byte* dScanB = (byte*)ScanB + Offset;

        //        for (int i = 0; i < Length; i++)
        //        {
        //            *dScanR++ = Source->R;
        //            *dScanG++ = Source->G;
        //            *dScanB++ = Source->B;
        //            Source++;
        //        }
        //    }
        //}
        //public void ScanLinePaste<T>(int OffsetX, int Y, IEnumerable<T> Source)
        //    where T : unmanaged, IPixel
        //{
        //    IEnumerator<T> Enumerator = Source.GetEnumerator();

        //    long Offset = (long)this.Stride * Y + OffsetX;
        //    if (this.Channels == 1)
        //    {
        //        byte* dScan0 = (byte*)Scan0 + Offset;
        //        if (StructType == typeof(T))
        //        {
        //            T* dScanT = (T*)dScan0;

        //            for (; Enumerator.MoveNext(); OffsetX++)
        //                *dScanT++ = Enumerator.Current;
        //        }
        //        else
        //        {
        //            for (; Enumerator.MoveNext(); OffsetX++)
        //            {
        //                T Pixel = Enumerator.Current;
        //                PixelCopyHandler(ref dScan0, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        //                dScan0++;
        //            }
        //        }
        //    }
        //    else if (this.Channels == 4)
        //    {
        //        byte* dScanA = (byte*)ScanA + Offset;
        //        byte* dScanR = (byte*)ScanR + Offset;
        //        byte* dScanG = (byte*)ScanG + Offset;
        //        byte* dScanB = (byte*)ScanB + Offset;

        //        for (; Enumerator.MoveNext(); OffsetX++)
        //        {
        //            T Pixel = Enumerator.Current;
        //            *dScanA++ = Pixel.A;
        //            *dScanR++ = Pixel.R;
        //            *dScanG++ = Pixel.G;
        //            *dScanB++ = Pixel.B;
        //        }
        //    }
        //    else
        //    {
        //        byte* dScanR = (byte*)ScanR + Offset;
        //        byte* dScanG = (byte*)ScanG + Offset;
        //        byte* dScanB = (byte*)ScanB + Offset;

        //        for (; Enumerator.MoveNext(); OffsetX++)
        //        {
        //            T Pixel = Enumerator.Current;
        //            *dScanR++ = Pixel.R;
        //            *dScanG++ = Pixel.G;
        //            *dScanB++ = Pixel.B;
        //        }
        //    }
        //}
        //public void ScanLinePaste<T>(int OffsetX, int Y, int Length, byte[] Source, int SourceOffset)
        //    where T : unmanaged, IPixel
        //{
        //    fixed (byte* pSource = &Source[SourceOffset])
        //        ScanLinePaste(OffsetX, Y, Length, (T*)pSource);
        //}
        //public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceR, byte* SourceG, byte* SourceB)
        //    => ScanLinePaste3(OffsetX, Y, Length, SourceR, SourceG, SourceB);
        //public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        //{
        //    fixed (byte* pSourceR = &SourceR[0],
        //                 pSourceG = &SourceG[0],
        //                 pSourceB = &SourceB[0])
        //        ScanLinePaste(OffsetX, Y, Length, pSourceR, pSourceG, pSourceB);
        //}
        //public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
        //    => ScanLinePaste4(OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB);
        //public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB)
        //{
        //    fixed (byte* pSourceA = &SourceA[0],
        //                 pSourceR = &SourceR[0],
        //                 pSourceG = &SourceG[0],
        //                 pSourceB = &SourceB[0])
        //        ScanLinePaste(OffsetX, Y, Length, pSourceA, pSourceR, pSourceG, pSourceB);
        //}

    }
}