using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class NearestRotatePixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;
        private readonly double Sin, Cos, FracX0, FracY0;
        private double FracX, FracY;

        public override int XLength { get; }

        public override int YLength { get; }

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

        private NearestRotatePixelAdapter(NearestRotatePixelAdapter<T> Adapter)
        {
            Sin = Adapter.Sin;
            Cos = Adapter.Cos;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            FracX = Adapter.FracX;
            FracY = Adapter.FracY;
            FracX0 = Adapter.FracX0;
            FracY0 = Adapter.FracY0;

            IsPixelValid = Adapter.IsPixelValid;
            IsEmptyPixel = Adapter.IsEmptyPixel;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();
        }
        public NearestRotatePixelAdapter(IImageContext Context, double Angle)
        {
            double Theta = Angle * MathHelper.UnitTheta;
            int Sw = Context.Width,
                Sh = Context.Height;

            Sin = Math.Sin(Theta);
            Cos = Math.Cos(Theta);
            XLength = (int)(Math.Abs(Sw * Cos) + Math.Abs(Sh * Sin));
            YLength = (int)(Math.Abs(Sw * Sin) + Math.Abs(Sh * Cos));
            FracX0 = -(XLength * Cos + YLength * Sin - Sw) / 2d;
            FracY0 = (XLength * Sin - YLength * Cos + Sh) / 2d;
            FracX = FracX0;
            FracY = FracY0;

            X = 0;
            Y = 0;
            Source = Context.GetAdapter<T>(0, 0);
        }
        public NearestRotatePixelAdapter(PixelAdapter<T> Adapter, double Angle)
        {
            double Theta = Angle * MathHelper.UnitTheta;
            int Sw = Adapter.XLength,
                Sh = Adapter.YLength;

            Sin = Math.Sin(Theta);
            Cos = Math.Cos(Theta);
            XLength = (int)(Math.Abs(Sw * Cos) + Math.Abs(Sh * Sin));
            YLength = (int)(Math.Abs(Sw * Sin) + Math.Abs(Sh * Cos));
            FracX0 = -(XLength * Cos + YLength * Sin - Sw) / 2d;
            FracY0 = (XLength * Sin - YLength * Cos + Sh) / 2d;
            FracX = FracX0;
            FracY = FracY0;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter;
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

            IsEmptyPixel = a1 < 0 || Source.XLength <= a1 || b1 < 0 || Source.YLength <= b1;
            if (IsEmptyPixel)
                return;

            Source.DangerousMove(a1, b1);
            IsPixelValid = true;
        }

        public override void DangerousMove(int X, int Y)
        {
            FracX = FracX0 + X * Cos + Y * Sin;
            FracY = FracY0 + Y * Cos - X * Sin;
            IsPixelValid = false;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            FracX += Cos * OffsetX;
            FracY -= Sin * OffsetX;
            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            FracX += Sin * OffsetY;
            FracY += Cos * OffsetY;
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            FracX += Cos;
            FracY -= Sin;
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousX()
        {
            FracX -= Cos;
            FracY += Sin;
            IsPixelValid = false;
        }

        public override void DangerousMoveNextY()
        {
            FracX += Sin;
            FracY += Cos;
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            FracX -= Sin;
            FracY -= Cos;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new NearestRotatePixelAdapter<T>(this);

    }
}