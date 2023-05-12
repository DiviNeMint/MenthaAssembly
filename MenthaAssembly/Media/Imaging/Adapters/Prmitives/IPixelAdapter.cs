using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the image adapter.
    /// </summary>
    public unsafe interface IPixelAdapter : IPixel
    {
        /// <summary>
        /// Gets the x-coordinate of this PixelAdapter.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the y-coordinate of this PixelAdapter.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the maximum x-coordinate of this PixelAdapter.
        /// </summary>
        public int MaxX { get; }

        /// <summary>
        /// Gets the maximum y-coordinate of this PixelAdapter.
        /// </summary>
        public int MaxY { get; }

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
        /// Moves to the specified coordinate.
        /// </summary>
        /// <param name="X">The specified x-coordinate.</param>
        /// <param name="Y">The specified y-coordinate.</param>
        public void Move(int X, int Y);

        /// <summary>
        /// Move the specified offset on the x-axis.
        /// </summary>
        /// <param name="OffsetX">The specified offset.</param>
        public void MoveX(int OffsetX);

        /// <summary>
        /// Move the specified offset on the y-axis.
        /// </summary>
        /// <param name="OffsetY">The specified offset.</param>
        public void MoveY(int OffsetY);

        /// <summary>
        /// Move to next on the x-axis.
        /// </summary>
        public void MoveNext();

        /// <summary>
        /// Move to previous on the x-axis.
        /// </summary>
        public void MovePrevious();

        /// <summary>
        /// Move to next on the y-axis.
        /// </summary>
        public void MoveNextLine();

        /// <summary>
        /// Move to previous on the y-axis.
        /// </summary>
        public void MovePreviousLine();

        /// <summary>
        /// Creates a new <see cref="IPixelAdapter"/> that is a copy of the current instance.
        /// </summary>
        public IPixelAdapter Clone();

    }
}