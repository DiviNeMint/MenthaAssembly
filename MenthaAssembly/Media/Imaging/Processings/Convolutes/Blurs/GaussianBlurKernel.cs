using System;
using System.Diagnostics;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a convolution kernel calculating by the 2-D gaussian function G(x,y) = (1 / 2πσ ^ 2 ) * e ^ -[(x ^ 2 + y ^ 2) / 2 * σ ^ 2].
    /// </summary>
    public sealed class GaussianBlurKernel : ConvoluteKernel
    {
        public override float[,] Matrix { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Level">The specified value to decide the size of kernel (n * n). <para/>n = (2 * Level) + 1.</param>
        /// <param name="Sigma">The specified parameter in gaussian function.</param>
        public GaussianBlurKernel(int Level, double Sigma)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int L = (Level << 1) + 1,
                C = L >> 1;

            double k = 0.5d / (Sigma * Sigma),
                   C0 = k;
            float Sum = (float)C0;
            float[,] Kernel = new float[L, L];
            Kernel[C, C] = Sum;

            for (int j = 0; j <= Level; j++)
            {
                double j2 = j * j;
                for (int i = 1; i <= Level; i++)
                {
                    float v = (float)(C0 * Math.Exp(-(j2 + i * i) * k));
                    Sum += v * 4f;
                    Kernel[C + j, C + i] = v;
                }
            }

            // Normalized
            Kernel[C, C] /= Sum;
            for (int j = 0; j <= Level; j++)
            {
                for (int i = 1; i <= Level; i++)
                {
                    float v = Kernel[C + j, C + i] / Sum;
                    Kernel[C + j, C + i] = v;
                    Kernel[C - j, C - i] = v;
                    Kernel[C + i, C - j] = v;
                    Kernel[C - i, C + j] = v;
                }
            }

            Matrix = Kernel;
            PatchWidth = L;
            PatchHeight = L;
            KernelSum = 1f;
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