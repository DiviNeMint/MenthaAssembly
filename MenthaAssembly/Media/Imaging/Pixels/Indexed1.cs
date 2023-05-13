using System;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
    public struct Indexed1 : IPixelIndexed
    {
        private byte Data;

        public int BitsPerPixel => 1;

        public int Length => 8;

        private static readonly int MaxIndex;
        internal static readonly int BaseMask;
        internal static readonly int[] IndexMasks;
        static Indexed1()
        {
            IndexMasks = new int[8];            // 8 / BitsPerPixel
            MaxIndex = IndexMasks.Length - 1;
            BaseMask = 0x01;                    // 0xFF >> (8 - BitsPerPixel);

            for (int i = 0; i < IndexMasks.Length; i++)      // i += BitsPerPixel
                IndexMasks[i] = ~(BaseMask << (MaxIndex - i));
        }

        public int this[int Index]
        {
            get => Index < 0 || Index > MaxIndex ?
                   throw new ArgumentOutOfRangeException() :
                   (Data >> (MaxIndex - Index)) & BaseMask;
            set
            {
                if (Index < 0 || Index > MaxIndex)
                    throw new ArgumentOutOfRangeException();

                Data = (byte)(((value & BaseMask) << (MaxIndex - Index)) | (Data & IndexMasks[Index]));
            }
        }

    }
}