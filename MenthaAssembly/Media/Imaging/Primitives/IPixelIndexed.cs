namespace MenthaAssembly.Media.Imaging.Primitives
{
    public interface IPixelIndexed : IPixelBase
    {
        internal byte Data { get; }

        public int this[int Index] { set; get; }

        public int Length { get; }

    }
}
