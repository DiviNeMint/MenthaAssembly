using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelIndexedAdapterBase<T, Struct>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public byte A
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].A;
            }
        }

        public byte R
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].R;
            }
        }

        public byte G
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].G;
            }
        }

        public byte B
        {
            get
            {
                int Index = GetPaletteIndex();
                return Palette[Index].B;
            }
        }

        public int BitsPerPixel => pScan->BitsPerPixel;

        public ImagePalette<T> Palette { get; }

        protected int XBit;
        private readonly int BitLength;
        private readonly long Stride;
        protected Struct* pScan;
        public PixelIndexedAdapterBase(PixelIndexedAdapterBase<T, Struct> Adapter)
        {
            pScan = Adapter.pScan;
            Stride = Adapter.Stride;
            BitLength = Adapter.BitLength;
            XBit = Adapter.XBit;
            Palette = Adapter.Palette;
            GetPaletteIndexFunc = Adapter.GetPaletteIndexFunc;
        }
        public PixelIndexedAdapterBase(Struct* pScan, long Stride, int XBit, ImagePalette<T> Palette)
        {
            this.pScan = pScan;
            this.Stride = Stride;

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

        public void Move(int Offset)
        {
            XBit += Offset;

            if (BitLength <= XBit)
            {
                do
                {
                    XBit -= BitLength;
                    pScan++;
                } while (BitLength <= XBit);
            }
            else if (XBit < 0)
            {
                do
                {
                    XBit += BitLength;
                    pScan--;
                } while (XBit < 0);
            }

            GetPaletteIndexFunc = ResetGetPaletteIndex;
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

        public void MoveNextLine()
            => pScan = (Struct*)((byte*)pScan + Stride);
        public void MovePreviousLine()
            => pScan = (Struct*)((byte*)pScan - Stride);
    }

    internal unsafe class PixelIndexedAdapter<T, Struct> : PixelIndexedAdapterBase<T, Struct>, IPixelAdapter<T>
        where T : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public PixelIndexedAdapter(PixelIndexedAdapter<T, Struct> Adapter) : base(Adapter)
        {

        }
        public PixelIndexedAdapter(Struct* pScan, long Stride, int XBit, GCHandle pPalette) : base(pScan, Stride, XBit, (ImagePalette<T>)pPalette.Target)
        {

        }

        public void Override(T Pixel)
        {
            if (!Palette.TryGetOrAdd(Pixel, out int Index))
                throw new IndexOutOfRangeException("Palette is full.");

            (*pScan)[XBit] = Index;
        }
        public void Override(IPixelAdapter<T> Adapter)
        {
            T Pixel;
            Adapter.OverrideTo(&Pixel);
            Override(Pixel);
        }
        public void OverrideTo(T* pData)
        {
            int Index = GetPaletteIndex();
            *pData = Palette[Index];
        }

        public void Overlay(T Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Overlay(IPixelAdapter<T> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverlayTo(T* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

        public IPixelAdapter<T> Clone()
            => new PixelIndexedAdapter<T, Struct>(this);

    }

    internal unsafe class PixelIndexedAdapter<T, U, Struct> : PixelIndexedAdapterBase<T, Struct>, IPixelAdapter<U>
        where T : unmanaged, IPixel
        where U : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        public PixelIndexedAdapter(PixelIndexedAdapter<T, U, Struct> Adapter) : base(Adapter)
        {

        }
        public PixelIndexedAdapter(Struct* pScan, long Stride, int XBit, GCHandle pPalette) : base(pScan, Stride, XBit, (ImagePalette<T>)pPalette.Target)
        {

        }

        public void Override(U Pixel)
            => Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Override(IPixelAdapter<U> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverrideTo(U* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Override(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

        public void Overlay(U Pixel)
            => Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public void Overlay(IPixelAdapter<U> Adapter)
            => Overlay(Adapter.A, Adapter.R, Adapter.G, Adapter.B);
        public void OverlayTo(U* pData)
        {
            int Index = GetPaletteIndex();
            T Pixel = Palette[Index];
            pData->Overlay(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        }

        public IPixelAdapter<U> Clone()
            => new PixelIndexedAdapter<T, U, Struct>(this);

    }

}
