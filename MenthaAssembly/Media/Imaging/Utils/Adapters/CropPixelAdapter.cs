using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class CropPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;

        public override int MaxX { get; }

        public override int MaxY { get; }

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
        public CropPixelAdapter(CropPixelAdapter<T> Adapter)
        {
            Source = Adapter.Source.Clone();
            Sx = Adapter.Sx;
            Sy = Adapter.Sy;
            MaxX = Adapter.Source.MaxX;
            MaxY = Adapter.Source.MaxY;
        }
        public CropPixelAdapter(IImageContext Context, int X, int Y, int Width, int Height)
        {
            Source = Context.GetAdapter<T>(X, Y);
            Sx = X;
            Sy = Y;
            MaxX = Math.Min(Width - 1, Source.MaxX - X);
            MaxY = Math.Min(Height - 1, Source.MaxY - Y);
        }
        public CropPixelAdapter(PixelAdapter<T> Adapter, int X, int Y, int Width, int Height)
        {
            Source = Adapter;
            Sx = X;
            Sy = Y;
            MaxX = Math.Min(Width - 1, Source.MaxX - X);
            MaxY = Math.Min(Height - 1, Source.MaxY - Y);
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
        protected internal override void InternalMoveX(int OffsetX)
            => Source.InternalMoveX(OffsetX);
        protected internal override void InternalMoveY(int OffsetY)
            => Source.InternalMoveY(OffsetY);

        protected internal override void InternalMoveNext()
            => Source.InternalMoveNext();
        protected internal override void InternalMovePrevious()
            => Source.InternalMovePrevious();

        protected internal override void InternalMoveNextLine()
            => Source.InternalMoveNextLine();
        protected internal override void InternalMovePreviousLine()
            => Source.InternalMovePreviousLine();

        public override PixelAdapter<T> Clone()
            => new CropPixelAdapter<T>(this);

    }
}