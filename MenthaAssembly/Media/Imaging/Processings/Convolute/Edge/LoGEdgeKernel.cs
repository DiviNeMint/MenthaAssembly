using System;

namespace MenthaAssembly.Media.Imaging
{
    public class LoGEdgeKernel : ConvoluteKernel
    {
        public override float[,] Matrix { get; }

        /// <summary>
        /// Initializes a kernel of size(n* n) where n = 2 * < paramref name="Level"/> + 1.
        /// </summary>
        public LoGEdgeKernel(int Level, float Sigma)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int L = (Level << 1) + 1;

            float[,] Kernel = new float[L, L];

            double s2 = Sigma * Sigma,
                   s4 = s2 * s2,
                   Tx, T;

            for (int i = -Level; i <= Level; i++)
            {
                Tx = i * i;
                for (int j = -Level; j <= Level; j++)
                {
                    T = -(Tx + j * j) / (2d * s2);
                    Kernel[Level + j, Level + i] = (float)Math.Round(-(1d + T) / (Math.PI * s4) * Math.Exp(T) * 1000);
                }
            }

            Matrix = Kernel;
            PatchWidth = L;
            PatchHeight = L;
            HalfWidth = Level;
            HalfHeight = Level;
        }

        protected override byte CalculateR(float FactorR)
            => (byte)FactorR.Clamp(0f, 255f);
        protected override byte CalculateG(float FactorG)
            => (byte)FactorG.Clamp(0f, 255f);
        protected override byte CalculateB(float FactorB)
            => (byte)FactorB.Clamp(0f, 255f);

    }
}
