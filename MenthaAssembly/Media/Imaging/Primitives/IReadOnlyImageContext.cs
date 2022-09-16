using MenthaAssembly.Media.Imaging.Utils;
using System;

namespace MenthaAssembly.Media.Imaging
{
    public interface IReadOnlyImageContext
    {
        public int Width { get; }

        public int Height { get; }

        public long Stride { get; }

        public int BitsPerPixel { get; }

        public int Channels { get; }

        public Type PixelType { get; }

        public Type StructType { get; }

        public IReadOnlyPixel this[int X, int Y] { get; }

        public IntPtr Scan0 { get; }

        public IntPtr ScanA { get; }

        public IntPtr ScanR { get; }

        public IntPtr ScanG { get; }

        public IntPtr ScanB { get; }

        public IImagePalette Palette { get; }

        public IReadOnlyPixelAdapter GetAdapter(int X, int Y);

    }
}
