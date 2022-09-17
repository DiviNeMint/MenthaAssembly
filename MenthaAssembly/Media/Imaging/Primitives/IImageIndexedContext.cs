namespace MenthaAssembly.Media.Imaging
{
    public interface IImageIndexedContext : IImageContext
    {
        public IImagePalette Palette { get; }

    }
}