using System;
using System.Collections.Generic;
using System.Text;

namespace MenthaAssembly
{
    public static class MathHelper
    {

        public static int Abs(this int This)
        {
            int Temp = This >> 31;
            return (This ^ Temp) - Temp;
        }

        public static void Swap(ref int X, ref int Y)
        {
            X ^= Y;
            Y ^= X;
            X ^= Y;
        }
    }
}
