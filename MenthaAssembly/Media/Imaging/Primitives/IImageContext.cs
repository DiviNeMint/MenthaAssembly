﻿using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public unsafe interface IImageContext : ICloneable
    {
        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public int BitsPerPixel { get; }

        public int Channels { get; }

        public Type PixelType { get; }

        public Type StructType { get; }

        public IPixel this[int X, int Y] { set; get; }

        public IntPtr Scan0 { get; }

        public IntPtr ScanA { get; }

        public IntPtr ScanR { get; }

        public IntPtr ScanG { get; }

        public IntPtr ScanB { get; }

        public IList<IPixel> Palette { get; }

        /// <summary>
        /// Create a new flipped IImageComtext.
        /// </summary>
        /// <param name="Mode">The flip mode.</param>
        public IImageContext Flip(FlipMode Mode);

        /// <summary>
        /// Creates a new cropped IImageContext.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public IImageContext Crop(int X, int Y, int Width, int Height);

        /// <summary>
        /// Creates a new filtered IImageContext.
        /// </summary>
        /// <param name="Kernel">The kernel used for convolution.</param>
        public IImageContext Convolute(ConvoluteKernel Kernel);

        /// <summary>
        /// Creates a new filtered IImageContext.
        /// </summary>
        /// <param name="Kernel">The kernel used for convolution.</param>
        /// <param name="KernelFactorSum">The factor used for the kernel summing.</param>
        /// <param name="KernelOffsetSum">The offset used for the kernel summing.</param>
        public IImageContext Convolute(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum);

        /// <summary>
        /// Creates a new casted IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IImageContext Cast<T>()
            where T : unmanaged, IPixel;

        /// <summary>
        /// Creates a new casted Indexed IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public IImageContext Cast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed;


        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel;
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel;
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel;
        public void BlockCopy(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB);

        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0) where T : unmanaged, IPixel;
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0) where T : unmanaged, IPixel;
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel;
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB);

        public void BlockPaste<T>(int X, int Y, int Width, int Height, T* Source) where T : unmanaged, IPixel;
        public void BlockPaste<T>(int X, int Y, int Width, int Height, T[] Source) where T : unmanaged, IPixel;
        public void BlockPaste<T>(int X, int Y, int Width, int Height, byte[] Source, int SourceOffset) where T : unmanaged, IPixel;
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceR, byte* SourceG, byte* SourceB);
        public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceR, byte[] SourceG, byte[] SourceB);
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB);
        public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB);

        public void ScanLinePaste<T>(int OffsetX, int Y, int Length, T* Source) where T : unmanaged, IPixel;
        public void ScanLinePaste<T>(int OffsetX, int Y, IEnumerable<T> Source) where T : unmanaged, IPixel;
        public void ScanLinePaste<T>(int OffsetX, int Y, int Length, byte[] Source, int SourceOffset) where T : unmanaged, IPixel;
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceR, byte* SourceG, byte* SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceR, byte[] SourceG, byte[] SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB);

    }
}
