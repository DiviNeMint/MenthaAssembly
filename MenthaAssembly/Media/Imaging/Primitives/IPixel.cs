using MenthaAssembly.Media.Imaging.Primitives;

namespace MenthaAssembly.Media.Imaging
{
    public interface IPixel : IPixelBase
    {
        public byte A { get; }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public void Override(byte A, byte R, byte G, byte B);
        public void Overlay(byte A, byte R, byte G, byte B);

    }
}
