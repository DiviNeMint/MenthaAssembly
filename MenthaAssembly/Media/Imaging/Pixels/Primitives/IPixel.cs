namespace MenthaAssembly.Media.Imaging
{
    public interface IPixel : IReadOnlyPixel
    {
        public void Override(byte A, byte R, byte G, byte B);

        public void Overlay(byte A, byte R, byte G, byte B);

    }

}