using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class RangesThresholdingPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private static readonly Type GrayType = typeof(Gray8);

        private readonly PixelAdapter<T> Source;

        public override int XLength => Source.XLength;

        public override int YLength => Source.YLength;

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

        private readonly byte[] Ranges;
        private RangesThresholdingPixelAdapter(RangesThresholdingPixelAdapter<T> Adapter)
        {
            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                _Value = Adapter._Value;
            }

            Ranges = Adapter.Ranges;
            GetGray = Adapter.GetGray;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();
        }
        public RangesThresholdingPixelAdapter(IImageContext Context, params byte[] Ranges)
        {
            if ((Ranges.Length & 0x01) > 1)
                throw new ArgumentException("The length of the range must be an even number.");

            this.Ranges = Ranges;
            GetGray = typeof(T) == GrayType ? a => a.R :
                                              a => a.ToGray();

            X = 0;
            Y = 0;
            Source = Context.GetAdapter<T>(0, 0);
        }
        public RangesThresholdingPixelAdapter(PixelAdapter<T> Adapter, params byte[] Ranges)
        {
            if ((Ranges.Length & 0x01) > 1)
                throw new ArgumentException("The length of the range must be an even number.");

            this.Ranges = Ranges;
            GetGray = typeof(T) == GrayType ? a => a.R :
                                              a => a.ToGray();

            X = 0;
            Y = 0;
            Source = Adapter;
            Adapter.DangerousMove(0, 0);
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
        private readonly Func<PixelAdapter<T>, byte> GetGray;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            byte Gray = GetGray(Source);

            bool IsContain = false;
            for (int i = 0; i < Ranges.Length;)
            {
                int Sv = Ranges[i++],
                    Ev = Ranges[i++];
                if (Sv <= Gray && Gray <= Ev)
                {
                    IsContain = true;
                    break;
                }

            }

            _Value = IsContain ? byte.MaxValue : byte.MinValue;
            IsPixelValid = true;
        }

        public override void DangerousMove(int X, int Y)
        {
            Source.DangerousMove(X, Y);
            IsPixelValid = false;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            Source.DangerousOffsetX(OffsetX);
            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            Source.DangerousOffsetY(OffsetY);
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            Source.DangerousMoveNextX();
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousX()
        {
            Source.DangerousMovePreviousX();
            IsPixelValid = false;
        }

        public override void DangerousMoveNextY()
        {
            Source.DangerousMoveNextY();
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            Source.DangerousMovePreviousY();
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new RangesThresholdingPixelAdapter<T>(this);

    }
}