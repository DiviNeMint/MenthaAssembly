﻿namespace MenthaAssembly.Media.Imaging.Utils
{
    public abstract class PixelIndexedAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public abstract void OverrideIndex(int Index);

    }

    internal sealed unsafe class PixelIndexedAdapter<T, Struct> : PixelIndexedAdapter<T>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public override int XLength { get; }

        public override int YLength { get; }

        private int Index;
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

        public ImagePalette<T> Palette { get; }

        private int XBit;
        private readonly int BitLength;
        private readonly Struct* pScan0;
        private readonly long Stride;
        private Struct* pScan;
        private PixelIndexedAdapter(PixelIndexedAdapter<T, Struct> Adapter)
        {
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Stride = Adapter.Stride;
            BitLength = Adapter.BitLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            Palette = Adapter.Palette;

            X = Adapter.X;
            Y = Adapter.Y;
            XBit = Adapter.XBit;
            pScan0 = Adapter.pScan0;
            pScan = Adapter.pScan;
        }
        public PixelIndexedAdapter(IImageIndexedContext Context, int X, int Y)
        {
            XLength = Context.Width - 1;
            YLength = Context.Height - 1;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            Palette = (ImagePalette<T>)Context.Palette;

            pScan0 = (Struct*)Context.Scan0[0];
            BitLength = pScan0->Length;

            this.X = X;
            this.Y = Y;
            DangerousMove(X, Y);
        }

        public override void OverrideIndex(int Index)
            => (*pScan)[XBit] = Index;

        public override void Override(T Pixel)
        {
            this.Pixel = Palette.GetOrAdd(Pixel, out Index);
            (*pScan)[XBit] = Index;
            IsPixelValid = true;
        }
        public override void Override(PixelAdapter<T> Adapter)
        {
            T Pixel;
            Adapter.OverrideTo(&Pixel);
            Override(Pixel);
        }
        public override void Override(byte A, byte R, byte G, byte B)
        {
            Pixel.Override(A, R, G, B);
            Pixel = Palette.GetOrAdd(Pixel, out Index);
            (*pScan)[XBit] = Index;
            IsPixelValid = true;
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
            EnsurePixel();
            Pixel.Overlay(A, R, G, B);
            Pixel = Palette.GetOrAdd(Pixel, out Index);
            (*pScan)[XBit] = Index;
            IsPixelValid = true;
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

        public int GetPaletteIndex()
        {
            EnsurePixel();
            return Index;
        }

        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            Index = (*pScan)[XBit];
            Pixel = Palette[Index];

            IsPixelValid = true;
        }

        public override void DangerousMove(int X, int Y)
        {
            int XBits = X * BitsPerPixel,
                OffsetX = XBits >> 3;

            XBit = (XBits & 0x07) / BitsPerPixel;
            pScan = pScan0 + Stride * Y + OffsetX;

            IsPixelValid = false;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            XBit += OffsetX;

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

            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            pScan += Stride * OffsetY;
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            XBit++;
            if (BitLength <= XBit)
            {
                XBit -= BitLength;
                pScan++;
            }
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousX()
        {
            XBit--;
            if (XBit < 0)
            {
                XBit += BitLength;
                pScan--;
            }
            IsPixelValid = false;
        }

        public override void DangerousMoveNextY()
        {
            pScan += Stride;
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            pScan -= Stride;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new PixelIndexedAdapter<T, Struct>(this);
    }

    internal sealed unsafe class PixelIndexedAdapter<T, U, Struct> : PixelIndexedAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public override int XLength { get; }

        public override int YLength { get; }

        private int Index;
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

        public ImagePalette<T> Palette { get; }

        private int XBit;
        private readonly int BitLength;
        private readonly Struct* pScan0;
        private readonly long Stride;
        private Struct* pScan;
        private PixelIndexedAdapter(PixelIndexedAdapter<T, U, Struct> Adapter)
        {
            X = Adapter.X;
            XBit = Adapter.XBit;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Stride = Adapter.Stride;
            BitLength = Adapter.BitLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            Palette = Adapter.Palette;
            pScan0 = Adapter.pScan0;
            pScan = Adapter.pScan;
        }
        public PixelIndexedAdapter(IImageIndexedContext Context, int X, int Y)
        {
            XLength = Context.Width;
            YLength = Context.Height;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;
            Palette = (ImagePalette<T>)Context.Palette;
            IsPixelValid = false;
            pScan0 = (Struct*)Context.Scan0[0];
            BitLength = pScan0->Length;

            this.X = X;
            this.Y = Y;
            DangerousMove(X, Y);
        }

        public override void OverrideIndex(int Index)
            => (*pScan)[XBit] = Index;

        public override void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<U> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
        {
            Pixel.Override(A, R, G, B);
            Pixel = Palette.GetOrAdd(Pixel, out Index);

            (*pScan)[XBit] = Index;
            IsPixelValid = true;
        }
        public override void OverrideTo(U* pData)
        {
            EnsurePixel();
            pData->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
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
            EnsurePixel();
            Pixel.Overlay(A, R, G, B);
            Pixel = Palette.GetOrAdd(Pixel, out Index);

            (*pScan)[XBit] = Index;
            IsPixelValid = true;
        }
        public override void OverlayTo(U* pData)
        {
            EnsurePixel();
            if (Pixel.A == byte.MaxValue)
                pData->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
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

        public int GetPaletteIndex()
        {
            EnsurePixel();
            return Index;
        }

        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            Index = (*pScan)[XBit];
            Pixel = Palette[Index];

            IsPixelValid = true;
        }

        public override void DangerousMove(int X, int Y)
        {
            int XBits = X * BitsPerPixel,
                OffsetX = XBits >> 3;

            XBit = (XBits & 0x07) / BitsPerPixel;
            pScan = pScan0 + Stride * Y + OffsetX;

            IsPixelValid = false;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            XBit += OffsetX;

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

            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            pScan += Stride * OffsetY;
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            XBit++;
            if (BitLength <= XBit)
            {
                XBit -= BitLength;
                pScan++;
            }
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousX()
        {
            XBit--;
            if (XBit < 0)
            {
                XBit += BitLength;
                pScan--;
            }
            IsPixelValid = false;
        }

        public override void DangerousMoveNextY()
        {
            pScan += Stride;
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            pScan -= Stride;
            IsPixelValid = false;
        }

        public override PixelAdapter<U> Clone()
            => new PixelIndexedAdapter<T, U, Struct>(this);

    }
}