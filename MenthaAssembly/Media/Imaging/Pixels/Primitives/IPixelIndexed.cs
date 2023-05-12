using MenthaAssembly.Media.Imaging.Primitives;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents the collection of color index.
    /// </summary>
    public interface IPixelIndexed : IPixelBase
    {
        /// <summary>
        /// Sets/Gets the color index at the specified index.
        /// </summary>
        /// <param name="Index">The specified index.</param>
        public int this[int Index] { set; get; }

        /// <summary>
        /// Get the length of the pixel collection for a byte.
        /// </summary>
        public int Length { get; }

    }
}