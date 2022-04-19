namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe interface IPixelAdapter<T> : IPixel
        where T : unmanaged, IPixel
    {
        public void Override(T Pixel);

        public void OverrideTo(T* pData);

        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void Overlay(T Pixel);

        public void OverlayTo(T* pData);

        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void MoveNext();

        public void MovePrevious();

        public void MoveNextLine();

        public void MovePreviousLine();

    }
}
