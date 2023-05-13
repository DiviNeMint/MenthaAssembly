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
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            StepX = Adapter.StepX;
            StepY = Adapter.StepY;
            FracX = Adapter.FracX;
            FracY = Adapter.FracY;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();
        }
        public NearestResizePixelAdapter(IImageContext Context, int NewWidth, int NewHeight)
        {
            XLength = NewWidth;
            YLength = NewHeight;
            StepX = (float)Context.Width / NewWidth;
            StepY = (float)Context.Height / NewHeight;

            X = 0;
            Y = 0;
            Source = Context.GetAdapter<T>(0, 0);
        }
        public NearestResizePixelAdapter(PixelAdapter<T> Adapter, int NewWidth, int NewHeight)
        {
            XLength = NewWidth;
            YLength = NewHeight;
            StepX = (float)Adapter.XLength / NewWidth;
            StepY = (float)Adapter.YLength / NewHeight;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter;
            Adapter.DangerousMove(0, 0);
        }
        internal NearestResizePixelAdapter(IImageContext Context, int X, int Y, float StepX, float StepY)
        {
            this.StepX = StepX;
            this.StepY = StepY;

            FracX = X * StepX;
            FracY = Y * StepY;

            int Tx = (int)Math.Floor(FracX),
                Ty = (int)Math.Floor(FracY);
            this.X = Tx;
            this.Y = Ty;
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

        public override void DangerousMove(int X, int Y)
        {
            FracX = X * StepX;
            FracY = Y * StepY;

            int Tx = (int)Math.Floor(FracX),
                Ty = (int)Math.Floor(FracY);
            Source.DangerousMove(Tx, Ty);

            FracX -= Tx;
            FracY -= Ty;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            FracX += StepX * OffsetX;

            int Dx = (int)Math.Floor(FracX);
            Source.DangerousOffsetX(Dx);

            FracX -= Dx;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            FracY += StepY * OffsetY;

            int Dy = (int)Math.Floor(FracY);
            Source.DangerousOffsetY(Dy);

            FracY -= Dy;
        }

        public override void DangerousMoveNextX()
        {
            FracX += StepX;
            while (FracX >= 1f)
            {
                FracX -= 1f;
                Source.DangerousMoveNextX();
            }
        }
        public override void DangerousMovePreviousX()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.DangerousMovePreviousX();
            }
        }

        public override void DangerousMoveNextY()
        {
            FracY += StepY;
            while (FracY >= 1f)
            {
                FracY -= 1f;
                Source.DangerousMoveNextY();
            }
        }
        public override void DangerousMovePreviousY()
        {
            FracY -= StepY;
            while (FracY < 0f)
            {
                FracY += 1f;
                Source.DangerousMovePreviousY();
            }
        }

        public override PixelAdapter<T> Clone()
            => new NearestResizePixelAdapter<T>(this);

    }
}