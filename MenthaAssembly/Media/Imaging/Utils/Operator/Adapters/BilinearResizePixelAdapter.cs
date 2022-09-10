using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class BilinearResizePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly float StepX, StepY;
        private float FracX, FracY;

        public override int MaxX { get; }

        public override int MaxY { get; }

        private byte _A;
        public override byte A => _A;

        private byte _R;
        public override byte R => _R;

        private byte _G;
        public override byte G => _G;

        private byte _B;
        public override byte B => _B;

        public override int BitsPerPixel
            => Source.BitsPerPixel;

        private readonly bool CalculateAlpth;
        public BilinearResizePixelAdapter(IImageContext Context, int X, int Y, float StepX, float StepY)
        {
            CalculateAlpth = Context.Channels == 4 || (Context.Channels == 1 && Context.BitsPerPixel == 32);

            if (!CalculateAlpth)
                _A = byte.MaxValue;

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

        public override void Override(T Pixel)
            => throw new NotSupportedException();
        public override void Override(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void Override(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void OverrideTo(T* pData)
        {
            EnsurePixel();
            pData->Override(A, R, G, B);
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = R;
            *pDataG = G;
            *pDataB = B;
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = A;
            *pDataR = R;
            *pDataG = G;
            *pDataB = B;
        }

        public override void Overlay(T Pixel)
            => throw new NotSupportedException();
        public override void Overlay(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void Overlay(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void OverlayTo(T* pData)
        {
            EnsurePixel();
            pData->Overlay(A, R, G, B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, A, R, G, B);
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
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

            PixelAdapter<T> p00 = Source,
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
                _A = (byte)(p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy);

            _R = (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy);
            _G = (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy);
            _B = (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy);
        }

        public override void Move(int Offset)
        {
            FracX += StepX * Offset;

            int Dx = (int)Math.Floor(FracX);
            Source.InternalMove(Dx);

            FracX -= Dx;
            IsPixelValid = false;
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
            IsPixelValid = false;
        }
        public override void MovePrevious()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.InternalMovePrevious();
            }
            IsPixelValid = false;
        }

        public override void MoveNextLine()
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
        public override void MovePreviousLine()
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