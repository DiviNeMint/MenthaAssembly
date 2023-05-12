using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class BilinearRotatePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly double Sin, Cos, FracX0, FracY0;
        private double FracX, FracY;

        private readonly int MaxSx, MaxSy;

        public override int MaxX { get; }

        public override int MaxY { get; }

        private byte _A = byte.MaxValue;
        public override byte A
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : _A;
            }
        }

        private byte _R;
        public override byte R
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : _R;
            }
        }

        private byte _G;
        public override byte G
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : _G;
            }
        }

        private byte _B;
        public override byte B
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : _B;
            }
        }

        public override int BitsPerPixel
            => Source.BitsPerPixel;

        private readonly bool CalculateAlpth;
        private BilinearRotatePixelAdapter(BilinearRotatePixelAdapter<T> Adapter)
        {
            if (Adapter.IsPixelValid)
            {
                IsPixelValid = Adapter.IsPixelValid;
                IsEmptyPixel = Adapter.IsEmptyPixel;

                if (!IsEmptyPixel)
                {
                    _A = Adapter._A;
                    _R = Adapter._R;
                    _G = Adapter._G;
                    _B = Adapter._B;
                }
            }

            Sin = Adapter.Sin;
            Cos = Adapter.Cos;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            MaxSx = Adapter.MaxSx;
            MaxSy = Adapter.MaxSy;
            FracX = Adapter.FracX;
            FracY = Adapter.FracY;
            FracX0 = Adapter.FracX0;
            FracY0 = Adapter.FracY0;
            Source = Adapter.Source.Clone();
            IsEmptyPixel = Adapter.IsEmptyPixel;
            CalculateAlpth = Adapter.CalculateAlpth;
        }
        public BilinearRotatePixelAdapter(IImageContext Context, double Angle)
        {
            double Theta = Angle * MathHelper.UnitTheta;

            Sin = Math.Sin(Theta);
            Cos = Math.Cos(Theta);
            MaxSx = Context.Width;
            MaxSy = Context.Height;

            MaxX = (int)(Math.Abs(MaxSx * Cos) + Math.Abs(MaxSy * Sin));
            MaxY = (int)(Math.Abs(MaxSx * Sin) + Math.Abs(MaxSy * Cos));
            FracX0 = -(MaxX * Cos + MaxY * Sin - MaxSx) / 2d;
            FracY0 = (MaxX * Sin - MaxY * Cos + MaxSy) / 2d;
            FracX = FracX0;
            FracY = FracY0;

            MaxX--;
            MaxY--;
            MaxSx--;
            MaxSy--;

            Source = Context.GetAdapter<T>(0, 0);
            CalculateAlpth = !PixelHelper.IsNonAlphaPixel(typeof(T));
        }
        public BilinearRotatePixelAdapter(PixelAdapter<T> Adapter, double Angle)
        {
            double Theta = Angle * MathHelper.UnitTheta;

            Sin = Math.Sin(Theta);
            Cos = Math.Cos(Theta);
            MaxSx = Adapter.MaxX;
            MaxSy = Adapter.MaxY;
            MaxX = (int)(Math.Abs(MaxSx * Cos) + Math.Abs(MaxSy * Sin));
            MaxY = (int)(Math.Abs(MaxSx * Sin) + Math.Abs(MaxSy * Cos));
            FracX0 = -(MaxX * Cos + MaxY * Sin - MaxSx) / 2d;
            FracY0 = (MaxX * Sin - MaxY * Cos + MaxSy) / 2d;
            FracX = FracX0;
            FracY = FracY0;

            MaxX--;
            MaxY--;
            MaxSx--;
            MaxSy--;

            Source = Adapter;
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
            if (!IsEmptyPixel)
                pData->Override(_A, _R, _G, _B);
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (!IsEmptyPixel)
            {
                *pDataR = _R;
                *pDataG = _G;
                *pDataB = _B;
            }
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (!IsEmptyPixel)
            {
                *pDataA = _A;
                *pDataR = _R;
                *pDataG = _G;
                *pDataB = _B;
            }
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
            if (IsEmptyPixel)
                return;

            if (_A == byte.MaxValue)
                pData->Override(byte.MaxValue, _R, _G, _B);
            else
                pData->Overlay(_A, _R, _G, _B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (IsEmptyPixel)
                return;

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
            if (IsEmptyPixel)
                return;

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

        private bool IsPixelValid = false,
                     IsEmptyPixel = true;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            int a1 = (int)Math.Floor(FracX),
                b1 = (int)Math.Floor(FracY);

            IsEmptyPixel = a1 < 0 || MaxSx < a1 || b1 < 0 || MaxSy < b1;
            if (IsEmptyPixel)
                return;

            Source.InternalMove(a1, b1);
            PixelAdapter<T> p00 = Source,
                            p10 = p00,
                            p01 = p00,
                            p11 = p10;

            if (b1 < MaxSy)
            {
                p10 = p00.Clone();
                p10.InternalMoveNextLine();
            }

            if (a1 < MaxSx)
            {
                p01 = p00.Clone();
                p11 = p10.Clone();
                p01.InternalMoveNext();
                p11.InternalMoveNext();
            }

            float TFracX = (float)(FracX - a1),
                  TFracY = (float)(FracY - b1),
                  IFracX = 1f - TFracX,
                  IFracY = 1f - TFracY,
                  IFxIFy = IFracX * IFracY,
                  IFxFy = IFracX * TFracY,
                  FxIFy = TFracX * IFracY,
                  FxFy = TFracX * TFracY;

            if (CalculateAlpth)
                _A = (byte)(p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy);

            _R = (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy);
            _G = (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy);
            _B = (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy);

            IsPixelValid = true;
        }

        protected internal override void InternalMove(int X, int Y)
        {
            FracX = FracX0 + X * Cos + Y * Sin;
            FracY = FracY0 + Y * Cos - X * Sin;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            FracX += Cos * OffsetX;
            FracY -= Sin * OffsetX;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            FracX += Sin * OffsetY;
            FracY += Cos * OffsetY;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            FracX += Cos;
            FracY -= Sin;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            FracX -= Cos;
            FracY += Sin;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            FracX += Sin;
            FracY += Cos;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            FracX -= Sin;
            FracY -= Cos;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new BilinearRotatePixelAdapter<T>(this);

    }
}
