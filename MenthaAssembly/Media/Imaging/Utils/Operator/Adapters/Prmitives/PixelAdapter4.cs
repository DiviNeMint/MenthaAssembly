namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapter4<T> : IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public int X { get; private set; } = -1;

        public int Y { get; private set; } = -1;

        public int MaxX { get; }

        public int MaxY { get; }

        public byte A => *pScanA;

        public byte R => *pScanR;

        public byte G => *pScanG;

        public byte B => *pScanB;

        public int BitsPerPixel { get; }

        private readonly long Stride;
        private byte* pScanA, pScanR, pScanG, pScanB;
        public PixelAdapter4(PixelAdapter4<T> Adapter)
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

        public void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Override(IPixelAdapter<T> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
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
        public void Overlay(IPixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void Overlay(byte A, byte R, byte G, byte B)
            => PixelHelper.Overlay(ref pScanA, ref pScanR, ref pScanG, ref pScanB, A, R, G, B);
        public void OverlayTo(T* pData)
            => pData->Overlay(*pScanA, *pScanR, *pScanG, *pScanB);
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, *pScanA, *pScanR, *pScanG, *pScanB);
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, *pScanA, *pScanR, *pScanG, *pScanB);

        public void Move(int Offset)
        {
            int Nx = MathHelper.Clamp(X + Offset, 0, MaxX),
                Dx = Nx - X;
            if (Dx != 0)
                InternalMove(Dx);
        }
        public void Move(int X, int Y)
        {
            X = MathHelper.Clamp(X, 0, MaxX);
            Y = MathHelper.Clamp(Y, 0, MaxY);

            if (X != this.X || Y != this.Y)
                InternalMove(X, Y);
        }

        public void MoveNext()
        {
            if (X < MaxX)
                InternalMoveNext();
        }
        public void MovePrevious()
        {
            if (0 < X)
                InternalMovePrevious();
        }

        public void MoveNextLine()
        {
            if (Y < MaxY)
                InternalMoveNextLine();
        }
        public void MovePreviousLine()
        {
            if (0 < Y)
                InternalMovePreviousLine();
        }

        private void InternalMove(int Offset)
        {
            X += Offset;
            pScanA += Offset;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }
        private void InternalMove(int X, int Y)
        {
            long Offset = Stride * (this.Y - Y) + (this.X - X);

            this.X = X;
            this.Y = Y;

            pScanA += Offset;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }
        private void InternalMoveNext()
        {
            X++;
            pScanA++;
            pScanR++;
            pScanG++;
            pScanB++;
        }
        private void InternalMovePrevious()
        {
            X--;
            pScanA--;
            pScanR--;
            pScanG--;
            pScanB--;
        }
        private void InternalMoveNextLine()
        {
            Y++;
            pScanA += Stride;
            pScanR += Stride;
            pScanG += Stride;
            pScanB += Stride;
        }
        private void InternalMovePreviousLine()
        {
            Y--;
            pScanA -= Stride;
            pScanR -= Stride;
            pScanG -= Stride;
            pScanB -= Stride;
        }

        void IPixelAdapter<T>.InternalMove(int Offset)
            => InternalMove(Offset);
        void IPixelAdapter<T>.InternalMove(int X, int Y)
            => InternalMove(X, Y);
        void IPixelAdapter<T>.InternalMoveNext()
            => InternalMoveNext();
        void IPixelAdapter<T>.InternalMovePrevious()
            => InternalMovePrevious();
        void IPixelAdapter<T>.InternalMoveNextLine()
            => InternalMoveNextLine();
        void IPixelAdapter<T>.InternalMovePreviousLine()
            => InternalMovePreviousLine();

        public IPixelAdapter<T> Clone()
            => new PixelAdapter4<T>(this);

    }
}