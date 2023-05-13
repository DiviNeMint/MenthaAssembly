using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class FilterPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        internal readonly ImagePatch Source;
        private readonly ImageFilter Filter;

        public override int XLength { get; }

        public override int YLength { get; }

        private byte _A;
        public override byte A
        {
            get
            {
                EnsurePixel();
                return _A;
            }
        }

        private byte _R;
        public override byte R
        {
            get
            {
                EnsurePixel();
                return _R;
            }
        }

        private byte _G;
        public override byte G
        {
            get
            {
                EnsurePixel();
                return _G;
            }
        }

        private byte _B;
        public override byte B
        {
            get
            {
                EnsurePixel();
                return _B;
            }
        }

        public override int BitsPerPixel { get; }

        private FilterPixelAdapter(FilterPixelAdapter<T> Adapter)
        {
            if (IsPixelValid)
            {
                _A = Adapter._A;
                _R = Adapter._R;
                _G = Adapter._G;
                _B = Adapter._B;
                IsPixelValid = true;
            }
            Args = Adapter.Args?.Clone();

            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = Adapter.Source.Clone();
            Filter = Adapter.Filter;
        }
        public FilterPixelAdapter(IImageContext Context, ImageFilter Filter) : this(Context, 0, 0, Filter)
        {
        }
        public FilterPixelAdapter(PixelAdapter<T> Adapter, ImageFilter Filter)
        {
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            BitsPerPixel = Adapter.BitsPerPixel;

            X = Adapter.X;
            Y = Adapter.Y;
            Source = new ImagePatch(Adapter, Filter.PatchWidth, Filter.PatchHeight);
            this.Filter = Filter;
        }
        internal FilterPixelAdapter(IImageContext Context, int X, int Y, ImageFilter Filter)
        {
            XLength = Context.Width;
            YLength = Context.Height;
            BitsPerPixel = Context.BitsPerPixel;

            this.X = X;
            this.Y = Y;
            Source = new ImagePatch(Context, X, Y, Filter.PatchWidth, Filter.PatchHeight);
            this.Filter = Filter;
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
            pData->Override(_A, _R, _G, _B);
        }
        public override void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataR = _R;
            *pDataG = _G;
            *pDataB = _B;
        }
        public override void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            *pDataA = _A;
            *pDataR = _R;
            *pDataG = _G;
            *pDataB = _B;
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
            pData->Overlay(_A, _R, _G, _B);
        }
        public override void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            PixelHelper.Overlay(ref pDataR, ref pDataG, ref pDataB, _A, _R, _G, _B);
        }
        public override void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB)
        {
            EnsurePixel();
            PixelHelper.Overlay(ref pDataA, ref pDataR, ref pDataG, ref pDataB, _A, _R, _G, _B);
        }

        private ImageFilterArgs Args;
        private bool IsPixelValid = false;
        private void EnsurePixel()
        {
            if (IsPixelValid)
                return;

            Args ??= new ImageFilterArgs();
            Filter.Filter(Source, Args, out _A, out _R, out _G, out _B);
            IsPixelValid = true;
        }

        public override void DangerousMove(int X, int Y)
        {
            Source.DangerousMove(X, Y);
            Args = null;
            IsPixelValid = false;
        }

        public override void DangerousOffsetX(int Delta)
        {
            Source.DangerousOffsetX(Delta);
            Args = null;
            IsPixelValid = false;
        }
        public override void DangerousOffsetY(int Delta)
        {
            Source.DangerousOffsetY(Delta);
            Args = null;
            IsPixelValid = false;
        }

        public override void DangerousMoveNextX()
        {
            Source.DangerousMoveNextX();
            IsPixelValid = false;
        }
        public override void DangerousMoveNextY()
        {
            Source.DangerousMoveNextY();
            Args = null;
            IsPixelValid = false;
        }

        public override void DangerousMovePreviousX()
        {
            Source.DangerousMovePreviousX();
            Args = null;
            IsPixelValid = false;
        }
        public override void DangerousMovePreviousY()
        {
            Source.DangerousMovePreviousY();
            Args = null;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new FilterPixelAdapter<T>(this);

    }
}