namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapter3<T> : IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private byte* pScanR, pScanG, pScanB;
        public PixelAdapter3(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            pScanR = pDataR;
            pScanG = pDataG;
            pScanB = pDataB;
        }

        public void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Override(byte A, byte R, byte G, byte B)
        {
            *pScanR = R;
            *pScanG = G;
            *pScanB = B;
        }
        public void OverrideTo(T* pData)
            => pData->Override(byte.MaxValue, *pScanR, *pScanG, *pScanB);
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataR = *pScanR;
            *pDataG = *pScanG;
            *pDataB = *pScanB;
        }
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataA = byte.MaxValue;
            *pDataR = *pScanR;
            *pDataG = *pScanG;
            *pDataB = *pScanB;
        }

        public void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Overlay(byte A, byte R, byte G, byte B)
            => PixelHelper.Overlay(ref pScanR, ref pScanG, ref pScanB, A, R, G, B);
        public void OverlayTo(T* pData)
            => pData->Overlay(byte.MaxValue, *pScanR, *pScanG, *pScanB);
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, byte.MaxValue, *pScanR, *pScanG, *pScanB);
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, byte.MaxValue, *pScanR, *pScanG, *pScanB);

        public void MoveNext()
        {
            pScanR++;
            pScanG++;
            pScanB++;
        }
        public void MovePrevious()
        {
            pScanR--;
            pScanG--;
            pScanB--;
        }

    }
}
