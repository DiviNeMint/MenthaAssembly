namespace MenthaAssembly.Media.Imaging
{
    public interface IImagePalette
    {
        int this[IReadOnlyPixel Color] { get; }

        IReadOnlyPixel this[int Index] { get; }

        bool TryGetOrAdd(IReadOnlyPixel Color, out int Index);

        int Count { get; }

        int Capacity { get; }

        public T[] Extract<T>()
            where T : unmanaged, IPixel;

    }
}