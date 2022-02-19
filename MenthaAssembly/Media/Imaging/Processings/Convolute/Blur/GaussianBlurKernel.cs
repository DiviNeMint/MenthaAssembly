using System;

namespace MenthaAssembly.Media.Imaging
{
    public class GaussianBlurKernel : ConvoluteKernel
    {
        public override float[,] Matrix { get; }

        /// <summary>
        /// Initializes a kernel of size (n * n) where n = 2 * <paramref name="Level"/> + 1.
        /// </summary>
        public GaussianBlurKernel(int Level)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int Row = Level << 1,
                L = Row + 1,
                RowSum = 1 << Row;

            // Init
            float[] Data = new float[Level + 1];
            Data[0] = 1f;
            for (int i = 1; i <= Level; i++)
                Data[i] = MathHelper.Combination(Row, i);

            int Tx, Ty;
            float Vx;
            float[,] Kernel = new float[L, L];
            for (int i = 0; i < Level; i++)
            {
                Vx = Data[i];
                for (int j = 0; j <= Level; j++)
                {
                    float v = Vx * Data[j];
                    Tx = Row - i;
                    Ty = Row - j;
                    Kernel[j, i] = v;
                    Kernel[i, Ty] = v;
                    Kernel[Tx, j] = v;
                    Kernel[Ty, Tx] = v;
                }
            }
            Vx = Data[Level];
            Kernel[Level, Level] = Vx * Vx;

            Matrix = Kernel;
            base.PatchWidth = L;
            base.PatchHeight = L;
            KernelSum = RowSum * RowSum;
            HalfWidth = Level;
            HalfHeight = Level;
        }

        protected override byte CalculateR(float FactorR)
            => (byte)(FactorR / KernelSum);
        protected override byte CalculateG(float FactorG)
            => (byte)(FactorG / KernelSum);
        protected override byte CalculateB(float FactorB)
            => (byte)(FactorB / KernelSum);

        private static int[] GetPascalRow(int Row)
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
