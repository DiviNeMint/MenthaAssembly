using System;

namespace MenthaAssembly.Media.Imaging
{
    public struct Indexed8 : IPixelIndexed
    {
        private byte Data;
        byte IPixelIndexed.Data => this.Data;

        public int BitsPerPixel => 8;

        public int Length => 1;

        public int this[int Index]
        {
            get => Index == 0 ?
                   Data >> 0 :
                   throw new ArgumentOutOfRangeException();
            set
            {
                if (Index != 0)
                    throw new ArgumentOutOfRangeException();

                Data = (byte)value;
            }
        }

    }
}
