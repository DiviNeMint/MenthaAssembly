using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class FlipPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private readonly PixelAdapter<T> Source;

        public override int XLength
            => Source.XLength;

        public override int YLength
            => Source.YLength;

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
                        ConvertX = x => XLength - x - 1;
                        ConvertY = y => y;
                        _InternalMoveNext = Source.InternalMovePreviousX;
                        _InternalMovePrevious = Source.InternalMoveNextX;
                        _InternalMoveNextLine = Source.InternalMoveNextY;
                        _InternalMovePreviousLine = Source.InternalMovePreviousY;
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        ConvertX = x => x;
                        ConvertY = y => YLength - y - 1;
                        _InternalMoveNext = Source.InternalMoveNextX;
                        _InternalMovePrevious = Source.InternalMovePreviousX;
                        _InternalMoveNextLine = Source.InternalMovePreviousY;
                        _InternalMovePreviousLine = Source.InternalMoveNextY;
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        ConvertX = x => XLength - x - 1;
                        ConvertY = y => YLength - y - 1;
                        _InternalMoveNext = Source.InternalMovePreviousX;
                        _InternalMovePrevious = Source.InternalMoveNextX;
                        _InternalMoveNextLine = Source.InternalMovePreviousY;
                        _InternalMovePreviousLine = Source.InternalMoveNextY;
                        break;
                    }
                case FlipMode.None:
                default:
                    {
                        ConvertX = x => x;
                        ConvertY = y => y;
                        _InternalMoveNext = Source.InternalMoveNextX;
                        _InternalMovePrevious = Source.InternalMovePreviousX;
                        _InternalMoveNextLine = Source.InternalMoveNextY;
                        _InternalMovePreviousLine = Source.InternalMovePreviousY;
                        break;
                    }
            }
        }
        private FlipPixelAdapter(FlipPixelAdapter<T> Adapter)
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
                        ConvertX = x => XLength - x - 1;
                        ConvertY = y => y;
                        _InternalMoveNext = Source.InternalMovePreviousX;
                        _InternalMovePrevious = Source.InternalMoveNextX;
                        _InternalMoveNextLine = Source.InternalMoveNextY;
                        _InternalMovePreviousLine = Source.InternalMovePreviousY;
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        ConvertX = x => x;
                        ConvertY = y => YLength - y - 1;
                        _InternalMoveNext = Source.InternalMoveNextX;
                        _InternalMovePrevious = Source.InternalMovePreviousX;
                        _InternalMoveNextLine = Source.InternalMovePreviousY;
                        _InternalMovePreviousLine = Source.InternalMoveNextY;
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        ConvertX = x => XLength - x - 1;
                        ConvertY = y => YLength - y - 1;
                        _InternalMoveNext = Source.InternalMovePreviousX;
                        _InternalMovePrevious = Source.InternalMoveNextX;
                        _InternalMoveNextLine = Source.InternalMovePreviousY;
                        _InternalMovePreviousLine = Source.InternalMoveNextY;
                        break;
                    }
                case FlipMode.None:
                default:
                    {
                        ConvertX = x => x;
                        ConvertY = y => y;
                        _InternalMoveNext = Source.InternalMoveNextX;
                        _InternalMovePrevious = Source.InternalMovePreviousX;
                        _InternalMoveNextLine = Source.InternalMoveNextY;
                        _InternalMovePreviousLine = Source.InternalMovePreviousY;
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
        protected internal override void InternalOffsetX(int OffsetX)
            => Source.InternalOffsetX(ConvertX(OffsetX) - ConvertX(0));
        protected internal override void InternalOffsetY(int OffsetY)
            => Source.InternalOffsetY(ConvertY(OffsetY) - ConvertY(0));

        protected internal override void InternalMoveNextX()
            => _InternalMoveNext();
        protected internal override void InternalMovePreviousX()
            => _InternalMovePrevious();

        protected internal override void InternalMoveNextY()
            => _InternalMoveNextLine();
        protected internal override void InternalMovePreviousY()
            => _InternalMovePreviousLine();

        public override PixelAdapter<T> Clone()
            => new FlipPixelAdapter<T>(this);

    }
}