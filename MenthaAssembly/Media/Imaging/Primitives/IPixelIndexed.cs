using MenthaAssembly.Media.Imaging.Primitives;

namespace MenthaAssembly.Media.Imaging
{
    public interface IPixelIndexed : IPixelBase
    {
        internal byte Data { get; }

        public int this[int Index] { set; get; }

        public int Length { get; }

    }
}
