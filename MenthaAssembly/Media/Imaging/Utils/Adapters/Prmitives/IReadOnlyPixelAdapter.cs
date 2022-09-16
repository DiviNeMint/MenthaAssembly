namespace MenthaAssembly.Media.Imaging.Utils
{
    public interface IReadOnlyPixelAdapter : IReadOnlyPixel
    {
        public int X { get; }

        public int Y { get; }

        public int MaxX { get; }

        public int MaxY { get; }

        public void Move(int X, int Y);

        public void MoveX(int OffsetX);

        public void MoveY(int OffsetY);

        public void MoveNext();

        public void MovePrevious();

        public void MoveNextLine();

        public void MovePreviousLine();

        public IReadOnlyPixelAdapter Clone();

    }
}