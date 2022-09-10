using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class CropPixelAdapter<T> : IPixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly IPixelAdapter<T> Source;

        public int X { set; get; }

        public int Y { set; get; }

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

        private readonly int Sx, Sy;
        public CropPixelAdapter(CropPixelAdapter<T> Adapter)
        {
            Source = Adapter.Source.Clone();
            Sx = Adapter.Sx;
            Sy = Adapter.Sy;
            MaxX = Adapter.Source.MaxX;
            MaxY = Adapter.Source.MaxY;
        }
        public CropPixelAdapter(IImageContext Context, int X, int Y, int Width, int Height)
        {
            Source = Context.GetAdapter<T>(X, Y);
            Sx = X;
            Sy = Y;
            MaxX = Math.Max(X + Width - 1, Source.MaxX);
            MaxY = Math.Max(Y + Height - 1, Source.MaxY);
        }
        public CropPixelAdapter(IPixelAdapter<T> Adapter, int X, int Y, int Width, int Height)
        {
            Source = Adapter;
            Sx = Adapter.X - X;
            Sy = Adapter.Y - Y;
            MaxX = Math.Max(X + Width - 1, Source.MaxX);
            MaxY = Math.Max(Y + Height - 1, Source.MaxY);
        }

        public void Override(T Pixel)
            => throw new NotSupportedException();
        public void Override(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public void Override(IPixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public void OverrideTo(T* pData)
            => Source.OverrideTo(pData);
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataR, pDataG, pDataB);
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public void Overlay(T Pixel)
            => throw new NotSupportedException();
        public void Overlay(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public void Overlay(IPixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public void OverlayTo(T* pData)
            => Source.OverlayTo(pData);
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataR, pDataG, pDataB);
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => Source.OverlayTo(pDataA, pDataR, pDataG, pDataB);

        public void Move(int Offset)
        {
            int Nx = MathHelper.Clamp(X + Offset, 0, MaxX),
                Dx = Nx - X;
            if (Dx != 0)
            {
                X = Nx;
                InternalMove(Dx);
            }
        }
        public void Move(int X, int Y)
        {
            X = MathHelper.Clamp(X, 0, MaxX);
            Y = MathHelper.Clamp(Y, 0, MaxY);

            if (X != this.X || Y != this.Y)
            {
                this.X = X;
                this.Y = Y;
                InternalMove(X, Y);
            }
        }
        private void InternalMove(int Offset)
            => Source.InternalMove(Offset);
        private void InternalMove(int X, int Y)
            => Source.InternalMove(Sx + X, Sy + Y);

        public void MoveNext()
        {
            if (X < MaxX)
            {
                X++;
                InternalMoveNext();
            }
        }
        public void MovePrevious()
        {
            if (0 < X)
            {
                X--;
                InternalMovePrevious();
            }
        }
        private void InternalMoveNext()
            => Source.InternalMoveNext();
        private void InternalMovePrevious()
            => Source.InternalMovePrevious();

        public void MoveNextLine()
        {
            if (Y < MaxY)
            {
                Y++;
                InternalMoveNextLine();
            }
        }
        public void MovePreviousLine()
        {
            if (0 < Y)
            {
                Y--;
                InternalMovePreviousLine();
            }
        }
        private void InternalMoveNextLine()
            => Source.InternalMoveNextLine();
        private void InternalMovePreviousLine()
            => Source.InternalMovePreviousLine();

        void IPixelAdapter<T>.InternalMove(int Offset)
            => InternalMove(Offset);
        void IPixelAdapter<T>.InternalMove(int X, int Y)
            => InternalMove(X, Y);
        void IPixelAdapter<T>.InternalMoveNext()
            => InternalMoveNext();
        void IPixelAdapter<T>.InternalMovePrevious()
            => InternalMovePrevious();
        void IPixelAdapter<T>.InternalMoveNextLine()
            => InternalMoveNextLine();
        void IPixelAdapter<T>.InternalMovePreviousLine()
            => InternalMovePreviousLine();

        public IPixelAdapter<T> Clone()
            => new CropPixelAdapter<T>(this);

    }
}