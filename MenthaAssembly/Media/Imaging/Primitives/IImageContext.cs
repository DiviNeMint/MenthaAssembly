using System;
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

        /// <summary>
        /// Creates a bitmap.<para/>
        /// When you no longer need the bitmap, call the <see cref="Win32.Graphic.DeleteObject(IntPtr)"/> function to delete it.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is a handle to a bitmap.<para/>
        /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        /// </returns>
        public IntPtr CreateHBitmap();

        #region BlockCopy
        /// <summary>
        /// Copies the specific block of pixels to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0);
        /// <summary>
        /// Copies the specific block of pixels to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The byte stride of <paramref name="Dest0"/>.</param>
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to a destination array where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in the <paramref name="Dest0"/> where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0);
        /// <summary>
        /// Copies the specific block of pixels to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0);
        /// <summary>
        /// Copies the specific block of pixels to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, int DestStride);

        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in the <paramref name="Dest0"/> where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, int DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, int DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in the <paramref name="Dest0"/> where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, int DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, int DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, int DestStride) where T : unmanaged, IPixel;

        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in the memory pointers.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in target addresses.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, int DestStride);

        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in the memory pointers.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, int DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in target addresses.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, int DestStride);

        #endregion

        #region ScanLineCopy
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination array.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ScanLineCopy(int X, int Y, int Length, byte[] Dest0);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination array where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in destination array where copying should start.</param>
        public void ScanLineCopy(int X, int Y, int Length, byte[] Dest0, int DestOffset);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void ScanLineCopy(int X, int Y, int Length, IntPtr Dest0);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to an address in memory.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ScanLineCopy(int X, int Y, int Length, byte* Dest0);

        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, T* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in destination array where copying should start.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0, int DestOffset) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in destination array where copying should start.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, IntPtr Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, byte* Dest0) where T : unmanaged, IPixel;

        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void ScanLineCopy3(int X, int Y, int Length, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void ScanLineCopy3(int X, int Y, int Length, byte* DestR, byte* DestG, byte* DestB);

        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void ScanLineCopy4(int X, int Y, int Length, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void ScanLineCopy4(int X, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB);

        #endregion

        #region BlockOverlayTo
        internal void BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* Dest0, int DestStride) where T : unmanaged, IPixel;
        internal void BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, int DestStride) where T : unmanaged, IPixel;
        internal void BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, int DestStride) where T : unmanaged, IPixel;

        #endregion

    }
}
