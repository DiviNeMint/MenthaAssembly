namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class PixelAdapter1<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public override int XLength { get; }

        public override int YLength { get; }

        public override byte A => pScan->A;

        public override byte R => pScan->R;

        public override byte G => pScan->G;

        public override byte B => pScan->B;

        public override int BitsPerPixel { get; }

        private readonly long Stride;
        private readonly byte* pScan0;
        private T* pScan;
        private PixelAdapter1(PixelAdapter1<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;
            pScan0 = Adapter.pScan0;
            pScan = Adapter.pScan;
        }
        public PixelAdapter1(IImageContext Context, int X, int Y)
        {
            XLength = Context.Width;
            YLength = Context.Height;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScan0 = (byte*)Context.Scan0[0];

            this.X = X;
            this.Y = Y;
            DangerousMove(X, Y);
        }

        public override void Override(T Pixel)
            => *pScan = Pixel;
        public override void Override(PixelAdapter<T> Adapter)
            => Adapter.OverrideTo(pScan);
        public override void Override(byte A, byte R, byte G, byte B)
            => pScan->Override(A, R, G, B);
        public override void OverrideTo(T* pData)
            => *pData = *pScan;

        public override void Overlay(T Pixel)
        {
            if (Pixel.A == byte.MaxValue)
                Override(Pixel);
            else
                Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
        public override void Overlay(PixelAdapter<T> Adapter)
            => Adapter.OverlayTo(pScan);
        public override void Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MaxValue)
                pScan->Override(A, R, G, B);
            else
                pScan->Overlay(A, R, G, B);
        }

        public override void DangerousMove(int X, int Y)
            => pScan = (T*)(pScan0 + Stride * Y + ((X * BitsPerPixel) >> 3));

        public override void DangerousOffsetX(int Delta)
            => pScan += Delta;
        public override void DangerousOffsetY(int Delta)
            => pScan = (T*)((byte*)pScan + Stride * Delta);

        public override void DangerousMoveNextX()
            => pScan++;
        public override void DangerousMovePreviousX()
            => pScan--;

        public override void DangerousMoveNextY()
            => pScan = (T*)((byte*)pScan + Stride);
        public override void DangerousMovePreviousY()
            => pScan = (T*)((byte*)pScan - Stride);

        public override PixelAdapter<T> Clone()
            => new PixelAdapter1<T>(this);

    }

    internal sealed unsafe class PixelAdapter1<T, U> : PixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
    {
        public override int XLength { get; }

        public override int YLength { get; }

        public override byte A => pScan->A;

        public override byte R => pScan->R;

        public override byte G => pScan->G;

        public override byte B => pScan->B;

        public override int BitsPerPixel { get; }

        private readonly long Stride;
        private readonly byte* pScan0;
        private T* pScan;
        private PixelAdapter1(PixelAdapter1<T, U> Adapter)
        {
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;

            X = Adapter.X;
            Y = Adapter.Y;
            pScan0 = Adapter.pScan0;
            pScan = Adapter.pScan;
        }
        public PixelAdapter1(IImageContext Context, int X, int Y)
        {
            XLength = Context.Width;
            YLength = Context.Height;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScan0 = (byte*)Context.Scan0[0];
            Move(X, Y);
        }

        public override void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<U> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
            => pScan->Override(A, R, G, B);
        public override void OverrideTo(U* pData)
            => pData->Override(A, R, G, B);

        public override void Overlay(U Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<U> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MaxValue)
                pScan->Override(A, R, G, B);
            else
                pScan->Overlay(A, R, G, B);
        }

        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            if (pScan->A == byte.MaxValue)
            {
                *pDataR = pScan->R;
                *pDataG = pScan->G;
                *pDataB = pScan->B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, pScan->A, pScan->R, pScan->G, pScan->B);
            }
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            if (pScan->A == byte.MaxValue)
            {
                *pDataA = byte.MaxValue;
                *pDataR = pScan->R;
                *pDataG = pScan->G;
                *pDataB = pScan->B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, pScan->A, pScan->R, pScan->G, pScan->B);
            }
        }

        public override void DangerousMove(int X, int Y)
            => pScan = (T*)(pScan0 + Stride * Y + ((X * BitsPerPixel) >> 3));

        public override void DangerousOffsetX(int Delta)
            => pScan += Delta;
        public override void DangerousOffsetY(int Delta)
            => pScan = (T*)((byte*)pScan + Stride * Delta);

        public override void DangerousMoveNextX()
            => pScan++;
        public override void DangerousMovePreviousX()
            => pScan--;

        public override void DangerousMoveNextY()
            => pScan = (T*)((byte*)pScan + Stride);
        public override void DangerousMovePreviousY()
            => pScan = (T*)((byte*)pScan - Stride);

        public override PixelAdapter<U> Clone()
            => new PixelAdapter1<T, U>(this);

    }
}