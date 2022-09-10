namespace MenthaAssembly.Media.Imaging.Utils
{
    public unsafe interface IPixelAdapter<T> : IPixel
        where T : unmanaged, IPixel
    {
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

        public void MoveNext();

        public void MovePrevious();

        public void MoveNextLine();

        public void MovePreviousLine();

        public IPixelAdapter<T> Clone();

    }
}
