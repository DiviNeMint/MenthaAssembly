using System;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
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

        /// <summary>
        /// Creates a new <see cref="IPixelAdapter"/> that is a copy of the current instance.
        /// </summary>
        public new IPixelAdapter Clone();

    }
}