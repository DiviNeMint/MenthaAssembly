using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class NearestResizePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly float StepX, StepY;
        private float FracX, FracY;

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

        private NearestResizePixelAdapter(NearestResizePixelAdapter<T> Adapter)
        {
            Source = Adapter.Source.Clone();
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            StepX = Adapter.StepX;
            StepY = Adapter.StepY;
            FracX = Adapter.FracX;
            FracY = Adapter.FracY;
        }
        public NearestResizePixelAdapter(IImageContext Context, int NewWidth, int NewHeight)
        {
            Source = Context.GetAdapter<T>(0, 0);
            XLength = NewWidth;
            YLength = NewHeight;
            StepX = (float)Context.Width / NewWidth;
            StepY = (float)Context.Height / NewHeight;
        }
        public NearestResizePixelAdapter(PixelAdapter<T> Adapter, int NewWidth, int NewHeight)
        {
            Source = Adapter;
            XLength = NewWidth;
            YLength = NewHeight;
            StepX = (float)Adapter.XLength / NewWidth;
            StepY = (float)Adapter.YLength / NewHeight;
            Adapter.InternalMove(0, 0);
        }
        internal NearestResizePixelAdapter(IImageContext Context, int X, int Y, float StepX, float StepY)
        {
            this.StepX = StepX;
            this.StepY = StepY;

            FracX = X * StepX;
            FracY = Y * StepY;

            int Tx = (int)Math.Floor(FracX),
                Ty = (int)Math.Floor(FracY);
            Source = Context.GetAdapter<T>(Tx, Ty);

            FracX -= Tx;
            FracY -= Ty;
        }

        public override void Override(T Pixel)
            => Source.Override(Pixel);
        public override void Override(PixelAdapter<T> Adapter)
            => Source.Override(Adapter);
        public override void Override(byte A, byte R, byte G, byte B)
            => Source.Override(A, R, G, B);
        public override void OverrideTo(T* pData)
            => Source.OverrideTo(pData);
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataR, pDataG, pDataB);
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public override void Overlay(T Pixel)
            => Source.Overlay(Pixel);
        public override void Overlay(PixelAdapter<T> Adapter)
            => Source.Overlay(Adapter);
        public override void Overlay(byte A, byte R, byte G, byte B)
            => Source.Overlay(A, R, G, B);
        public override void OverlayTo(T* pData)
            => Source.OverlayTo(pData);
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataR, pDataG, pDataB);
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataA, pDataR, pDataG, pDataB);

        protected internal override void InternalMove(int X, int Y)
        {
            FracX = X * StepX;
            FracY = Y * StepY;

            int Tx = (int)Math.Floor(FracX),
                Ty = (int)Math.Floor(FracY);
            Source.InternalMove(Tx, Ty);

            FracX -= Tx;
            FracY -= Ty;
        }
        protected internal override void InternalOffsetX(int OffsetX)
        {
            FracX += StepX * OffsetX;

            int Dx = (int)Math.Floor(FracX);
            Source.InternalOffsetX(Dx);

            FracX -= Dx;
        }
        protected internal override void InternalOffsetY(int OffsetY)
        {
            FracY += StepY * OffsetY;

            int Dy = (int)Math.Floor(FracY);
            Source.InternalOffsetY(Dy);

            FracY -= Dy;
        }

        protected internal override void InternalMoveNextX()
        {
            FracX += StepX;
            while (FracX >= 1f)
            {
                FracX -= 1f;
                Source.InternalMoveNextX();
            }
        }
        protected internal override void InternalMovePreviousX()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.InternalMovePreviousX();
            }
        }

        protected internal override void InternalMoveNextY()
        {
            FracY += StepY;
            while (FracY >= 1f)
            {
                FracY -= 1f;
                Source.InternalMoveNextY();
            }
        }
        protected internal override void InternalMovePreviousY()
        {
            FracY -= StepY;
            while (FracY < 0f)
            {
                FracY += 1f;
                Source.InternalMovePreviousY();
            }
        }

        public override PixelAdapter<T> Clone()
            => new NearestResizePixelAdapter<T>(this);

    }
}