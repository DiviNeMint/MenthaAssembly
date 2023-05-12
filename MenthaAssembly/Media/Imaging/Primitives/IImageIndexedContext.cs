using System;

namespace MenthaAssembly.Media.Imaging
{
    public interface IImageIndexedContext : IImageContext
    {
        /// <summary>
        /// Get the struct type of image.
        /// </summary>
        public Type StructType { get; }

        public IImagePalette Palette { get; }

    }
}