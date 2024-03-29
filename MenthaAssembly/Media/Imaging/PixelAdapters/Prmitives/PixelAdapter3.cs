﻿using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal sealed unsafe class PixelAdapter3<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        public override int XLength { get; }

        public override int YLength { get; }

        public override byte A => byte.MaxValue;

        public override byte R => *pScanR;

        public override byte G => *pScanG;

        public override byte B => *pScanB;

        public override int BitsPerPixel { get; }

        private readonly long Stride;
        private byte* pScanR, pScanG, pScanB;
        private PixelAdapter3(PixelAdapter3<T> Adapter)
        {
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Stride = Adapter.Stride;
            BitsPerPixel = Adapter.BitsPerPixel;

            X = Adapter.X;
            Y = Adapter.Y;
            pScanR = Adapter.pScanR;
            pScanG = Adapter.pScanG;
            pScanB = Adapter.pScanB;
        }
        public PixelAdapter3(IImageContext Context, int X, int Y)
        {
            XLength = Context.Width;
            YLength = Context.Height;
            Stride = Context.Stride;
            BitsPerPixel = Context.BitsPerPixel;

            IntPtr[] Scan0 = Context.Scan0;
            pScanR = (byte*)Scan0[0];
            pScanG = (byte*)Scan0[1];
            pScanB = (byte*)Scan0[2];

            this.X = 0;
            this.Y = 0;
            DangerousMove(X, Y);
        }

        public override void Override(T Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Override(PixelAdapter<T> Adapter)
            => Override(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Override(byte A, byte R, byte G, byte B)
        {
            *pScanR = R;
            *pScanG = G;
            *pScanB = B;
        }
        public override void OverrideTo(T* pData)
            => pData->Override(byte.MaxValue, *pScanR, *pScanG, *pScanB);
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            *pDataA = byte.MaxValue;
            *pDataR = *pScanR;
            *pDataG = *pScanG;
            *pDataB = *pScanB;
        }

        public override void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public override void Overlay(PixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public override void Overlay(byte A, byte R, byte G, byte B)
            => PixelHelper.Overlay(ref pScanR, ref pScanG, ref pScanB, A, R, G, B);
        public override void OverlayTo(T* pData)
            => pData->Override(byte.MaxValue, *pScanR, *pScanG, *pScanB);
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
            => OverrideTo(pDataR, pDataG, pDataB);
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
            => OverrideTo(pDataA, pDataR, pDataG, pDataB);

        public override void Move(int X, int Y)
        {
            X = MathHelper.Clamp(X, 0, XLength);
            Y = MathHelper.Clamp(Y, 0, YLength);

            if (X != this.X || Y != this.Y)
                DangerousMove(X, Y);
        }

        public override void DangerousMove(int X, int Y)
        {
            long Offset = Stride * (this.Y - Y) + (this.X - X);

            this.X = X;
            this.Y = Y;

            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }
        public override void DangerousOffsetX(int OffsetX)
        {
            pScanR += OffsetX;
            pScanG += OffsetX;
            pScanB += OffsetX;
        }
        public override void DangerousOffsetY(int OffsetY)
        {
            long Offset = Stride * OffsetY;
            pScanR += Offset;
            pScanG += Offset;
            pScanB += Offset;
        }

        public override void DangerousMoveNextX()
        {
            pScanR++;
            pScanG++;
            pScanB++;
        }
        public override void DangerousMovePreviousX()
        {
            pScanR--;
            pScanG--;
            pScanB--;
        }

        public override void DangerousMoveNextY()
        {
            pScanR += Stride;
            pScanG += Stride;
            pScanB += Stride;
        }
        public override void DangerousMovePreviousY()
        {
            pScanR -= Stride;
            pScanG -= Stride;
            pScanB -= Stride;
        }

        public override PixelAdapter<T> Clone()
            => new PixelAdapter3<T>(this);

    }
}