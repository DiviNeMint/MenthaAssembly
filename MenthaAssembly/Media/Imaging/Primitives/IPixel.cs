using MenthaAssembly.Media.Imaging.Primitives;

namespace MenthaAssembly.Media.Imaging
{
    public interface IPixel : IPixelBase
    {
        public byte A { get; }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

    }
}
