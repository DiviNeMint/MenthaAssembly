using MenthaAssembly.Media.Imaging.Primitives;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a pixel with an alpha component, a red component, a green component, and a blue component.
    /// </summary>
    public interface IReadOnlyPixel : IPixelBase
    {
        /// <summary>
        /// Gets the alpha component value for this pixel.
        /// </summary>
        public byte A { get; }

        /// <summary>
        /// Gets the red component value for this pixel.
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// Gets the green component value for this pixel.
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// Gets the blue component value for this pixel.
        /// </summary>
        public byte B { get; }

    }
}