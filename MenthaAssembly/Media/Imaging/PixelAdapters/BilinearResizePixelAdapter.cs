using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class BilinearResizePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly int SourceMaxX, SourceMaxY;
        private readonly float StepX, StepY;
        private float FracX, FracY;

        public override int XLength { get; }

        public override int YLength { get; }

        private byte _A = byte.MaxValue;
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
        private BilinearResizePixelAdapter(BilinearResizePixelAdapter<T> Adapter)
        {
            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                _A = Adapter._A;
                _R = Adapter._R;
                _G = Adapter._G;
                _B = Adapter._B;
            }

            StepX = Adapter.StepX;
            StepY = Adapter.StepY;
            FracX = Adapter.FracX;
            FracY = Adapter.FracY;
            IFracY = Adapter.IFracY;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();
            SourceMaxX = Adapter.SourceMaxX;
            SourceMaxY = Adapter.SourceMaxY;
            CalculateAlpth = Adapter.CalculateAlpth;
        }
        public BilinearResizePixelAdapter(IImageContext Context, int NewWidth, int NewHeight)
        {
            int Sw = Context.Width,
                Sh = Context.Height;

            StepX = (float)Sw / NewWidth;
            StepY = (float)Sh / NewHeight;
            XLength = NewWidth;
            YLength = NewHeight;

            X = 0;
            Y = 0;
            Source = Context.GetAdapter<T>(0, 0);
            SourceMaxX = Sw - 1;
            SourceMaxY = Sh - 1;
            CalculateAlpth = !PixelHelper.IsNonAlphaPixel(typeof(T));
        }
        public BilinearResizePixelAdapter(PixelAdapter<T> Adapter, int NewWidth, int NewHeight)
        {
            int Sw = Adapter.XLength,
                Sh = Adapter.YLength;

            StepX = (float)Sw / NewWidth;
            StepY = (float)Sh / NewHeight;
            XLength = NewWidth;
            YLength = NewHeight;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter;
            SourceMaxX = Sw - 1;
            SourceMaxY = Sh - 1;
            CalculateAlpth = !PixelHelper.IsNonAlphaPixel(typeof(T));
            Adapter.DangerousMove(0, 0);
        }
        internal BilinearResizePixelAdapter(IImageContext Context, int X, int Y, float StepX, float StepY)
        {
            int Sw = Context.Width,
                Sh = Context.Height;

            // Can calculate by step & context size
            XLength = -1;
            YLength = -1;

            this.StepX = StepX;
            this.StepY = StepY;

            FracX = X * StepX;
            FracY = Y * StepY;

            int Tx = (int)Math.Floor(FracX),
                Ty = (int)Math.Floor(FracY);
            this.X = Tx;
            this.Y = Ty;
            Source = Context.GetAdapter<T>(Tx, Ty);
            SourceMaxX = Sw - 1;
            SourceMaxY = Sh - 1;

            FracX -= Tx;
            FracY -= Ty;
            IFracY = 1f - FracY;

            CalculateAlpth = !PixelHelper.IsNonAlphaPixel(typeof(T));
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
            if (_A == byte.MaxValue)
                pData->Override(byte.MaxValue, _R, _G, _B);
            else
                pData->Overlay(_A, _R, _G, _B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (_A == byte.MaxValue)
            {
                *pDataR = _R;
                *pDataG = _G;
                *pDataB = _B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, _A, _R, _G, _B);
            }
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (_A == byte.MaxValue)
            {
                *pDataA = byte.MaxValue;
                *pDataR = _R;
                *pDataG = _G;
                *pDataB = _B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, _A, _R, _G, _B);
            }
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

            if (Source.Y < SourceMaxY)
            {
                p10 = p00.Clone();
                p10.DangerousMoveNextY();
            }

            if (Source.X < SourceMaxX)
            {
                p01 = p00.Clone();
                p11 = p10.Clone();
                p01.DangerousMoveNextX();
                p11.DangerousMoveNextX();
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

            IsPixelValid = true;
        }

        public override void DangerousMove(int X, int Y)
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
        public override void DangerousOffsetX(int OffsetX)
        {
            FracX += StepX * OffsetX;

            int Dx = (int)Math.Floor(FracX);
            Source.OffsetX(Dx);

            FracX -= Dx;
            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            FracY += StepY * OffsetY;

            int Dy = (int)Math.Floor(FracY);
            Source.OffsetY(Dy);

            FracY -= Dy;
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            FracX += StepX;
            while (FracX >= 1f)
            {
                FracX -= 1f;
                Source.MoveNextX();
            }
            IsPixelValid = false;
        }
        public override void DangerousMoveNextY()
        {
            FracY += StepY;
            while (FracY >= 1f)
            {
                FracY -= 1f;
                Source.MoveNextY();
            }

            IFracY = 1f - FracY;
            IsPixelValid = false;
        }

        public override void DangerousMovePreviousX()
        {
            FracX -= StepX;
            while (FracX < 0f)
            {
                FracX += 1f;
                Source.MovePreviousX();
            }
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            FracY -= StepY;
            while (FracY < 0f)
            {
                FracY += 1f;
                Source.MovePreviousY();
            }

            IFracY = 1f - FracY;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new BilinearResizePixelAdapter<T>(this);

    }
}