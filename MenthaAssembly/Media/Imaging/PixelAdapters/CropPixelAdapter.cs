using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class CropPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;

        public override int XLength { get; }

        public override int YLength { get; }

        public override byte A
            => Source.A;

        public override byte R
            => Source.R;

        public override byte G
            => Source.G;

        public override byte B
            => Source.B;

        public override int BitsPerPixel
            => Source.BitsPerPixel;

        private readonly int Sx, Sy;
        private CropPixelAdapter(CropPixelAdapter<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();

            Sx = Adapter.Sx;
            Sy = Adapter.Sy;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
        }
        public CropPixelAdapter(IImageContext Context, int X, int Y, int Width, int Height)
        {
            X = 0;
            Y = 0;
            Source = Context.GetAdapter<T>(X, Y);

            Sx = X;
            Sy = Y;
            XLength = Math.Min(Width, Source.XLength - X);
            YLength = Math.Min(Height, Source.YLength - Y);
        }
        public CropPixelAdapter(PixelAdapter<T> Adapter, int X, int Y, int Width, int Height)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter;

            Sx = X;
            Sy = Y;
            XLength = Math.Min(Width, Source.XLength - X);
            YLength = Math.Min(Height, Source.YLength - Y);
        }

        public override void Override(T Pixel)
            => throw new NotSupportedException();
        public override void Override(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void Override(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void OverrideTo(T* pData)
            => Source.OverrideTo(pData);
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataR, pDataG, pDataB);
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public override void Overlay(T Pixel)
            => throw new NotSupportedException();
        public override void Overlay(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void Overlay(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void OverlayTo(T* pData)
            => Source.OverlayTo(pData);
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataR, pDataG, pDataB);
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataA, pDataR, pDataG, pDataB);

        public override void DangerousMove(int X, int Y)
            => Source.DangerousMove(Sx + X, Sy + Y);
        public override void DangerousOffsetX(int OffsetX)
            => Source.DangerousOffsetX(OffsetX);
        public override void DangerousOffsetY(int OffsetY)
            => Source.DangerousOffsetY(OffsetY);

        public override void DangerousMoveNextX()
            => Source.DangerousMoveNextX();
        public override void DangerousMovePreviousX()
            => Source.DangerousMovePreviousX();

        public override void DangerousMoveNextY()
            => Source.DangerousMoveNextY();
        public override void DangerousMovePreviousY()
            => Source.DangerousMovePreviousY();

        public override PixelAdapter<T> Clone()
            => new CropPixelAdapter<T>(this);

    }
}