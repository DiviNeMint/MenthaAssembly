namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapter3<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public override int MaxX { get; }

        public override int MaxY { get; }

        public override byte A => byte.MaxValue;

        public override byte R => *pScanR;

        public override byte G => *pScanG;

        public override byte B => *pScanB;

        public override int BitsPerPixel { get; }

        private readonly long Stride;
        private byte* pScanR, pScanG, pScanB;
        public PixelAdapter3(PixelAdapter3<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;
            pScanR = Adapter.pScanR;
            pScanG = Adapter.pScanG;
            pScanB = Adapter.pScanB;
        }
        public PixelAdapter3(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScanR = (byte*)Context.ScanR;
            pScanG = (byte*)Context.ScanG;
            pScanB = (byte*)Context.ScanB;
            Move(X, Y);
        }

        public override void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<T> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
        {
            *pScanR = R;
            *pScanG = G;
            *pScanB = B;
        }
        public override void OverrideTo(T* pData)
            => pData->Override(byte.MaxValue, *pScanR, *pScanG, *pScanB);
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataA = byte.MaxValue;
            *pDataR = *pScanR;
            *pDataG = *pScanG;
            *pDataB = *pScanB;
        }

        public override void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
            => PixelHelper.Overlay(ref pScanR, ref pScanG, ref pScanB, A, R, G, B);
        public override void OverlayTo(T* pData)
            => pData->Override(byte.MaxValue, *pScanR, *pScanG, *pScanB);
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => OverrideTo(pDataR, pDataG, pDataB);
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public override void Move(int X, int Y)
        {
            X = MathHelper.Clamp(X, 0, MaxX);
            Y = MathHelper.Clamp(Y, 0, MaxY);

            if (X != this.X || Y != this.Y)
                InternalMove(X, Y);
        }

        protected internal override void InternalMove(int X, int Y)
        {
            long Offset = Stride * (this.Y - Y) + (this.X - X);

            this.X = X;
            this.Y = Y;

            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            pScanR += OffsetX;
            pScanG += OffsetX;
            pScanB += OffsetX;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            long Offset = Stride * OffsetY;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }

        protected internal override void InternalMoveNext()
        {
            pScanR++;
            pScanG++;
            pScanB++;
        }
        protected internal override void InternalMovePrevious()
        {
            pScanR--;
            pScanG--;
            pScanB--;
        }

        protected internal override void InternalMoveNextLine()
        {
            pScanR += Stride;
            pScanG += Stride;
            pScanB += Stride;
        }
        protected internal override void InternalMovePreviousLine()
        {
            pScanR -= Stride;
            pScanG -= Stride;
            pScanB -= Stride;
        }

        public override PixelAdapter<T> Clone()
            => new PixelAdapter3<T>(this);

    }

    internal unsafe class CalculatedPixelAdapter3<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public override int MaxX { get; }

        public override int MaxY { get; }

        private T Pixel;

        public override byte A => byte.MaxValue;

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

        private readonly long Stride;
        private byte* pScanR, pScanG, pScanB;
        public CalculatedPixelAdapter3(CalculatedPixelAdapter3<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;
            pScanR = Adapter.pScanR;
            pScanG = Adapter.pScanG;
            pScanB = Adapter.pScanB;
        }
        public CalculatedPixelAdapter3(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScanR = (byte*)Context.ScanR;
            pScanG = (byte*)Context.ScanG;
            pScanB = (byte*)Context.ScanB;
            Move(X, Y);
        }

        public override void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<T> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
        {
            *pScanR = R;
            *pScanG = G;
            *pScanB = B;
        }
        public override void OverrideTo(T* pData)
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

        public override void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MaxValue)
                Override(A, R, G, B);
            else
                PixelHelper.Overlay(ref pScanR, ref pScanG, ref pScanB, A, R, G, B);
        }
        public override void OverlayTo(T* pData)
        {
            EnsurePixel();
            *pData = Pixel;
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = byte.MaxValue;
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }

        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            T p = Pixel;
            p.Override(byte.MaxValue, *pScanR, *pScanG, *pScanB);
            Pixel = p;

            IsPixelValid = true;
        }

        public override void Move(int X, int Y)
        {
            X = MathHelper.Clamp(X, 0, MaxX);
            Y = MathHelper.Clamp(Y, 0, MaxY);

            if (X != this.X || Y != this.Y)
                InternalMove(X, Y);
        }

        protected internal override void InternalMove(int X, int Y)
        {
            long Offset = Stride * (this.Y - Y) + (this.X - X);

            this.X = X;
            this.Y = Y;

            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            pScanR += OffsetX;
            pScanG += OffsetX;
            pScanB += OffsetX;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            long Offset = Stride * OffsetY;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            pScanR++;
            pScanG++;
            pScanB++;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            pScanR--;
            pScanG--;
            pScanB--;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            pScanR += Stride;
            pScanG += Stride;
            pScanB += Stride;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            pScanR -= Stride;
            pScanG -= Stride;
            pScanB -= Stride;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new CalculatedPixelAdapter3<T>(this);

    }
}