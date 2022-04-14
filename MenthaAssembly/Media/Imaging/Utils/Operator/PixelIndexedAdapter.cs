using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelIndexedAdapterBase<T, Struct>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public ImagePalette<T> Palette { get; }

        protected int XBit;
        private readonly int BitLength;
        protected Struct* pScan;
        public PixelIndexedAdapterBase(Struct* pScan, int XBit, ImagePalette<T> Palette)
        {
            this.pScan = pScan;

            this.XBit = XBit;
            BitLength = pScan->Length;
            while (this.XBit < 0)
            {
                this.XBit += BitLength;
                this.pScan--;
            }
            while (BitLength <= this.XBit)
            {
                this.XBit -= BitLength;
                this.pScan++;
            }

            this.Palette = Palette;
            GetPaletteIndexFunc = ResetGetPaletteIndex;
        }

        public void Override(byte A, byte R, byte G, byte B)
        {
            T Pixel = PixelHelper.ToPixel<T>(A, R, G, B);
            if (!Palette.TryGetOrAdd(Pixel, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            (*pScan)[XBit] = Index;
        }
        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }
        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            *pDataA = Pixel.A;
            *pDataR = Pixel.R;
            *pDataG = Pixel.G;
            *pDataB = Pixel.B;
        }

        public void Overlay(byte A, byte R, byte G, byte B)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            Pixel.Overlay(A, R, G, B);

            if (!Palette.TryGetOrAdd(Pixel, out Index))
                throw new IndexOutOfRangeException("Palette is full.");

            (*pScan)[XBit] = Index;
        }
        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

        private Func<int> GetPaletteIndexFunc;
        public int GetPaletteIndex()
            => GetPaletteIndexFunc();
        private int ResetGetPaletteIndex()
        {
            int Index = (*pScan)[XBit];
            GetPaletteIndexFunc = () => Index;
            return Index;
        }

        public void MoveNext()
        {
            XBit++;
            if (BitLength <= XBit)
            {
                XBit -= BitLength;
                pScan++;
            }
            GetPaletteIndexFunc = ResetGetPaletteIndex;
        }
        public void MovePrevious()
        {
            XBit--;
            if (XBit < 0)
            {
                XBit += BitLength;
                pScan--;
            }
            GetPaletteIndexFunc = ResetGetPaletteIndex;
        }

    }

    internal unsafe class PixelIndexedAdapter<T, Struct> : PixelIndexedAdapterBase<T, Struct>, IPixelAdapter<T>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public PixelIndexedAdapter(Struct* pScan, int XBit, GCHandle pPalette) : base(pScan, XBit, (ImagePalette<T>)pPalette.Target)
        {

        }

        public void Override(T Pixel)
        {
            if (!Palette.TryGetOrAdd(Pixel, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            (*pScan)[XBit] = Index;
        }
        public void OverrideTo(T* pData)
        {
            int Index = GetPaletteIndex();
            *pData = Palette[Index];
        }

        public void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void OverlayTo(T* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
    }

    internal unsafe class PixelIndexedAdapter<T, U, Struct> : PixelIndexedAdapterBase<T, Struct>, IPixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public PixelIndexedAdapter(Struct* pScan, int XBit, GCHandle pPalette) : base(pScan, XBit, (ImagePalette<T>)pPalette.Target)
        {

        }

        public void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void OverrideTo(U* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

        public void Overlay(U Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void OverlayTo(U* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }
    }

}
