namespace MenthaAssembly.Media.Imaging.Primitives
{
    /// <summary>
    /// Represents a pixel base.
    /// </summary>
    public interface IPixelBase
    {
        /// <summary>
        /// Get the length in bits.
        /// </summary>
        public int BitsPerPixel { get; }

    }
}