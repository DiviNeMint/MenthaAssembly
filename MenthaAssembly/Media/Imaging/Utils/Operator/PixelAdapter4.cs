namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapter4<T> : IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private byte* pScanA, pScanR, pScanG, pScanB;
        public PixelAdapter4(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            pScanA = pDataA;
            pScanR = pDataR;
            pScanG = pDataG;
            pScanB = pDataB;
        }

        public void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Override(byte A, byte R, byte G, byte B)
        {
            *pScanA = A;
            *pScanR = R;
            *pScanG = G;
            *pScanB = B;
        }
        public void OverrideTo(T* pData)
            => pData->Override(*pScanA, *pScanR, *pScanG, *pScanB);
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataR = *pScanR;
            *pDataG = *pScanG;
            *pDataB = *pScanB;
        }
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataA = *pScanA;
            *pDataR = *pScanR;
            *pDataG = *pScanG;
            *pDataB = *pScanB;
        }

        public void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Overlay(byte A, byte R, byte G, byte B)
            => PixelHelper.Overlay(ref pScanA, ref pScanR, ref pScanG, ref pScanB, A, R, G, B);
        public void OverlayTo(T* pData)
            => pData->Overlay(*pScanA, *pScanR, *pScanG, *pScanB);
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, *pScanA, *pScanR, *pScanG, *pScanB);
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, *pScanA, *pScanR, *pScanG, *pScanB);

        public void MoveNext()
        {
            pScanA++;
            pScanR++;
            pScanG++;
            pScanB++;
        }
        public void MovePrevious()
        {
            pScanA--;
            pScanR--;
            pScanG--;
            pScanB--;
        }

    }
}
