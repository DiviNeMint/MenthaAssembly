using System;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the image adapter with the specified pixel type.
    /// </summary>
    public abstract unsafe class PixelAdapter<T> : IPixelAdapter, ICloneable
        where T : unmanaged, IPixel
    {
        private static readonly ParallelOptions DefaultParallelOptions = new();
        protected static readonly Type PixelType = typeof(T);

        public int X { protected set; get; } = int.MinValue;

        public int Y { protected set; get; } = int.MinValue;

        public abstract int MaxX { get; }

        public abstract int MaxY { get; }

        Type IPixelAdapter.PixelType
            => PixelType;

        public abstract byte A { get; }

        public abstract byte R { get; }

        public abstract byte G { get; }

        public abstract byte B { get; }

        public abstract int BitsPerPixel { get; }

        /// <summary>
        /// Overrides the current color components with the specified pixel.
        /// </summary>
        /// <param name="Pixel">The specified pixel.</param>
        public abstract void Override(T Pixel);

        /// <summary>
        /// Overrides the current color components with the color components of the specified adapter.
        /// </summary>
        /// <param name="Adapter">The specified pixel.</param>
        public abstract void Override(PixelAdapter<T> Adapter);

        /// <summary>
        /// Overrides the current color components with the specified color components.
        /// </summary>
        /// <param name="A">The specified alpha component.</param>
        /// <param name="R">The specified red component.</param>
        /// <param name="G">The specified green component.</param>
        /// <param name="B">The specified blue component.</param>
        public abstract void Override(byte A, byte R, byte G, byte B);

        /// <summary>
        /// Overrides the current color components to the specified data pointer.
        /// </summary>
        /// <param name="pData">The specified data pointer.</param>
        public virtual void OverrideTo(T* pData)
            => pData->Override(A, R, G, B);

        public virtual void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataR = R;
            *pDataG = G;
            *pDataB = B;
        }

        public virtual void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataA = A;
            *pDataR = R;
            *pDataG = G;
            *pDataB = B;
        }

        /// <summary>
        /// Overlays the current color components with the specified pixel.
        /// </summary>
        /// <param name="Pixel">The specified pixel.</param>
        public abstract void Overlay(T Pixel);

        /// <summary>
        /// Overlays the current color components with the color components of the specified adapter.
        /// </summary>
        /// <param name="Adapter">The specified pixel.</param>
        public abstract void Overlay(PixelAdapter<T> Adapter);

        /// <summary>
        /// Overlays the current color components with the specified color components.
        /// </summary>
        /// <param name="A">The specified alpha component.</param>
        /// <param name="R">The specified red component.</param>
        /// <param name="G">The specified green component.</param>
        /// <param name="B">The specified blue component.</param>
        public abstract void Overlay(byte A, byte R, byte G, byte B);

        /// <summary>
        /// Overlays the current color components to the specified data pointer.
        /// </summary>
        /// <param name="pData">The specified data pointer.</param>
        public virtual void OverlayTo(T* pData)
        {
            byte A = this.A;
            if (A == byte.MaxValue)
                OverrideTo(pData);
            else
                pData->Overlay(A, R, G, B);
        }

        public virtual void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            byte A = this.A;
            if (A == byte.MaxValue)
                OverrideTo(pDataR, pDataG, pDataB);
            else
                PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, A, R, G, B);
        }

        public virtual void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            byte A = this.A;
            if (A == byte.MaxValue)
                OverrideTo(pDataA, pDataR, pDataG, pDataB);
            else
                PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, A, R, G, B);
        }

        public virtual void Move(int X, int Y)
        {
            if (this.X == X)
                MoveY(Y - this.Y);
            else if (this.Y == Y)
                MoveX(X - this.X);
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

        /// <summary>
        /// Creates a new <see cref="ImageContext{T}"/> that is a copy of the current instance.
        /// </summary>
        public virtual ImageContext<T> ToImageContext()
        {
            int W = MaxX + 1,
                H = MaxY + 1;
            ImageContext<T> Context = new ImageContext<T>(W, H);
            PixelAdapter<T> Dest = Context.GetAdapter<T>(0, 0);

            InternalMove(0, 0);
            for (int j = 0; j < H; j++, InternalMoveNextLine(), Dest.InternalMoveNextLine())
            {
                for (int i = 0; i < W; i++, InternalMoveNext(), Dest.InternalMoveNext())
                    Dest.Override(this);

                InternalMoveX(-W);
                Dest.InternalMoveX(-W);
            }

            return Context;
        }
        /// <summary>
        /// Creates a new <see cref="ImageContext{T}"/> that is a copy of the current instance.
        /// </summary>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public virtual ImageContext<T> ToImageContext(ParallelOptions Options)
        {
            int H = MaxY + 1;
            ImageContext<T> Context = new ImageContext<T>(MaxX + 1, H);
            Parallel.For(0, H, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = Clone(),
                                Dest = Context.GetAdapter<T>(0, j);

                Sorc.InternalMove(0, j);
                for (int i = 0; i <= MaxX; i++, Sorc.InternalMoveNext(), Dest.InternalMoveNext())
                    Dest.Override(Sorc);

            });

            return Context;
        }

        /// <summary>
        /// Creates a new <see cref="PixelAdapter{T}"/> that is a copy of the current instance.
        /// </summary>
        public abstract PixelAdapter<T> Clone();
        IPixelAdapter IPixelAdapter.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

    }
}