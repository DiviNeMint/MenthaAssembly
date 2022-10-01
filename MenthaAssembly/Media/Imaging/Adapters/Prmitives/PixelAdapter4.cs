namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapter4<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public override int MaxX { get; }

        public override int MaxY { get; }

        public override byte A => *pScanA;

        public override byte R => *pScanR;

        public override byte G => *pScanG;

        public override byte B => *pScanB;

        public override int BitsPerPixel { get; }

        private readonly long Stride;
        private byte* pScanA, pScanR, pScanG, pScanB;
        private PixelAdapter4(PixelAdapter4<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;
            pScanA = Adapter.pScanA;
            pScanR = Adapter.pScanR;
            pScanG = Adapter.pScanG;
            pScanB = Adapter.pScanB;
        }
        public PixelAdapter4(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScanA = (byte*)Context.ScanA;
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
            *pScanA = A;
            *pScanR = R;
            *pScanG = G;
            *pScanB = B;
        }

        public override void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
            => PixelHelper.Overlay(ref pScanA, ref pScanR, ref pScanG, ref pScanB, A, R, G, B);

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

            pScanA += Offset;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            pScanA += OffsetX;
            pScanR += OffsetX;
            pScanG += OffsetX;
            pScanB += OffsetX;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            long Offset = Stride * OffsetY;
            pScanA += Offset;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }

        protected internal override void InternalMoveNext()
        {
            pScanA++;
            pScanR++;
            pScanG++;
            pScanB++;
        }
        protected internal override void InternalMovePrevious()
        {
            pScanA--;
            pScanR--;
            pScanG--;
            pScanB--;
        }

        protected internal override void InternalMoveNextLine()
        {
            pScanA += Stride;
            pScanR += Stride;
            pScanG += Stride;
            pScanB += Stride;
        }
        protected internal override void InternalMovePreviousLine()
        {
            pScanA -= Stride;
            pScanR -= Stride;
            pScanG -= Stride;
            pScanB -= Stride;
        }

        public override PixelAdapter<T> Clone()
            => new PixelAdapter4<T>(this);

    }

    internal unsafe class CalculatedPixelAdapter4<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public override int MaxX { get; }

        public override int MaxY { get; }

        private T Pixel;

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

        private readonly long Stride;
        private byte* pScanA, pScanR, pScanG, pScanB;
        private CalculatedPixelAdapter4(CalculatedPixelAdapter4<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;
            pScanA = Adapter.pScanA;
            pScanR = Adapter.pScanR;
            pScanG = Adapter.pScanG;
            pScanB = Adapter.pScanB;
        }
        public CalculatedPixelAdapter4(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScanA = (byte*)Context.ScanA;
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
            *pScanA = A;
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
                PixelHelper.Overlay(ref pScanA, ref pScanR, ref pScanG, ref pScanB, A, R, G, B);
        }
        public override void OverlayTo(T* pData)
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

            T p = Pixel;
            p.Override(*pScanA, *pScanR, *pScanG, *pScanB);
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

            pScanA += Offset;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            pScanA += OffsetX;
            pScanR += OffsetX;
            pScanG += OffsetX;
            pScanB += OffsetX;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            long Offset = Stride * OffsetY;
            pScanA += Offset;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            pScanA++;
            pScanR++;
            pScanG++;
            pScanB++;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            pScanA--;
            pScanR--;
            pScanG--;
            pScanB--;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            pScanA += Stride;
            pScanR += Stride;
            pScanG += Stride;
            pScanB += Stride;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            pScanA -= Stride;
            pScanR -= Stride;
            pScanG -= Stride;
            pScanB -= Stride;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new CalculatedPixelAdapter4<T>(this);

    }

}