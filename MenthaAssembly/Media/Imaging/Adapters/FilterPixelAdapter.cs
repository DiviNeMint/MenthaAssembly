using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public sealed unsafe class FilterPixelAdapter<T> : PixelAdapter<T>
        where T : unmanaged, IPixel
    {
        internal readonly ImagePatch Source;
        private readonly ImageFilter Filter;

        public override int MaxX { get; }

        public override int MaxY { get; }

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

            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            BitsPerPixel = Adapter.BitsPerPixel;

            Source = Adapter.Source.Clone();
            Filter = Adapter.Filter;
        }
        public FilterPixelAdapter(IImageContext Context, ImageFilter Filter) : this(Context, 0, 0, Filter)
        {
        }
        public FilterPixelAdapter(PixelAdapter<T> Adapter, ImageFilter Filter)
        {
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            BitsPerPixel = Adapter.BitsPerPixel;

            this.Filter = Filter;
            Source = new ImagePatch(Adapter, Filter.PatchWidth, Filter.PatchHeight);
        }
        internal FilterPixelAdapter(IImageContext Context, int X, int Y, ImageFilter Filter)
        {
            MaxX = Context.Width - 1;
            MaxY = Context.Height - 1;
            BitsPerPixel = Context.BitsPerPixel;
            this.Filter = Filter;
            Source = new ImagePatch(Context, X, Y, Filter.PatchWidth, Filter.PatchHeight);
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

            if (Args is null)
                Args = new ImageFilterArgs();

            Filter.Filter(Source, Args, out _A, out _R, out _G, out _B);
            IsPixelValid = true;
        }

        protected internal override void InternalMove(int X, int Y)
        {
            Source.Move(X, Y);
            Args = null;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveX(int OffsetX)
        {
            Source.Move(Source.X + OffsetX, Source.Y);
            Args = null;
            IsPixelValid = false;
        }
        protected internal override void InternalMoveY(int OffsetY)
        {
            Source.Move(Source.X, Source.Y + OffsetY);
            Args = null;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNext()
        {
            Source.MoveNext();
            IsPixelValid = false;
        }
        protected internal override void InternalMovePrevious()
        {
            Source.MovePrevious();
            Args = null;
            IsPixelValid = false;
        }

        protected internal override void InternalMoveNextLine()
        {
            Source.MoveNextLine();
            Args = null;
            IsPixelValid = false;
        }
        protected internal override void InternalMovePreviousLine()
        {
            Source.MovePreviousLine();
            Args = null;
            IsPixelValid = false;
        }

        public override PixelAdapter<T> Clone()
            => new FilterPixelAdapter<T>(this);

    }
}