using System;

namespace MenthaAssembly.Media.Imaging
{
    public class SobelEdgeKernel : ConvoluteKernel
    {
        public override int[,] Kernel { get; }

        public override int KernelWidth { get; }

        public override int KernelHeight { get; }

        /// <summary>
        /// Initializes a kernel of size (n * n) where n =2 * <paramref name="Level"/> + 1.
        /// </summary>
        public SobelEdgeKernel(int Level, Sides Side)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int L = (Level << 1) + 1,
                k = 1,
                Tx, T;

            for (int i = 2; i <= Level; i++)
                k = k * i / GCD(k, i);

            k <<= 1;

            for (int i = 2; i <= Level; i++)
            {
                Tx = i * i;
                for (int j = 1; j < i; j++)
                {
                    T = j * j + Tx;
                    k = k * T / GCD(k, T);
                }
            }

            int[,] Kernel = new int[L, L];

            Action<int, int, int> AddValue = null;
            switch (Side)
            {
                case Sides.Left:
                    AddValue = (x, y, v) => Kernel[Level + y, Level - x] = v;
                    break;
                case Sides.Right:
                    AddValue = (x, y, v) => Kernel[Level + y, Level + x] = v;
                    break;
                case Sides.Top:
                    AddValue = (x, y, v) => Kernel[Level - x, Level + y] = v;
                    break;
                case Sides.Bottom:
                    AddValue = (x, y, v) => Kernel[Level + x, Level + y] = v;
                    break;
            }

            for (int i = -Level; i <= Level; i++)
            {
                Tx = i * i;
                for (int j = -Level; j <= Level; j++)
                {
                    if (Tx == 0 & j == 0)
                        continue;

                    AddValue(i, j, i * k / (Tx + j * j));
                }
            }

            this.Kernel = Kernel;
            KernelWidth = L;
            KernelHeight = L;
            HalfWidth = Level;
            HalfHeight = Level;
        }

        private static int GCD(int a, int b)
        {
            if (a < b)
                MathHelper.Swap(ref a, ref b);

            int t = a % b;
            return t > 0 ? GCD(b, t) : b;
        }

    }
}
