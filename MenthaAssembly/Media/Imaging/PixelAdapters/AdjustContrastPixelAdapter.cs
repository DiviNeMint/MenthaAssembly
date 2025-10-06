using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class AdjustContrastPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        private static readonly Type GrayType = typeof(Gray8);

        private readonly IPixelAdapter Adapter;

        public override int XLength { get; }

        public override int YLength { get; }

#pragma warning disable IDE0044 // 新增唯讀修飾元
        // If sets readonly, it will cause pixel's value can't change.
        private T Pixel;
#pragma warning restore IDE0044 // 新增唯讀修飾元

        public override byte A
        {
            get
            {
                EnsurePixel();
                return Pixel.A;
            }
        }

        public override byte R
        {
            get
            {
                EnsurePixel();
                return Pixel.R;
            }
        }

        public override byte G
        {
            get
            {
                EnsurePixel();
                return Pixel.G;
            }
        }

        public override byte B
        {
            get
            {
                EnsurePixel();
                return Pixel.B;
            }
        }

        public override int BitsPerPixel { get; }

        public double Contrast { get; }

        private readonly Action Adjuster;
        private readonly Dictionary<byte, byte> Cache;
        private AdjustContrastPixelAdapter(AdjustContrastPixelAdapter<T> Adapter)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            Cache = Adapter.Cache;
            Adjuster = Adapter.Adjuster;
            Contrast = Adapter.Contrast;
            this.Adapter = Adapter.Adapter.Clone();

            if (Adapter.IsPixelValid)
            {
                IsPixelValid = true;
                Pixel = Adapter.Pixel;
            }
        }
        public AdjustContrastPixelAdapter(IPixelAdapter Adapter, double Contrast)
        {
            X = Adapter.X;
            Y = Adapter.Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;
            Cache = [];
            Adjuster = typeof(T) == GrayType ? EnhanceGray : EnhanceRGB;
            this.Contrast = Contrast;
            this.Adapter = Adapter;

            void EnhanceGray()
            {
                byte Gray = Adjust(Adapter.R);
                Pixel.Override(Adapter.A, Gray, Gray, Gray);
            }
            void EnhanceRGB()
            {
                Pixel.Override(Adapter.A, Adjust(Adapter.R), Adjust(Adapter.G), Adjust(Adapter.B));
            }
        }

        public override void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<T> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
            => Adapter.Override(A, R, G, B);

        public override void OverrideTo(T* pData)
        {
            EnsurePixel();
            *pData = Pixel;
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = Pixel.A;
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }

        public override void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MaxValue)
                Adapter.Override(A, R, G, B);
            else
                Adapter.Overlay(A, R, G, B);
        }

        public override void OverlayTo(T* pData)
        {
            EnsurePixel();
            if (Pixel.A == byte.MaxValue)
                *pData = Pixel;
            else
                pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (Pixel.A == byte.MaxValue)
            {
                *pDataR = Pixel.R;
                *pDataG = Pixel.G;
                *pDataB = Pixel.B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
            }
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            if (Pixel.A == byte.MaxValue)
            {
                *pDataA = byte.MaxValue;
                *pDataR = Pixel.R;
                *pDataG = Pixel.G;
                *pDataB = Pixel.B;
            }
            else
            {
                PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
            }
        }

        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            Pixel.Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
            IsPixelValid = true;
        }
        private byte Adjust(byte Value)
        {
            if (Cache.TryGetValue(Value, out byte Result))
                return Result;

            double NewValue = (Value - 127.5) * Contrast + 127.5;
            Result = NewValue switch
            {
                < 0 => byte.MinValue,
                > 255 => byte.MaxValue,
                _ => (byte)NewValue
            };

            Cache[Value] = Result;
            return Result;
        }

        public override void DangerousMove(int X, int Y)
        {
            Adapter.DangerousMove(X, Y);
            IsPixelValid = false;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            Adapter.DangerousOffsetX(OffsetX);
            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            Adapter.DangerousOffsetY(OffsetY);
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            Adapter.DangerousMoveNextX();
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousX()
        {
            Adapter.DangerousMovePreviousX();
            IsPixelValid = false;
        }

        public override void DangerousMoveNextY()
        {
            Adapter.DangerousMoveNextY();
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            Adapter.DangerousMovePreviousY();
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new AdjustContrastPixelAdapter<T>(this);

    }
}