using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a contour in image.
    /// </summary>
    public interface IImageContour : IEnumerable<KeyValuePair<int, ImageContourScanLine>>, ICloneable
    {
        public IReadOnlyDictionary<int, ImageContourScanLine> Contents { get; }

        public double OffsetX { get; }

        public double OffsetY { get; }

        /// <summary>
        /// Generate the contents of contour.
        /// </summary>
        public void EnsureContents();

        /// <summary>
        /// Gets the bound of contour.
        /// </summary>
        public Bound<int> Bound { get; }

        /// <summary>
        /// Flips the contour by the specified mode.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center to flip.</param>
        /// <param name="Cy">The y-coordinate of the center to flip.</param>
        /// <param name="Flip">The mode to flip.</param>
        public void Flip(double Cx, double Cy, FlipMode Flip);

        /// <summary>
        /// Offsets the contour's coordinates by the specified amounts.
        /// </summary>
        /// <param name="Dx">The amount to offset x-coordinate.</param>
        /// <param name="Dy">The amount to offset y-coordinate.</param>
        public void Offset(double Dx, double Dy);

        /// <summary>
        /// Crops the contour to special rectangle.
        /// </summary>
        /// <param name="MinX">The left of the special rectangle.</param>
        /// <param name="MaxX">The right of the special rectangle.</param>
        /// <param name="MinY">The top of the special rectangle.</param>
        /// <param name="MaxY">The bottom of the special rectangle.</param>
        public void Crop(double MinX, double MaxX, double MinY, double MaxY);

        /// <summary>
        /// Rotates the contour about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public void Rotate(double Cx, double Cy, double Theta);

        /// <summary>
        /// Scales this contour around the origin.
        /// </summary>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public void Scale(double ScaleX, double ScaleY);

        /// <summary>
        /// Creates a new contour that is a copy of the current instance.
        /// </summary>
        public new IImageContour Clone();

    }
}