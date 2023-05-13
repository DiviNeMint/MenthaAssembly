﻿using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class MedianNeighborThresholdingPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private static readonly Type GrayType = typeof(Gray8);

        private readonly ImagePatch Source;

        private readonly int MedianIndex;
        public int Level { get; }

        public override int XLength { get; }

        public override int YLength { get; }

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

        private MedianNeighborThresholdingPixelAdapter(MedianNeighborThresholdingPixelAdapter<T> Adapter)
        {
            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                _Value = Adapter._Value;
            }

            if (Adapter.IsCacheValid)
            {
                IsCacheValid = true;
                Cache = new List<byte>(Adapter.Cache);
            }
            else
            {
                Cache = new List<byte>();
            }

            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Level = Adapter.Level;
            BitsPerPixel = Adapter.BitsPerPixel;
            GetGray = Adapter.GetGray;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();
            MedianIndex = Adapter.MedianIndex;
        }
        public MedianNeighborThresholdingPixelAdapter(IImageContext Context, int Level)
        {
            Cache = new List<byte>();
            XLength = Context.Width;
            YLength = Context.Height;
            this.Level = Level;
            BitsPerPixel = Context.BitsPerPixel;
            GetGray = typeof(T) == GrayType ? a => a.R :
                                              a => a.ToGray();

            int Size = (Level << 1) + 1;
            X = 0;
            Y = 0;
            Source = new ImagePatch(Context, Size, Size);
            MedianIndex = (Size * Size) >> 1;
        }
        public MedianNeighborThresholdingPixelAdapter(PixelAdapter<T> Adapter, int Level)
        {
            Cache = new List<byte>();
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            this.Level = Level;
            BitsPerPixel = Adapter.BitsPerPixel;
            GetGray = typeof(T) == GrayType ? a => a.R :
                                              a => a.ToGray();

            int Size = (Level << 1) + 1;
            X = 0;
            Y = 0;
            Source = new ImagePatch(Adapter, Size, Size);
            MedianIndex = (Size * Size) >> 1;
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

        private bool IsPixelValid = false,
                     IsCacheValid = false;
        private readonly Func<IReadOnlyPixel, byte> GetGray;
        private readonly List<byte> Cache = new();
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            List<byte> Left = new();
            int Rx = Source.Width - 1;
            byte Gray = GetGray(Source[Level, Level]);

            for (int j = 0; j < Source.Height; j++)
                AddOrder(Left, GetGray(Source[0, j]));

            if (!IsCacheValid)
            {
                Cache.Clear();
                Cache.AddRange(Left);

                for (int i = 1; i < Rx; i++)
                    for (int j = 0; j < Source.Height; j++)
                        AddOrder(Cache, GetGray(Source[i, j]));
            }

            for (int j = 0; j < Source.Height; j++)
                AddOrder(Cache, GetGray(Source[Rx, j]));

            byte Median = Cache[MedianIndex];

            _Value = Gray < Median ? byte.MinValue : byte.MaxValue;

            foreach (byte Lv in Left)
                Cache.Remove(Lv);

            IsPixelValid = true;
            IsCacheValid = true;
        }

        private void AddOrder(List<byte> Collection, byte Value)
        {
            int i = 0;
            for (; i < Collection.Count; i++)
                if (Value < Collection[i])
                    break;

            Collection.Insert(i, Value);
        }

        public override void DangerousMove(int X, int Y)
        {
            Source.Move(X, Y);
            IsPixelValid = false;
            IsCacheValid = false;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            Source.Move(Source.X + OffsetX, Source.Y);
            IsPixelValid = false;
            IsCacheValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            Source.Move(Source.X, Source.Y + OffsetY);
            IsPixelValid = false;
            IsCacheValid = false;
        }

        public override void DangerousMoveNextX()
        {
            Source.MoveNextX();
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousX()
        {
            Source.MovePreviousX();
            IsPixelValid = false;
            IsCacheValid = false;
        }

        public override void DangerousMoveNextY()
        {
            Source.MoveNextY();
            IsPixelValid = false;
            IsCacheValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            Source.MovePreviousY();
            IsPixelValid = false;
            IsCacheValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new MedianNeighborThresholdingPixelAdapter<T>(this);

    }
}