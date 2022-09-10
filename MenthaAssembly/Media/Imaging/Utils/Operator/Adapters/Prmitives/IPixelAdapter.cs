namespace MenthaAssembly.Media.Imaging.Utils
{
    public unsafe interface IPixelAdapter<T> : IPixel
        where T : unmanaged, IPixel
    {
        public int X { get; }

        public int Y { get; }

        public int MaxX { get; }

        public int MaxY { get; }

        public void Override(T Pixel);

        public void Override(IPixelAdapter<T> Adapter);

        public void OverrideTo(T* pData);

        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void Overlay(T Pixel);

        public void Overlay(IPixelAdapter<T> Adapter);

        public void OverlayTo(T* pData);

        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void Move(int Offset);

        public void Move(int X, int Y);

        public void MoveNext();

        public void MovePrevious();

        public void MoveNextLine();

        public void MovePreviousLine();

        internal void InternalMove(int Offset);

        internal void InternalMove(int X, int Y);

        internal void InternalMoveNext();

        internal void InternalMovePrevious();

        internal void InternalMoveNextLine();

        internal void InternalMovePreviousLine();

        public IPixelAdapter<T> Clone();

    }
}