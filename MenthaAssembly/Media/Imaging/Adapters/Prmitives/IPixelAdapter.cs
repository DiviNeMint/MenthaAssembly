using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public unsafe interface IPixelAdapter : IPixel
    {
        public int X { get; }

        public int Y { get; }

        public int MaxX { get; }

        public int MaxY { get; }

        public Type PixelType { get; }

        public void OverrideTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverrideTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverlayTo(byte* pDataR, byte* pDataG, byte* pDataB);

        public void OverlayTo(byte* pDataA, byte* pDataR, byte* pDataG, byte* pDataB);

        public void Move(int X, int Y);

        public void MoveX(int OffsetX);

        public void MoveY(int OffsetY);

        public void MoveNext();

        public void MovePrevious();

        public void MoveNextLine();

        public void MovePreviousLine();

        public IPixelAdapter Clone();

    }
}