namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class CastPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly IPixelAdapter Adapter;

        public override int XLength { get; }

        public override int YLength { get; }

#pragma warning disable IDE0044 // 新增唯讀修飾元
        // If sets readonly, it will cause pixel's value can't change.
        private T _Pixel;
#pragma warning restore IDE0044 // 新增唯讀修飾元
        public T Pixel
        {
            get
            {
                EnsurePixel();
                return _Pixel;
            }
        }

        public override byte A
        {
            get
            {
                EnsurePixel();
                return _Pixel.A;
            }
        }

        public override byte R
        {
            get
            {
                EnsurePixel();
                return _Pixel.R;
            }
        }

        public override byte G
        {
            get
            {
                EnsurePixel();
                return _Pixel.G;
            }
        }

        public override byte B
        {
            get
            {
                EnsurePixel();
                return _Pixel.B;
            }
        }

        public override int BitsPerPixel { get; }

        private CastPixelAdapter(CastPixelAdapter<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            this.Adapter = Adapter.Adapter.Clone();

            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                _Pixel = Adapter._Pixel;
            }
        }
        public CastPixelAdapter(IPixelAdapter Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            this.Adapter = Adapter;
        }

        public override void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<T> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
            => Adapter.Override(A, R, G, B);

        public override void OverrideTo(T* pData)
        {
            EnsurePixel();
            *pData = _Pixel;
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = _Pixel.R;
            *pDataG = _Pixel.G;
            *pDataB = _Pixel.B;
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = _Pixel.A;
            *pDataR = _Pixel.R;
            *pDataG = _Pixel.G;
            *pDataB = _Pixel.B;
        }

        public override void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MaxValue)
                Adapter.Override(A, R, G, B);
            else
                Adapter.Overlay(A, R, G, B);
        }

        public override void OverlayTo(T* pData)
        {
            EnsurePixel();
            if (_Pixel.A == byte.MaxValue)
                *pData = _Pixel;
            else
                pData->Overlay(_Pixel.A, _Pixel.R, _Pixel.G, _Pixel.B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (_Pixel.A == byte.MaxValue)
            {
                *pDataR = _Pixel.R;
                *pDataG = _Pixel.G;
                *pDataB = _Pixel.B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, _Pixel.A, _Pixel.R, _Pixel.G, _Pixel.B);
            }
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (_Pixel.A == byte.MaxValue)
            {
                *pDataA = byte.MaxValue;
                *pDataR = _Pixel.R;
                *pDataG = _Pixel.G;
                *pDataB = _Pixel.B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, _Pixel.A, _Pixel.R, _Pixel.G, _Pixel.B);
            }
        }

        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            _Pixel.Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
            IsPixelValid = true;
        }

        public override void DangerousMove(int X, int Y)
        {
            Adapter.DangerousMove(X, Y);
            IsPixelValid = false;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            Adapter.DangerousOffsetX(OffsetX);
            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            Adapter.DangerousOffsetY(OffsetY);
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            Adapter.DangerousMoveNextX();
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousX()
        {
            Adapter.DangerousMovePreviousX();
            IsPixelValid = false;
        }

        public override void DangerousMoveNextY()
        {
            Adapter.DangerousMoveNextY();
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            Adapter.DangerousMovePreviousY();
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new CastPixelAdapter<T>(this);

    }
}