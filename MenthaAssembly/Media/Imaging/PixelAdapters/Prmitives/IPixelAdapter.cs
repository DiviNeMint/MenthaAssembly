using System;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the pixel adapter with the specified pixel type in image.
    /// </summary>
    public unsafe interface IPixelAdapter<T> : IPixelAdapter
        where T : unmanaged, IPixel
    {
        /// <summary>
        /// Overrides the current color components with the specified pixel.
        /// </summary>
        /// <param name="Pixel">The specified pixel.</param>
        public void Override(T Pixel);

        /// <summary>
        /// Overrides the current color components with the color components of the specified adapter.
        /// </summary>
        /// <param name="Adapter">The specified pixel.</param>
        public void Override(PixelAdapter<T> Adapter);

        /// <summary>
        /// Overrides the current color components to the specified data pointer.
        /// </summary>
        /// <param name="pData">The specified data pointer.</param>
        public void OverrideTo(T* pData);

        /// <summary>
        /// Overlays the current color components with the specified pixel.
        /// </summary>
        /// <param name="Pixel">The specified pixel.</param>
        public void Overlay(T Pixel);

        /// <summary>
        /// Overlays the current color components with the color components of the specified adapter.
        /// </summary>
        /// <param name="Adapter">The specified pixel.</param>
        public void Overlay(PixelAdapter<T> Adapter);

        /// <summary>
        /// Overlays the current color components to the specified data pointer.
        /// </summary>
        /// <param name="pData">The specified data pointer.</param>
        public void OverlayTo(T* pData);

        /// <summary>
        /// Creates a new <see cref="ImageContext{T}"/> that is a copy of the current instance.
        /// </summary>
        public ImageContext<T> ToImageContext();

        /// <summary>
        /// Creates a new <see cref="ImageContext{T}"/> that is a copy of the current instance.
        /// </summary>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> ToImageContext(ParallelOptions Options);

        /// <summary>
        /// Creates a new <see cref="IPixelAdapter{T}"/> that is a copy of the current instance.
        /// </summary>
        public new IPixelAdapter<T> Clone();

    }

    /// <summary>
    /// Represents the pixel adapter in image.
    /// </summary>
    public unsafe interface IPixelAdapter : IImageAdapter, IPixel
    {
        /// <summary>
        /// Gets the pixel type for this PixelAdapter.
        /// </summary>
        public Type PixelType { get; }

        /// <summary>
        /// Overrides the current color components to the specified pointers.
        /// </summary>
        /// <param name="pDataR">The specified pointer of red component.</param>
        /// <param name="pDataG">The specified pointer of green component.</param>
        /// <param name="pDataB">The specified pointer of blue component.</param>
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB);

        /// <summary>
        /// Overrides the current color components to the specified pointers.
        /// </summary>
        /// <param name="pDataA">The specified pointer of alpha component.</param>
        /// <param name="pDataR">The specified pointer of red component.</param>
        /// <param name="pDataG">The specified pointer of green component.</param>
        /// <param name="pDataB">The specified pointer of blue component.</param>
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        /// <summary>
        /// Overlays the current color components to the specified pointers.
        /// </summary>
        /// <param name="pDataR">The specified pointer of red component.</param>
        /// <param name="pDataG">The specified pointer of green component.</param>
        /// <param name="pDataB">The specified pointer of blue component.</param>
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB);

        /// <summary>
        /// Overlays the current color components to the specified pointers.
        /// </summary>
        /// <param name="pDataA">The specified pointer of alpha component.</param>
        /// <param name="pDataR">The specified pointer of red component.</param>
        /// <param name="pDataG">The specified pointer of green component.</param>
        /// <param name="pDataB">The specified pointer of blue component.</param>
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);


        internal void InternalMove(int X, int Y);
        internal void InternalOffsetX(int Delta);
        internal void InternalOffsetY(int Delta);
        internal void InternalMoveNextX();
        internal void InternalMoveNextY();
        internal void InternalMovePreviousX();
        internal void InternalMovePreviousY();

        /// <summary>
        /// Creates a new <see cref="IPixelAdapter"/> that is a copy of the current instance.
        /// </summary>
        public IPixelAdapter Clone();

    }

}