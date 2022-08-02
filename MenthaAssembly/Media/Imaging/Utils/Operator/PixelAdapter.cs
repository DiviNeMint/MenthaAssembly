namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapterBase<T>
        where T : unmanaged, IPixel
    {
        public byte A => pScan->A;

        public byte R => pScan->R;

        public byte G => pScan->G;

        public byte B => pScan->B;

        public int BitsPerPixel => pScan->BitsPerPixel;

        private readonly long Stride;
        protected T* pScan;
        public PixelAdapterBase(T* pData, long Stride)
        {
            pScan = pData;
            this.Stride = Stride;
        }

        public void Override(byte A, byte R, byte G, byte B)
            => pScan->Override(A, R, G, B);
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataR = pScan->R;
            *pDataG = pScan->G;
            *pDataB = pScan->B;
        }
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataA = pScan->A;
            *pDataR = pScan->R;
            *pDataG = pScan->G;
            *pDataB = pScan->B;
        }

        public void Overlay(byte A, byte R, byte G, byte B)
            => pScan->Overlay(A, R, G, B);
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, pScan->A, pScan->R, pScan->G, pScan->B);
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, pScan->A, pScan->R, pScan->G, pScan->B);

        public void Move(int Offset)
            => pScan += Offset;

        public void MoveNext()
            => pScan++;
        public void MovePrevious()
            => pScan--;

        public void MoveNextLine()
            => pScan = (T*)((byte*)pScan + Stride);
        public void MovePreviousLine()
            => pScan = (T*)((byte*)pScan - Stride);
    }

    internal unsafe class PixelAdapter<T> : PixelAdapterBase<T>, IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public PixelAdapter(byte* pData, long Stride) : base((T*)pData, Stride)
        {
        }

        public void Override(T Pixel)
            => *pScan = Pixel;
        public void OverrideTo(T* pData)
            => *pData = *pScan;

        public void Overlay(T Pixel)
        {
            if (Pixel.A == byte.MaxValue)
                Override(Pixel);
            else
                Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
        public void OverlayTo(T* pData)
        {
            if (pScan->A == byte.MaxValue)
                OverrideTo(pData);
            else
                pData->Overlay(pScan->A, pScan->R, pScan->G, pScan->B);
        }

    }

    internal unsafe class PixelAdapter<T, U> : PixelAdapterBase<T>, IPixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
    {
        public PixelAdapter(byte* pData, long Stride) : base((T*)pData, Stride)
        {
        }

        public void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void OverrideTo(U* pData)
            => pData->Override(pScan->A, pScan->R, pScan->G, pScan->B);

        public void Overlay(U Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void OverlayTo(U* pData)
            => pData->Overlay(pScan->A, pScan->R, pScan->G, pScan->B);

    }

}
