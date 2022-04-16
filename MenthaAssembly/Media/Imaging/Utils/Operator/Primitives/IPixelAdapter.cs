namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe interface IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public void Override(T Pixel);

        public void Override(byte A, byte R, byte G, byte B);

        public void OverrideTo(T* pData);

        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void Overlay(T Pixel);

        public void Overlay(byte A, byte R, byte G, byte B);

        public void OverlayTo(T* pData);

        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void MoveNext();

        public void MovePrevious();

    }
}
