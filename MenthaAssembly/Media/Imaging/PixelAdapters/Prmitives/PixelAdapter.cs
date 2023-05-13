using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the pixel adapter with the specified pixel type in image.
    /// </summary>
    public abstract unsafe class PixelAdapter<T> : IPixelAdapter, ICloneable
        where T : unmanaged, IPixel
    {
        private static readonly ParallelOptions DefaultParallelOptions = new();
        protected static readonly Type PixelType = typeof(T);

        public int X {  set; get; } = int.MinValue;

        public int Y {  set; get; } = int.MinValue;

        public abstract int XLength { get; }

        public abstract int YLength { get; }

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

        public virtual void OffsetX(int Delta)
        {
            int Nx = MathHelper.Clamp(X + Delta, 0, XLength - 1),
                Dx = Nx - X;
            if (Dx != 0)
            {
                X = Nx;
                DangerousOffsetX(Dx);
            }
        }

        public virtual void OffsetY(int Delta)
        {
            int Ny = MathHelper.Clamp(Y + Delta, 0, YLength - 1),
                Dy = Ny - Y;
            if (Dy != 0)
            {
                Y = Ny;
                DangerousOffsetY(Dy);
            }
        }

        public virtual void MoveNextX()
        {
            if (X < XLength - 1)
            {
                X++;
                DangerousMoveNextX();
            }
        }

        public virtual void MoveNextY()
        {
            if (Y < YLength - 1)
            {
                Y++;
                DangerousMoveNextY();
            }
        }

        public virtual void MovePreviousX()
        {
            if (0 < X)
            {
                X--;
                DangerousMovePreviousX();
            }
        }

        public virtual void MovePreviousY()
        {
            if (0 < Y)
            {
                Y--;
                DangerousMovePreviousY();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void DangerousMove(int X, int Y);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void DangerousOffsetX(int Delta);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void DangerousOffsetY(int Delta);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void DangerousMoveNextX();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void DangerousMoveNextY();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void DangerousMovePreviousX();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void DangerousMovePreviousY();

        /// <summary>
        /// Creates a new <see cref="ImageContext{T}"/> that is a copy of the current instance.
        /// </summary>
        public virtual ImageContext<T> ToImageContext()
        {
            ImageContext<T> Context = new(XLength, YLength);
            PixelAdapter<T> Dest = Context.GetAdapter<T>(0, 0);

            DangerousMove(0, 0);

            int Dx = -XLength;
            for (int j = 0; j < YLength; j++, DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < XLength; i++, DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(this);

                DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
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

                Sorc.DangerousMove(0, j);
                for (int i = 0; i < XLength; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
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
        IImageAdapter IImageAdapter.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

    }
}