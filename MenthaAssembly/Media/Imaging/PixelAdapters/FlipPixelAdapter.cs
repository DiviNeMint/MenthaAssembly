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
                        _InternalMoveNext = Source.DangerousMovePreviousX;
                        _InternalMovePrevious = Source.DangerousMoveNextX;
                        _InternalMoveNextLine = Source.DangerousMoveNextY;
                        _InternalMovePreviousLine = Source.DangerousMovePreviousY;
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        ConvertX = x => x;
                        ConvertY = y => YLength - y - 1;
                        _InternalMoveNext = Source.DangerousMoveNextX;
                        _InternalMovePrevious = Source.DangerousMovePreviousX;
                        _InternalMoveNextLine = Source.DangerousMovePreviousY;
                        _InternalMovePreviousLine = Source.DangerousMoveNextY;
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        ConvertX = x => XLength - x - 1;
                        ConvertY = y => YLength - y - 1;
                        _InternalMoveNext = Source.DangerousMovePreviousX;
                        _InternalMovePrevious = Source.DangerousMoveNextX;
                        _InternalMoveNextLine = Source.DangerousMovePreviousY;
                        _InternalMovePreviousLine = Source.DangerousMoveNextY;
                        break;
                    }
                case FlipMode.None:
                default:
                    {
                        ConvertX = x => x;
                        ConvertY = y => y;
                        _InternalMoveNext = Source.DangerousMoveNextX;
                        _InternalMovePrevious = Source.DangerousMovePreviousX;
                        _InternalMoveNextLine = Source.DangerousMoveNextY;
                        _InternalMovePreviousLine = Source.DangerousMovePreviousY;
                        break;
                    }
            }
        }
        private FlipPixelAdapter(FlipPixelAdapter<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();
            ConvertX = Adapter.ConvertX;
            ConvertY = Adapter.ConvertY;
            _InternalMoveNext = Adapter._InternalMoveNext;
            _InternalMovePrevious = Adapter._InternalMovePrevious;
            _InternalMoveNextLine = Adapter._InternalMoveNextLine;
            _InternalMovePreviousLine = Adapter._InternalMovePreviousLine;
        }
        public FlipPixelAdapter(IImageContext Context, FlipMode Mode) : this(Mode)
        {
            X = 0;
            Y = 0;
            Source = Context.GetAdapter<T>(0, 0);
            DangerousMove(0, 0);
        }
        public FlipPixelAdapter(PixelAdapter<T> Adapter, FlipMode Mode) : this(Mode)
        {
            X = ConvertX(Adapter.X);
            Y = ConvertY(Adapter.Y);
            Source = Adapter;
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
            => Source.DangerousMove(ConvertX(X), ConvertY(Y));
        public override void DangerousOffsetX(int OffsetX)
            => Source.DangerousOffsetX(ConvertX(OffsetX) - ConvertX(0));
        public override void DangerousOffsetY(int OffsetY)
            => Source.DangerousOffsetY(ConvertY(OffsetY) - ConvertY(0));

        public override void DangerousMoveNextX()
            => _InternalMoveNext();
        public override void DangerousMovePreviousX()
            => _InternalMovePrevious();

        public override void DangerousMoveNextY()
            => _InternalMoveNextLine();
        public override void DangerousMovePreviousY()
            => _InternalMovePreviousLine();

        public override PixelAdapter<T> Clone()
            => new FlipPixelAdapter<T>(this);

    }
}