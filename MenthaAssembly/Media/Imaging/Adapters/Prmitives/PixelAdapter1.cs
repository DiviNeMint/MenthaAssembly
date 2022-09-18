namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelAdapter1<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public override int MaxX { get; }

        public override int MaxY { get; }

        public override byte A => pScan->A;

        public override byte R => pScan->R;

        public override byte G => pScan->G;

        public override byte B => pScan->B;

        public override int BitsPerPixel { get; }

        private readonly long Stride;
        private readonly byte* pScan0;
        protected T* pScan;
        public PixelAdapter1(PixelAdapter1<T> Adapter)
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
        public PixelAdapter1(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScan0 = (byte*)Context.Scan0;
            Move(X, Y);
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
            => pScan->Overlay(A, R, G, B);

        protected internal override void InternalMove(int X, int Y)
            => pScan = (T*)(pScan0 + Stride * Y + ((X * BitsPerPixel) >> 3));
        protected internal override void InternalMoveX(int OffsetX)
            => pScan += OffsetX;
        protected internal override void InternalMoveY(int OffsetY)
            => pScan = (T*)((byte*)pScan + Stride * OffsetY);

        protected internal override void InternalMoveNext()
            => pScan++;
        protected internal override void InternalMovePrevious()
            => pScan--;

        protected internal override void InternalMoveNextLine()
            => pScan = (T*)((byte*)pScan + Stride);
        protected internal override void InternalMovePreviousLine()
            => pScan = (T*)((byte*)pScan - Stride);

        public override PixelAdapter<T> Clone()
            => new PixelAdapter1<T>(this);

    }

    internal unsafe class PixelAdapter1<T, U> : PixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
    {
        public override int MaxX { get; }

        public override int MaxY { get; }

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

        private readonly long Stride;
        private readonly byte* pScan0;
        protected T* pScan;
        public PixelAdapter1(PixelAdapter1<T, U> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;
            pScan0 = Adapter.pScan0;
            pScan = Adapter.pScan;

            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                Pixel = Adapter.Pixel;
            }
        }
        public PixelAdapter1(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            pScan0 = (byte*)Context.Scan0;
            Move(X, Y);
        }

        public override void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<U> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
            => pScan->Override(A, R, G, B);
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
            => pScan->Overlay(A, R, G, B);
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

            Pixel.Override(pScan->A, pScan->R, pScan->G, pScan->B);
            IsPixelValid = true;
        }

        protected internal override void InternalMove(int X, int Y)
        {
            pScan = (T*)(pScan0 + Stride * Y + ((X * BitsPerPixel) >> 3));
            IsPixelValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            pScan += OffsetX;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            pScan = (T*)((byte*)pScan + Stride * OffsetY);
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            pScan++;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            pScan--;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            pScan = (T*)((byte*)pScan + Stride);
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            pScan = (T*)((byte*)pScan - Stride);
            IsPixelValid = false;
        }

        public override PixelAdapter<U> Clone()
            => new PixelAdapter1<T, U>(this);

    }

}