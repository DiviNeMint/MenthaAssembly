using MenthaAssembly.Media.Imaging.Primitives;
using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public interface IImageContext
    {
        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public int BitsPerPixel { get; }

        public int Channels { get; }

        public Type PixelType { get; }

        public IPixel this[int X, int Y] { set; get; }

        public IntPtr Scan0 { get; }

        public IntPtr ScanA { get; }

        public IntPtr ScanR { get; }

        public IntPtr ScanG { get; }

        public IntPtr ScanB { get; }

        public IList<BGRA> Palette { get; }

        public void DrawLine(int X0, int Y0, int X1, int Y1, byte A, byte R, byte G, byte B, int PenWidth);
        public void DrawLine(double X0, double Y0, double X1, double Y1, IPixel Color, double PenWidth);

    }
}
