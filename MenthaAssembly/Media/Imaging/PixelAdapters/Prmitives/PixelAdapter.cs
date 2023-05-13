using System;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the pixel adapter with the specified pixel type in image.
    /// </summary>
    public abstract unsafe class PixelAdapter<T> : IPixelAdapter<T>, ICloneable
        where T : unmanaged, IPixel
    {
        private static readonly ParallelOptions DefaultParallelOptions = new();
        protected static readonly Type PixelType = typeof(T);

        public int X { protected set; get; } = int.MinValue;

        public int Y { protected set; get; } = int.MinValue;

        public abstract int XLength { get; }

        public abstract int YLength { get; }

        Type IPixelAdapter.PixelType
            => PixelType;

        public abstract byte A { get; }

        public abstract byte R { get; }

        public abstract byte G { get; }

        public abstract byte B { get; }

        public abstract int BitsPerPixel { get; }

        public abstract void Override(T Pixel);

        public abstract void Override(PixelAdapter<T> Adapter);

        /// <summary>
        /// Overrides the current color components with the specified color components.
        /// </summary>
        /// <param name="A">The specified alpha component.</param>
        /// <param name="R">The specified red component.</param>
        /// <param name="G">The specified green component.</param>
        /// <param name="B">The specified blue component.</param>
        public abstract void Override(byte A, byte R, byte G, byte B);

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

        public abstract void Overlay(T Pixel);

        public abstract void Overlay(PixelAdapter<T> Adapter);

        /// <summary>
        /// Overlays the current color components with the specified color components.
        /// </summary>
        /// <param name="A">The specified alpha component.</param>
        /// <param name="R">The specified red component.</param>
        /// <param name="G">The specified green component.</param>
        /// <param name="B">The specified blue component.</param>
        public abstract void Overlay(byte A, byte R, byte G, byte B);

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
                OffsetY(Y - this.Y);
            else if (this.Y == Y)
                OffsetX(X - this.X);
            else
            {
                X = MathHelper.Clamp(X, 0, XLength - 1);
                Y = MathHelper.Clamp(Y, 0, YLength - 1);
                this.X = X;
                this.Y = Y;
                InternalMove(X, Y);
            }
        }

        public virtual void OffsetX(int Delta)
        {
            int Nx = MathHelper.Clamp(X + Delta, 0, XLength - 1),
                Dx = Nx - X;
            if (Dx != 0)
            {
                X = Nx;
                InternalOffsetX(Dx);
            }
        }

        public virtual void OffsetY(int Delta)
        {
            int Ny = MathHelper.Clamp(Y + Delta, 0, YLength - 1),
                Dy = Ny - Y;
            if (Dy != 0)
            {
                Y = Ny;
                InternalOffsetY(Dy);
            }
        }

        public virtual void MoveNextX()
        {
            if (X < XLength - 1)
            {
                X++;
                InternalMoveNextX();
            }
        }

        public virtual void MoveNextY()
        {
            if (Y < YLength - 1)
            {
                Y++;
                InternalMoveNextY();
            }
        }

        public virtual void MovePreviousX()
        {
            if (0 < X)
            {
                X--;
                InternalMovePreviousX();
            }
        }

        public virtual void MovePreviousY()
        {
            if (0 < Y)
            {
                Y--;
                InternalMovePreviousY();
            }
        }

        protected internal abstract void InternalMove(int X, int Y);
        protected internal abstract void InternalOffsetX(int Delta);
        protected internal abstract void InternalOffsetY(int Delta);
        protected internal abstract void InternalMoveNextX();
        protected internal abstract void InternalMoveNextY();
        protected internal abstract void InternalMovePreviousX();
        protected internal abstract void InternalMovePreviousY();

        void IPixelAdapter.InternalMove(int X, int Y) => InternalMove(X, Y);
        void IPixelAdapter.InternalOffsetX(int Delta) => InternalOffsetX(Delta);
        void IPixelAdapter.InternalOffsetY(int Delta) => InternalOffsetY(Delta);
        void IPixelAdapter.InternalMoveNextX() => InternalMoveNextX();
        void IPixelAdapter.InternalMoveNextY() => InternalMoveNextY();
        void IPixelAdapter.InternalMovePreviousX() => InternalMovePreviousX();
        void IPixelAdapter.InternalMovePreviousY() => InternalMovePreviousY();

        /// <summary>
        /// Creates a new <see cref="ImageContext{T}"/> that is a copy of the current instance.
        /// </summary>
        public virtual ImageContext<T> ToImageContext()
        {
            ImageContext<T> Context = new(XLength, YLength);
            PixelAdapter<T> Dest = Context.GetAdapter<T>(0, 0);

            InternalMove(0, 0);

            int Dx = -XLength;
            for (int j = 0; j < YLength; j++, InternalMoveNextY(), Dest.InternalMoveNextY())
            {
                for (int i = 0; i < XLength; i++, InternalMoveNextX(), Dest.InternalMoveNextX())
                    Dest.Override(this);

                InternalOffsetX(Dx);
                Dest.InternalOffsetX(Dx);
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
            ImageContext<T> Context = new(XLength, YLength);
            Parallel.For(0, YLength, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = Clone(),
                                Dest = Context.GetAdapter<T>(0, j);

                Sorc.InternalMove(0, j);
                for (int i = 0; i < XLength; i++, Sorc.InternalMoveNextX(), Dest.InternalMoveNextX())
                    Dest.Override(Sorc);

            });

            return Context;
        }

        /// <summary>
        /// Creates a new <see cref="PixelAdapter{T}"/> that is a copy of the current instance.
        /// </summary>
        public abstract PixelAdapter<T> Clone();
        IPixelAdapter<T> IPixelAdapter<T>.Clone()
            => Clone();
        IPixelAdapter IPixelAdapter.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

    }
}