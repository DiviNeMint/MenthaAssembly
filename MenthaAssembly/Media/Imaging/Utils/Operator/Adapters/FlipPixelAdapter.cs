using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class FlipPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;

        public override int MaxX
            => Source.MaxX;

        public override int MaxY
            => Source.MaxY;

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

        private readonly Func<int, int> ConvertX, ConvertY;
        private readonly Action _InternalMoveNext, _InternalMovePrevious, _InternalMoveNextLine, _InternalMovePreviousLine;
        private FlipPixelAdapter(FlipMode Mode)
        {
            switch (Mode)
            {
                case FlipMode.Horizontal:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => y;
                        _InternalMoveNext = () => Source.InternalMovePrevious();
                        _InternalMovePrevious = () => Source.InternalMoveNext();
                        _InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        _InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        ConvertX = x => x;
                        ConvertY = y => MaxY - y;
                        _InternalMoveNext = () => Source.InternalMoveNext();
                        _InternalMovePrevious = () => Source.InternalMovePrevious();
                        _InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        _InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => MaxY - y;
                        _InternalMoveNext = () => Source.InternalMovePrevious();
                        _InternalMovePrevious = () => Source.InternalMoveNext();
                        _InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        _InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.None:
                default:
                    {
                        ConvertX = x => x;
                        ConvertY = y => y;
                        _InternalMoveNext = () => Source.InternalMoveNext();
                        _InternalMovePrevious = () => Source.InternalMovePrevious();
                        _InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        _InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
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
            _InternalMoveNext = Adapter._InternalMoveNext;
            _InternalMovePrevious = Adapter._InternalMovePrevious;
            _InternalMoveNextLine = Adapter._InternalMoveNextLine;
            _InternalMovePreviousLine = Adapter._InternalMovePreviousLine;
        }
        public FlipPixelAdapter(IImageContext Context, FlipMode Mode) : this(Mode)
        {
            Source = Context.GetAdapter<T>(0, 0);
            InternalMove(0, 0);
        }
        public FlipPixelAdapter(PixelAdapter<T> Adapter, FlipMode Mode) : this(Mode)
        {
            Source = Adapter;

            switch (Mode)
            {
                case FlipMode.Horizontal:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => y;
                        _InternalMoveNext = () => Source.InternalMovePrevious();
                        _InternalMovePrevious = () => Source.InternalMoveNext();
                        _InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        _InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        ConvertX = x => x;
                        ConvertY = y => MaxY - y;
                        _InternalMoveNext = () => Source.InternalMoveNext();
                        _InternalMovePrevious = () => Source.InternalMovePrevious();
                        _InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        _InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        ConvertX = x => MaxX - x;
                        ConvertY = y => MaxY - y;
                        _InternalMoveNext = () => Source.InternalMovePrevious();
                        _InternalMovePrevious = () => Source.InternalMoveNext();
                        _InternalMoveNextLine = () => Source.InternalMovePreviousLine();
                        _InternalMovePreviousLine = () => Source.InternalMoveNextLine();
                        break;
                    }
                case FlipMode.None:
                default:
                    {
                        ConvertX = x => x;
                        ConvertY = y => y;
                        _InternalMoveNext = () => Source.InternalMoveNext();
                        _InternalMovePrevious = () => Source.InternalMovePrevious();
                        _InternalMoveNextLine = () => Source.InternalMoveNextLine();
                        _InternalMovePreviousLine = () => Source.InternalMovePreviousLine();
                        break;
                    }
            }

            X = ConvertX(Adapter.X);
            Y = ConvertY(Adapter.Y);
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

        protected internal override void InternalMove(int X, int Y)
            => Source.InternalMove(ConvertX(X), ConvertY(Y));
        protected internal override void InternalMoveX(int OffsetX)
            => Source.InternalMoveX(ConvertX(OffsetX) - ConvertX(0));
        protected internal override void InternalMoveY(int OffsetY)
            => Source.InternalMoveY(ConvertY(OffsetY) - ConvertY(0));

        protected internal override void InternalMoveNext()
            => _InternalMoveNext();
        protected internal override void InternalMovePrevious()
            => _InternalMovePrevious();

        protected internal override void InternalMoveNextLine()
            => _InternalMoveNextLine();
        protected internal override void InternalMovePreviousLine()
            => _InternalMovePreviousLine();

        public override PixelAdapter<T> Clone()
            => new FlipPixelAdapter<T>(this);

    }
}