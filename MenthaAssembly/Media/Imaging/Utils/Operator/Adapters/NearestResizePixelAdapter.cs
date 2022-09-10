using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class NearestResizePixelAdapter<T> : IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly IPixelAdapter<T> Source;
        private readonly float StepX, StepY;
        private float FracX, FracY;

        public int X
            => throw new NotImplementedException();

        public int Y
            => throw new NotImplementedException();

        public int MaxX { get; }

        public int MaxY { get; }

        public byte A
            => Source.A;

        public byte R
            => Source.R;

        public byte G
            => Source.G;

        public byte B
            => Source.B;

        public int BitsPerPixel
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

        public void Override(T Pixel)
            => Source.Override(Pixel);
        public void Override(IPixelAdapter<T> Adapter)
            => Source.Override(Adapter);
        public void Override(byte A, byte R, byte G, byte B)
            => Source.Override(A, R, G, B);
        public void OverrideTo(T* pData)
            => Source.OverrideTo(pData);
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataR, pDataG, pDataB);
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public void Overlay(T Pixel)
            => Source.Overlay(Pixel);
        public void Overlay(IPixelAdapter<T> Adapter)
            => Source.Overlay(Adapter);
        public void Overlay(byte A, byte R, byte G, byte B)
            => Source.Overlay(A, R, G, B);
        public void OverlayTo(T* pData)
            => Source.OverlayTo(pData);
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataR, pDataG, pDataB);
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataA, pDataR, pDataG, pDataB);

        public void Move(int Offset)
        {
            FracX += StepX * Offset;

            int Dx = (int)Math.Floor(FracX);
            Source.InternalMove(Dx);

            FracX -= Dx;
        }
        public void Move(int X, int Y)
            => throw new NotImplementedException();

        public void MoveNext()
        {
            FracX += StepX;
            while (FracX >= 1f)
            {
                FracX -= 1f;
                Source.InternalMoveNext();
            }
        }
        public void MovePrevious()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.InternalMovePrevious();
            }
        }

        public void MoveNextLine()
        {
            FracY += StepY;
            while (FracY >= 1f)
            {
                FracY -= 1f;
                Source.InternalMoveNextLine();
            }
        }
        public void MovePreviousLine()
        {
            FracY -= StepY;
            while (FracY < 0f)
            {
                FracY += 1f;
                Source.InternalMovePreviousLine();
            }
        }

        void IPixelAdapter<T>.InternalMove(int Offset)
            => Move(Offset);
        void IPixelAdapter<T>.InternalMove(int X, int Y)
            => Move(X, Y);
        void IPixelAdapter<T>.InternalMoveNext()
            => MoveNext();
        void IPixelAdapter<T>.InternalMovePrevious()
            => MovePrevious();
        void IPixelAdapter<T>.InternalMoveNextLine()
            => MoveNextLine();
        void IPixelAdapter<T>.InternalMovePreviousLine()
            => MovePreviousLine();

        public IPixelAdapter<T> Clone()
            => throw new NotImplementedException();

    }
}