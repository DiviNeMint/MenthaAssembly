using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents an indexed image.
    /// </summary>
    public interface IImageIndexedContext : IImageContext
    {
        /// <summary>
        /// Gets the struct type for this image.
        /// </summary>
        public Type StructType { get; }

        /// <summary>
        /// Gets the palette for this image.
        /// </summary>
        public IImagePalette Palette { get; }

    }
}