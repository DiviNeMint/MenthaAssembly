using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class NearestResizePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly float StepX, StepY;
        private float FracX, FracY;

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

        public NearestResizePixelAdapter(IImageContext Context, int X, int Y, float StepX, float StepY)
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

        public override void Move(int Offset)
        {
            FracX += StepX * Offset;

            int Dx = (int)Math.Floor(FracX);
            Source.InternalMove(Dx);

            FracX -= Dx;
        }
        public override void Move(int X, int Y)
            => throw new NotImplementedException();

        public override void MoveNext()
        {
            FracX += StepX;
            while (FracX >= 1f)
            {
                FracX -= 1f;
                Source.InternalMoveNext();
            }
        }
        public override void MovePrevious()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.InternalMovePrevious();
            }
        }

        public override void MoveNextLine()
        {
            FracY += StepY;
            while (FracY >= 1f)
            {
                FracY -= 1f;
                Source.InternalMoveNextLine();
            }
        }
        public override void MovePreviousLine()
        {
            FracY -= StepY;
            while (FracY < 0f)
            {
                FracY += 1f;
                Source.InternalMovePreviousLine();
            }
        }

        protected internal override void InternalMove(int Offset)
            => Move(Offset);
        protected internal override void InternalMove(int X, int Y)
            => Move(X, Y);

        protected internal override void InternalMoveNext()
            => MoveNext();
        protected internal override void InternalMovePrevious()
            => MovePrevious();

        protected internal override void InternalMoveNextLine()
            => MoveNextLine();
        protected internal override void InternalMovePreviousLine()
            => MovePreviousLine();

        public override PixelAdapter<T> Clone()
            => throw new NotImplementedException();

    }
}