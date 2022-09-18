using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class ThresholdingPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private static readonly Type GrayType = typeof(Gray8);

        private readonly PixelAdapter<T> Source;

        public override int MaxX => Source.MaxX;

        public override int MaxY => Source.MaxY;

        private byte _Value;
        public override byte A
        {
            get
            {
                EnsurePixel();
                return _Value;
            }
        }

        public override byte R
        {
            get
            {
                EnsurePixel();
                return _Value;
            }
        }

        public override byte G
        {
            get
            {
                EnsurePixel();
                return _Value;
            }
        }

        public override byte B
        {
            get
            {
                EnsurePixel();
                return _Value;
            }
        }

        public override int BitsPerPixel
            => Source.BitsPerPixel;

        private readonly byte Threshold;
        public ThresholdingPixelAdapter(ThresholdingPixelAdapter<T> Adapter)
        {
            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                _Value = Adapter._Value;
            }

            Threshold = Adapter.Threshold;
            Source = Adapter.Source.Clone();
            InternalEnsurePixel = Adapter.InternalEnsurePixel;
        }
        public ThresholdingPixelAdapter(IImageContext Context, byte Threshold)
        {
            this.Threshold = Threshold;
            Source = Context.GetAdapter<T>(0, 0);
            InternalEnsurePixel = typeof(T) == GrayType ? a => a.R :
                                                          a => a.ToGray();
        }
        public ThresholdingPixelAdapter(PixelAdapter<T> Adapter, byte Threshold)
        {
            Adapter.InternalMove(0, 0);
            this.Threshold = Threshold;
            Source = Adapter;
            InternalEnsurePixel = typeof(T) == GrayType ? a => a.R :
                                                          a => a.ToGray();
        }
        internal ThresholdingPixelAdapter(IImageContext Context, int X, int Y, byte Threshold)
        {
            this.Threshold = Threshold;
            Source = Context.GetAdapter<T>(X, Y);
            InternalEnsurePixel = typeof(T) == GrayType ? a => a.R :
                                                          a => a.ToGray();
        }

        public override void Override(T Pixel)
            => throw new NotSupportedException();
        public override void Override(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void Override(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void OverrideTo(T* pData)
        {
            EnsurePixel();
            pData->Override(_Value, _Value, _Value, _Value);
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = _Value;
            *pDataG = _Value;
            *pDataB = _Value;
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = _Value;
            *pDataR = _Value;
            *pDataG = _Value;
            *pDataB = _Value;
        }

        public override void Overlay(T Pixel)
            => throw new NotSupportedException();
        public override void Overlay(PixelAdapter<T> Adapter)
            => throw new NotSupportedException();
        public override void Overlay(byte A, byte R, byte G, byte B)
            => throw new NotSupportedException();
        public override void OverlayTo(T* pData)
        {
            EnsurePixel();
            if (_Value == byte.MaxValue)
                pData->Override(_Value, _Value, _Value, _Value);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (_Value == byte.MaxValue)
            {
                *pDataR = _Value;
                *pDataG = _Value;
                *pDataB = _Value;
            }
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (_Value == byte.MaxValue)
            {
                *pDataA = _Value;
                *pDataR = _Value;
                *pDataG = _Value;
                *pDataB = _Value;
            }
        }

        private bool IsPixelValid = false;
        private readonly Func<PixelAdapter<T>, byte> InternalEnsurePixel;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            _Value = InternalEnsurePixel(Source) < Threshold ? byte.MinValue : byte.MaxValue;
            IsPixelValid = true;
        }

        protected internal override void InternalMove(int X, int Y)
        {
            Source.InternalMove(X, Y);
            IsPixelValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            Source.InternalMoveX(OffsetX);
            IsPixelValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            Source.InternalMoveY(OffsetY);
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            Source.InternalMoveNext();
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            Source.InternalMovePrevious();
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            Source.InternalMoveNextLine();
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            Source.InternalMovePreviousLine();
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new ThresholdingPixelAdapter<T>(this);

    }
}