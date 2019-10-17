using System;

namespace MenthaAssembly.Interfaces
{
    public interface IImageContext
    {
        int Width { get; }

        int Height { get; }

        int Channel { get; }

        IntPtr Scan0 { get; }

        IntPtr ScanA { get; }

        IntPtr ScanR { get; }

        IntPtr ScanG { get; }

        IntPtr ScanB { get; }

        int Stride { get; }

        int PixelBytes { get; }
    }
}
