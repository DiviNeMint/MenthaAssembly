using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class CropPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;

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

        private readonly int Sx, Sy;
        private CropPixelAdapter(CropPixelAdapter<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();

            Sx = Adapter.Sx;
            Sy = Adapter.Sy;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
        }
        public CropPixelAdapter(IImageContext Context, int X, int Y, int Width, int Height)
        {
            X = 0;
            Y = 0;
            Source = Context.GetAdapter<T>(X, Y);

            Sx = X;
            Sy = Y;
            XLength = Math.Min(Width, Source.XLength - X);
            YLength = Math.Min(Height, Source.YLength - Y);
        }
        public CropPixelAdapter(PixelAdapter<T> Adapter, int X, int Y, int Width, int Height)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter;

            Sx = X;
            Sy = Y;
            XLength = Math.Min(Width, Source.XLength - X);
            YLength = Math.Min(Height, Source.YLength - Y);
        }

        public override void Override(T Pixel)
            => throw new NotSupportedException();
        public override void Override(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void Override(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void OverrideTo(T* pData)
            => Source.OverrideTo(pData);
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataR, pDataG, pDataB);
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public override void Overlay(T Pixel)
            => throw new NotSupportedException();
        public override void Overlay(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void Overlay(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void OverlayTo(T* pData)
            => Source.OverlayTo(pData);
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataR, pDataG, pDataB);
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataA, pDataR, pDataG, pDataB);

        public override void DangerousMove(int X, int Y)
            => Source.DangerousMove(Sx + X, Sy + Y);
        public override void DangerousOffsetX(int OffsetX)
            => Source.DangerousOffsetX(OffsetX);
        public override void DangerousOffsetY(int OffsetY)
            => Source.DangerousOffsetY(OffsetY);

        public override void DangerousMoveNextX()
            => Source.DangerousMoveNextX();
        public override void DangerousMovePreviousX()
            => Source.DangerousMovePreviousX();

        public override void DangerousMoveNextY()
            => Source.DangerousMoveNextY();
        public override void DangerousMovePreviousY()
            => Source.DangerousMovePreviousY();

        public override PixelAdapter<T> Clone()
            => new CropPixelAdapter<T>(this);

    }
    public sealed unsafe class CropPixelAdapter : IPixelAdapter
    {
        private readonly IPixelAdapter Source;

        public int X { set; get; } = int.MinValue;

        public int Y { set; get; } = int.MinValue;

        public int XLength { get; }

        public int YLength { get; }

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

        public Type PixelType
            => Source.PixelType;

        private readonly int Sx, Sy;
        private CropPixelAdapter(CropPixelAdapter Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();

            Sx = Adapter.Sx;
            Sy = Adapter.Sy;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
        }
        public CropPixelAdapter(IImageContext Context, int X, int Y, int Width, int Height)
        {
            X = 0;
            Y = 0;
            Source = Context.GetAdapter(X, Y);

            Sx = X;
            Sy = Y;
            XLength = Math.Min(Width, Source.XLength - X);
            YLength = Math.Min(Height, Source.YLength - Y);
        }
        public CropPixelAdapter(IPixelAdapter Adapter, int X, int Y, int Width, int Height)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter;

            Sx = X;
            Sy = Y;
            XLength = Math.Min(Width, Source.XLength - X);
            YLength = Math.Min(Height, Source.YLength - Y);
        }

        public void Override(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataR, pDataG, pDataB);
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public void Overlay(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataR, pDataG, pDataB);
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataA, pDataR, pDataG, pDataB);

        public void Move(int X, int Y)
        {
            if (this.X == X)
                OffsetY(Y - this.Y);
            else if (this.Y == Y)
                OffsetX(X - this.X);
            else
            {
                X = MathHelper.Clamp(X, 0, XLength - 1);
                Y = MathHelper.Clamp(Y, 0, YLength - 1);
                this.X = X;
                this.Y = Y;
                DangerousMove(X, Y);
            }
        }
        public void OffsetX(int Delta)
        {
            int Nx = MathHelper.Clamp(X + Delta, 0, XLength - 1),
                Dx = Nx - X;
            if (Dx != 0)
            {
                X = Nx;
                DangerousOffsetX(Dx);
            }
        }
        public void OffsetY(int Delta)
        {
            int Ny = MathHelper.Clamp(Y + Delta, 0, YLength - 1),
                Dy = Ny - Y;
            if (Dy != 0)
            {
                Y = Ny;
                DangerousOffsetY(Dy);
            }
        }

        public void MoveNextX()
        {
            if (X < XLength - 1)
            {
                X++;
                DangerousMoveNextX();
            }
        }
        public void MoveNextY()
        {
            if (Y < YLength - 1)
            {
                Y++;
                DangerousMoveNextY();
            }
        }
        public void MovePreviousX()
        {
            if (0 < X)
            {
                X--;
                DangerousMovePreviousX();
            }
        }
        public void MovePreviousY()
        {
            if (0 < Y)
            {
                Y--;
                DangerousMovePreviousY();
            }
        }

        public void DangerousMove(int X, int Y)
            => Source.DangerousMove(Sx + X, Sy + Y);
        public void DangerousOffsetX(int OffsetX)
            => Source.DangerousOffsetX(OffsetX);
        public void DangerousOffsetY(int OffsetY)
            => Source.DangerousOffsetY(OffsetY);

        public void DangerousMoveNextX()
            => Source.DangerousMoveNextX();
        public void DangerousMovePreviousX()
            => Source.DangerousMovePreviousX();

        public void DangerousMoveNextY()
            => Source.DangerousMoveNextY();
        public void DangerousMovePreviousY()
            => Source.DangerousMovePreviousY();

        public IPixelAdapter Clone()
            => new CropPixelAdapter(this);
        IImageAdapter IImageAdapter.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

    }
}