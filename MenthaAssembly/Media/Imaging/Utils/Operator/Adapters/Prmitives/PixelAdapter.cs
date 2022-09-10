namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapterBase<T>
        where T : unmanaged, IPixel
    {
        public int X { get; private set; } = -1;

        public int Y { get; private set; } = -1;

        public int MaxX { get; }

        public int MaxY { get; }

        public byte A => pScan->A;

        public byte R => pScan->R;

        public byte G => pScan->G;

        public byte B => pScan->B;

        public int BitsPerPixel { get; }

        private readonly long Stride;
        private readonly byte* pScan0;
        protected T* pScan;
        public PixelAdapterBase(PixelAdapterBase<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;
            pScan0 = Adapter.pScan0;
            pScan = Adapter.pScan;
        }
        public PixelAdapterBase(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScan0 = (byte*)Context.Scan0;
            Move(X, Y);
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
        protected void InternalMove(int Offset)
        {
            X += Offset;
            pScan += Offset;
        }
        protected void InternalMove(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            pScan = (T*)(pScan0 + Stride * Y + ((X * BitsPerPixel) >> 3));
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
        protected void InternalMoveNext()
        {
            X++;
            pScan++;
        }
        protected void InternalMovePrevious()
        {
            X--;
            pScan--;
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
        protected void InternalMoveNextLine()
        {
            Y++;
            pScan = (T*)((byte*)pScan + Stride);
        }
        protected void InternalMovePreviousLine()
        {
            Y--;
            pScan = (T*)((byte*)pScan - Stride);
        }

    }

    internal unsafe class PixelAdapter<T> : PixelAdapterBase<T>, IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public PixelAdapter(PixelAdapter<T> Adapter) : base(Adapter)
        {

        }
        public PixelAdapter(IImageContext Context, int X, int Y) : base(Context, X, Y)
        {
        }

        public void Override(T Pixel)
            => *pScan = Pixel;
        public void Override(IPixelAdapter<T> Adapter)
            => Adapter.OverrideTo(pScan);
        public void OverrideTo(T* pData)
            => *pData = *pScan;

        public void Overlay(T Pixel)
        {
            if (Pixel.A == byte.MaxValue)
                Override(Pixel);
            else
                Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
        public void Overlay(IPixelAdapter<T> Adapter)
            => Adapter.OverlayTo(pScan);
        public void OverlayTo(T* pData)
        {
            if (pScan->A == byte.MaxValue)
                OverrideTo(pData);
            else
                pData->Overlay(pScan->A, pScan->R, pScan->G, pScan->B);
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
            => new PixelAdapter<T>(this);

    }

    internal unsafe class PixelAdapter<T, U> : PixelAdapterBase<T>, IPixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
    {
        public PixelAdapter(PixelAdapter<T, U> Adapter) : base(Adapter)
        {

        }
        public PixelAdapter(IImageContext Context, int X, int Y) : base(Context, X, Y)
        {
        }

        public void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Override(IPixelAdapter<U> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverrideTo(U* pData)
            => pData->Override(pScan->A, pScan->R, pScan->G, pScan->B);

        public void Overlay(U Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Overlay(IPixelAdapter<U> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverlayTo(U* pData)
            => pData->Overlay(pScan->A, pScan->R, pScan->G, pScan->B);

        void IPixelAdapter<U>.InternalMove(int Offset)
            => InternalMove(Offset);
        void IPixelAdapter<U>.InternalMove(int X, int Y)
            => InternalMove(X, Y);
        void IPixelAdapter<U>.InternalMoveNext()
            => InternalMoveNext();
        void IPixelAdapter<U>.InternalMovePrevious()
            => InternalMovePrevious();
        void IPixelAdapter<U>.InternalMoveNextLine()
            => InternalMoveNextLine();
        void IPixelAdapter<U>.InternalMovePreviousLine()
            => InternalMovePreviousLine();

        public IPixelAdapter<U> Clone()
            => new PixelAdapter<T, U>(this);

    }

}