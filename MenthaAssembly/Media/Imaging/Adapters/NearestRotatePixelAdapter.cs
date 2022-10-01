using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class NearestRotatePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly double Sin, Cos, FracX0, FracY0;
        private double FracX, FracY;

        private readonly int MaxSx, MaxSy;

        public override int MaxX { get; }

        public override int MaxY { get; }

        public override byte A
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : Source.A;
            }
        }

        public override byte R
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : Source.A;
            }
        }

        public override byte G
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : Source.A;
            }
        }

        public override byte B
        {
            get
            {
                EnsurePixel();
                return IsEmptyPixel ? byte.MinValue : Source.A;
            }
        }

        public override int BitsPerPixel
            => Source.BitsPerPixel;

        private readonly bool CalculateAlpth;
        private NearestRotatePixelAdapter(NearestRotatePixelAdapter<T> Adapter)
        {
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
            IsPixelValid = Adapter.IsPixelValid;
            IsEmptyPixel = Adapter.IsEmptyPixel;
            CalculateAlpth = Adapter.CalculateAlpth;
        }
        public NearestRotatePixelAdapter(IImageContext Context, double Angle)
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
            CalculateAlpth = Context.Channels == 4 || (Context.Channels == 1 && Context.BitsPerPixel == 32);
        }
        public NearestRotatePixelAdapter(PixelAdapter<T> Adapter, double Angle)
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
            CalculateAlpth = Adapter.BitsPerPixel != 32;
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
                Source.OverrideTo(pData);
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (!IsEmptyPixel)
                Source.OverrideTo(pDataR, pDataG, pDataB);
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (!IsEmptyPixel)
                Source.OverrideTo(pDataA, pDataR, pDataG, pDataB);
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
            if (!IsEmptyPixel)
                Source.OverlayTo(pData);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (!IsEmptyPixel)
                Source.OverlayTo(pDataR, pDataG, pDataB);
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (!IsEmptyPixel)
                Source.OverlayTo(pDataA, pDataR, pDataG, pDataB);
        }

        private bool IsPixelValid = false,
                     IsEmptyPixel = true;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            int a1 = (int)Math.Round(FracX),
                b1 = (int)Math.Round(FracY);

            IsEmptyPixel = a1 < 0 || MaxSx < a1 || b1 < 0 || MaxSy < b1;
            if (IsEmptyPixel)
                return;

            Source.InternalMove(a1, b1);
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
            => new NearestRotatePixelAdapter<T>(this);

    }
}
