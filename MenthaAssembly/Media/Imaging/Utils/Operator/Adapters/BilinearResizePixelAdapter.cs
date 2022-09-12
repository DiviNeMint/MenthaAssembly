using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class BilinearResizePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly float StepX, StepY;
        private float FracX, FracY;

        public override int MaxX { get; }

        public override int MaxY { get; }

        private byte _A;
        public override byte A
        {
            get
            {
                EnsurePixel();
                return _A;
            }
        }

        private byte _R;
        public override byte R
        {
            get
            {
                EnsurePixel();
                return _R;
            }
        }

        private byte _G;
        public override byte G
        {
            get
            {
                EnsurePixel();
                return _G;
            }
        }

        private byte _B;
        public override byte B
        {
            get
            {
                EnsurePixel();
                return _B;
            }
        }

        public override int BitsPerPixel
            => Source.BitsPerPixel;

        private readonly bool CalculateAlpth;
        public BilinearResizePixelAdapter(BilinearResizePixelAdapter<T> Adapter)
        {
            _A = Adapter._A;
            _R = Adapter._R;
            _G = Adapter._G;
            _B = Adapter._B;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            StepX = Adapter.StepX;
            StepY = Adapter.StepY;
            FracX = Adapter.FracX;
            FracY = Adapter.FracY;
            IFracY = Adapter.IFracY;
            Source = Adapter.Source.Clone();
            IsPixelValid = Adapter.IsPixelValid;
            CalculateAlpth = Adapter.CalculateAlpth;
        }
        public BilinearResizePixelAdapter(IImageContext Context, int NewWidth, int NewHeight)
        {
            Source = Context.GetAdapter<T>(0, 0);
            StepX = (float)Context.Width / NewWidth;
            StepY = (float)Context.Height / NewHeight;
            MaxX = NewWidth - 1;
            MaxY = NewHeight - 1;
            CalculateAlpth = Context.Channels == 4 || (Context.Channels == 1 && Context.BitsPerPixel == 32);
        }
        public BilinearResizePixelAdapter(PixelAdapter<T> Adapter, int NewWidth, int NewHeight)
        {
            Source = Adapter;
            StepX = (float)(Adapter.MaxX + 1) / NewWidth;
            StepY = (float)(Adapter.MaxY + 1) / NewHeight;
            MaxX = NewWidth - 1;
            MaxY = NewHeight - 1;
            CalculateAlpth = Adapter.BitsPerPixel != 32;
            Adapter.InternalMove(0, 0);
        }
        internal BilinearResizePixelAdapter(IImageContext Context, int X, int Y, float StepX, float StepY)
        {
            CalculateAlpth = Context.Channels == 4 || (Context.Channels == 1 && Context.BitsPerPixel == 32);

            if (!CalculateAlpth)
                _A = byte.MaxValue;

            // Can calculate by step & context size
            MaxX = -1;
            MaxY = -1;

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
            pData->Override(_A, _R, _G, _B);
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = _R;
            *pDataG = _G;
            *pDataB = _B;
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = _A;
            *pDataR = _R;
            *pDataG = _G;
            *pDataB = _B;
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
            pData->Overlay(_A, _R, _G, _B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, _A, _R, _G, _B);
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, _A, _R, _G, _B);
        }

        private float IFracY = 1f;
        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            PixelAdapter<T> p00 = Source,
                            p10 = p00,
                            p01 = p00,
                            p11 = p10;

            if (Source.Y < Source.MaxY)
            {
                p10 = p00.Clone();
                p10.InternalMoveNextLine();
            }

            if (Source.X < Source.MaxX)
            {
                p01 = p00.Clone();
                p11 = p10.Clone();
                p01.InternalMoveNext();
                p11.InternalMoveNext();
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

        protected internal override void InternalMove(int X, int Y)
        {
            FracX = X * StepX;
            FracY = Y * StepY;

            int Tx = (int)Math.Floor(FracX),
                Ty = (int)Math.Floor(FracY);
            Source.Move(Tx, Ty);

            FracX -= Tx;
            FracY -= Ty;
            IFracY = 1f - FracY;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            FracX += StepX * OffsetX;

            int Dx = (int)Math.Floor(FracX);
            Source.MoveX(Dx);

            FracX -= Dx;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            FracY += StepY * OffsetY;

            int Dy = (int)Math.Floor(FracY);
            Source.MoveY(Dy);

            FracY -= Dy;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            FracX += StepX;
            while (FracX >= 1f)
            {
                FracX -= 1f;
                Source.MoveNext();
            }
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.MovePrevious();
            }
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            FracY += StepY;
            while (FracY >= 1f)
            {
                FracY -= 1f;
                Source.MoveNextLine();
            }

            IFracY = 1f - FracY;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            FracY -= StepY;
            while (FracY < 0f)
            {
                FracY += 1f;
                Source.MovePreviousLine();
            }

            IFracY = 1f - FracY;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new BilinearResizePixelAdapter<T>(this);

    }
}