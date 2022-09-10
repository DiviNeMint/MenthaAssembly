using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelIndexedAdapterBase<T, Struct>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public int X { get; private set; } = -1;

        public int Y { get; private set; } = -1;

        public int MaxX { get; }

        public int MaxY { get; }

        public byte A
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].A;
            }
        }

        public byte R
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].R;
            }
        }

        public byte G
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].G;
            }
        }

        public byte B
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].B;
            }
        }

        public int BitsPerPixel { get; }

        public ImagePalette<T> Palette { get; }

        protected int XBit;
        private readonly int BitLength;
        private readonly Struct* pScan0;
        private readonly long Stride;
        protected Struct* pScan;
        public PixelIndexedAdapterBase(PixelIndexedAdapterBase<T, Struct> Adapter)
        {
            X = Adapter.X;
            XBit = Adapter.XBit;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Stride = Adapter.Stride;
            BitLength = Adapter.BitLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            Palette = Adapter.Palette;
            pScan0 = Adapter.pScan0;
            pScan = Adapter.pScan;
            GetPaletteIndexFunc = Adapter.GetPaletteIndexFunc;
        }
        public PixelIndexedAdapterBase(IImageContext Context, int X, int Y)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            Palette = (ImagePalette<T>)Context.Palette.Handle.Target;
            GetPaletteIndexFunc = ResetGetPaletteIndex;
            pScan0 = (Struct*)Context.Scan0;
            BitLength = pScan0->Length;
            Move(X, Y);
        }

        public void Override(byte A, byte R, byte G, byte B)
        {
            T Pixel = PixelHelper.ToPixel<T>(A, R, G, B);
            if (!Palette.TryGetOrAdd(Pixel, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            (*pScan)[XBit] = Index;
        }
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            *pDataA = Pixel.A;
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }

        public void Overlay(byte A, byte R, byte G, byte B)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            Pixel.Overlay(A, R, G, B);

            if (!Palette.TryGetOrAdd(Pixel, out Index))
                throw new IndexOutOfRangeException("Palette is full.");

            (*pScan)[XBit] = Index;
        }
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

        private Func<int> GetPaletteIndexFunc;
        public int GetPaletteIndex()
            => GetPaletteIndexFunc();
        private int ResetGetPaletteIndex()
        {
            int Index = (*pScan)[XBit];
            GetPaletteIndexFunc = () => Index;
            return Index;
        }

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
            XBit += Offset;

            if (BitLength <= XBit)
            {
                do
                {
                    XBit -= BitLength;
                    pScan++;
                } while (BitLength <= XBit);
            }
            else if (XBit < 0)
            {
                do
                {
                    XBit += BitLength;
                    pScan--;
                } while (XBit < 0);
            }

            GetPaletteIndexFunc = ResetGetPaletteIndex;
        }
        protected void InternalMove(int X, int Y)
        {
            this.X = X;
            this.Y = Y;

            int XBits = X * BitsPerPixel,
                OffsetX = XBits >> 3;

            XBit = (XBits & 0x07) / BitsPerPixel;
            pScan = pScan0 + Stride * Y + OffsetX;
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
            XBit++;
            if (BitLength <= XBit)
            {
                XBit -= BitLength;
                pScan++;
            }
            GetPaletteIndexFunc = ResetGetPaletteIndex;
        }
        protected void InternalMovePrevious()
        {
            X--;
            XBit--;
            if (XBit < 0)
            {
                XBit += BitLength;
                pScan--;
            }
            GetPaletteIndexFunc = ResetGetPaletteIndex;
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
            pScan += Stride;
        }
        protected void InternalMovePreviousLine()
        {
            Y--;
            pScan -= Stride;
        }

    }

    internal unsafe class PixelIndexedAdapter<T, Struct> : PixelIndexedAdapterBase<T, Struct>, IPixelAdapter<T>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {

        public PixelIndexedAdapter(PixelIndexedAdapter<T, Struct> Adapter) : base(Adapter)
        {

        }
        public PixelIndexedAdapter(IImageContext Context, int X, int Y) : base(Context, X, Y)
        {
        }

        public void Override(T Pixel)
        {
            if (!Palette.TryGetOrAdd(Pixel, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            (*pScan)[XBit] = Index;
        }
        public void Override(IPixelAdapter<T> Adapter)
        {
            T Pixel;
            Adapter.OverrideTo(&Pixel);
            Override(Pixel);
        }
        public void OverrideTo(T* pData)
        {
            int Index = GetPaletteIndex();
            *pData = Palette[Index];
        }

        public void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Overlay(IPixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverlayTo(T* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
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
            => new PixelIndexedAdapter<T, Struct>(this);
    }

    internal unsafe class PixelIndexedAdapter<T, U, Struct> : PixelIndexedAdapterBase<T, Struct>, IPixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public PixelIndexedAdapter(PixelIndexedAdapter<T, U, Struct> Adapter) : base(Adapter)
        {

        }
        public PixelIndexedAdapter(IImageContext Context, int X, int Y) : base(Context, X, Y)
        {
        }

        public void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Override(IPixelAdapter<U> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverrideTo(U* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

        public void Overlay(U Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Overlay(IPixelAdapter<U> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverlayTo(U* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

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
            => new PixelIndexedAdapter<T, U, Struct>(this);

    }

}