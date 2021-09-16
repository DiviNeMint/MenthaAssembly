using System;

namespace MenthaAssembly.Media.Imaging
{
    public class GaussianBlurKernel : ConvoluteKernel
    {
        public override int[,] Kernel { get; }

        public override int KernelWidth { get; }

        public override int KernelHeight { get; }

        public override int KernelSum { get; }

        /// <summary>
        /// Initializes a kernel of size (n * n) where n =2 * <paramref name="Level"/> + 1.
        /// </summary>
        public GaussianBlurKernel(int Level)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int L = (Level << 1) + 1,
                Temp;
            int[] Data = GetPascalRow(L);

            int[,] Kernel = new int[L, L];
            for (int i = 0; i < L; i++)
            {
                int k = Data[i];
                for (int j = 0; j < L; j++)
                    Kernel[j, i] = Data[j] * k;
            }

            this.Kernel = Kernel;
            KernelWidth = L;
            KernelHeight = L;
            Temp = 1 << (Level << 1);
            KernelSum = Temp * Temp;
        }

        private int[] GetPascalRow(int Row)
        {
            int[] Data = new int[Row];
            for (int i = 0; i < Row; i++)
                Data[i] = 1;

            for (int i = 2; i < Row; i++)
                for (int j = i - 1; j > 0; j--)
                    Data[j] += Data[j - 1];

            return Data;
        }
    }
}
