using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class FlipPixelAdapter<T> : IPixelAdapter<T>
            where T : unmanaged, IPixel
    {
        private readonly IPixelAdapter<T> Source;

        public int X { set; get; }

        public int Y { set; get; }

        public int MaxX
            => Source.MaxX;

        public int MaxY
            => Source.MaxY;

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

        private readonly Func<int, int> ConvertX, ConvertY;
        private readonly Action InternalMoveNext, InternalMovePrevious, InternalMoveNextLine, InternalMovePreviousLine;
        private FlipPixelAdapter(FlipMode Mode)
        {
            switch (Mode)
            {
                case FlipMode.Horizontal:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => y;
                        InternalMoveNext = () => Source.InternalMovePrevious();
                        InternalMovePrevious = () => Source.InternalMoveNext();
                        InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        ConvertX = x => x;
                        ConvertY = y => MaxY - y;
                        InternalMoveNext = () => Source.InternalMoveNext();
                        InternalMovePrevious = () => Source.InternalMovePrevious();
                        InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => MaxY - y;
                        InternalMoveNext = () => Source.InternalMovePrevious();
                        InternalMovePrevious = () => Source.InternalMoveNext();
                        InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.None:
                default:
                    {
                        ConvertX = x => x;
                        ConvertY = y => y;
                        InternalMoveNext = () => Source.InternalMoveNext();
                        InternalMovePrevious = () => Source.InternalMovePrevious();
                        InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
                        break;
                    }
            }
        }
        public FlipPixelAdapter(FlipPixelAdapter<T> Adapter)
        {
            Source = Adapter.Source.Clone();
            X = Adapter.X;
            Y = Adapter.Y;
            ConvertX = Adapter.ConvertX;
            ConvertY = Adapter.ConvertY;
            InternalMoveNext = Adapter.InternalMoveNext;
            InternalMovePrevious = Adapter.InternalMovePrevious;
            InternalMoveNextLine = Adapter.InternalMoveNextLine;
            InternalMovePreviousLine = Adapter.InternalMovePreviousLine;
        }
        public FlipPixelAdapter(IImageContext Context, FlipMode Mode) : this(Mode)
        {
            Source = Context.GetAdapter<T>(0, 0);
            InternalMove(0, 0);
        }
        public FlipPixelAdapter(IPixelAdapter<T> Adapter, FlipMode Mode) : this(Mode)
        {
            Source = Adapter;

            switch (Mode)
            {
                case FlipMode.Horizontal:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => y;
                        InternalMoveNext = () => Source.InternalMovePrevious();
                        InternalMovePrevious = () => Source.InternalMoveNext();
                        InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        ConvertX = x => x;
                        ConvertY = y => MaxY - y;
                        InternalMoveNext = () => Source.InternalMoveNext();
                        InternalMovePrevious = () => Source.InternalMovePrevious();
                        InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => MaxY - y;
                        InternalMoveNext = () => Source.InternalMovePrevious();
                        InternalMovePrevious = () => Source.InternalMoveNext();
                        InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.None:
                default:
                    {
                        ConvertX = x => x;
                        ConvertY = y => y;
                        InternalMoveNext = () => Source.InternalMoveNext();
                        InternalMovePrevious = () => Source.InternalMovePrevious();
                        InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
                        break;
                    }
            }

            X = ConvertX(Adapter.X);
            Y = ConvertY(Adapter.Y);
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
            => Source.InternalMove(ConvertX(Offset) - ConvertX(0));
        private void InternalMove(int X, int Y)
            => Source.InternalMove(ConvertX(X), ConvertY(Y));

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
            => new FlipPixelAdapter<T>(this);

    }

}