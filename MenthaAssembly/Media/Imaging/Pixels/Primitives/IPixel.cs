namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents an editable pixel in an image.
    /// </summary>
    public interface IPixel : IReadOnlyPixel
    {
        /// <summary>
        /// Overrides this pixel with the specified color components.
        /// </summary>
        /// <param name="A">The specified alpha component.</param>
        /// <param name="R">The specified red component.</param>
        /// <param name="G">The specified green component.</param>
        /// <param name="B">The specified blue component.</param>
        public void Override(byte A, byte R, byte G, byte B);

        /// <summary>
        /// Overlays this pixel with the specified color components.
        /// </summary>
        /// <param name="A">The specified alpha component.</param>
        /// <param name="R">The specified red component.</param>
        /// <param name="G">The specified green component.</param>
        /// <param name="B">The specified blue component.</param>
        public void Overlay(byte A, byte R, byte G, byte B);

    }
}