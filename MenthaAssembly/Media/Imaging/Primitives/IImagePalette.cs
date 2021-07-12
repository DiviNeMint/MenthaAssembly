namespace MenthaAssembly.Media.Imaging
{
    public interface IImagePalette
    {
        int this[IPixel Color] { get; }

        IPixel this[int Index] { get; }

        bool TryGetOrAdd(IPixel Color, out int Index);

        int Count { get; }

        int Capacity { get; }

        public T[] Extract<T>()
            where T : unmanaged, IPixel;

    }
}
