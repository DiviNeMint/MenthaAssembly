namespace MenthaAssembly.Media.Imaging.Primitives
{
    public interface IPixel : IPixelBase
    {
        public byte A { get; }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

    }
}
