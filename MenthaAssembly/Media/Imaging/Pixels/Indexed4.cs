using System;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
    public struct Indexed4 : IPixelIndexed
    {
        private byte Data;

        public int BitsPerPixel => 4;

        public int Length => 2;

        private static readonly int MaxIndex;
        internal static readonly int BaseMask;
        internal static readonly int[] IndexMasks;
        static Indexed4()
        {
            IndexMasks = new int[2];            // 8 / BitsPerPixel
            MaxIndex = IndexMasks.Length - 1;
            BaseMask = 0x0F;                    // 0xFF >> (8 - BitsPerPixel);

            for (int i = 0; i < IndexMasks.Length; i++)
                IndexMasks[i] = ~(BaseMask << ((MaxIndex - i) << 2));   // ~(BaseMask << (MaxIndex - i * BitsPerPixel)); 
        }

        public int this[int Index]
        {
            get => Index < 0 || Index > MaxIndex ? throw new ArgumentOutOfRangeException() :
                                                   (Data >> ((MaxIndex - Index) << 2)) & BaseMask;
            set
            {
                if (Index < 0 || Index > MaxIndex)
                    throw new ArgumentOutOfRangeException();

                Data = (byte)(((value & BaseMask) << ((MaxIndex - Index) << 2)) | (Data & IndexMasks[Index]));
            }
        }

    }
}