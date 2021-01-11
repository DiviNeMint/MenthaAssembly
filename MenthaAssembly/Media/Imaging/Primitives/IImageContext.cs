using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging
{
    public unsafe interface IImageContext : ICloneable
    {
        public int Width { get; }

        public int Height { get; }

        public long Stride { get; }

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

        #region Graphic Processings
        /// <summary>
        /// Create a new flipped IImageComtext.
        /// </summary>
        /// <param name="Mode">The flip mode.</param>
        public IImageContext Flip(FlipMode Mode);

        #region Crop
        /// <summary>
        /// Creates a new cropped IImageContext.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public IImageContext Crop(int X, int Y, int Width, int Height);
        /// <summary>
        /// Creates a new cropped IImageContext.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public IImageContext ParallelCrop(int X, int Y, int Width, int Height);
        /// <summary>
        /// Creates a new cropped IImageContext.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public IImageContext ParallelCrop(int X, int Y, int Width, int Height, ParallelOptions Options);

        #endregion

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

        #region Cast
        /// <summary>
        /// Creates a new casted IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public IImageContext Cast<T>() where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new casted IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public IImageContext ParallelCast<T>() where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new casted IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public IImageContext ParallelCast<T>(ParallelOptions Options) where T : unmanaged, IPixel;

        /// <summary>
        /// Creates a new casted Indexed IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        public IImageContext Cast<T, U>() where T : unmanaged, IPixel  where U : unmanaged, IPixelIndexed;
        /// <summary>
        /// Creates a new casted Indexed IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        public IImageContext ParallelCast<T, U>() where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;
        /// <summary>
        /// Creates a new casted Indexed IImageContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public IImageContext ParallelCast<T, U>(ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;

        #endregion

        public void Clear(IPixel Color);
        public void ParallelClear(IPixel Color);

        /// <summary>
        /// Creates a bitmap.<para/>
        /// When you no longer need the bitmap, call the <see cref="Win32.Graphic.DeleteObject(IntPtr)"/> function to delete it.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is a handle to a bitmap.<para/>
        /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        /// </returns>
        public IntPtr CreateHBitmap();

        #endregion

        #region Line Rendering

        #region Line
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLine(Int32Point P0, Int32Point P1, IPixel Color);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLine(int X0, int Y0, int X1, int Y1, IPixel Color);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawLine(Int32Point P0, Int32Point P1, IImageContext Pen);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawLine(Int32Point P0, Int32Point P1, ImageContour Contour, IPixel Fill);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawLine(int X0, int Y0, int X1, int Y1, ImageContour Contour, IPixel Fill);

        #endregion

        #region Arc
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Start">The coordinate of the start.</param>
        /// <param name="End">The coordinate of the end.</param>
        /// <param name="Center">The coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Color">The color for the arc.</param>
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, IPixel Color);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start.</param>
        /// <param name="Sy">The y-coordinate of the start.</param>
        /// <param name="Ex">The x-coordinate of the end.</param>
        /// <param name="Ey">The y-coordinate of the end.</param>
        /// <param name="Cx">The x-coordinate of the arc center point.</param>
        /// <param name="Cy">The y-coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Color">The color for the arc.</param>
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IPixel Color);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Start">The coordinate of the start.</param>
        /// <param name="End">The coordinate of the end.</param>
        /// <param name="Center">The coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>        
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, IImageContext Pen);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start.</param>
        /// <param name="Sy">The y-coordinate of the start.</param>
        /// <param name="Ex">The x-coordinate of the end.</param>
        /// <param name="Ey">The y-coordinate of the end.</param>
        /// <param name="Cx">The x-coordinate of the arc center point.</param>
        /// <param name="Cy">The y-coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Start">The coordinate of the start.</param>
        /// <param name="End">The coordinate of the end.</param>
        /// <param name="Center">The coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start.</param>
        /// <param name="Sy">The y-coordinate of the start.</param>
        /// <param name="Ex">The x-coordinate of the end.</param>
        /// <param name="Ey">The y-coordinate of the end.</param>
        /// <param name="Cx">The x-coordinate of the arc center point.</param>
        /// <param name="Cy">The y-coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill);

        #endregion


        #endregion

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
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride);
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
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride);
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
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride);
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
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride);

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
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride) where T : unmanaged, IPixel;
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
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel;
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
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel;
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
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride) where T : unmanaged, IPixel;
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
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel;
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
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel;
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
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel;

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
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride);
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
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride);
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
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride);
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
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride);

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
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride);
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
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride);
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
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride);
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
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride);

        /// <summary>
        /// Copies the specific block of pixels to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0);
        /// <summary>
        /// Copies the specific block of pixels to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The byte stride of <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride);
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
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0);
        /// <summary>
        /// Copies the specific block of pixels to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0);
        /// <summary>
        /// Copies the specific block of pixels to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride);

        /// <summary>
        /// Copies the specific block of pixels to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The byte stride of <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options);

        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride) where T : unmanaged, IPixel;
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
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride) where T : unmanaged, IPixel;
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
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel;

        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;

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
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB);
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
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride);
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
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride);
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
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB);
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
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride);
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
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB);
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
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride);

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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options);

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
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB);
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
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride);
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
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride);
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
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB);
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
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride);
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
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB);
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
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride);

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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options);
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
        /// <param name="Options">An object that configures the behavior of this operation.</param>
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options);

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
        internal void BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel;
        internal void BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride) where T : unmanaged, IPixel;
        internal void BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride) where T : unmanaged, IPixel;

        #endregion

    }
}
