namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class CastPixelAdapter<T, U> : PixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Adapter;

        public override int XLength { get; }

        public override int YLength { get; }

#pragma warning disable IDE0044 // 新增唯讀修飾元
        // If sets readonly, it will cause pixel's value can't change.
        private U Pixel;
#pragma warning restore IDE0044 // 新增唯讀修飾元

        public override byte A
        {
            get
            {
                EnsurePixel();
                return Pixel.A;
            }
        }

        public override byte R
        {
            get
            {
                EnsurePixel();
                return Pixel.R;
            }
        }

        public override byte G
        {
            get
            {
                EnsurePixel();
                return Pixel.G;
            }
        }

        public override byte B
        {
            get
            {
                EnsurePixel();
                return Pixel.B;
            }
        }

        public override int BitsPerPixel { get; }

        private CastPixelAdapter(CastPixelAdapter<T, U> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            this.Adapter = Adapter.Adapter;

            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                Pixel = Adapter.Pixel;
            }
        }
        public CastPixelAdapter(PixelAdapter<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            this.Adapter = Adapter;
        }

        public override void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<U> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
            => Adapter.Override(A, R, G, B);

        public override void OverrideTo(U* pData)
        {
            EnsurePixel();
            *pData = Pixel;
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = Pixel.A;
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }

        public override void Overlay(U Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<U> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MaxValue)
                Adapter.Override(A, R, G, B);
            else
                Adapter.Overlay(A, R, G, B);
        }

        public override void OverlayTo(U* pData)
        {
            EnsurePixel();
            if (Pixel.A == byte.MaxValue)
                *pData = Pixel;
            else
                pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (Pixel.A == byte.MaxValue)
            {
                *pDataR = Pixel.R;
                *pDataG = Pixel.G;
                *pDataB = Pixel.B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
            }
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (Pixel.A == byte.MaxValue)
            {
                *pDataA = byte.MaxValue;
                *pDataR = Pixel.R;
                *pDataG = Pixel.G;
                *pDataB = Pixel.B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
            }
        }

        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            Pixel.Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
            IsPixelValid = true;
        }

        protected internal override void InternalMove(int X, int Y)
        {
            Adapter.InternalMove(X, Y);
            IsPixelValid = false;
        }
        protected internal override void InternalOffsetX(int OffsetX)
        {
            Adapter.InternalOffsetX(OffsetX);
            IsPixelValid = false;
        }
        protected internal override void InternalOffsetY(int OffsetY)
        {
            Adapter.InternalOffsetY(OffsetY);
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextX()
        {
            Adapter.InternalMoveNextX();
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousX()
        {
            Adapter.InternalMovePreviousX();
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextY()
        {
            Adapter.InternalMoveNextY();
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousY()
        {
            Adapter.InternalMovePreviousY();
            IsPixelValid = false;
        }

        public override PixelAdapter<U> Clone()
            => new CastPixelAdapter<T, U>(this);

    }
}