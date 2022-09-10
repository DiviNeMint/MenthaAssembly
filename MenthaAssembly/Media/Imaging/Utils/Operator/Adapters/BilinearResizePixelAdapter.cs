using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class BilinearResizePixelAdapter<T> : IPixelAdapter<T>
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

        public byte A { set; get; }

        public byte R { set; get; }

        public byte G { set; get; }

        public byte B { set; get; }

        public int BitsPerPixel
            => Source.BitsPerPixel;

        private readonly bool CalculateAlpth;
        public BilinearResizePixelAdapter(IImageContext Context, int X, int Y, float StepX, float StepY)
        {
            CalculateAlpth = Context.Channels == 4 || (Context.Channels == 1 && Context.BitsPerPixel == 32);

            if (!CalculateAlpth)
                A = byte.MaxValue;

            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;

            this.StepX = StepX;
            this.StepY = StepY;

            FracX = X * StepX;
            FracY = Y * StepY;

            int Tx = (int)Math.Floor(FracX),
                Ty = (int)Math.Floor(FracY);
            Source = Context.GetAdapter<T>(Tx, Ty);

            FracX -= Tx;
            FracY -= Ty;
            IFracY = 1f - FracY;
        }

        public void Override(T Pixel)
            => throw new NotSupportedException();
        public void Override(IPixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public void Override(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public void OverrideTo(T* pData)
        {
            EnsurePixel();
            pData->Override(A, R, G, B);
        }
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = R;
            *pDataG = G;
            *pDataB = B;
        }
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = A;
            *pDataR = R;
            *pDataG = G;
            *pDataB = B;
        }

        public void Overlay(T Pixel)
            => throw new NotSupportedException();
        public void Overlay(IPixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public void Overlay(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public void OverlayTo(T* pData)
        {
            EnsurePixel();
            pData->Overlay(A, R, G, B);
        }
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, A, R, G, B);
        }
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, A, R, G, B);
        }

        private bool IsPixelValid = false;

        private float IFracY;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            IPixelAdapter<T> p00 = Source,
                             p10 = p00,
                             p01 = p00,
                             p11 = p10;

            if (Source.Y < MaxY)
            {
                p10 = p00.Clone();
                p10.MoveNextLine();
            }

            if (Source.X < MaxX)
            {
                p01 = p00.Clone();
                p11 = p10.Clone();
                p01.MoveNext();
                p11.MoveNext();
            }

            float IFracX = 1f - FracX,
                  IFxIFy = IFracX * IFracY,
                  IFxFy = IFracX * FracY,
                  FxIFy = FracX * IFracY,
                  FxFy = FracX * FracY;

            if (CalculateAlpth)
                A = (byte)(p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy);

            R = (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy);
            G = (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy);
            B = (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy);
        }

        public void Move(int Offset)
        {
            FracX += StepX * Offset;

            int Dx = (int)Math.Floor(FracX);
            Source.InternalMove(Dx);

            FracX -= Dx;
            IsPixelValid = false;
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
            IsPixelValid = false;
        }
        public void MovePrevious()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.InternalMovePrevious();
            }
            IsPixelValid = false;
        }

        public void MoveNextLine()
        {
            FracY += StepY;
            while (FracY >= 1f)
            {
                FracY -= 1f;
                Source.InternalMoveNextLine();
            }

            IFracY = 1f - FracY;
            IsPixelValid = false;
        }
        public void MovePreviousLine()
        {
            FracY -= StepY;
            while (FracY < 0f)
            {
                FracY += 1f;
                Source.InternalMovePreviousLine();
            }

            IFracY = 1f - FracY;
            IsPixelValid = false;
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