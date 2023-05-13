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
            Source = Adapter.Source.Clone();
            Sx = Adapter.Sx;
            Sy = Adapter.Sy;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
        }
        public CropPixelAdapter(IImageContext Context, int X, int Y, int Width, int Height)
        {
            Source = Context.GetAdapter<T>(X, Y);
            Sx = X;
            Sy = Y;
            XLength = Math.Min(Width, Source.XLength - X);
            YLength = Math.Min(Height, Source.YLength - Y);
        }
        public CropPixelAdapter(PixelAdapter<T> Adapter, int X, int Y, int Width, int Height)
        {
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

        protected internal override void InternalMove(int X, int Y)
            => Source.InternalMove(Sx + X, Sy + Y);
        protected internal override void InternalOffsetX(int OffsetX)
            => Source.InternalOffsetX(OffsetX);
        protected internal override void InternalOffsetY(int OffsetY)
            => Source.InternalOffsetY(OffsetY);

        protected internal override void InternalMoveNextX()
            => Source.InternalMoveNextX();
        protected internal override void InternalMovePreviousX()
            => Source.InternalMovePreviousX();

        protected internal override void InternalMoveNextY()
            => Source.InternalMoveNextY();
        protected internal override void InternalMovePreviousY()
            => Source.InternalMovePreviousY();

        public override PixelAdapter<T> Clone()
            => new CropPixelAdapter<T>(this);

    }
}