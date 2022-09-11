namespace MenthaAssembly.Media.Imaging.Utils
{
    public abstract unsafe class PixelAdapter<T> : IPixel
        where T : unmanaged, IPixel
    {
        public int X { protected set; get; } = -1;

        public int Y { protected set; get; } = -1;

        public abstract int MaxX { get; }

        public abstract int MaxY { get; }

        public abstract byte A { get; }

        public abstract byte R { get; }

        public abstract byte G { get; }

        public abstract byte B { get; }

        public abstract int BitsPerPixel { get; }

        public abstract void Override(T Pixel);

        public abstract void Override(PixelAdapter<T> Adapter);

        public abstract void Override(byte A, byte R, byte G, byte B);

        public abstract void OverrideTo(T* pData);

        public abstract void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public abstract void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public abstract void Overlay(T Pixel);

        public abstract void Overlay(PixelAdapter<T> Adapter);

        public abstract void Overlay(byte A, byte R, byte G, byte B);

        public abstract void OverlayTo(T* pData);

        public abstract void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public abstract void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public virtual void Move(int X, int Y)
        {
            if (this.X == X)
                MoveY(this.Y - Y);
            else if (this.Y == Y)
                MoveX(this.X - X);
            else
            {
                X = MathHelper.Clamp(X, 0, MaxX);
                Y = MathHelper.Clamp(Y, 0, MaxY);
                this.X = X;
                this.Y = Y;
                InternalMove(X, Y);
            }
        }

        public virtual void MoveX(int OffsetX)
        {
            int Nx = MathHelper.Clamp(X + OffsetX, 0, MaxX),
                Dx = Nx - X;
            if (Dx != 0)
            {
                X = Nx;
                InternalMoveX(Dx);
            }
        }

        public virtual void MoveY(int OffsetY)
        {
            int Ny = MathHelper.Clamp(Y + OffsetY, 0, MaxY),
                Dy = Ny - Y;
            if (Dy != 0)
            {
                Y = Ny;
                InternalMoveY(Dy);
            }
        }

        public virtual void MoveNext()
        {
            if (X < MaxX)
            {
                X++;
                InternalMoveNext();
            }
        }

        public virtual void MovePrevious()
        {
            if (0 < X)
            {
                X--;
                InternalMovePrevious();
            }
        }

        public virtual void MoveNextLine()
        {
            if (Y < MaxY)
            {
                Y++;
                InternalMoveNextLine();
            }
        }

        public virtual void MovePreviousLine()
        {
            if (0 < Y)
            {
                Y--;
                InternalMovePreviousLine();
            }
        }

        protected internal abstract void InternalMove(int X, int Y);

        protected internal abstract void InternalMoveX(int OffsetX);

        protected internal abstract void InternalMoveY(int OffsetY);

        protected internal abstract void InternalMoveNext();

        protected internal abstract void InternalMovePrevious();

        protected internal abstract void InternalMoveNextLine();

        protected internal abstract void InternalMovePreviousLine();

        public abstract PixelAdapter<T> Clone();

    }
}