using System;
using System.Diagnostics;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class MeanNeighborThresholdingPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private static readonly Type GrayType = typeof(Gray8);

        private readonly ImagePatch Source;

        private readonly int Denominator;
        public int Level { get; }

        public override int MaxX { get; }

        public override int MaxY { get; }

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

        public override int BitsPerPixel { get; }

        private MeanNeighborThresholdingPixelAdapter(MeanNeighborThresholdingPixelAdapter<T> Adapter)
        {
            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                _Value = Adapter._Value;
            }

            if (Adapter.IsCacheValid)
            {
                IsCacheValid = true;
                Cache = Adapter.Cache;
            }

            X = Adapter.X;
            Y = Adapter.Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Level = Adapter.Level;
            Source = Adapter.Source.Clone();
            BitsPerPixel = Adapter.BitsPerPixel;
            Denominator = Adapter.Denominator;
            GetGray = Adapter.GetGray;
        }
        public MeanNeighborThresholdingPixelAdapter(IImageContext Context, int Level)
        {
            X = 0;
            Y = 0;
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            this.Level = Level;

            int L = (Level << 1) + 1;
            IPixelAdapter Adapter = Context.GetAdapter(0, 0);
            Source = new ImagePatch(Adapter, 0, 0, L, L);
            Denominator = L * L;
            BitsPerPixel = Context.BitsPerPixel;
            GetGray = typeof(T) == GrayType ? a => a.R :
                                              a => a.ToGray();
        }
        public MeanNeighborThresholdingPixelAdapter(PixelAdapter<T> Adapter, int Level)
        {
            Adapter.InternalMove(0, 0);
            X = 0;
            Y = 0;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            this.Level = Level;

            int L = (Level << 1) + 1;
            Source = new ImagePatch(Adapter, L, L);
            Denominator = L * L;
            BitsPerPixel = Adapter.BitsPerPixel;
            GetGray = typeof(T) == GrayType ? a => a.R :
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

        private bool IsPixelValid = false,
                     IsCacheValid = false;
        private readonly Func<IReadOnlyPixel, byte> GetGray;
        private int Cache;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            int Gray = GetGray(Source[Level, Level]),
                Rx = Source.Width - 1,
                Left = 0;

            for (int j = 0; j < Source.Height; j++)
                Left += GetGray(Source[0, j]);

            if (!IsCacheValid)
            {
                Cache = Left;
                for (int i = 1; i < Rx; i++)
                    for (int j = 0; j < Source.Height; j++)
                        Cache += GetGray(Source[i, j]);
            }

            for (int j = 0; j < Source.Height; j++)
                Cache += GetGray(Source[Rx, j]);

            int Tg = Cache / Denominator;
            _Value = Gray < Tg ? byte.MinValue : byte.MaxValue;

            Cache -= Left;

            IsPixelValid = true;
            IsCacheValid = true;
        }

        protected internal override void InternalMove(int X, int Y)
        {
            Source.Move(X, Y);
            IsPixelValid = false;
            IsCacheValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            Source.Move(Source.X + OffsetX, Source.Y);
            IsPixelValid = false;
            IsCacheValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            Source.Move(Source.X, Source.Y + OffsetY);
            IsPixelValid = false;
            IsCacheValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            Source.MoveNext();
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            Source.MovePrevious();
            IsPixelValid = false;
            IsCacheValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            Source.MoveNextLine();
            IsPixelValid = false;
            IsCacheValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            Source.MovePreviousLine();
            IsPixelValid = false;
            IsCacheValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new MeanNeighborThresholdingPixelAdapter<T>(this);

    }
}